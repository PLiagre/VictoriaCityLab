# CLAUDE.md

Guide pour Claude Code (brief, critères, revue) dans Victoria CityLab.

## Produit

CityLab est le **laboratoire Unity** et la **vue ville** de ForgeHistory.
Ce n'est pas un second jeu. La simulation de production vit dans
`PLiagre/ForgeHistory` sous `sim/` (`python -m sim`), couche 2 « Villes ».
`LocalCitySimulation`, l'horloge locale et `CitySaveService` sont des
adaptateurs de laboratoire, jamais le runtime cible.

Ne pas modifier ForgeHistory depuis ce dépôt. Si `sim/` ou le contrat
d'intégration doivent changer, écrire une demande dans
`hermes/requests/` — jamais un patch implicite.

## Langue

Français clair. Dire ce qui a été fait, pourquoi, et ce qui reste.

## Rôles (ADR-0002)

| acteur | fait | ne fait pas |
|---|---|---|
| Hermes | propose, cadance, lance | code produit, brief, verdict, fusion |
| Claude Code | brief + critères ; revue lecture seule | exécuter le lot, fusionner |
| Cursor | exécute dans `agent/*` (ou `cursor/*` ici) | prononcer la compatibilité Unity, fusionner |
| worker Unity Windows | EditMode du SHA exact | PlayMode graphique, fusion |
| propriétaire | fusionne | — |

Une proposition Hermes n'est pas une instruction. Le brief sous
`harness/queue/briefs/` est la seule source d'instruction d'un lot.

## Revue

Nouvelle invocation, `--permission-mode plan`, outils de lecture seulement.
Relire le diff et, si le lot touche le jeu, les preuves `unity-windows`.
Ne fusionne pas.

## Commandes

```bash
python3 -m unittest discover -s harness/tests -v
python3 -m unittest Tools.tests.test_unity_windows_worker -v
python3 Tools/unity_nunit.py <editmode.xml> --summary /tmp/summary.json
```

Unity `6000.0.43f1` n'est pas sur la VM Linux. Les  tests EditMode réels
passent par `Tools/run_unity_windows_worker.ps1` sur le worker Windows.

## Routing

| fichiers | lire |
|---|---|
| `harness/queue/briefs/**` | ce brief-là seulement |
| `Docs/ROADMAP.md` | mémoire produit |
| `hermes/**` | pilotage, pas une instruction d'exécutant |
| `Packages/com.victoria.citymode/**` | laboratoire + contrats portables |
| `Docs/Integration/FORGEHISTORY_CITY_MODE_CONTRACT.md` | frontière de données v1 |
