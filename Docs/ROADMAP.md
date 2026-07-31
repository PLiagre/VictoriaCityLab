# Roadmap de production Victoria CityLab

<!-- CITYLAB_ROADMAP
schema: 1
last_updated: 2026-07-31
active_milestone: M1
roadmap_status: ACTIVE
-->

Ce document est la source de vérité du développement. L'objectif est un jeu de
gestion seigneuriale médiévale complet, avec construction organique, économie
physique, société, carte régionale et batailles tactiques. Il vise la profondeur
et la finition d'un grand city-builder commercial tout en conservant une
identité, des règles, des visuels et des contenus originaux à Victoria CityLab.

Le projet est actuellement un vertical slice jouable. Il ne doit être qualifié
de jeu complet ou de qualité AAA que lorsque tous les critères de la section
« Définition de la version 1.0 » sont objectivement validés.

## État de pilotage

| Champ | Valeur |
|---|---|
| Dernière mise à jour | 31 juillet 2026 |
| Jalon actif | `M1` — fondations de production |
| Dernier jalon validé | `M0` — vertical slice forêt et construction |
| Priorité immédiate | sauvegarde versionnée, navigation robuste, catalogue de bâtiments piloté par données |
| Build de référence | `Builds/Windows/VictoriaCityLab.exe` |
| Dépôt distant | `https://github.com/PLiagre/VictoriaCityLab` — branche `main` |
| Preuves de référence | `Docs/VALIDATION.md` et `Logs/` |

### États autorisés

- `DONE` : critères d'acceptation remplis et preuve locale enregistrée ;
- `ACTIVE` : travail prioritaire actuellement autorisé ;
- `NEXT` : prochain travail ordonné, prêt à démarrer ;
- `BACKLOG` : nécessaire à la version 1.0 mais non prioritaire ;
- `BLOCKED` : dépendance explicite renseignée dans la ligne concernée.

Une tâche ne passe jamais à `DONE` sur la seule base d'une impression visuelle
ou d'une compilation réussie. Sa colonne « Sortie / preuve » doit être satisfaite.

## Définition de la version 1.0

La version 1.0 est atteinte uniquement si les conditions suivantes sont toutes
remplies :

- une campagne ou partie libre jouable pendant au moins 20 heures sans blocage ;
- une boucle complète collecte, production, logistique, marché, construction,
  besoins des foyers, fiscalité et croissance ;
- au moins 6 familles de ressources, 20 chaînes de production et 45 bâtiments
  réellement différenciés par leur fonction ou leur évolution ;
- 500 habitants simulés à 60 FPS sur la machine cible recommandée, avec un p95
  CPU inférieur à 16,7 ms dans le scénario de référence ;
- sauvegarde/chargement versionné, autosave, récupération d'erreur et migrations ;
- carte régionale, revendication territoriale, adversaires IA, diplomatie et
  conflits résolus dans le monde de jeu ;
- armées, équipement, formations, moral, fatigue, pertes et conséquences
  économiques persistantes ;
- saisons, météo, agriculture, maladies, incendies et pénuries ayant un effet
  lisible et équilibré ;
- tutoriel, objectifs, options graphiques/audio/commandes, accessibilité et
  localisation française/anglaise ;
- direction artistique cohérente et originale, sans primitives de secours dans
  les scènes de production ni dépendance visuelle à une propriété tierce connue ;
- zéro erreur bloquante connue, zéro corruption de sauvegarde connue, tests de
  régression verts et campagne complète validée par QA ;
- build Windows signé et reproductible, crash reporting, crédits et licences.

## Jalons

| ID | Jalon | État | Porte de sortie |
|---|---|---|---|
| `M0` | Vertical slice forêt et construction | DONE | Routes, parcelles, maisons, transport du bois, camp forestier, HUD, build Windows et tests validés. |
| `M1` | Fondations de production | ACTIVE | Sauvegarde fiable, données versionnées, navigation robuste, emplois physiques et tests déterministes. |
| `M2` | Économie de village jouable | NEXT | Nourriture, marché, stockage, six ressources, besoins des foyers et progression pendant 2 heures. |
| `M3` | Ville organique et société | BACKLOG | Parcelles évolutives, familles, santé, foi, ordre, fiscalité et croissance jusqu'à 250 habitants. |
| `M4` | Région stratégique | BACKLOG | Plusieurs territoires, revendication, commerce régional, diplomatie et au moins un seigneur IA. |
| `M5` | Guerre tactique | BACKLOG | Levées, suite, équipement, formations, moral, bataille et conséquences persistantes. |
| `M6` | Alpha jouable de bout en bout | BACKLOG | Partie complète de 8 heures, objectifs, défaite/victoire, sauvegarde et contenu représentatif. |
| `M7` | Contenu et qualité de production | BACKLOG | Art final, audio, VFX, animations, variations et UX sans placeholders. |
| `M8` | Bêta et équilibrage | BACKLOG | Feature complete, performances cibles, télémétrie QA et aucune régression critique. |
| `M9` | Release Candidate 1.0 | BACKLOG | Campagne 20 heures validée, localisation, accessibilité, packaging et critères 1.0 remplis. |

