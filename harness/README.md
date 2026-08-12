# Harnais full-auto CityLab

Ce dossier adapte l'architecture multi-roles de ForgeHistory a Victoria
CityLab. La roadmap reste la source unique d'instruction. L'orchestrateur
selectionne l'unique increment `EN_COURS`, lance un Generateur Codex, execute
les portes mecaniques, puis confie le diff a une seconde invocation Codex en
lecture seule. Seul un verdict structure `PASS` autorise la publication.

La publication cree une branche `codex/auto/*`, un commit, une pull request,
puis demande l'auto-merge GitHub. Elle exige un worktree propre au depart et
refuse les chemins de CI, d'orchestration, de gouvernance et toutes les sources
Vendor. Une modification de production exige la synchronisation de ROADMAP,
PROTOTYPE_STATUS et VALIDATION.

Commandes :

```powershell
# Preflight complet, sans agent ni ecriture
powershell -ExecutionPolicy Bypass -File Tools/run_full_auto.ps1 -DryRun -AllowDirty

# Execution locale sans publication
powershell -ExecutionPolicy Bypass -File Tools/run_full_auto.ps1

# Boucle complete avec branche, PR et auto-merge
powershell -ExecutionPolicy Bypass -File Tools/run_full_auto.ps1 -Publish

# Tests du harnais
py -m unittest discover -s harness/tests -v
```

Arret d'urgence : passer `mode` a `manual`, definir
`CITYLAB_FULL_AUTO_PAUSE=1`, creer `.git/citylab-full-auto.pause`, ou poser le
label GitHub `pipeline/pause` sur une issue ouverte.

