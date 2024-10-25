# Define the relative path to your project directory
cd SecondaryWatchdog

# Run the dotnet publish command
dotnet publish SecondaryWatchdog.csproj

cd ..
cd PrimaryWatchdog

dotnet publish PrimaryWatchdog.csproj

cd ..


Copy-Item -Path (Get-ChildItem "..\WindowsPersistence\SecondaryWatchdog\bin\Release\net8.0-windows\win-x64\publish\*.exe" | Select-Object -First 1).FullName -Destination "..\WindowsPersistence\OutputBinaries" -Force

Copy-Item -Path (Get-ChildItem "..\WindowsPersistence\PrimaryWatchdog\bin\Release\net8.0-windows\win-x64\publish\*.exe" | Select-Object -First 1).FullName -Destination "..\WindowsPersistence\OutputBinaries" -Force
 

Read-Host -Prompt "Press Enter to exit"