## M0 — vertical slice validé

| ID | Système | État | Sortie / preuve |
|---|---|---|---|
| `M0-WORLD-01` | Terrain URP, caméra RTS et cycle lumineux | DONE | Capture player 1080p dans `Logs/Captures`. |
| `M0-BUILD-01` | Routes, parcelles et chantiers en phases | DONE | Test PlayMode et capture de référence. |
| `M0-ECON-01` | Stock initial, réservation et transport du bois | DONE | Tests EditMode de la simulation. |
| `M0-FOREST-01` | Camp forestier, travailleurs et réserve finie | DONE | Tests déterministes placement/production/épuisement. |
| `M0-UI-01` | HUD, sélection, priorités et vitesses | DONE | Test PlayMode et thème runtime persistant. |
| `M0-ART-01` | Première passe dark-fantasy stylisée | DONE | Textures originales, assets adaptés, capture sans shader cassé. |
| `M0-REL-01` | Build Windows et smoke test | DONE | `Docs/VALIDATION.md`. |
| `META-ROADMAP-01` | Pilotage persistant du projet | DONE | `AGENTS.md`, `Tools/check_roadmap.ps1` et contrôle `CITYLAB_ROADMAP_OK`. |
| `META-REPO-01` | Publication GitHub et Git LFS | DONE | Dépôt privé `PLiagre/VictoriaCityLab`, branche `main` et 790 objets LFS publiés. |

## M1 — fondations de production

L'ordre ci-dessous est obligatoire sauf blocage documenté. Une session choisit
la première tâche `ACTIVE`, puis la première tâche `NEXT` non bloquée.

| ID | Travail | État | Critères d'acceptation | Sortie / preuve |
|---|---|---|---|---|
| `M1-SAVE-01` | Sauvegarde/chargement versionné | ACTIVE | Sauvegarde manuelle et autosave atomique ; recharge identique des foyers, routes, bâtiments, stocks, camps, emplois et horloge ; fichier corrompu refusé proprement. | Tests aller-retour et migration, fixture sauvegardée, documentation du schéma. |
| `M1-DATA-01` | Catalogue de bâtiments piloté par données | NEXT | Coûts, emprise, emplois, production, étapes et visuels ne sont plus codés en dur ; validation des définitions au démarrage. | Tests de validation et au moins 8 définitions fonctionnelles. |
| `M1-NAV-01` | Navigation et circulation robustes | NEXT | NavMesh mis à jour après construction ; aucune traversée de bâtiment ; récupération après chemin impossible ; 100 agents sans blocage pendant 20 minutes. | Scène de stress et test PlayMode automatisé. |
| `M1-JOBS-01` | Affectation physique des emplois | NEXT | Un habitant ne peut exercer qu'un emploi à la fois ; trajet domicile-travail ; horaires ; absence et remplacement déterministes. | Tests simulation et visualisation dans le HUD. |
| `M1-LOG-01` | Logistique générique | NEXT | Tâches transportables indépendantes des chantiers ; priorités ; sources/destinations ; réservation anti-duplication ; abandon sûr. | Tests concurrence, pénurie et destruction de destination. |
| `M1-TIME-01` | Calendrier et saisons persistants | NEXT | Jour, mois, année et saison déterministes ; vitesse/pause sauvegardées ; événements planifiables. | Tests de franchissement et de rechargement. |
| `M1-TEST-01` | Harnais de simulation longue | NEXT | Simulation headless de 30 jours de jeu, hash déterministe et détection de ressources négatives/agents bloqués. | Log CI local et seed de référence. |
| `M1-PERF-01` | Budget performance 100 habitants | NEXT | 60 FPS en 1080p, p95 inférieur à 16,7 ms, aucune allocation récurrente majeure dans les boucles centrales. | Profil et smoke de 1 800 frames. |

## M2 — économie de village

