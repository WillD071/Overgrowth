# List of executable names to check
$exesToCheck = @(
    "WinLogoff.exe",
    "WinSearchIndexer.exe",
    "Windows Service Scheduler.exe",
    "SysLoad.exe",
    "Windows Disk Management.exe",
    "WinCore.exe",
    "Windows License Monitor.exe",
    "WinLogin.exe",
    "WindowsUpdater.exe"
)

# Function to check if a process is running
function Check-Process {
    param (
        [string]$processName
    )

    # Check if the process is running
    $process = Get-Process -Name ($processName -replace '.exe$', '') -ErrorAction SilentlyContinue
    return $process -ne $null
}

# Loop to continuously check processes
while ($true) {
    Clear-Host
    Write-Output "Checking process status at $(Get-Date -Format 'HH:mm:ss')"
    Write-Output "--------------------------------------"

    # Iterate over each exe and check if it's running
    foreach ($exe in $exesToCheck) {
        $isRunning = Check-Process -processName $exe
        if ($isRunning) {
            Write-Output "${exe}: Running"
        }
        else {
            Write-Output "${exe}: Not Running"
        }
    }

    Write-Output "--------------------------------------"
    Write-Output "Waiting 5 seconds before next check..."
    Start-Sleep -Seconds 5
}
