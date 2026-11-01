param(
    [ValidatePattern('^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$')]
    [string]$Version = ('source-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
)

. (Join-Path $PSScriptRoot 'Vm.Common.ps1')

$Config = Get-NganImeVmConfig
$Target = Get-NganImeVmSshTarget
if (-not (Test-NganImeVmRunning)) {
    throw 'Start NganImeWin10 before attempting to synchronize source files.'
}

# Each transfer gets a new immutable guest directory. Existing test source is never deleted.
$RemoteDirectory = "$($Config.GuestRoot)\source\$Version"
Invoke-NganImeGuestSsh -Command "New-Item -ItemType Directory -Force -Path '$RemoteDirectory' | Out-Null"

# SCP copies only the standalone demo; it never transfers or mounts the host workspace itself.
& scp -r -C $Config.SourcePath "$Target`:$($RemoteDirectory.Replace('\', '/'))/"
if ($LASTEXITCODE -ne 0) {
    throw "Source synchronization failed with exit code $LASTEXITCODE."
}

Write-Output "Synchronized AvlnImeDemo to $RemoteDirectory."