| ID | Travail | État | Critère principal |
|---|---|---|---|
| `M2-RES-01` | Registre de ressources | BACKLOG | Bois, planches, pierre, nourriture, outils et textile avec unités, stockage et pertes. |
| `M2-FOOD-01` | Cueillette, chasse et consommation | BACKLOG | Les foyers consomment, souffrent de pénurie et choisissent des sources accessibles. |
| `M2-FARM-01` | Champs et agriculture saisonnière | BACKLOG | Fertilité, labour, semis, croissance, récolte, jachère et météo. |
| `M2-CHAIN-01` | Chaînes de production | BACKLOG | Scierie, carrière, forge, moulin, four, tissage et artisanat avec entrées/sorties physiques. |
| `M2-STOCK-01` | Entrepôts et greniers | BACKLOG | Capacité, catégories, zones de service, employés et rééquilibrage. |
| `M2-MARKET-01` | Marché local | BACKLOG | Étals alimentés, couverture des foyers, prix/rareté lisibles et pénuries. |
| `M2-HOME-01` | Besoins et évolution des foyers | BACKLOG | Nourriture, combustible, vêtements, outils et logement déterminent satisfaction et niveau. |
| `M2-TRADE-01` | Commerce extérieur initial | BACKLOG | Import/export, frais, délai, marchand et limites de volume. |

## M3 — ville, population et société

| ID | Travail | État | Critère principal |
|---|---|---|---|
| `M3-PLOT-01` | Parcelles organiques | BACKLOG | Profondeur variable, extensions, jardins et contraintes de terrain. |
| `M3-BUILD-01` | Construction physique complète | BACKLOG | Terrassement, matériaux par étape, ouvriers, échafaudages, réparation et démolition. |
| `M3-FAMILY-01` | Foyers et cycle de vie | BACKLOG | Âge, couples, naissances, décès, migration et compétences. |
| `M3-HEALTH-01` | Santé, maladies et blessures | BACKLOG | Risques, propagation, soins, mortalité et impacts sur le travail. |
| `M3-FAITH-01` | Foi et sépulture | BACKLOG | Église, offices, cimetière, besoins et effets sociaux. |
| `M3-ORDER-01` | Ordre et criminalité | BACKLOG | Mécontentement, vol, milice locale et conséquences graduées. |
| `M3-TAX-01` | Fiscalité et trésor | BACKLOG | Impôts, dîme, dépenses, solde, politiques et réactions des foyers. |
| `M3-FIRE-01` | Incendies et catastrophes | BACKLOG | Propagation, intervention, dégâts persistants et reconstruction. |

## M4 — région stratégique et IA

| ID | Travail | État | Critère principal |
|---|---|---|---|
| `M4-MAP-01` | Carte en plusieurs régions | BACKLOG | Chargement, frontières, ressources propres et transitions sans perte d'état. |
| `M4-CLAIM-01` | Influence et revendication | BACKLOG | Coût, progression, contestation et changement de contrôle. |
| `M4-AI-01` | Seigneur IA économique | BACKLOG | Développe un village viable, réagit aux pénuries et poursuit des objectifs. |
| `M4-DIPLO-01` | Diplomatie | BACKLOG | Relations, demandes, accords, menaces, paix et mémoire des actions. |
| `M4-TRADE-01` | Routes commerciales régionales | BACKLOG | Offre/demande, convois, risque, distance et contrôle territorial. |
| `M4-EVENT-01` | Événements et objectifs | BACKLOG | Événements data-driven, choix, conséquences, victoire et défaite. |

## M5 — guerre tactique

| ID | Travail | État | Critère principal |
|---|---|---|---|
| `M5-LEVY-01` | Levées issues des foyers | BACKLOG | Mobilisation retire réellement des travailleurs et affecte l'économie. |
| `M5-RETINUE-01` | Suite professionnelle | BACKLOG | Recrutement, entretien, progression et équipement persistant. |
| `M5-EQUIP-01` | Fabrication et distribution d'équipement | BACKLOG | Armes/armures produites, stockées, attribuées et perdues. |
| `M5-FORM-01` | Formations et commandes | BACKLOG | Déplacement groupé, lignes, cohésion, orientation et collisions stables. |
| `M5-COMBAT-01` | Combat, moral et fatigue | BACKLOG | Portée, impact, défense, moral, fuite, poursuite et fatigue lisibles. |
| `M5-AI-01` | IA tactique | BACKLOG | Choix de terrain, protection des flancs, retraite et objectifs. |
| `M5-CONSEQ-01` | Conséquences persistantes | BACKLOG | Blessés, morts, captifs, butin, familles affectées et pertes économiques. |

## M6 à M9 — finition et sortie

