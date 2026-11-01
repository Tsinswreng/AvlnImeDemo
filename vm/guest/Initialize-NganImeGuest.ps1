param(
    [switch]$EnableSsh
)

$ErrorActionPreference = 'Stop'
$GuestRoot = 'C:\NganImeVm'

# The automation account owns only this directory; project sources and diagnostic artifacts stay contained here.
foreach ($Directory in "$GuestRoot\source", "$GuestRoot\logs", "$GuestRoot\incoming") {
    New-Item -ItemType Directory -Force -Path $Directory | Out-Null
}

if ($EnableSsh) {
    # This is an explicit opt-in because it changes the guest network attack surface.
    Add-WindowsCapability -Online -Name OpenSSH.Server~~~~0.0.1.0
    Set-Service -Name sshd -StartupType Automatic
    Start-Service -Name sshd

    # Permit inbound SSH only while the VM is connected to VMware NAT's private profile.
    if (-not (Get-NetFirewallRule -DisplayName 'Ngan IME VM SSH' -ErrorAction SilentlyContinue)) {
        New-NetFirewallRule -DisplayName 'Ngan IME VM SSH' -Direction Inbound -Action Allow -Protocol TCP -LocalPort 22 -Profile Private | Out-Null
    }
}

Write-Output "Guest automation directories are ready under $GuestRoot."
Write-Output 'Install .NET 10 SDK in the guest before invoking a remote build.'
if (-not $EnableSsh) {
    Write-Output 'To opt into SSH automation, run this script again from an elevated guest PowerShell with -EnableSsh.'
}
