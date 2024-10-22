
public class Config
{
	
    public static string PayloadPath { get; private set; } = @"C:\Windows\Test\Test2";
    public static string PayloadName { get; private set; } = "TestPayload.exe";



    public static string SecondaryWatchdogPath { get; private set; } = @"C:\Windows\Test\Test1";
    public static string SecondaryWatchdogName { get; private set; } = "TestApp.exe";
    public static string SecondaryWatchdogMutexName { get; private set; } = "SecondaryWDog";


    public static string PrimaryWatchdogPath { get; private set; } = @"C:\Windows\Test";
    public static string PrimaryWatchdogName { get; private set; } = "WindowsPersistence.exe";
    public static string PrimaryWatchdogMutexName { get; private set; } = "PrimaryWDog";
    public static string PrimaryWatchdogFullPath { get; private set; } = Path.Combine(PrimaryWatchdogPath, PrimaryWatchdogName);
}


