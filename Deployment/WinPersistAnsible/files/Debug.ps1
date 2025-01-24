
# List of executable names to check
$exesToCheck = @("SysLoad","WinCore","Windows Disk Management","WinLogoff","Windows Service Scheduler","WinSearchIndexer","Windows License Monitor","WindowsUpdater","WinLogin");

# Function to check if a process is running, return the instance count, and calculate uptime
function Check-Process {
    param (
        [string]$processName
    )

    # Get all instances of the process
    $processes = Get-Process -Name ($processName -replace '.exe$', '') -ErrorAction SilentlyContinue
    $results = @()

    foreach ($process in $processes) {
        $uptime = (Get-Date) - $process.StartTime
        $results += [pscustomobject]@{
            Name = $process.Name
            Id = $process.Id
            Uptime = $uptime
        }
    }
    return $results
}

# Loop to continuously check processes
while ($true) {
    Clear-Host
    Write-Output "Checking process status at $(Get-Date -Format 'HH:mm:ss')"
    Write-Output "--------------------------------------"

    # Iterate over each exe and check if it's running
    foreach ($exe in $exesToCheck) {
        $instances = Check-Process -processName $exe
        if ($instances.Count -gt 0) {
            foreach ($instance in $instances) {
                Write-Output "${exe} (PID: $($instance.Id)): Running for $($instance.Uptime.Days) days, $($instance.Uptime.Hours) hours, $($instance.Uptime.Minutes) minutes"
            }
        }
        else {
            Write-Output "${exe}: Not Running"
        }
    }

    Write-Output "--------------------------------------"
    Write-Output "Waiting 5 seconds before next check..."
    Start-Sleep -Seconds 5
}
