param(
    [string]$RoadmapPath = "Docs/ROADMAP.md"
)

$ErrorActionPreference = "Stop"
$resolvedProject = Split-Path -Parent $PSScriptRoot
$resolvedRoadmap = Join-Path $resolvedProject $RoadmapPath

if (-not (Test-Path -LiteralPath $resolvedRoadmap -PathType Leaf)) {
    Write-Error "CITYLAB_ROADMAP_MISSING path=$resolvedRoadmap"
    exit 1
}

$content = Get-Content -LiteralPath $resolvedRoadmap -Raw -Encoding UTF8
$requiredPatterns = @(
    '(?m)^schema:\s*1\s*$',
    '(?m)^last_updated:\s*\d{4}-\d{2}-\d{2}\s*$',
    '(?m)^active_milestone:\s*M\d+\s*$',
    '(?m)^roadmap_status:\s*(ACTIVE|BLOCKED|COMPLETE)\s*$',
    '(?m)^## D.finition de la version 1\.0\s*$',
    '(?m)^## Jalons\s*$',
    '(?m)^## Protocole de mise . jour\s*$',
    '(?m)^## Journal d.avancement\s*$'
)

foreach ($pattern in $requiredPatterns) {
    if ($content -notmatch $pattern) {
        Write-Error "CITYLAB_ROADMAP_INVALID missing_pattern=$pattern"
        exit 2
    }
}

$lastUpdated = [regex]::Match($content, '(?m)^last_updated:\s*(\d{4}-\d{2}-\d{2})\s*$').Groups[1].Value
$activeMilestone = [regex]::Match($content, '(?m)^active_milestone:\s*(M\d+)\s*$').Groups[1].Value
$roadmapStatus = [regex]::Match($content, '(?m)^roadmap_status:\s*(ACTIVE|BLOCKED|COMPLETE)\s*$').Groups[1].Value
$activeTasks = ([regex]::Matches($content, '\|\s*ACTIVE\s*\|')).Count
$nextTasks = ([regex]::Matches($content, '\|\s*NEXT\s*\|')).Count
$doneTasks = ([regex]::Matches($content, '\|\s*DONE\s*\|')).Count

if ($roadmapStatus -eq "ACTIVE" -and $activeTasks -lt 1) {
    Write-Error "CITYLAB_ROADMAP_INVALID status=ACTIVE but no ACTIVE task exists"
    exit 3
}

if ($activeTasks -gt 2) {
    Write-Error "CITYLAB_ROADMAP_INVALID active_tasks=$activeTasks expected_maximum=2"
    exit 4
}

$updatedDate = [DateTime]::ParseExact($lastUpdated, "yyyy-MM-dd", [Globalization.CultureInfo]::InvariantCulture)
$ageDays = [Math]::Floor(((Get-Date).Date - $updatedDate.Date).TotalDays)
$freshness = if ($ageDays -gt 30) { "STALE" } else { "CURRENT" }

Write-Output "CITYLAB_ROADMAP_OK milestone=$activeMilestone status=$roadmapStatus active_tasks=$activeTasks next_tasks=$nextTasks done_entries=$doneTasks last_updated=$lastUpdated freshness=$freshness"

if ($freshness -eq "STALE") {
    Write-Warning "Roadmap non mise a jour depuis $ageDays jours. Verifier les statuts avant de travailler."
}
