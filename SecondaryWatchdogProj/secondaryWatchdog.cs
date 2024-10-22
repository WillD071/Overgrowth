
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
            using (Mutex mutex = new Mutex(false, Config.SecondaryWatchdogMutexName, out bool isNewInstance))
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
                watchdogHelper.watchdogHelper.verifyFilePathsSourceAndDest(Config.PrimaryWatchdogPath, Config.PrimaryWatchdogName);
                watchdogHelper.watchdogHelper.CheckAndRunWatchdog(Config.PrimaryWatchdogPath, Config.PrimaryWatchdogName, Config.PrimaryWatchdogMutexName);
                Thread.Sleep(1000);
            }
        }

    }
}