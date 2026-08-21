# Instructions permanentes Victoria CityLab

Ces règles s'appliquent à toute session de travail ouverte dans ce dépôt.

## Démarrage obligatoire

Avant toute modification :

1. lire entièrement `Docs/ROADMAP.md` ;
2. lire `Docs/PROTOTYPE_STATUS.md` et `Docs/VALIDATION.md` ;
3. exécuter `powershell -ExecutionPolicy Bypass -File Tools/check_roadmap.ps1` ;
4. vérifier `git status --short` et préserver les changements déjà présents ;
5. si un brief existe sous `harness/queue/briefs/`, c'est l'instruction du
   lot ; sinon prendre la tâche `ACTIVE` de la roadmap puis l'unique
   incrément `EN_COURS` ;
6. citer l'identifiant de roadmap (et le brief s'il y en a un) dans la
   première mise à jour de session.

Une demande explicite du propriétaire peut changer la priorité, mais la
roadmap doit alors être mise à jour pour refléter cette décision.

## Pilotage (ADR-0002)

- Hermes propose, cadance et lance. Il n'écrit pas le code produit, ni un
  brief, ni un verdict, et il ne fusionne pas.
- Claude Code rédige le brief et les critères, puis relit en lecture
  seule dans une nouvelle invocation.
- Cursor exécute dans un worktree `agent/*` (les lots Cloud Agent de ce
  dépôt peuvent porter le préfixe `cursor/`).
- Si le lot touche le jeu : worker Unity Windows, `workflow_dispatch`,
  check `unity-windows`, SHA d'une branche `main` / `agent/*` / `cursor/*`.
- Le propriétaire fusionne. Pas d'auto-fusion. Pas de cron producteur.
- Ne pas modifier ForgeHistory depuis ici ; une demande amont s'écrit
  dans `hermes/requests/`.

L'ancien pipeline full-auto est archivé (`mode: manual`). Voir
`Docs/Automation/FULL_AUTO.md`.

## Suivi obligatoire

Toute session qui modifie du code, des assets, des scènes, des réglages ou de la
documentation de production doit, avant sa conclusion :

- mettre à jour les lignes concernées de `Docs/ROADMAP.md` ;
- conserver les identifiants existants ;
- ne marquer `DONE` qu'avec les critères d'acceptation et une preuve vérifiable ;
- mettre à jour `last_updated`, l'état de pilotage et le journal d'avancement ;
- synchroniser `Docs/PROTOTYPE_STATUS.md` si une fonction devient réellement
  jouable ;
- synchroniser `Docs/VALIDATION.md` si les tests, builds ou mesures changent ;
- exécuter le contrôle de roadmap et `git diff --check`.

## Limites de qualité

- Ne jamais déclarer le jeu « AAA », « complet », « alpha », « bêta » ou « 1.0 »
  sans satisfaire la porte correspondante de `Docs/ROADMAP.md`.
- Préserver l'architecture du package `Packages/com.victoria.citymode` et la
  séparation des assets hôtes décrite dans `Docs/GOVERNANCE.md`.
- Ne jamais modifier directement une source Vendor pour l'intégrer : créer une
  variante sous `Assets/CityLabHost/Adapted` et mettre à jour l'audit.
- Toute nouvelle mécanique de simulation doit être déterministe ou documenter
  explicitement pourquoi elle ne peut pas l'être.
- `LocalCitySimulation` reste un adaptateur de laboratoire ; la production
  économique urbaine appartient à ForgeHistory `sim/`.
