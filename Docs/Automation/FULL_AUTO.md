# Pipeline full-auto Victoria CityLab

## Architecture

Le mécanisme adapte l'architecture du commit
`3807764933c0a7521ae03a4038dd4f197186fffa` de ForgeHistory au dépôt Unity.
Les responsabilités sont séparées et chaque transition laisse une preuve :

1. Hermes prépare le plan de l'incrément `EN_COURS`.
2. Codex produit dans une branche `codex/*`, sans droit de verdict.
3. La CI GitHub exécute les portes mécaniques.
4. Cursor audite le diff et écrit un constat structuré.
5. Claude contredit chaque constat dans une session distincte.
6. La politique déterministe enregistre `PASS` ou `REJECT` et avance le
   ledger append-only.
7. Le merge bot exige simultanément CI verte, décision `PASS` sur `main`
   et chemins autorisés.
8. Après fusion, un workflow vérifie le SHA, archive audit/revue/décision et
   clôt le ledger à `AUDIT_ARCHIVED`.

Le workflow de preuve utilise la même chaîne sans modifier le jeu. Il crée une
preuve sous `Automation/Proofs/`, ce qui permet de contrôler le mécanisme de
bout en bout avant de lui confier un incrément de production.

## Workflows

- `citylab-full-auto` : déclenchement manuel ou toutes les six heures ;
- `full-auto-ci` : tests du harnais, roadmap, ledger et diff ;
- `pipeline-audit` : audit Cursor, challenge Claude et décision versionnée ;
- `merge-bot` : fusion conditionnelle et fermée ;
- `pipeline-verify` : vérification post-fusion et archivage ;
- `hermes-dashboard` : tableau de bord GitHub/ledger.

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
  producteur échoue fermé.
- Le propriétaire peut toujours fermer la PR, désactiver la tâche planifiée ou
  ouvrir une issue `pipeline/pause`.

La tâche `META-AUTO-01` ne peut passer à `DONE` qu'avec les URL d'un run
réussi, d'une PR fusionnée automatiquement et d'une archive dont le ledger est
valide.
