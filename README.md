# WindowsPersistence

This project combines many Windows persistence techniques and aims to make it impossible for the user to prevent the binary from running. It is intended for use in cybersecurity competitions where root access is held. It is currently a work in progress, with more documentation to be added as features are fleshed out and tested.

## How it works:

Two watchdog processes watch each other and a payload, constantly ensuring that everything is always running. Upon the first run as an administrator, it sets up all the persistence techniques, monitors to ensure they remain in place, and grants permissions on the registry keys so they can be edited by anyone in the future.

Once the user restarts, the watchdog no longer runs as a superuser but continues to ensure that the binaries are running and that all persistence techniques are checked and corrected if changed. The watchdog also tries to prevent shutdown or restart events, though this hasn't been tested extensively yet.

## How to Use:

1. **Set up `config.cs`**  
   Configure the values you want or are using for the payload. I set inconspicuous Windows names for the watchdogs, but this can be changed. The most important detail is the file path.  
   **Note:** _Please do not choose the Windows directory or any directory with special permissions._ While this would work for some time, it is bound to break much easier.

2. **Deploy**  
   You need the payload, the watchdog, and the secondary watchdog files to be in the exact same directory before you run the primary watchdog as an administrator. This is all that is needed. Any file path you specify for the payload or secondary watchdog will be created, and those files will be copied there.

3. **Run `Compile.ps1`**  
   Ensure the necessary version of .NET is installed. This will create the watchdog binaries in the "Testing" folder.

4. **Deploy again**  
   Place the watchdog executables with the payload in the specified "PrimaryWatchdogPath". **Run the primary watchdog as an administrator**, and the tool should start running. You may need to create the directory if it doesn't already exist.

**Note:** Be careful when testing because it's extremely hard to make it stop running.

## Example Deploy:

**Secnario:** Deploying Watchdog to C:\CoreSystem\Temp (fake system looking directory at first glance)

**1 - Add correct files:** Place Primary Watchdog (defaults to "Windows Service Manager.exe"), Secondary Watchdog (Defaults to "Windows Disk Manager.exe"), and payload (Defaults to testPayload.exe) in **C:\CoreSystem\Temp**

**2 - Run as Administrator:** Run the Primary Watchdog (defaults to "Windows Service Manager.exe") **as adminstrator**. Assuming you set up Config.cs correctly, the payload should be running indefinitely



