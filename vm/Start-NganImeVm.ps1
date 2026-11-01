. (Join-Path $PSScriptRoot 'Vm.Common.ps1')

# Starts only the new isolated guest. It never opens or alters the retired Win10 VM.
if (Test-NganImeVmRunning) {
    Write-Output 'NganImeWin10 is already running.'
    return
}

$Config = Get-NganImeVmConfig
Invoke-NganImeVmrun -Arguments @('start', $Config.VmPath, 'gui')
Write-Output 'NganImeWin10 was started in VMware Workstation.'
