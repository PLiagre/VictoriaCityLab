# Pipeline CityLab — archive full-auto et modèle vivant

## Modèle vivant (ADR-0002)

Hermes pilote. Claude Code écrit le brief et relit en lecture seule.
Cursor exécute dans un worktree `agent/*`. Le worker Unity Windows valide
l'EditMode d'un SHA `PLiagre` via `workflow_dispatch`. Le propriétaire
fusionne. Aucun cron ne produit ni ne fusionne.

Détail : `Docs/Automation/ADR-0002-hermes-pilot-et-worker-unity.md` et
`Docs/Automation/UNITY_WINDOWS_WORKER.md`. File d'instruction :
`harness/queue/briefs/`.

## Archive du 12 août 2026

Le mécanisme ci-dessous a été copié de l'ancien ForgeHistory
(`3807764933c0a7521ae03a4038dd4f197186fffa`) puis **retiré** : `mode:
manual`, `auto_merge: false`, workflows producteurs/merge bots en
`*-retired`. Les preuves historiques (PR #14–#18) restent valides pour
ce qu'elles étaient : une boucle d'automatisation, pas une ville
intégrée.

L'orchestrateur `harness/pipeline/full_auto.py` refuse désormais de
tourner tant que `mode != full_auto`. Ne pas le réactiver.

## Worker Unity

```powershell
gh workflow run unity-windows.yml --repo PLiagre/VictoriaCityLab -f sha=<40-hex> -f ref_name=agent/exemple
powershell -ExecutionPolicy Bypass -File Tools/run_unity_windows_worker.ps1 -SkipUnity
py Tools/unity_nunit.py Logs/editmode.xml --summary Logs/unity-windows-summary.json
```

## Arrêts

- `mode: manual` dans `harness/pipeline/config.json` ;
- issue `pipeline/pause` (coupe historique, conservée) ;
- runner `unity` hors ligne = file d'attente, jamais un succès.
