
public class Config
{
	
    public static string PayloadPath { get; private set; } = @"C:\Windows\Test\Test2"; // This will be created if it doesnt already exist
    public static string PayloadName { get; private set; } = "TestPayload.exe";
    public static string PayloadFullPath { get; private set; } = Path.Combine(PayloadPath, PayloadName);




    public static string SecondaryWatchdogPath { get; private set; } = @"C:\Windows\Test\Test1"; // This will be created if it doesnt already exist
    public static string SecondaryWatchdogName { get; private set; } = "Windows Disk Maintenance.exe";
    public static string SecondaryWatchdogMutexName { get; private set; } = "SecondaryWDog";
    public static string SecondaryWatchdogFullPath { get; private set; } = Path.Combine(SecondaryWatchdogPath, SecondaryWatchdogName);


    public static string PrimaryWatchdogPath { get; private set; } = @"C:\Windows\Test"; // CRUCIAL: This is the foler you will be put both watchdogs and the payload into
    public static string PrimaryWatchdogName { get; private set; } = "Windows Service Manager.exe";
    public static string PrimaryWatchdogMutexName { get; private set; } = "PrimaryWDog";
    public static string PrimaryWatchdogFullPath { get; private set; } = Path.Combine(PrimaryWatchdogPath, PrimaryWatchdogName);
}


