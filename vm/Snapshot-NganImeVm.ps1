param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$')]
    [string]$Name
)

. (Join-Path $PSScriptRoot 'Vm.Common.ps1')

# A snapshot is deliberately never overwritten or deleted by automation.
if (-not (Test-NganImeVmRunning)) {
    throw 'Start NganImeWin10 and wait for Windows to finish booting before taking a VMware snapshot.'
}

$Config = Get-NganImeVmConfig
Invoke-NganImeVmrun -Arguments @('snapshot', $Config.VmPath, $Name)
Write-Output "Created VMware snapshot: $Name"
