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
    '(?m)^## Contrat d.une session Codex\s*$',
    '(?m)^### Prompt de lancement recommand.\s*$',
    '(?m)^## Sessions Codex ordonn.es\s*$',
    '(?m)^## Strat.gie d.assets 3D de qualit.\s*$',
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
$codexSection = [regex]::Match(
    $content,
    '(?s)## Sessions Codex ordonn.es\s+(.*?)\s+## Strat.gie d.assets 3D de qualit.'
).Groups[1].Value
$codexEntries = @(
    [regex]::Matches(
        $codexSection,
        '(?m)^\|\s*(\d{2})\s*\|\s*(PROUV\u00C9|EN_COURS|\u00C0_FAIRE|BLOQU\u00C9)\s*\|\s*`([^`]+)`\s*\|'
    ) | ForEach-Object { $_ }
)
$codexIncrements = $codexEntries.Count
$codexInProgress = @($codexEntries | Where-Object { $_.Groups[2].Value -eq 'EN_COURS' })
$codexBlocked = @($codexEntries | Where-Object { $_.Groups[2].Value -match '^BLOQU\u00C9$' }).Count
$codexProved = @($codexEntries | Where-Object { $_.Groups[2].Value -match '^PROUV\u00C9$' }).Count
$activeTaskRows = @(
    [regex]::Matches(
        $content,
        '(?m)^\|\s*`([^`]+-[^`]+)`\s*\|.*\|\s*ACTIVE\s*\|'
    ) | ForEach-Object { $_ }
)
$definedTaskIds = @(
    [regex]::Matches(
        $content,
        '(?m)^\|\s*`([^`]+)`\s*\|.*\|\s*(?:DONE|ACTIVE|NEXT|BACKLOG|BLOCKED)\s*\|'
    ) | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique
)

if ($roadmapStatus -eq "ACTIVE" -and $activeTasks -lt 1) {
    Write-Error "CITYLAB_ROADMAP_INVALID status=ACTIVE but no ACTIVE task exists"
    exit 3
}

if ($activeTasks -gt 2) {
    Write-Error "CITYLAB_ROADMAP_INVALID active_tasks=$activeTasks expected_maximum=2"
    exit 4
}

if ($roadmapStatus -eq "ACTIVE" -and $codexIncrements -lt 1) {
    Write-Error "CITYLAB_ROADMAP_INVALID status=ACTIVE but no ordered Codex increment exists"
    exit 5
}

for ($index = 0; $index -lt $codexEntries.Count; $index++) {
    $actualOrder = [int]$codexEntries[$index].Groups[1].Value
    $expectedOrder = $index + 1
    if ($actualOrder -ne $expectedOrder) {
        Write-Error "CITYLAB_ROADMAP_INVALID codex_order=$actualOrder expected=$expectedOrder"
        exit 6
    }

    $taskId = $codexEntries[$index].Groups[3].Value
    if ($taskId -notin $definedTaskIds) {
        Write-Error "CITYLAB_ROADMAP_INVALID codex_task_without_definition=$taskId"
        exit 7
    }
}

if ($roadmapStatus -eq "ACTIVE" -and $codexInProgress.Count -ne 1) {
    Write-Error "CITYLAB_ROADMAP_INVALID status=ACTIVE codex_in_progress=$($codexInProgress.Count) expected=1"
    exit 8
}

if ($roadmapStatus -eq "ACTIVE" -and $activeTaskRows.Count -ne 1) {
    Write-Error "CITYLAB_ROADMAP_INVALID status=ACTIVE active_task_rows=$($activeTaskRows.Count) expected=1"
    exit 9
}

if ($roadmapStatus -eq "ACTIVE") {
    $activeTaskId = $activeTaskRows[0].Groups[1].Value
    $inProgressTaskId = $codexInProgress[0].Groups[3].Value
    if ($activeTaskId -ne $inProgressTaskId) {
        Write-Error "CITYLAB_ROADMAP_INVALID active_task=$activeTaskId codex_in_progress_task=$inProgressTaskId"
        exit 10
    }
}

$updatedDate = [DateTime]::ParseExact($lastUpdated, "yyyy-MM-dd", [Globalization.CultureInfo]::InvariantCulture)
$ageDays = [Math]::Floor(((Get-Date).Date - $updatedDate.Date).TotalDays)
$freshness = if ($ageDays -gt 30) { "STALE" } else { "CURRENT" }

Write-Output "CITYLAB_ROADMAP_OK milestone=$activeMilestone status=$roadmapStatus active_tasks=$activeTasks next_tasks=$nextTasks codex_increments=$codexIncrements codex_in_progress=$($codexInProgress.Count) codex_proved=$codexProved codex_blocked=$codexBlocked done_entries=$doneTasks last_updated=$lastUpdated freshness=$freshness"

if ($freshness -eq "STALE") {
    Write-Warning "Roadmap non mise a jour depuis $ageDays jours. Verifier les statuts avant de travailler."
}
