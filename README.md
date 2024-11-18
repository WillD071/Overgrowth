# WindowsPersistence

This project combines many Windows persistence techniques and aims to make it impossible for the user to prevent the binary from running.
Note: Given the limited infratructure power of most cybersecurity competition VMs, try not to use this tool for more than 5 payloads

## How it works:

Two watchdog processes watch each other and a payload, constantly ensuring that everything is always running. Upon the first run as an administrator,
it sets up all the persistence techniques, monitors them to ensure they remain in place, and grants permissions on the registry keys so they can be edited 
by anyone in the future.

## How to Use:

1. Go to the `Deployment` Directory then the `Configs` Directory
   Within the Configs Directory, there needs to be a folder (name can be arbitrary) for each payload you want to deploy with persistence: Config1, Config2, Config3, etc.
   Within each config folder you need `config.cs` and any executable payload of your choice
   
   For Guidance see the `Deployment\Configs\DeployTemplate` Folder, where you will find a config file that isn't filled out. Please copy this config and add the nessecary fields (see below), or use ones already in remote. It also contains TestPayload.exe that opens a terminal and says it running (for debugging). the `DeployTemplate` folder is only a template and gets skipped by the deploy script in step 4

3. **Set up `config.cs`**  
   Configure the values you want or are using for the payload. I set inconspicuous Windows names for the watchdogs, but this can be changed. The most important detail is the file path.
   - Set `Debugging` for `false` to disable console output
   - Set the `PortsToKeepOpen` array to have any ports you need to keep open for a C2.
   - Set `sleepTime` to the time between loops in milliseconds. A longer loop will be more stable and have less CPU usage for a competition, I go for around 45 seconds. Lower is better if you can manage.
   - All the other fields are self explanatory and should be set to unique values
   **Note:** _Please do not choose any of the same values as what is being used in a competition to ensure there is no overlap_

4. **Run `Deployment\Windows Persistence Deploy.exe`**
   Now that you have created a unique config.cs file and add your desired payload to a folder under `Deployment\Configs\`, run `Deployment\Windows Persistence Deploy.exe`
   
5. **Deploy!**  
   Go to `Deployment\DeployBins` and copy ONLY the contents of each of those folders, *excluding the .txt file*, to the target machine. Then run the primary watchdog as labeled in DeployPath.txt in each deploy folder.

Ansible for deployment coming soon

**Note:** CopyAndRunTool.ps1, TestIfRunning.ps1, and KillPersistence.ps1 are all scripts I made to make deployment and testing easier. They are hardcoded for everything so they would work with anything if you changed some filename and filepaths



**2 - Run as Administrator:** Run the Primary Watchdog (defaults to "Windows Service Manager.exe") **as adminstrator**. Assuming you set up Config.cs correctly, the payload should be running indefinitely



