
//THIS FILE IS TO BE COMPILED SEPERATELY IN DEPLOYMENT
/*

This secondary watchdog monitors the main watchdog. It verifies if it running. 
\
If not running, it verifies the existence of the file then runs it.

if doesnt exist, it takes from another copy of the watchdog, copies it to the orig location, then runs it
if the secondary location of the primary watchdog gets removed, the secondary watchdog copies it back again


*/
using System.Diagnostics;

namespace MonitorWatchdog
{
    public class MonitorWatchdog
    {
        static void Main(string[] args)
        {
            bool isAdmin = watchdogHelper.IsRunningAsAdministrator();
            watchdogHelper.Log("Current process is running with " + (isAdmin ? "Administrator" : "User") + " privileges.");

            using (Mutex mutex = new Mutex(false, Config.SecondaryWatchdogMutexName, out bool isNewInstance))
            {
                if (!isNewInstance && isAdmin)
                {
                    watchdogHelper.Log("Another instance of the watchdog is already running.");

                    int? PID = watchdogHelper.GetProcessIdByName(Process.GetCurrentProcess().ProcessName);

                    if (PID.HasValue)
                    {
                        string permissionLevel = watchdogHelper.GetProcessPermissionLevel((int)PID);
                        // rest of your code here

                        if (permissionLevel != "Administrator")
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
                }
                else if (!isNewInstance)
                {
                    Environment.Exit(0);
                }

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
            //  loop for frequent checks
            int SecondarySleep = Config.sleepTime + 5000;
            while (true)
            {
                watchdogHelper.verifyFilePathsSourceAndDest(Config.PrimaryWatchdogPath, Config.PrimaryWatchdogName);
                watchdogHelper.CheckAndRunWatchdog(Config.PrimaryWatchdogPath, Config.PrimaryWatchdogName, Config.PrimaryWatchdogMutexName);
                Thread.Sleep(SecondarySleep);
            }
        }

    }
}