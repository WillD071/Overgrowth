using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace watchdogHelper
{
    public class watchdogHelper
    {

        public static bool IsMutexRunning(string mutexName)
        {
            bool isNewInstance;

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

        public static bool IsProcessRunning(string processName)
        {
            // Get a list of processes by name
            processName = processName.Replace(".exe", "");

            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }

        public static void verifyFilePathsSourceAndDest(string destinationPath, string filename) //checks for if file exists then copies it if not
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
                        Console.WriteLine($"'{filename}' found and copied to {destPathFile}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error copying file: {ex.Message}");
                    }
                }
                else if (File.Exists(destPathFile))
                {
                    try
                    {
                        File.Copy(destPathFile, sourcePathFile, overwrite: false);
                        Console.WriteLine($"'{filename}' found and copied to {sourcePathFile}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error copying file: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"'{filename}' not found in the current directory.");
                }
            }
        }


        public static void runBinary(string filePath, string arguments = "")
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
                        System.Console.WriteLine(e.Data);
                    }
                };
                process.BeginOutputReadLine();

                // Note: No WaitForExit(), allowing the process to run without blocking
            }
        }


        public static void CheckAndRunWatchdog(string watchdogPath, string watchdogName, string mutex)
        {
            if (!IsMutexRunning(mutex))
            {
                runBinary(Path.Combine(watchdogPath, watchdogName));
            }
        }

        public static void CheckAndRunPayload(string payloadPath, string payloadName)
        {
            if (!IsProcessRunning(payloadName))
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
                Console.WriteLine($"An error occurred while creating the directory: {ex.Message}");
            }
        }


    }
}