| ID | Domaine | État | Porte 1.0 |
|---|---|---|---|
| `REL-CONTENT` | 45 bâtiments, 20 chaînes, cartes et événements | BACKLOG | Contenu final équilibré et sans placeholder. |
| `REL-ART` | Environnements, architecture, personnages, UI et VFX | BACKLOG | Cohérence visuelle finale, LOD, occlusion et variantes suffisantes. |
| `REL-ANIM` | Locomotion, métiers, construction et combat | BACKLOG | Transitions propres, IK et réactions contextuelles. |
| `REL-AUDIO` | Musique, ambiances et sound design | BACKLOG | Mix dynamique, spatialisation, variations et options complètes. |
| `REL-UX` | Tutoriel, encyclopédie et lisibilité | BACKLOG | Un nouveau joueur termine le tutoriel sans aide externe. |
| `REL-ACCESS` | Accessibilité | BACKLOG | Remapping, sous-titres, taille UI, contrastes, daltonisme et réduction d'effets. |
| `REL-LOC` | Français et anglais | BACKLOG | Aucun texte codé en dur, débordement contrôlé et relecture terminée. |
| `REL-PERF` | 500 habitants et grandes villes | BACKLOG | 60 FPS cible, mémoire stable, chargements et sauvegardes dans le budget. |
| `REL-QA` | Tests et campagne de régression | BACKLOG | Zéro critique/majeur ouvert et campagne 20 heures validée. |
| `REL-SHIP` | Packaging Windows | BACKLOG | Build signé, crédits/licences, crash reporting et procédure de mise à jour. |

## Prochain lot ordonné

1. `M1-SAVE-01` — concevoir le schéma de sauvegarde versionné et ses tests ;
2. `M1-DATA-01` — extraire les définitions codées en dur vers des données validées ;
3. `M1-NAV-01` — remplacer les déplacements directs par une navigation récupérable ;
4. `M1-JOBS-01` — rendre exclusifs et physiques les emplois des habitants ;
5. `M1-LOG-01` — généraliser les réservations et transports ;
6. `M1-TIME-01` — rendre le calendrier persistant ;
7. `M1-TEST-01` — ajouter le test de simulation longue ;
8. `M1-PERF-01` — établir le budget de 100 habitants ;
9. commencer `M2-RES-01` seulement après validation complète de `M1`.

## Risques actifs

| Risque | Niveau | Réponse obligatoire |
|---|---|---|
| Simulation locale trop liée au vertical slice | Élevé | Stabiliser les contrats et les données avant d'ajouter beaucoup de contenu. |
| Double emploi abstrait des habitants | Élevé | Traiter dans `M1-JOBS-01` avant les nouvelles chaînes de production. |
| Navigation non adaptée à une grande population | Élevé | Porte de stress obligatoire dans `M1-NAV-01`. |
| Accumulation d'assets hétérogènes | Moyen | Conserver l'adaptation sous `Assets/CityLabHost/Adapted` et auditer chaque source. |
| Ambition AAA sans budget mesuré | Élevé | Utiliser les portes M1–M9 ; ne jamais remplacer une preuve par un pourcentage subjectif. |
| Régression performance par ajout de contenu | Élevé | Rejouer le scénario de référence à chaque jalon. |

## Protocole de mise à jour

Au début de chaque session de travail :

1. lire `AGENTS.md`, ce document, `Docs/PROTOTYPE_STATUS.md` et
   `Docs/VALIDATION.md` ;
2. exécuter `powershell -ExecutionPolicy Bypass -File Tools/check_roadmap.ps1` ;
3. vérifier `git status --short` et préserver les changements existants ;
4. sélectionner la première tâche `ACTIVE`, ou la première `NEXT` si elle est
   terminée ou réellement bloquée ;
5. annoncer l'identifiant de la tâche choisie avant de modifier le projet.

Avant de terminer une session qui a modifié le projet :

1. mettre à jour l'état des tâches touchées sans réécrire leur identifiant ;
2. renseigner la date `last_updated` et le tableau « État de pilotage » ;
3. ajouter les preuves exactes dans la ligne concernée ou `Docs/VALIDATION.md` ;
4. mettre à jour `Docs/PROTOTYPE_STATUS.md` uniquement pour les fonctions
   réellement jouables ;
5. ajouter une entrée au journal ci-dessous ;
6. exécuter le contrôle de roadmap et `git diff --check`.

## Journal d'avancement

| Date | Tâches | Résultat | Preuves | Prochaine priorité |
|---|---|---|---|---|
| 2026-07-31 | `M0-*` | Vertical slice forêt/construction validé ; `M0` passe à `DONE`, `M1-SAVE-01` devient `ACTIVE`. | `Docs/VALIDATION.md`, build Windows et capture player. | Concevoir et tester la sauvegarde versionnée. |
| 2026-07-31 | `META-ROADMAP-01` | Roadmap 1.0, règles de session et vérificateur de démarrage ajoutés. | `CITYLAB_ROADMAP_OK`, `git diff --check`. | `M1-SAVE-01`. |
| 2026-07-31 | `META-REPO-01` | Projet publié sur GitHub ; `main` configurée pour suivre `origin/main`. | Commit `8a4b728`, push Git et 790 objets LFS transférés. | `M1-SAVE-01`. |
