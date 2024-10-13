using System.Diagnostics;

//THIS FILE IS TO BE COMPILED SEPERATELY IN DEPLOYMENT
/*

This secondary watchdog monitors the main watchdog. It verifies if it running. 
\
If not running, it verifies the existence of the file then runs it.

if doesnt exist, it takes from another copy of the watchdog, copies it to the orig location, then runs it
if the secondary location of the primary watchdog gets removed, the secondary watchdog copies it back again


*/
namespace MonitorWatchdog
{
    public class MonitorWatchdog
    {
        static void Main(string[] args)
        {
            string watchdogProcessName = "WindowsPersistence";  // Name of the watchdog binary (without .exe)
            string watchdogBinaryPath = @"C:\Users\will\source\repos\WindowsPersistence\WindowsPersistence\bin\Debug\net8.0\WindowsPersistence.exe"; // Full path to the watchdog binary

            while (true)
            {
                if (!IsProcessRunning(watchdogProcessName))
                {
                    Console.WriteLine("Watchdog process not running. Starting it now...");

                    // Start the watchdog process
                    StartWatchdog(watchdogBinaryPath);
                }
                else
                {
                    Console.WriteLine("Watchdog process is running.");
                }

                // Sleep before checking again
                Thread.Sleep(2000); // Check every 5 seconds
            }
        }

        // Check if a process is running by name
        static bool IsProcessRunning(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }

        // Start the watchdog binary
        static void StartWatchdog(string watchdogBinaryPath)
        {
            if (File.Exists(watchdogBinaryPath))
            {
                Process.Start(watchdogBinaryPath);
            }
            else
            {
                Console.WriteLine($"Watchdog binary not found at {watchdogBinaryPath}");
            }
        }
    }
}