# Pipeline full-auto Victoria CityLab

## Architecture

Le mécanisme adapte l'architecture du commit
`3807764933c0a7521ae03a4038dd4f197186fffa` de ForgeHistory au dépôt Unity.
Les responsabilités sont séparées et chaque transition laisse une preuve :

1. Hermes prépare le plan de l'incrément `EN_COURS`.
2. Codex produit dans une branche `codex/*`, sans droit de verdict.
3. Les portes mécaniques et une invocation Claude distincte évaluent chaque lot.
4. La PR exécute toujours la CI GitHub et la politique de chemins fermée.
5. Si le lot est un point critique, l'orchestrateur ajoute
   `pipeline/critical-audit` : Cursor audite alors le diff et Claude challenge
   son verdict dans une nouvelle invocation.
6. Pour une PR critique, la politique déterministe enregistre `PASS` ou
   `REJECT` dans le ledger append-only ; une PR courante saute cette étape.
7. Le merge bot exige CI et chemins autorisés pour toutes les PR, plus une
   décision critique `PASS` au SHA exact lorsque le label critique est présent.
8. Après fusion d'une PR critique seulement, un workflow vérifie le SHA,
   archive audit/revue/décision et clôt le ledger à `AUDIT_ARCHIVED`.

Le workflow de preuve utilise la même chaîne sans modifier le jeu. Il crée une
preuve sous `Automation/Proofs/`, ce qui permet de contrôler le mécanisme de
bout en bout avant de lui confier un incrément de production.

## Workflows

- `citylab-full-auto` : déclenchement manuel ou toutes les six heures ;
- `full-auto-ci` : tests du harnais, roadmap, ledger et diff ;
- `pipeline-audit` : audit Cursor ciblé par label critique ou lancement manuel
  PR+SHA, challenge Claude et décision versionnée ;
- `merge-bot` : fusion conditionnelle et fermée ;
- `pipeline-verify` : vérification post-fusion et archivage ;
- `hermes-dashboard` : tableau de bord GitHub/ledger.

## Points critiques audités par Cursor

Une ouverture, une mise à jour ou une synchronisation de PR ne déclenche plus
Cursor. Le label `pipeline/critical-audit` est ajouté automatiquement seulement
pour les tâches suivantes :

| Domaine critique | Tâches |
|---|---|
| Autorité, package et hôte | `M3-FH-02`, `M3-FH-03`, `M3-FH-05`, `M3-FH-07` |
| Transition et cycle de vue | `M3-FH-04` |
| Conservation, synchronisation, streaming et sauvegarde | `M4-FH-LOD-01`, `M4-FH-SYNC-01`, `M4-FH-STREAM-01`, `M4-FH-SAVE-01` |
| Parcours intégré et robustesse campagne | `M6-GAME-01`, `M6-SAVE-01` |
| Portes de livraison | `REL-PERF`, `REL-QA`, `REL-SHIP` |

Une PR manuelle devient également critique si elle touche la sécurité, les
workflows, le harnais, la gouvernance, une migration de données ou une frontière
d'autorité. Le propriétaire ajoute alors explicitement
`pipeline/critical-audit`, ou lance `pipeline-audit` manuellement avec le numéro
de PR et son SHA attendu. Le SHA est revérifié avant l'audit pour empêcher une
décision sur une révision devenue obsolète.

Les lots courants de gameplay, HUD, art ou contenu ne sont pas audités par
Cursor sauf label explicite. Ils conservent l'évaluation Claude séparée avant
publication, les tests ciblés, la CI, la roadmap et la politique de chemins.

Les workflows privilégiés tournent sur le runner privé portant le label
`citylab-full-auto`. Ils utilisent les sessions locales déjà authentifiées de
`gh`, `codex`, `agent` et `claude`, ainsi que le profil Hermes
`citylab-local-orchestrator`. Aucun jeton de ces outils n'est copié dans le
dépôt ou dans les secrets GitHub.

Si Claude Code répond avec une limite d'usage 429, le rôle évaluateur utilise
le modèle Claude Sonnet exposé par l'abonnement Cursor. Le transport et le
modèle exacts sont inscrits dans le verdict ; aucun repli vers Codex n'est
autorisé pour juger une production Codex.

## Installation du runner

```powershell
hermes profile create citylab-local-orchestrator --clone-from forge-local-orchestrator
powershell -ExecutionPolicy Bypass -File Tools/install_full_auto_runner.ps1
```

Le script enregistre un runner dédié au dépôt, crée la tâche planifiée
`HermesCityLab-GitHubRunner`, la démarre et vérifie sa présence via l'API
GitHub.

## Exécution

```powershell
# Validation locale sans publication
powershell -ExecutionPolicy Bypass -File Tools/run_full_auto.ps1 -DryRun

# Incrément de roadmap complet
powershell -ExecutionPolicy Bypass -File Tools/run_full_auto.ps1 -Publish

# Preuve distante contrôlée
gh workflow run full-auto.yml --repo PLiagre/VictoriaCityLab -f mode=proof
```

## Sécurité et arrêts

- Une issue ouverte portant le label `pipeline/pause` bloque la génération.
- Les workflows, le harnais, `AGENTS.md`, la roadmap, les tests et les sources
  Vendor ne sont jamais fusionnés par le merge bot.
- Les changements de production doivent aussi mettre à jour
  `Docs/ROADMAP.md`, `Docs/PROTOTYPE_STATUS.md` et `Docs/VALIDATION.md`.
- Trois refus ou un plateau arrêtent la boucle et produisent un état `STUCK`.
- Un verdict absent, illisible, associé au mauvais SHA ou rendu par le
  producteur échoue fermé lorsqu'un audit critique est requis.
- Le propriétaire peut toujours fermer la PR, désactiver la tâche planifiée ou
  ouvrir une issue `pipeline/pause`.

La tâche `META-AUTO-01` est `DONE` depuis le cycle du 12 août 2026 :
[run](https://github.com/PLiagre/VictoriaCityLab/actions/runs/31606929060),
[PR témoin auto-fusionnée](https://github.com/PLiagre/VictoriaCityLab/pull/14),
[audit](https://github.com/PLiagre/VictoriaCityLab/pull/15),
[archive](https://github.com/PLiagre/VictoriaCityLab/pull/16) et
[dashboard Hermes](https://github.com/PLiagre/VictoriaCityLab/pull/18).
