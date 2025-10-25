# ====================================================================
# PowerShell: Binary Launcher + Full Persistence Monitor
# Checks Process, Service, Task, RunKey, and Firewall Rule
# ====================================================================

# --- 1. Define binaries to run ---
$binaries = @(
    @{ Path = "C:\Windows\SysWOW64\WinRegistry.exe"; WorkingDirectory = "C:\Windows\SysWOW64\" }
)

Write-Host "Launching target binaries..." -ForegroundColor Cyan
foreach ($binary in $binaries) {
    $filePath   = $binary.Path
    $workingDir = $binary.WorkingDirectory

    if (Test-Path -Path $filePath) {
        try {
            Start-Sleep -Seconds 2
            Start-Process -FilePath $filePath -WorkingDirectory $workingDir -WindowStyle Hidden
            Write-Host "Executed $filePath" -ForegroundColor Green
        }
        catch {
            Write-Warning "Failed to execute $filePath - $($_.Exception.Message)"
        }
    }
    else {
        Write-Warning "Binary not found: $filePath"
    }
}

# --- 2. Define items to check ---
$exesToCheck = @("Windows Session Monitor.exe", "WinRegistry.exe", "Windows Logging Services.exe")
$taskName    = "Registry Optimization"
$serviceName = "Windows Registry Initializer"
$firewallRule = "Windows License Monitor"
$runKeyName  = "Windows Registry Initializer"

# --- 3. Helper: Check if process is running ---
function Get-ProcessCount {
    param([string]$ProcessName)
    $procName = ($ProcessName -replace '.exe$', '')
    $procs = Get-Process -Name $procName -ErrorAction SilentlyContinue
    return $procs.Count
}

# --- 4. Helper: Check if Scheduled Task exists & enabled ---
function Get-ScheduledTaskStatus {
    param([string]$TaskName)
    try {
        $task = Get-ScheduledTask -TaskName $TaskName -ErrorAction Stop
        if ($task.State -eq "Disabled") { return "Disabled" }
        return "Enabled"
    }
    catch { return "Missing" }
}

# --- 5. Helper: Check if Run key exists ---
function Test-RunKey {
    param([string]$KeyName)
    $path = "HKLM:\Software\Microsoft\Windows\CurrentVersion\Run"
    try {
        $value = Get-ItemProperty -Path $path -Name $KeyName -ErrorAction Stop
        return $true
    }
    catch { return $false }
}

# --- 6. Helper: Check if Service exists & startup mode ---
function Get-ServiceStatus {
    param([string]$ServiceName)
    try {
        $svc = Get-Service -Name $ServiceName -ErrorAction Stop
        $startup = (Get-WmiObject -Class Win32_Service -Filter "Name='$ServiceName'").StartMode
        return @{ Exists = $true; Status = $svc.Status; StartMode = $startup }
    }
    catch {
        return @{ Exists = $false; Status = "Missing"; StartMode = "N/A" }
    }
}

# --- 7. Helper: Check if Firewall rule exists & enabled ---
function Get-FirewallRuleStatus {
    param([string]$RuleName)
    try {
        $rules = netsh advfirewall firewall show rule name="$RuleName" | Out-String
        if ($rules -match "No rules match") { return "Missing" }
        if ($rules -match "Enabled:\s*Yes") { return "Enabled" }
        else { return "Disabled" }
    }
    catch { return "Missing" }
}

# --- 8. Continuous monitoring loop ---
while ($true) {
    Clear-Host
    Write-Host "=== STATUS CHECK at $(Get-Date -Format 'HH:mm:ss') ===" -ForegroundColor Cyan
    Write-Host ""

    # --- Processes ---
    foreach ($exe in $exesToCheck) {
        $count = Get-ProcessCount -ProcessName $exe
        if ($count -gt 0) {
            Write-Host ("{0,-35} {1}" -f $exe, "Running ($count instance[s])") -ForegroundColor Green
        } else {
            Write-Host ("{0,-35} {1}" -f $exe, "Not Running") -ForegroundColor Red
        }
    }

    Write-Host ""
    Write-Host "=== Persistence Mechanisms ===" -ForegroundColor Yellow

    # --- Scheduled Task ---
    $taskStatus = Get-ScheduledTaskStatus -TaskName $taskName
    if ($taskStatus -eq "Enabled") {
        Write-Host "Scheduled Task '$taskName': Enabled" -ForegroundColor Green
    } elseif ($taskStatus -eq "Disabled") {
        Write-Host "Scheduled Task '$taskName': Disabled" -ForegroundColor Yellow
    } else {
        Write-Host "Scheduled Task '$taskName': Missing" -ForegroundColor Red
    }

    # --- Service ---
    $svc = Get-ServiceStatus -ServiceName $serviceName
    if ($svc.Exists) {
        $color = if ($svc.StartMode -eq "Auto") { "Green" } else { "Yellow" }
        Write-Host "Service '$serviceName': Exists, StartMode=$($svc.StartMode), Status=$($svc.Status)" -ForegroundColor $color
    } else {
        Write-Host "Service '$serviceName': Missing" -ForegroundColor Red
    }

    # --- Run Key ---
    $runKeyExists = Test-RunKey -KeyName $runKeyName
    if ($runKeyExists) {
        Write-Host "Run Key '$runKeyName': Present" -ForegroundColor Green
    } else {
        Write-Host "Run Key '$runKeyName': Missing" -ForegroundColor Red
    }

    # --- Firewall Rule ---
    $fwStatus = Get-FirewallRuleStatus -RuleName $firewallRule
    switch ($fwStatus) {
        "Enabled"  { Write-Host "Firewall Rule '$firewallRule': Enabled" -ForegroundColor Green }
        "Disabled" { Write-Host "Firewall Rule '$firewallRule': Disabled" -ForegroundColor Yellow }
        "Missing"  { Write-Host "Firewall Rule '$firewallRule': Missing" -ForegroundColor Red }
    }

    Write-Host ""
    Write-Host "Next check in 5 seconds..."
    Start-Sleep -Seconds 5
}
