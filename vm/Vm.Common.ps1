Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-NganImeVmConfig {
    <#
    .SYNOPSIS
    Loads and validates the fixed host paths owned by the isolated IME test VM.
    #>
    $ConfigPath = Join-Path $PSScriptRoot 'VmConfig.psd1'
    $Config = Import-PowerShellDataFile -LiteralPath $ConfigPath
    foreach ($PropertyName in 'VmPath', 'VmrunPath', 'GuestRoot', 'SourcePath', 'LogRoot') {
        if ([string]::IsNullOrWhiteSpace($Config[$PropertyName])) {
            throw "VM configuration is missing ${PropertyName}: $ConfigPath"
        }
    }
    if (-not (Test-Path -LiteralPath $Config.VmPath)) {
        throw "The isolated VM has not been created or was moved: $($Config.VmPath)"
    }
    if (-not (Test-Path -LiteralPath $Config.VmrunPath)) {
        throw "VMware vmrun was not found: $($Config.VmrunPath)"
    }
    return $Config
}

function Invoke-NganImeVmrun {
    <#
    .SYNOPSIS
    Executes one VMware Workstation command against the dedicated POC VM.
    #>
    param(
        [Parameter(Mandatory)]
        [string[]]$Arguments
    )

    $Config = Get-NganImeVmConfig
    & $Config.VmrunPath -T ws @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "vmrun failed with exit code ${LASTEXITCODE}: $($Arguments -join ' ')"
    }
}

function Test-NganImeVmRunning {
    <#
    .SYNOPSIS
    Returns whether VMware currently reports the dedicated POC VM as running.
    #>
    $Config = Get-NganImeVmConfig
    $RunningVms = & $Config.VmrunPath -T ws list
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to list running VMware guests; vmrun returned $LASTEXITCODE."
    }
    return $RunningVms -contains $Config.VmPath
}

function Get-NganImeVmSshTarget {
    <#
    .SYNOPSIS
    Gets the explicit guest SSH identity without storing credentials in the repository.
    .NOTES
    The caller must set NGAN_IME_VM_HOST and NGAN_IME_VM_USER after Windows and OpenSSH
    have been configured manually inside the guest.
    #>
    if ([string]::IsNullOrWhiteSpace($env:NGAN_IME_VM_HOST)) {
        throw 'Set NGAN_IME_VM_HOST to the guest IP address before remote automation.'
    }
    if ([string]::IsNullOrWhiteSpace($env:NGAN_IME_VM_USER)) {
        throw 'Set NGAN_IME_VM_USER to the dedicated guest account before remote automation.'
    }
    return "$($env:NGAN_IME_VM_USER)@$($env:NGAN_IME_VM_HOST)"
}

function Invoke-NganImeGuestSsh {
    <#
    .SYNOPSIS
    Runs a PowerShell command in the guest after its SSH host key was manually verified.
    #>
    param(
        [Parameter(Mandatory)]
        [string]$Command
    )

    $Target = Get-NganImeVmSshTarget
    & ssh -o StrictHostKeyChecking=yes $Target $Command
    if ($LASTEXITCODE -ne 0) {
        throw "Guest SSH command failed with exit code $LASTEXITCODE."
    }
}
