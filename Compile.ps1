# Define the relative path to your project directory
$projectDir = Get-Location

# Parse Config.cs for Watchdog names, ensuring only one result
$configPath = Join-Path -Path $projectDir -ChildPath "Config.cs"
$primaryWatchdogName = (Select-String -Path $configPath -Pattern 'public static string PrimaryWatchdogName\s*{.*?} = "(.*?)";' | Select-Object -First 1).Matches.Groups[1].Value
$secondaryWatchdogName = (Select-String -Path $configPath -Pattern 'public static string SecondaryWatchdogName\s*{.*?} = "(.*?)";' | Select-Object -First 1).Matches.Groups[1].Value

# Check if names were found
if (-not $primaryWatchdogName -or -not $secondaryWatchdogName) {
    Write-Host "Error: Could not retrieve watchdog names from Config.cs" -ForegroundColor Red
    exit
}

# Define the output directory
$outputDir = "..\WindowsPersistence\OutputBinaries"

# Delete all files in OutputBinaries except TestPayload.exe
Get-ChildItem -Path $outputDir -File | Where-Object { $_.Name -ne "TestPayload.exe" } | Remove-Item -Force

# Publish SecondaryWatchdog and PrimaryWatchdog projects
dotnet publish "..\WindowsPersistence\SecondaryWatchdog\SecondaryWatchdog.csproj" -o "..\WindowsPersistence\SecondaryWatchdog\bin\Release\net8.0-windows\win-x64\publish"
dotnet publish "..\WindowsPersistence\PrimaryWatchdog\PrimaryWatchdog.csproj" -o "..\WindowsPersistence\PrimaryWatchdog\bin\Release\net8.0-windows\win-x64\publish"

# Define the source paths for the published executables
$secondarySource = (Get-ChildItem "..\WindowsPersistence\SecondaryWatchdog\bin\Release\net8.0-windows\win-x64\publish\*.exe" | Select-Object -First 1).FullName
$primarySource = (Get-ChildItem "..\WindowsPersistence\PrimaryWatchdog\bin\Release\net8.0-windows\win-x64\publish\*.exe" | Select-Object -First 1).FullName

# Copy and rename the binaries based on Config.cs values
Copy-Item -Path $secondarySource -Destination (Join-Path -Path $outputDir -ChildPath $secondaryWatchdogName) -Force
Copy-Item -Path $primarySource -Destination (Join-Path -Path $outputDir -ChildPath $primaryWatchdogName) -Force

# Prompt to exit
Read-Host -Prompt "Press Enter to exit"
