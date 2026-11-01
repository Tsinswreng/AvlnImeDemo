param(
    [string]$VmRoot = 'E:\_\VM\NganImeWin10',
    [string]$IsoPath = 'E:\_\VM\Win10\Iso\Windows10.iso',
    [string]$VmwareRoot = 'D:\SOFT\VMware\VMware_Workstation17',
    [switch]$Start
)

$ErrorActionPreference = 'Stop'

if (Test-Path -LiteralPath $VmRoot) {
    throw "VM directory already exists; refusing to overwrite: $VmRoot"
}
if (-not (Test-Path -LiteralPath $IsoPath)) {
    throw "Windows ISO was not found: $IsoPath"
}

$VdiskManager = Join-Path $VmwareRoot 'vmware-vdiskmanager.exe'
$Vmrun = Join-Path $VmwareRoot 'vmrun.exe'
if (-not (Test-Path -LiteralPath $VdiskManager)) {
    throw "vmware-vdiskmanager.exe was not found: $VdiskManager"
}
if (-not (Test-Path -LiteralPath $Vmrun)) {
    throw "vmrun.exe was not found: $Vmrun"
}

New-Item -ItemType Directory -Path $VmRoot | Out-Null
$DiskPath = Join-Path $VmRoot 'NganImeWin10.vmdk'
$VmxPath = Join-Path $VmRoot 'NganImeWin10.vmx'

& $VdiskManager -c -s 80GB -a lsilogic -t 0 $DiskPath
if ($LASTEXITCODE -ne 0) {
    throw "vmware-vdiskmanager failed with exit code $LASTEXITCODE"
}

$IsoForVmx = $IsoPath.Replace('\', '\\')
$DiskForVmx = $DiskPath.Replace('\', '\\')
$Vmx = @"
.encoding = "UTF-8"
config.version = "8"
virtualHW.version = "21"
virtualHW.productCompatibility = "hosted"
displayName = "NganImeWin10"
guestOS = "windows9-64"
firmware = "efi"
memsize = "8192"
numvcpus = "4"
cpuid.coresPerSocket = "2"

scsi0.present = "TRUE"
scsi0.virtualDev = "lsisas1068"
scsi0:0.present = "TRUE"
scsi0:0.fileName = "$DiskForVmx"
scsi0:0.deviceType = "scsi-hardDisk"

sata0.present = "TRUE"
sata0:0.present = "TRUE"
sata0:0.deviceType = "cdrom-image"
sata0:0.fileName = "$IsoForVmx"

ethernet0.present = "TRUE"
ethernet0.connectionType = "nat"
ethernet0.virtualDev = "e1000e"

svga.present = "TRUE"
svga.autodetect = "TRUE"
svga.graphicsMemoryKB = "131072"
mks.enable3d = "FALSE"

sound.present = "FALSE"
printer.present = "FALSE"
floppy0.present = "FALSE"
usb.present = "FALSE"
ehci.present = "FALSE"
xhci.present = "FALSE"
tools.syncTime = "TRUE"
snapshot.disabled = "FALSE"
"@

Set-Content -LiteralPath $VmxPath -Value $Vmx -Encoding ASCII
Write-Output "Created VM: $VmxPath"

if ($Start) {
    & $Vmrun -T ws start $VmxPath gui
    if ($LASTEXITCODE -ne 0) {
        throw "vmrun start failed with exit code $LASTEXITCODE"
    }
}
