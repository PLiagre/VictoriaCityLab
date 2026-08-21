# Harnais CityLab

Le harnais conserve les portes mécaniques (roadmap, chemins, ledger
historique, parseur Unity). Il **ne produit plus** tout seul.

`harness/pipeline/config.json` est en `mode: manual` avec
`auto_merge: false`. `full_auto.py` refuse de s'exécuter dans ce mode.

La source d'instruction d'un lot est un brief sous
`harness/queue/briefs/`. Hermes propose ; Claude rédige ; Cursor exécute.

```bash
python3 -m unittest discover -s harness/tests -v
python3 -m unittest Tools.tests.test_unity_windows_worker -v
python3 Tools/unity_nunit.py <editmode.xml> --summary /tmp/unity-windows-summary.json
```

Arrêt d'urgence : laisser `mode` à `manual`. Réactiver `full_auto` est
une décision propriétaire, pas un défaut d'agent.
