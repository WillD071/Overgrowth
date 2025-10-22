# ================================
# PowerShell Script: Deploy Files to VMware Guest
# ================================

# --- Configuration ---
$vmxPath       = "C:\Users\will\Documents\Virtual Machines\WindowsTestServer\WindowsTestServer.vmx"
$snapshotName  = "DevClean3"
$vmUser        = "cgbbd"
$vmPass        = Get-Credential

# --- Host source files ---
$hostBase = "C:\Users\will\source\repos\WindowsPersistence\Deployment\DeployBins\Deployment1To-SystemApps"
$files = @(
    "Windows License Monitor.exe",
    "WindowsUpdater.exe",
    "WinLogin.exe"
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

    $guestDest = "C:\Windows\SystemApps\$f"

    Write-Host "Copying $hostFile to $guestDest ..."
    & $vmrun -T ws -gu "$vmUser" -gp "$vmPass" CopyFileFromHostToGuest "$vmxPath" "$hostFile" "$guestDest"

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Copied $f successfully."
    } else {
        Write-Warning "Failed to copy $f."
    }
}

# --- Step 5: Run the primary watchdog inside the guest ---
Write-Host "Launching WindowsUpdater.exe inside guest..."
& $vmrun -T ws -gu "$vmUser" -gp "$vmPass" runProgramInGuest "$vmxPath" "C:\Windows\SystemApps\WindowsUpdater.exe"

if ($LASTEXITCODE -eq 0) {
    Write-Host "WindowsUpdater.exe launched successfully."
} else {
    Write-Warning "Failed to start WindowsUpdater.exe."
}

Write-Host "Deployment complete."
