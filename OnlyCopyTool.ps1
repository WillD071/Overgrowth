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