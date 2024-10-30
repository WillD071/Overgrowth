# Define executable names for process termination
$processNames = @(
    "WinLogoff",
    "WinSearchIndexer",
    "Windows Service Scheduler",
    "SysLoad",
    "Windows Disk Management",
    "WinCore",
    "Windows License Monitor",
    "WinLogin",
    "WindowsUpdater"
)

# Attempt to stop each process if it's running
foreach ($processName in $processNames) {
    $process = Get-Process -Name $processName -ErrorAction SilentlyContinue
    if ($process) {
        try {
            Stop-Process -Name $processName -Force
            Write-Output "Terminated process: $processName"
        }
        catch {
            Write-Output "Failed to terminate process: $processName - $($_.Exception.Message)"
        }
    }
    else {
        Write-Output "Process not found: $processName"
    }
}

# Define file paths for deletion
$paths = @(
    "C:\Windows\WinLogoff.exe",
    "C:\Windows\System32\WinSearchIndexer.exe",
    "C:\Windows\Fonts\Windows Service Scheduler.exe",
"C:\Windows\Fonts\WinSearchIndexer.exe",
"C:\Windows\Fonts\WinLogoff.exe",
    "C:\Windows\SysLoad.exe",
    "C:\Windows\SystemApps\Windows Disk Management.exe",
    "C:\Windows\SysWOW64\WinCore.exe",
"C:\Windows\SysWOW64\Windows Disk Management.exe",
"C:\Windows\SysWOW64\SysLoad.exe",
    "C:\Windows\Windows License Monitor.exe",
    "C:\Windows\assembly\WinLogin.exe",
    "C:\Windows\Boot\WindowsUpdater.exe",
"C:\Windows\Boot\WinLogin.exe",
"C:\Windows\Boot\Windows License Monitor.exe"
)

# Loop through each path and delete if it exists
foreach ($path in $paths) {
    if (Test-Path -Path $path) {
        try {
            Remove-Item -Path $path -Force
            Write-Output "Deleted: $path"
        }
        catch {
            Write-Output "Failed to delete: $path - $($_.Exception.Message)"
        }
    }
    else {
        Write-Output "File not found: $path"
    }
}
