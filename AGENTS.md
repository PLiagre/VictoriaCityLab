# Instructions permanentes Victoria CityLab

Ces règles s'appliquent à toute session de travail ouverte dans ce dépôt.

## Démarrage obligatoire

Avant toute modification :

1. lire entièrement `Docs/ROADMAP.md` ;
2. lire `Docs/PROTOTYPE_STATUS.md` et `Docs/VALIDATION.md` ;
3. exécuter `powershell -ExecutionPolicy Bypass -File Tools/check_roadmap.ps1` ;
4. vérifier `git status --short` et préserver les changements déjà présents ;
5. travailler d'abord sur la tâche `ACTIVE` de la roadmap, puis sur la première
   tâche `NEXT` non bloquée ;
6. citer l'identifiant de roadmap choisi dans la première mise à jour de session.

Une demande explicite de l'utilisateur peut changer la priorité, mais la
roadmap doit alors être mise à jour pour refléter cette décision.

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
