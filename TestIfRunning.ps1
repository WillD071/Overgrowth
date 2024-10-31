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

# Function to check if a process is running and return the instance count
function Check-Process {
    param (
        [string]$processName
    )

    # Get all instances of the process
    $processes = Get-Process -Name ($processName -replace '.exe$', '') -ErrorAction SilentlyContinue
    return $processes.Count
}

# Loop to continuously check processes
while ($true) {
    Clear-Host
    Write-Output "Checking process status at $(Get-Date -Format 'HH:mm:ss')"
    Write-Output "--------------------------------------"

    # Iterate over each exe and check if it's running
    foreach ($exe in $exesToCheck) {
        $instanceCount = Check-Process -processName $exe
        if ($instanceCount -gt 0) {
            Write-Output "${exe}: Running ($instanceCount instance(s))"
        }
        else {
            Write-Output "${exe}: Not Running"
        }
    }

    Write-Output "--------------------------------------"
    Write-Output "Waiting 5 seconds before next check..."
    Start-Sleep -Seconds 5
}
