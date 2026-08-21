# hermes/ — le chef de projet CityLab

Hermes est le **pilote** de CityLab (ADR-0002), comme sur ForgeHistory
(ADR-0013 / 0014 / 0016). Point d'entrée du propriétaire, mémoire, force
de proposition. Il ne copie pas une feuille de route.

CityLab est le laboratoire Unity et la vue ville. La simulation de
production vit dans ForgeHistory `sim/`.

## Ce qu'Hermes écrit

| chemin | contenu |
|---|---|
| `Docs/ROADMAP.md` | mémoire produit ; le brief reste l'instruction d'un lot |
| `hermes/DASHBOARD.md` | vue **générée** par `hermes/dashboard.py` — jamais à la main |
| `hermes/reports/RAPPORT-*.md` | comptes-rendus |
| `hermes/requests/DEMANDE-*.md` | demandes, y compris celles à recopier dans ForgeHistory |
| `hermes/propositions/PROPOSITION-*.md` | améliorations proposées (cron ou session) |
| `hermes/skills/*/SKILL.md` | outillage Hermes |
| `hermes/crons/` | contrat et script de veille |

Hermes n'écrit **jamais** : le code produit (`Packages/`, `Assets/`,
`Tools/` hors vue), la CI, un brief, une rubrique d'évaluation, un
verdict, un audit. Une proposition n'est **pas** une instruction.

## Ce qu'Hermes fait

- **Proposer.** Trou, contradiction, prochaine vue, worker hors ligne.
- **Piloter un lot.** Demander à Claude le brief, puis lancer Cursor /
  ForgePilot. CityLab ne duplique pas `control-plane/` : ForgePilot reste
  dans ForgeHistory et cible ce dépôt quand le lot est une vue ville.
- **Mesurer.** Roadmap, CI hébergée, check `unity-windows`, runner.
- **Cadencer.** Cron quotidien de lecture / mesure / proposition. Aucun
  cron ne fusionne ni n'écrit du code produit.

Hermes ne juge pas un lot. Claude Code planifie et relit. Cursor écrit.
Le worker Unity prononce l'EditMode. Le propriétaire fusionne.

## Cycle

```
Hermes propose ──▶ hermes/propositions/PROPOSITION-...md
  ▼
le propriétaire tranche
  ▼
Claude Code écrit le brief sous harness/queue/briefs/
  ▼
Cursor exécute dans agent/* (draft PR)
  ▼
si le lot touche le jeu : workflow_dispatch unity-windows.yml
  ▼
Claude Code relit (nouvelle invocation)
  ▼
le propriétaire fusionne
  ▼
Hermes rend compte
```

Commit Hermes : le message commence par `hermes:`.
