# Architecture d'automatisation CityLab

Cette architecture reproduit les principes de ForgeHistory au lieu d'en
copier les historiques de briefs : orchestration deterministe, roles separes,
preuve ecrite avant execution, budget borne, decision fermee et publication
conditionnelle.

```text
ROADMAP (unique EN_COURS)
        |
        v
orchestrateur deterministe -- kill switch / verrou / budget 3 iterations
        |
        v
Hermes (planification, jamais de code ni de verdict)
        |
        v
Generateur Codex (ecriture, jamais de verdict)
        |
        v
portes mecaniques (roadmap + tests harnais + diff-check)
        |
        v
Evaluateur Claude distinct (lecture seule, JSON PASS/REJECT)
        |
        +-- REJECT --> feedback versionne dans Logs/FullAuto --> nouvelle iteration
        |
        +-- PASS --> PR codex/* --> audit Cursor --> challenge Claude
                                            |
                                            v
                       decision + ledger --> merge bot --> archive
```

Les contrats de role sont dans `Architecture/agents/`. Les dossiers
`inbox/`, `reviews/`, `decisions/` et `archive/` matérialisent les frontières
d'écriture Cursor, Claude, politique déterministe et archivage. Le ledger JSONL
est append-only et refuse toute transition hors de la machine d'état.

Le mode operatoire,
les prerequis de secrets et les limites sont dans
`Docs/Automation/FULL_AUTO.md`.
