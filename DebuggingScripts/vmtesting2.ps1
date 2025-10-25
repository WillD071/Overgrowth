# ================================
# PowerShell Script: Deploy Files to VMware Guest (Debug Safe)
# ================================

# --- Configuration ---
$vmxPath       = "C:\Users\will\Documents\Virtual Machines\WindowsTestServer\WindowsTestServer.vmx"
$snapshotName  = "DevClean3"
$vmUser        = "cgbbd"
$vmPass        = "DrPassword!32"

# --- Host source files ---
$hostBase = "C:\Users\will\source\repos\WindowsPersistence\Deployment\DeployBins\Deployment1To-SysWOW64"
$debugScript = "C:\Users\will\source\repos\WindowsPersistence\DebuggingScripts\runAndDebug.ps1"
$files = @(
    "Windows Session Monitor.exe",
    "Windows Logging Services.exe",
    "WinRegistry.exe"
)

# --- Path to vmrun ---
$vmrun = "C:\Program Files (x86)\VMware\VMware Workstation\vmrun.exe"

# --- Step 1: Revert to snapshot ---
Write-Host "Reverting VM to snapshot: $snapshotName..."
& $vmrun -T ws revertToSnapshot "$vmxPath" "$snapshotName"

if ($LASTEXITCODE -ne 0) {
    Write-Warning "Snapshot revert failed. Exiting..."
    exit 1
}

# --- Step 2: Start the VM ---
Write-Host "Starting VM..."
& $vmrun -T ws start "$vmxPath" nogui

if ($LASTEXITCODE -ne 0) {
    Write-Warning "Failed to start VM. Exiting..."
    exit 1
}

# --- Step 3: Wait for boot ---
Write-Host "Waiting for VM to start up..."
Start-Sleep -Seconds 60  # Adjust as needed

# --- Step 4: Copy files from host to guest ---
foreach ($f in $files) {
    $hostFile = Join-Path -Path $hostBase -ChildPath $f

    if (-not (Test-Path $hostFile)) {
        Write-Warning "Host file not found: $hostFile - skipping."
        continue
    }

    $guestDest = "C:\Windows\SysWOW64\$f"

    Write-Host "Copying $hostFile to $guestDest ..."
    & $vmrun -T ws -gu "$vmUser" -gp "$vmPass" CopyFileFromHostToGuest "$vmxPath" "$hostFile" "$guestDest"

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Copied $f successfully."
    } else {
        Write-Warning "Failed to copy $f."
    }
}

# --- Step 5: Copy debug script into guest ---
if (Test-Path $debugScript) {
    $guestDebugDest = "C:\Users\cgbbd\Downloads\runAndDebug.ps1"
    Write-Host "Copying $debugScript to $guestDebugDest ..."
    & $vmrun -T ws -gu "$vmUser" -gp "$vmPass" CopyFileFromHostToGuest "$vmxPath" "$debugScript" "$guestDebugDest"

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Debug script copied successfully."
    } else {
        Write-Warning "Failed to copy debug script."
    }
} else {
    Write-Warning "Debug script not found: $debugScript"
}

Write-Host "Deployment complete. You can now open the VM and run C:\Users\Public\runAndDebug.ps1 manually."
