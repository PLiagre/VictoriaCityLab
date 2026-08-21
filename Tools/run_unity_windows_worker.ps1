param(
    [Parameter(Mandatory = $false)]
    [string]$Sha,
    [Parameter(Mandatory = $false)]
    [string]$RefName,
    [Parameter(Mandatory = $false)]
    [string]$ProjectPath,
    [Parameter(Mandatory = $false)]
    [string]$UnityExe = 'C:\Program Files\Unity\Hub\Editor\6000.0.43f1\Editor\Unity.exe',
    [Parameter(Mandatory = $false)]
    [string]$Results,
    [Parameter(Mandatory = $false)]
    [string]$Log,
    [Parameter(Mandatory = $false)]
    [string]$Summary,
    [switch]$SkipUnity
)

$ErrorActionPreference = 'Stop'
$expectedUnity = '6000.0.43f1'
$root = Split-Path -Parent $PSScriptRoot
if (-not $ProjectPath) { $ProjectPath = $root }

function Assert-OwnedSha {
    param(
        [Parameter(Mandatory = $true)][string]$Commit,
        [Parameter(Mandatory = $false)][string]$ExpectedRef
    )
    if ($Commit -notmatch '^[0-9a-f]{40}$') {
        throw "SHA Unity invalide (40 hex exigés): $Commit"
    }
    git -C $root cat-file -e "$Commit^{commit}" 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "SHA $Commit absent de ce dépôt"
    }
    $refs = @(git -C $root for-each-ref --format='%(refname:short)' refs/remotes/origin --contains $Commit)
    $allowed = @()
    foreach ($ref in $refs) {
        $name = $ref -replace '^origin/', ''
        if ($name -eq 'main' -or $name -like 'agent/*' -or $name -like 'cursor/*') {
            $allowed += $name
        }
    }
    if ($allowed.Count -eq 0) {
        throw "SHA $Commit hors d'une branche PLiagre (main, agent/*, cursor/*)"
    }
    if ($ExpectedRef) {
        $normalized = $ExpectedRef -replace '^refs/heads/', '' -replace '^origin/', ''
        if ($normalized -ne 'main' -and $normalized -notlike 'agent/*' -and $normalized -notlike 'cursor/*') {
            throw "Branche refusée: $ExpectedRef"
        }
        if ($allowed -notcontains $normalized) {
            throw "SHA $Commit n'appartient pas à $normalized"
        }
    }
    Write-Host "CITYLAB_OWNED_SHA_OK sha=$Commit refs=$($allowed -join ',')"
}

$versionFile = Join-Path $ProjectPath 'ProjectSettings/ProjectVersion.txt'
if (-not (Test-Path -LiteralPath $versionFile)) {
    throw "ProjectVersion.txt introuvable: $versionFile"
}
$versionText = Get-Content -LiteralPath $versionFile -Raw
if ($versionText -notmatch [regex]::Escape($expectedUnity)) {
    throw "Unity projet != $expectedUnity"
}

if ($Sha) {
    Assert-OwnedSha -Commit $Sha -ExpectedRef $RefName
}

if ($SkipUnity) {
    Write-Host "CITYLAB_UNITY_WINDOWS_SKIPPED reason=parse-only"
    exit 0
}

if (-not (Test-Path -LiteralPath $UnityExe)) {
    throw "Unity $expectedUnity introuvable: $UnityExe"
}

if (-not $Results) {
    $Results = if ($env:RUNNER_TEMP) { Join-Path $env:RUNNER_TEMP 'editmode.xml' } else { Join-Path $root 'Logs/editmode-unity-windows.xml' }
}
if (-not $Log) {
    $Log = if ($env:RUNNER_TEMP) { Join-Path $env:RUNNER_TEMP 'unity.log' } else { Join-Path $root 'Logs/unity-windows.log' }
}
if (-not $Summary) {
    $Summary = if ($env:RUNNER_TEMP) { Join-Path $env:RUNNER_TEMP 'unity-windows-summary.json' } else { Join-Path $root 'Logs/unity-windows-summary.json' }
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Results) | Out-Null
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Log) | Out-Null

Write-Host "CITYLAB_UNITY_WINDOWS_START version=$expectedUnity project=$ProjectPath"

# Pas de -quit avec -runTests : Unity Test Framework ne le supporte pas.
$unityArgs = @(
    $UnityExe,
    '-batchmode',
    '-nographics',
    '-projectPath', $ProjectPath,
    '-runTests',
    '-testPlatform', 'EditMode',
    '-testResults', $Results,
    '-logFile', $Log
)

$lock = Join-Path $root 'Tools/run_unity_locked.py'
$python = Get-Command py -ErrorAction SilentlyContinue
if ($python) {
    & py $lock -- @unityArgs
} else {
    & python3 $lock -- @unityArgs
}
$unityCode = $LASTEXITCODE

& py (Join-Path $root 'Tools/unity_nunit.py') $Results --summary $Summary
if ($LASTEXITCODE -ne 0) {
    throw "XML EditMode refusé (unity_exit=$unityCode)"
}
if ($unityCode -ne 0) {
    throw "Unity a quitté avec le code $unityCode malgré un XML vert"
}

Write-Host "CITYLAB_UNITY_WINDOWS_OK summary=$Summary"
exit 0
