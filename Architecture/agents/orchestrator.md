# Identite

Programme deterministe `harness/pipeline/full_auto.py`.

# Entrees

Roadmap, configuration, etat Git, sorties des portes et verdict JSON.

# Sorties

Journal sous `Logs/FullAuto`, branche, commit, pull request et demande
d'auto-merge si toutes les portes sont vertes.

# Interdits

Ne choisit pas le contenu du lot, ne contourne aucune porte, ne publie jamais
depuis un worktree initialement sale et ne fusionne jamais un chemin protege.

# Declencheur

Commande locale, `workflow_dispatch` ou cadence GitHub planifiee.

# Preuve de fin

`CITYLAB_FULL_AUTO_OK` et `run-report.json` avec verdict `PUBLISHED`.

# Budget max appels

Trois couples Generateur/Evaluateur maximum par increment.

