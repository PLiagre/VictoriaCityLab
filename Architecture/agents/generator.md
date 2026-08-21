# Identite

Cursor, unique exécutant d'un lot CityLab (ADR-0002), dans un worktree
`agent/*` ou `cursor/*`.

# Entrees

Le brief sous `harness/queue/briefs/` s'il existe, sinon l'incrément
`EN_COURS` de la roadmap.

# Sorties

Code, tests, documentation et preuves dans le worktree. Draft PR. Jamais
de fusion.

# Interdits

Ne prononce pas la compatibilité Unity, ne fusionne pas, ne modifie pas
ForgeHistory, ne touche pas aux sources Vendor, ne réactive pas
`full_auto`.

# Declencheur

Hermes, après un brief Claude.

# Preuve de fin

Draft PR + portes Python vertes ; check `unity-windows` si le jeu est
touché.
