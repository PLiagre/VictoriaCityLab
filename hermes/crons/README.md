# Crons Hermes — CityLab

Contrat des tâches planifiées (ADR-0002).

## Autorisé

Un cron **quotidien**, lecture / mesure / proposition :

1. lire git, `Docs/ROADMAP.md`, l'âge de `hermes/DASHBOARD.md` ;
2. exécuter les tests Python du harnais et du parseur Unity ;
3. écrire `hermes/propositions/DERNIERE-VEILLE.md` (gitignoré) ;
4. n'ouvrir une `PROPOSITION-*.md` que s'il y a un constat **nouveau**.

Le workflow GitHub `hermes-daily.yml` tourne sur `ubuntu-latest` (pas le
runner personnel) et n'a que `contents: read`.

## Interdit

- `git push`, `gh pr merge`, toute fusion ;
- écrire du code produit ou de la CI ;
- lancer le worker Unity, ForgePilot `--run`, ou `run_full_auto.ps1` ;
- rédiger un brief ou un verdict ;
- réactiver `mode: full_auto`.
