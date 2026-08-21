# Architecture d'automatisation CityLab

Le modèle vivant est ADR-0002 : Hermes pilote, Claude Code briefe et
relit, Cursor exécute, le worker Unity valide l'EditMode, le propriétaire
fusionne.

```text
Hermes (proposition, cadence)
        |
        v
Claude Code (brief + critères, lecture seule)
        |
        v
Cursor dans agent/* (draft PR)
        |
        v
portes mécaniques (roadmap, tests Python, diff-check)
        |
        +-- si le lot touche le jeu --> unity-windows (workflow_dispatch)
        |
        v
Claude Code (nouvelle invocation, revue)
        |
        v
propriétaire fusionne
```

L'ancien graphe Codex → merge bot est archivé. Les dossiers `inbox/`,
`reviews/`, `decisions/` et le ledger restent lisibles. `mode: manual`
interdit de relancer `full_auto.py`.
