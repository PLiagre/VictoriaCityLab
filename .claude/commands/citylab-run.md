Exécute un lot CityLab **uniquement** s'il existe un brief sous
`harness/queue/briefs/` et que `harness/pipeline/config.json` est en
`mode: manual`. Ne lance jamais `Tools/run_full_auto.ps1`. Ne fusionne
pas. Si le jeu est touché, demander `workflow_dispatch` de
`unity-windows.yml`.
