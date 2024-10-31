# WindowsPersistence

This project combines many Windows persistence techniques and aims to make it impossible for the user to prevent the binary from running.

## How it works:

Two watchdog processes watch each other and a payload, constantly ensuring that everything is always running. Upon the first run as an administrator,
it sets up all the persistence techniques, monitors to ensure they remain in place, and grants permissions on the registry keys so they can be edited 
by anyone in the future.

## How to Use:

1. **Set up `config.cs`**  
   Configure the values you want or are using for the payload. I set inconspicuous Windows names for the watchdogs, but this can be changed. The most important detail is the file path.
   - Set `Debugging` for `false` to disable console output
   - Set the `PortsToKeepOpen` array to have any ports you need to keep open for a C2.
   - Set `sleepTime` to the time between loops in milliseconds. A longer loop will be more stable and have less CPU usage for a competition, I go for around 45 seconds. Lower is better if you can manage.
   - All the other fields are self explanatory and should be set to unique values
   **Note:** _Please do not choose any of the same values as what is being used in a competition to ensure there is no overlap_

3. Ensure the **OutputType** field in PrimaryWatchdog\PrimaryWatchdog.csproj and SecondaryWatchdog\SecondaryWatchdog.csproj is set to `WinEXE` not `exe`. Winexe means the program will not make a window when ran in the background.

4. **Read Before Next Steps**  
   You need the payload, the watchdog, and the secondary watchdog files to be in the exact same directory before you run the primary watchdog as an administrator. This is all that is needed. Any file path you specify for the payload or secondary watchdog will be created, and those files will be copied there.

5. **Put your payload in the PutPayloadHere folder** - Ensure your payload is the only .exe in the folder.

6. **Run `Compile.ps1` in its current directory**  
   Ensure that you have .NET8 installed and any other dependencies that arise. This will create the watchdog binaries in the "Testing" folder. Fix any errors that formed and resolve dependencies. This will place your watchdog, secondary watchdog, and your renamed payload in the **OutputBinaries** folder.

7. **Deploy**  
   Place the watchdog executables with the payload in the specified "PrimaryWatchdogPath" in Config.cs. **Run the primary watchdog (The .EXE specified in the PrimaryWatchDogName) as an administrator**, now persistence is running. 

**Note:** CopyAndRunTool.ps1, TestIfRunning.ps1, and KillPersistence.ps1 are all scripts I made to make deployment and testing easier. They are hardcoded for everything so they would work with anything if you changed some filename and filepaths



## Example Deploy:

**Secnario:** Deploying Watchdog to C:\CoreSystem\Temp

**1 - Add correct files:** Place Primary Watchdog (defaults to "Windows Service Manager.exe"), Secondary Watchdog (Defaults to "Windows Disk Manager.exe"), and payload (Defaults to testPayload.exe) in **C:\CoreSystem\Temp**

**2 - Run as Administrator:** Run the Primary Watchdog (defaults to "Windows Service Manager.exe") **as adminstrator**. Assuming you set up Config.cs correctly, the payload should be running indefinitely



