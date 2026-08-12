[CmdletBinding()]
param(
    [string]$Repository = 'PLiagre/VictoriaCityLab',
    [string]$RunnerRoot = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'ChatGPT\hermes-citylab\.private\runner')
)

$ErrorActionPreference = 'Stop'
$CurrentUser = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
$ExpectedName = "citylab-full-auto-$($env:COMPUTERNAME.ToLowerInvariant())"
$TaskName = 'HermesCityLab-GitHubRunner'
git config --global core.longpaths true
New-Item -ItemType Directory -Force -Path $RunnerRoot | Out-Null

$Existing = gh api "repos/$Repository/actions/runners" | ConvertFrom-Json
$Known = $Existing.runners | Where-Object { $_.name -eq $ExpectedName } | Select-Object -First 1
if (-not $Known) {
    $Downloads = gh api "repos/$Repository/actions/runners/downloads" | ConvertFrom-Json
    $Package = $Downloads | Where-Object { $_.os -eq 'win' -and $_.architecture -eq 'x64' } | Select-Object -First 1
    if (-not $Package) {
        $Release = gh api 'repos/actions/runner/releases/latest' | ConvertFrom-Json
        $Asset = $Release.assets | Where-Object { $_.name -match '^actions-runner-win-x64-.*\.zip$' } | Select-Object -First 1
        if ($Asset) {
            $Package = [pscustomobject]@{
                filename = $Asset.name
                download_url = $Asset.browser_download_url
            }
        }
    }
    if (-not $Package) { throw 'Paquet GitHub Actions Runner Windows x64 introuvable.' }

    $Archive = Join-Path $RunnerRoot $Package.filename
    $VersionName = [IO.Path]::GetFileNameWithoutExtension([IO.Path]::GetFileNameWithoutExtension($Package.filename))
    $VersionRoot = Join-Path $RunnerRoot $VersionName
    if (-not (Test-Path -LiteralPath $Archive)) {
        Invoke-WebRequest -Uri $Package.download_url -OutFile $Archive -UseBasicParsing
    }
    New-Item -ItemType Directory -Force -Path $VersionRoot | Out-Null
    if (-not (Test-Path -LiteralPath (Join-Path $VersionRoot 'config.cmd'))) {
        Expand-Archive -LiteralPath $Archive -DestinationPath $VersionRoot
    }

    $Token = gh api --method POST "repos/$Repository/actions/runners/registration-token" --jq '.token'
    Push-Location $VersionRoot
    try {
        & .\config.cmd --unattended --replace --url "https://github.com/$Repository" --token $Token `
            --name $ExpectedName --labels 'citylab-full-auto' --work '_work'
        if ($LASTEXITCODE -ne 0) { throw "config.cmd a échoué avec le code $LASTEXITCODE" }
    } finally {
        Pop-Location
        $Token = $null
    }
}

$ConfigFile = Get-ChildItem -LiteralPath $RunnerRoot -Recurse -Force -Filter '.runner' | Select-Object -First 1
if (-not $ConfigFile) { throw 'Fichier .runner local introuvable.' }
$RunnerDirectory = $ConfigFile.DirectoryName
$RunCmd = Join-Path $RunnerDirectory 'run.cmd'
$PowerShell = (Get-Command powershell.exe).Source
$Arguments = "-NoProfile -WindowStyle Hidden -Command `"& '$RunCmd'`""
$Action = New-ScheduledTaskAction -Execute $PowerShell -Argument $Arguments -WorkingDirectory $RunnerDirectory
$Trigger = New-ScheduledTaskTrigger -AtLogOn -User $CurrentUser
$Settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -ExecutionTimeLimit ([TimeSpan]::Zero) -MultipleInstances IgnoreNew
Register-ScheduledTask -TaskName $TaskName -Action $Action -Trigger $Trigger -Settings $Settings `
    -Description 'Runner GitHub privé pour la boucle full-auto Victoria CityLab' -User $CurrentUser -RunLevel Limited -Force | Out-Null
Start-ScheduledTask -TaskName $TaskName

Start-Sleep -Seconds 2
$Registered = (gh api "repos/$Repository/actions/runners" | ConvertFrom-Json).runners |
    Where-Object { $_.name -eq $ExpectedName } | Select-Object -First 1
if (-not $Registered) { throw 'Runner enregistré introuvable après configuration.' }
Write-Host "CITYLAB_RUNNER_OK name=$ExpectedName status=$($Registered.status) root=$RunnerDirectory"
