
for ($i = 1; $i -le 5; $i++) {
    Write-Output "Iteration $i"
    
    # Define executable names for process termination
    $processNames = @("Windows Session Monitor","WinRegistry","Windows Logging Services");

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

    # Loop through each process name and delete any matching .exe files found under the specified directories
    $directories = @( "C:\Windows\Fonts\", "C:\Windows\SysWOW64\")  # List of directories to check
    foreach ($dir in $directories) {
        foreach ($processName in $processNames) {
            # Find all matching .exe files in the specified directory only (no subdirectories)
            $files = Get-ChildItem -Path $dir -Filter "$processName.exe" -ErrorAction SilentlyContinue
            if ($files) {
                foreach ($file in $files) {
                    try {
                        Remove-Item -Path $file.FullName -Force
                        Write-Output "Deleted: $($file.FullName)"
                    }
                    catch {
                        Write-Output "Failed to delete: $($file.FullName) - $($_.Exception.Message)"
                    }
                }
            }
            else {
                Write-Output "No matching files found for: $processName.exe in $dir"
            }
        }
    }

    # Wait for 2 seconds before the next iteration
    Start-Sleep -Seconds 2
}
