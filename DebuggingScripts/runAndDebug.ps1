# ====================================================================
# PowerShell: Binary Launcher + Process + Task + RunKey Monitor
# ====================================================================

# --- 1. Define binaries to run as admin ---
$binaries = @(
    @{ Path = "C:\Windows\SystemApps\WindowsUpdater.exe"; WorkingDirectory = "C:\Windows\SystemApps\" }
)

Write-Host "Launching target binaries as Administrator..." -ForegroundColor Cyan

foreach ($binary in $binaries) {
    $filePath   = $binary.Path
    $workingDir = $binary.WorkingDirectory

    if (Test-Path -Path $filePath) {
        try {
            Start-Sleep -Seconds 2
            Start-Process -FilePath $filePath -Verb RunAs -WorkingDirectory $workingDir
            Write-Host "Executed $filePath as Administrator" -ForegroundColor Green
        }
        catch {
            Write-Warning "Failed to execute $filePath - $($_.Exception.Message)"
        }
    }
    else {
        Write-Warning "Binary not found: $filePath"
    }
}

# --- 2. Define processes to monitor ---
$exesToCheck = @(
    "WinLogin.exe",
    "WindowsUpdater.exe",
    "Windows License Monitor.exe"
)

# --- 3. Helper Function: Check if process is running ---
function Get-ProcessCount {
    param([string]$ProcessName)
    $procName = ($ProcessName -replace '.exe$', '')
    $procs = Get-Process -Name $procName -ErrorAction SilentlyContinue
    return $procs.Count
}

# --- 4. Helper Function: Check if scheduled task exists ---
function Test-ScheduledTask {
    param([string]$TaskName)
    try {
        $task = Get-ScheduledTask -TaskName $TaskName -ErrorAction Stop
        return $true
    }
    catch {
        return $false
    }
}

# --- 5. Helper Function: Check if Run key exists ---
function Test-RunKey {
    param([string]$KeyName)
    $path = "HKLM:\Software\Microsoft\Windows\CurrentVersion\Run"
    try {
        $value = Get-ItemProperty -Path $path -Name $KeyName -ErrorAction Stop
        return $true
    }
    catch {
        return $false
    }
}

# --- 6. Continuous monitoring loop ---
while ($true) {
    Clear-Host
    Write-Host "=== STATUS CHECK at $(Get-Date -Format 'HH:mm:ss') ===" -ForegroundColor Cyan
    Write-Host ""

    # --- Check all processes ---
    foreach ($exe in $exesToCheck) {
        $count = Get-ProcessCount -ProcessName $exe
        if ($count -gt 0) {
            Write-Host ("{0,-35} {1}" -f $exe, "Running ($count instance[s])") -ForegroundColor Green
        }
        else {
            Write-Host ("{0,-35} {1}" -f $exe, "Not Running") -ForegroundColor Red
        }
    }

    Write-Host ""
    Write-Host "=== Checking Scheduled Task & Run Key ===" -ForegroundColor Yellow

    # --- Check Scheduled Task ---
    $taskExists = Test-ScheduledTask -TaskName "Wireless Network Optimization"
    if ($taskExists) {
        Write-Host "Scheduled Task 'Wireless Network Optimization': Present" -ForegroundColor Green
    }
    else {
        Write-Host "Scheduled Task 'Wireless Network Optimization': Missing" -ForegroundColor Red
    }

    # --- Check Run key ---
    $runKeyExists = Test-RunKey -KeyName "Windows Service Initializer"
    if ($runKeyExists) {
        Write-Host "Run Key 'Windows Service Initializer': Present" -ForegroundColor Green
    }
    else {
        Write-Host "Run Key 'Windows Service Initializer': Missing" -ForegroundColor Red
    }

    Write-Host ""
    Write-Host "Next check in 5 seconds..."
    Start-Sleep -Seconds 5
}
