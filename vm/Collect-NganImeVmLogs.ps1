param(
    [ValidatePattern('^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$')]
    [string]$Label = ('manual-test-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
)

. (Join-Path $PSScriptRoot 'Vm.Common.ps1')

$Config = Get-NganImeVmConfig
$Target = Get-NganImeVmSshTarget
$OutputDirectory = Join-Path $Config.LogRoot $Label
if (Test-Path -LiteralPath $OutputDirectory) {
    throw "Refusing to overwrite a prior log collection: $OutputDirectory"
}
New-Item -ItemType Directory -Path $OutputDirectory | Out-Null

# Export only recent application and system events. This does not change guest configuration.
$GuestArchive = "$($Config.GuestRoot)\logs\$Label.zip"
$GuestCommand = @"
`$logDirectory = '$($Config.GuestRoot)\logs\$Label'
New-Item -ItemType Directory -Force -Path `$logDirectory | Out-Null
Get-WinEvent -FilterHashtable @{ LogName = 'Application'; StartTime = (Get-Date).AddHours(-2) } -ErrorAction SilentlyContinue | Export-Clixml "`$logDirectory\Application.clixml"
Get-WinEvent -FilterHashtable @{ LogName = 'System'; StartTime = (Get-Date).AddHours(-2) } -ErrorAction SilentlyContinue | Export-Clixml "`$logDirectory\System.clixml"
Get-ChildItem -Path '$($Config.GuestRoot)' -Recurse -Include '*.log','*.dmp' -ErrorAction SilentlyContinue | Copy-Item -Destination `$logDirectory -Force -ErrorAction SilentlyContinue
Compress-Archive -Path "`$logDirectory\*" -DestinationPath '$GuestArchive' -Force
"@
Invoke-NganImeGuestSsh -Command $GuestCommand

& scp "$Target`:$($GuestArchive.Replace('\', '/'))" $OutputDirectory
if ($LASTEXITCODE -ne 0) {
    throw "Log download failed with exit code $LASTEXITCODE."
}
Write-Output "Collected guest logs in $OutputDirectory."
