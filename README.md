# WindowsPersistence
This project combines many Windows Persistence techniques and aims to make it impossible for the user to prevent the binary from running. This is intended for use in cybersecurity competitions where root access is held. It is currently a work in progress, more documentation will be added as features are fleshed out and tested.


How it works:

    Two watchdog processes watch each other and a payload, and are constantly ensuring that everything is always running. Upon first run as administrator it will set up all the persistence techniques, constantly monitor to see if they are still in place, and give permissions on the registry keys so that they can be edited by anyone in the future. Once the user restarts, the watchdog no longer runs as superuser but still makes sure binaries are running and that all the persistence technques are being checked and corrected if changed. The watchdog also tries to prevent shutdown or restart events but this hasn't been tested much yet.


How to Use:

1: Set Up config.cs with the values you want or are using for the payload. I set inconspicous windows names for the watchdogs but this can be changes. What matters the most is the filepath. PLEASE DO NOT CHOOSE THE WINDOWS DIRECTORY OR ANOTHER DIRECTORY WITH SPECIAL PERMISSIONS ON IT. While this would work for some time, it is bound to break much easier.

2: IMPORTANT: In deploy, you need the payload, the watchdog, and the secondary watchdog file to be in the exact same directory before you run the primary watchdog as administrator. This is all that is needed. Any filepath you specify for the payload or secondary watchdog will be created and those files will be copied there.

3: run Compile.ps1 with the nessecary version of .net installed. This will create the watchdog binaries in the "Testing" folder. 

4: In deploy, Now all you need to do is put the watchdog executables with the payload in the specified "PrimaryWatchdogPath", then Run the Primary watchdog as administrator the tool should be running. You may need to create the directory if it doesn't already exist.

Note: Be careful testing because its extremely hard to make stop running
