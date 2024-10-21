using Newtonsoft.Json.Linq;
using ServiceStack.Text;
using System.Diagnostics;
using System.Reflection;

namespace watchdogHelper{
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
                mutex.ReleaseMutex();
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
        Process[] processes = Process.GetProcessesByName(processName);
        return processes.Length > 0;
    }

        public static void verifyFilePathsSourceAndDest(string destinationPath, string filename) //checks for if file exists then copies it if not
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string sourcePathFile = Path.Combine(currentDirectory, filename);
            string destPathFile = Path.Combine(destinationPath, filename);


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
    

    public static void runBinary(string binaryPath, string arguments = "")
    {
        try
        {
            // Initialize the process start information
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = binaryPath,
                Arguments = arguments,
                UseShellExecute = false,   // Set to true if you want to use the system shell
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = false      // Set to false if you want a window to appear
            };

            // Start the process
            using (Process process = Process.Start(startInfo))
            {
                // Capture output if needed
                string output = process.StandardOutput.ReadToEnd();
                string errors = process.StandardError.ReadToEnd();

                // Wait for the process to exit
                process.WaitForExit();

                // Output the results
                Console.WriteLine("Output:");
                Console.WriteLine(output);

                if (!string.IsNullOrEmpty(errors))
                {
                    Console.WriteLine("Errors:");
                    Console.WriteLine(errors);
                }

                Console.WriteLine("Process exited with code " + process.ExitCode);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to run binary: {ex.Message}");
        }
    }

    public static void CheckAndRunWatchdog(string watchdogPath, string watchdogName, string mutex)
    {
            if(!IsMutexRunning(mutex)){
                runBinary(Path.Combine(watchdogPath, watchdogName));
            }
    }

    public static void CheckAndRunPayload(string payloadPath, string payloadName)
    {
            if(!IsProcessRunning(payloadName)){
                runBinary(Path.Combine(payloadPath, payloadName));
            }
    }

    
    }
}