[CmdletBinding()]
param(
    [switch]$DryRun,
    [switch]$Publish,
    [switch]$AllowDirty
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$arguments = @('harness/pipeline/full_auto.py')

if ($DryRun) {
    $arguments += '--dry-run'
}
if ($Publish) {
    $arguments += '--publish'
}
if ($AllowDirty) {
    $arguments += '--allow-dirty'
}

Push-Location $root
try {
    & py @arguments
    exit $LASTEXITCODE
}
finally {
    Pop-Location
}

