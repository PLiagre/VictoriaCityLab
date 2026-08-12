# Roadmap de production Victoria CityLab

<!-- CITYLAB_ROADMAP
schema: 1
last_updated: 2026-08-12
active_milestone: M3
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
| Dernière mise à jour | 12 août 2026 |
| Jalon actif | `M3` — ville organique et société |
| Dernier jalon validé | `M2` — économie de village jouable |
| Priorité immédiate | `M3-BUILD-01` — construction physique complète via la boucle full-auto |
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
| `M1` | Fondations de production | DONE | Sauvegarde fiable, données versionnées, navigation robuste, emplois physiques et tests déterministes. |
| `M2` | Économie de village jouable | DONE | Six ressources, alimentation, agriculture, sept chaînes, stockage local, marché, besoins, commerce et simulation 60 jours/2 heures sans invariant cassé. |
| `M3` | Ville organique et société | ACTIVE | Parcelles évolutives, familles, santé, foi, ordre, fiscalité et croissance jusqu'à 250 habitants. |
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
| `META-AUTO-01` | Architecture full-auto multi-acteurs | DONE | Runner `citylab-full-auto-pe` en ligne ; run #31606929060 ; PR #14 auditée PASS puis fusionnée automatiquement ; audit #15 et archive terminale #16 ; dashboard Hermes #18. |

## M1 — fondations de production

L'ordre ci-dessous est obligatoire sauf blocage documenté. Une session choisit
la première tâche `ACTIVE`, puis la première tâche `NEXT` non bloquée.

| ID | Travail | État | Critères d'acceptation | Sortie / preuve |
|---|---|---|---|---|
| `M1-ASSET-01` | Noyau Asset Factory intégré hors Unity | DONE | Blender détecté ; sources Vendor inventoriées par SHA-256 ; catalogue déterministe ; aucune source modifiée et aucun lancement Unity. | Tests Python 2/2, Blender 5.2.0 LTS, 728 modèles/55 textures, `ASSET_FACTORY_INVENTORY_OK`. |
| `M1-ASSET-02` | Admission Vendor et recettes de transformation | DONE | Nouveau pack détecté sans Unity ; provenance/licence obligatoires ; composants épinglés par hash ; source immuable ; recette atomique validée. | Profil GanzSe vérifié contre l'inventaire ; découverte d'un pack inconnu testée ; dry-run générique de 32 copies/50 774 256 octets ; publication atomique testée dans la suite de 10 tests Python. |
| `M1-ASSET-03` | Kit architectural et grammaire Blender | DONE | Fonction, style et graine séparés ; maison, scierie et grenier régénérables ; quatre phases cumulatives ; au moins trois variantes ; échelle/pivot/orientation normalisés ; sortie identique à graine identique. | Scierie, maison et grenier A/B/C générés et publiés ; 4 phases × 3 LOD ; GLB bit-identiques à graine constante ; FBX contrôlés hors Unity. |
| `M1-ASSET-04` | Laboratoire de textures PBR | DONE | Trim sheet bois/pierre/toit ; bake BaseColor/Normal/AO/Roughness/Metallic ; masques de variation ; résultat lisible aux trois zooms. | 6 cartes 2048² publiées, 6 537 776 octets ; graph/recette versionnés ; déterminisme 6/6 ; contrastes bois/pierre/toit validés à 512/256/128 px. |
| `M1-ASSET-05` | QA et publication Asset Factory | BLOCKED | Noms, UV, LOD, colliders, hashes, budgets et licence contrôlés ; revue humaine ; publication dry-run par défaut et sans lancement Unity implicite. | Toutes les portes techniques et Unity sont vertes : 56 FBX, 1 038 meshes/UV, 24 prefabs bâtiment, 8 personnages, zéro script manquant/double LOD et captures runtime. Seule l'approbation artistique humaine reste ouverte ; la production continue sans l'attendre sur instruction utilisateur. |
| `M1-ASSET-06` | Pilote de huit bâtiments | DONE | Huit fonctions jouables disposent de trois variantes cohérentes ; aucun prefab Vendor direct ; catalogue visuel data-driven. | Résidence, scierie, grenier, entrepôt, marché, forge, grange et chapelle sont constructibles ; 24 variantes, 4 phases × 3 LOD, capacités distinctes 120/160/24/2/12/32 et capture player `m1-eight-functions-runtime-20260801.png`. |
| `M1-CHAR-01` | Population modulaire procédurale | DONE | Deux genres, trois âges, quatre morphologies, coiffures et visages combinables ; huit rôles sociaux lisibles ; rig Humanoid partagé, LOD et sélection déterministe ; animations validées sans intersection majeure. | 24 corps et 8 rôles, rig 52 os, avatars Humanoid, 3 LOD, sélection déterministe ; revue player des 8 rôles sur idle/marche/travail sans intersection majeure, `CITYLAB_CHARACTER_REVIEW_OK`. |
| `M1-SAVE-01` | Sauvegarde/chargement versionné | DONE | Sauvegarde manuelle et autosave atomique ; recharge identique des foyers, routes, bâtiments, stocks, camps, emplois et horloge ; fichier corrompu refusé proprement. | F5/F9 et autosave 120 s intégrés ; 4/4 tests dédiés, migration v0, checksum/corruption et remplacement atomique validés ; round-trip player `CITYLAB_SAVE_RUNTIME_OK`; build et smoke verts. |
| `M1-DATA-01` | Catalogue de bâtiments piloté par données | DONE | Coûts, emprise, emplois, production, étapes et visuels ne sont plus codés en dur ; validation des définitions au démarrage. | Catalogue JSON v1 de 8 définitions consommé par simulation, placement, HUD et visuels ; validation stricte et scénario déterministe six fonctions dans la suite 23/23 Editor. |
| `M1-NAV-01` | Navigation et circulation robustes | DONE | NavMesh mis à jour après construction ; aucune traversée de bâtiment ; récupération après chemin impossible ; 100 agents sans blocage pendant 20 minutes. | Grille A* 128² déterministe, NavMesh Unity incrémental, récupération des cibles bloquées et stress 100 habitants/20 minutes sans échec ; 30/30 Editor et 1/1 PlayMode. |
| `M1-JOBS-01` | Affectation physique des emplois | DONE | Un habitant ne peut exercer qu'un emploi à la fois ; trajet domicile-travail ; horaires ; absence et remplacement déterministes. | Huit métiers exclusifs, journées 08h–18h, trajets physiques, absence/replacement par seed, HUD emploi/présence et rechargement identique ; 4 tests dédiés. |
| `M1-LOG-01` | Logistique générique | DONE | Tâches transportables indépendantes des chantiers ; priorités ; sources/destinations ; réservation anti-duplication ; abandon sûr. | Contrat persistant source/destination et priorité ; 4/4 tests priorité, concurrence, pénurie et destruction d'une destination non-chantier ; suite 34/34 Editor et 1/1 PlayMode. |
| `M1-TIME-01` | Calendrier et saisons persistants | DONE | Jour, mois, année et saison déterministes ; vitesse/pause sauvegardées ; événements planifiables. | 3/3 tests franchissement saison/année, ordre d'événements et rechargement exact pause/vitesse ; 37/37 Editor et 1/1 PlayMode. |
| `M1-TEST-01` | Harnais de simulation longue | DONE | Simulation headless de 30 jours de jeu, hash déterministe et détection de ressources négatives/agents bloqués. | Graine 140001, 35 989 ticks, hash courant `f5c411a9...753a82`, minimum ressource 0, zéro échec navigation et zéro agent bloqué ; 70/70 Editor. |
| `M1-PERF-01` | Budget performance 100 habitants | DONE | 60 FPS en 1080p, p95 inférieur à 16,7 ms, aucune allocation récurrente majeure dans les boucles centrales. | Player 100 habitants/1 800 frames : 60,1 FPS, p95 16,650 ms ; boucle 1 200 ticks : 0 collecte gen0 ; build 308 796 131 octets. |

## M2 — économie de village

| ID | Travail | État | Critère principal |
|---|---|---|---|
| `M2-RES-01` | Registre de ressources | DONE | Bois, planches, pierre, nourriture, outils et textile avec unités, stockage et pertes. | Six définitions et stocks persistants ; capacité/débordement, réservation/consommation et pertes journalières déterministes ; 3/3 tests dédiés, 42/42 Editor. |
| `M2-FOOD-01` | Cueillette, chasse et consommation | DONE | Les foyers consomment, souffrent de pénurie et choisissent des sources accessibles. | Cueillette/chasse physiques, retour au stock, consommation quotidienne, choix accessible et faim persistante ; 3/3 tests dédiés, 45/45 Editor. |
| `M2-FARM-01` | Champs et agriculture saisonnière | DONE | Fertilité bornée, labour, semis, croissance modulée par météo, récolte en nourriture et retour en jachère ; deux champs initiaux et état visible au HUD ; 3/3 tests dédiés, 48/48 Editor et 1/1 PlayMode. |
| `M2-CHAIN-01` | Chaînes de production | DONE | Sept recettes déterministes — scierie, carrière, forge, moulin, four, tissage et atelier — consomment leurs entrées locales et publient leurs sorties via la logistique générique multi-ressource ; ateliers visibles dans la partie de référence ; 3/3 tests dédiés, 51/51 Editor et 1/1 PlayMode. |
| `M2-STOCK-01` | Entrepôts et greniers | DONE | Stocks locaux persistants : nourriture au grenier, cinq catégories à l'entrepôt, capacité totale exacte, rayons de service, gardiens physiques et rééquilibrage vers 50 % ; 3/3 tests dédiés, 54/54 Editor et 1/1 PlayMode. |
| `M2-MARKET-01` | Marché local | DONE | Commerçant présent, étals nourriture/outils/textile alimentés physiquement depuis le dépôt le plus proche, couverture des foyers, rareté 0–1000, prix 1000–2000 et pénuries quotidiennes ; 3/3 tests, 57/57 Editor et 1/1 PlayMode. |
| `M2-HOME-01` | Besoins et évolution des foyers | DONE | Nourriture quotidienne, combustible/vêtements mensuels, outils bimensuels et logement pondèrent une satisfaction 0–1000 et quatre niveaux persistants ; pénuries séparées et HUD agrégé ; 3/3 tests, 60/60 Editor et 1/1 PlayMode. |
| `M2-TRADE-01` | Commerce extérieur initial | DONE | Ordres import/export persistants, volume maximal 40, capacité/trésor vérifiés, réservation et annulation sûres, frais 10 %, délai 2–3 jours et marchand interpolé ; 3/3 tests dédiés. |

## M3 — ville, population et société

| ID | Travail | État | Critère principal |
|---|---|---|---|
| `M3-PLOT-01` | Parcelles organiques | DONE | Parcelles orientées sur la route à frontage/profondeur variables ; pente maximale 180 ‰ et chevauchements refusés ; jardins persistants et jusqu'à deux extensions selon le niveau du foyer ; 3/3 tests dédiés, 67/67 Editor, 1/1 PlayMode, build et smoke player verts. |
| `M3-BUILD-01` | Construction physique complète | ACTIVE | Socle jouable validé : terrassement échantillonné puis pierre/bois/planches/outils livrés par phase par l'équipe affectée, avec HUD, visuel et reload exact. Restent les échafaudages, la réparation et la démolition pour fermer la tâche ; l'incrément est désormais consommé par la boucle full-auto. |
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

## Sessions Codex ordonnées

| Ordre | Suivi | Tâche | Incrément de session | Preuve de fermeture de l'incrément |
|---:|---|---|---|---|
| 01 | EN_COURS | `M3-BUILD-01` | Échafaudages, usure, réparation et démolition déterministes. | Tests EditMode/PlayMode, reload exact, HUD et validation mise à jour. |

Cette file est consommée par le workflow full-auto toutes les six heures. Un
incrément ne devient `PROUVÉ` qu'après production Codex, évaluation Claude,
audit Cursor, CI verte, fusion GitHub et archivage du ledger.

## Prochain lot ordonné

1. `M3-BUILD-01` — enrichir la construction physique ;
2. `M3-FAMILY-01` — ajouter les foyers et leur cycle de vie ;
3. reprendre ensuite l'ordre M3 existant ;
4. `M1-ASSET-05` reste une approbation humaine isolée qui ne bloque pas M3.

## Risques actifs

| Risque | Niveau | Réponse obligatoire |
|---|---|---|
| Simulation locale trop liée au vertical slice | Élevé | Stabiliser les contrats et les données avant d'ajouter beaucoup de contenu. |
| Extension des flux au-delà du bois | Moyen | Réutiliser le contrat de ressources et d'extrémités de `M1-LOG-01` sans recréer de transport spécifique. |
| Coût de navigation à plus de 100 habitants | Moyen | Conserver le test 20 minutes et profiler à nouveau dans `M1-PERF-01`. |
| Accumulation d'assets hétérogènes | Moyen | Conserver l'adaptation sous `Assets/CityLabHost/Adapted` et auditer chaque source. |
| Dérivés Store non traçables ou redistribuables | Élevé | Provenance, licence, hashes d'entrée, workbench non publié et sortie uniquement intégrée au jeu. |
| Pipeline conversationnel non reproductible | Élevé | Blender headless et recettes versionnées sont la source de vérité ; MCP réservé à l'exploration. |
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
| 2026-08-12 | `META-AUTO-01` | Port complet de l'architecture ForgeHistory engagé : rôles Hermes/Codex/Cursor/Claude, harnais fail-closed, audits, décisions, ledger, CI, fusion et archivage automatisés. La tâche reste `ACTIVE` jusqu'au cycle distant de preuve. | 15/15 tests Python ; six workflows YAML chargés ; profil Hermes `citylab-local-orchestrator` exécuté localement. | Installer le runner, publier le bootstrap et obtenir une PR de preuve fusionnée/archivée automatiquement. |
| 2026-08-12 | `META-AUTO-01` | Boucle full-auto prouvée de bout en bout et passée à `DONE` : Hermes orchestre, Codex produit, Cursor audite, Claude challenge, la CI et le merge bot décident, puis l'audit est archivé. Trois cycles refusés restent enregistrés comme preuves fail-closed. | [run full-auto 31606929060](https://github.com/PLiagre/VictoriaCityLab/actions/runs/31606929060), [PR preuve #14](https://github.com/PLiagre/VictoriaCityLab/pull/14), [audit #15](https://github.com/PLiagre/VictoriaCityLab/pull/15), [archive #16](https://github.com/PLiagre/VictoriaCityLab/pull/16), [dashboard #18](https://github.com/PLiagre/VictoriaCityLab/pull/18), ledger à `AUDIT_ARCHIVED`. | `M3-BUILD-01` redevient `ACTIVE` et son incrément reste `EN_COURS`. |
| 2026-07-31 | `M0-*` | Vertical slice forêt/construction validé ; `M0` passe à `DONE`, `M1-SAVE-01` devient `ACTIVE`. | `Docs/VALIDATION.md`, build Windows et capture player. | Concevoir et tester la sauvegarde versionnée. |
| 2026-07-31 | `META-ROADMAP-01` | Roadmap 1.0, règles de session et vérificateur de démarrage ajoutés. | `CITYLAB_ROADMAP_OK`, `git diff --check`. | `M1-SAVE-01`. |
| 2026-07-31 | `META-REPO-01` | Projet publié sur GitHub ; `main` configurée pour suivre `origin/main`. | Commit `8a4b728`, push Git et 790 objets LFS transférés. | `M1-SAVE-01`. |
| 2026-08-01 | `M1-ASSET-01`, `M1-ASSET-02` | Asset Factory fusionnée au périmètre CityLab : doctor Blender, inventaire déterministe, gouvernance hors Unity et première recette maison à six composants. La priorité utilisateur décale temporairement `M1-SAVE-01`. | Tests Python 2/2 ; Blender 5.2.0 LTS ; 728 modèles/55 textures ; inventaire et recette verts ; aucun lancement Unity. | Finaliser l'admission et le dry-run de publication de `M1-ASSET-02`. |
| 2026-08-01 | `M1-ASSET-03` | Scierie dark-fantasy générée après trois passes visuelles, publiée sous `Assets/CityLabHost/Adapted/Factory` et raccordée au catalogue CityLab avec fallback. L'import, la compilation et la validation en jeu restent différés à la prochaine session Unity autorisée. | 39 980 / 19 990 / 7 995 triangles ; hash mesh `5992f52d...`; GLB identique à graine constante ; FBX publié `6e51c333...` ; trois previews inspectées ; aucun lancement Unity. | Générer la maison et le grenier de `M1-ASSET-03`, puis contrôler le prefab en Unity lorsqu'il sera disponible. |
| 2026-08-01 | `M1-ASSET-03` | Le pilote devient une famille procédurale A/B/C. Chaque variante conserve la finition approuvée et expose base, ossature, toiture et détails en couches cumulatives. Le contrat est généralisé à toutes les recettes de bâtiment et raccordé au progrès déterministe de construction. | 3 tests Python ; 3 variantes GLB bit-identiques à graine constante ; 12 meshes FBX validés par variante ; LOD0 de 39 980 à 41 952 triangles ; copies publiées conformes ; aucune ouverture du projet Unity CityLab. | Appliquer le même contrat à la maison puis au grenier et valider l'import/runtime lors d'une session CityLab autorisée. |
| 2026-08-01 | `M1-ASSET-03`, `M1-ASSET-06`, `M1-CHAR-01` | Le kit couvre désormais huit familles : scierie, résidence, grenier, entrepôt, marché, forge, grange et chapelle. Les sept nouvelles familles sont publiées en A/B/C avec le contrat cumulatif. Le catalogue de population définit genres, âges, morphologies, coiffures et huit rôles sociaux ; l'audit distingue les composants réellement disponibles des corps encore à produire. | 24 FBX publiés, 12 meshes chacun ; 21/21 nouvelles variantes sous budgets et déterministes ; manifest `building_pilot.json` ; audit GanzSe : 217 FBX, 25 cheveux et 18 pièces par catégorie de tenue ; aucun lancement Unity CityLab. | Importer et inspecter le pilote en Unity autorisé, puis générer et valider les morphologies de `M1-CHAR-01`. |
| 2026-08-01 | `M1-ASSET-06`, `M1-CHAR-01` | Retour artistique intégré : les sept familles possèdent désormais quatre murs fermés, des systèmes constructifs différenciés et davantage de décor fonctionnel. Huit propositions de population combinent les pièces GanzSe par rôle et exposent honnêtement les silhouettes encore manquantes. | 21/21 FBX sous 60k/30k/12k, 12 meshes chacun, GLB bit-identiques après régénération ; hashes publiés conformes ; 8 rendus personnages et rapport de revue ; aucun lancement Unity. | Faire approuver la passe bâtiment, puis produire le set partagé de six corps et les vêtements civils/religieux/mendiant. |
| 2026-08-01 | `M1-CHAR-01` | Production population effectuée hors Unity : six bases genre/âge déclinées en quatre morphologies et huit capsules sociales lisibles, toutes sur le rig GanzSe commun. Les FBX sont publiés dans la frontière Adapted ; l'importeur Humanoid, les prefabs LOD et la sélection déterministe dans CityLab sont préparés mais non exécutés. | 24 corps + 8 rôles, 52 os, 3 LOD ; 32/32 validations FBX avec couverture skin 100 % ; déterminisme canonique 32/32 ; 6 tests Python ; manifest `character_factory.json` ; aucun lancement Unity. | Revue artistique, puis import/animation/clipping et raccord `CityVisualLibrary` pendant une session Unity autorisée. |
| 2026-08-01 | `M1-ASSET-02`, `M1-ASSET-04`, `M1-ASSET-05` | Admission Vendor terminée, premier trim PBR cohérent publié et QA Factory transversale passée hors Unity. Les accessoires de personnages ont reçu des UV déterministes après détection par la QA. | 10/10 tests Python ; 56 FBX et 1 038/1 038 meshes avec UV ; 6 cartes 2048² ; 0 collider ; publication dry-run/atomique testée ; planche `factory_review_board.png` ; aucun lancement Unity. | Approbation artistique de la planche, puis `M1-SAVE-01` tant que les portes Unity sont différées. |
| 2026-08-01 | `M1-SAVE-01` | Persistance versionnée préparée sans ouvrir Unity : sauvegarde manuelle F5, autosave atomique, chargement F9 après checksum, refus propre des corruptions et migration v0 vers v1. La tâche reste `NEXT` jusqu'à l'exécution des tests Unity. | `CitySaveService.cs` compilé avec les références Unity 6 sans lancer l'éditeur ; fixture v0/hash vérifiée ; 4 tests Editor ajoutés ; suite Python 11/11 ; `Docs/SAVE_SCHEMA.md`. | Exécuter les 4 tests EditMode et un aller-retour player lors d'une session CityLab autorisée. |
| 2026-08-01 | `M1-SAVE-01`, `M1-ASSET-05`, `M1-ASSET-06`, `M1-CHAR-01` | La sauvegarde versionnée est validée de bout en bout et passe à `DONE`. Les 24 bâtiments et 8 rôles ont été importés dans Unity ; une régression de double LOD et un script de camp non sérialisable ont été corrigés. La capture player hors écran est désormais fiable. | 19/19 EditMode, 1/1 PlayMode, `CITYLAB_SAVE_RUNTIME_OK`, build 308 751 907 octets, smoke 600 frames à 60 FPS/p95 16,683 ms, capture `m1-factory-runtime-20260801.png`, zéro script manquant et zéro double LOD. | Approbation artistique `M1-ASSET-05`, puis revue clipping `M1-CHAR-01` et raccord data-driven `M1-ASSET-06`/`M1-DATA-01`. |
| 2026-08-01 | `M1-CHAR-01`, `M1-ASSET-06`, `M1-DATA-01` | Revue animation achevée ; les huit fonctions de bâtiment sont constructibles et leurs effets sont publiés par un catalogue JSON validé. L'approbation humaine de `M1-ASSET-05` est isolée en `BLOCKED` sans bloquer la suite, conformément à l'instruction utilisateur. | 23/23 Editor, 1/1 PlayMode, `CITYLAB_CHARACTER_REVIEW_OK`, `CITYLAB_BUILDING_REVIEW_OK`, build 308 770 643 octets, trois captures personnages et capture huit fonctions. | `M1-NAV-01` devient `ACTIVE`. |
| 2026-08-01 | `M1-NAV-01`, `M1-JOBS-01` | Les déplacements directs sont remplacés par un A* déterministe avec récupération et NavMesh Unity actualisé. Les emplois deviennent exclusifs et physiques, avec horaires, trajet domicile-travail, absences et remplacements stables, sans double comptage des bûcherons. | 30/30 Editor, 1/1 PlayMode, stress 100 habitants/20 minutes sans échec, rechargement emploi identique, build 308 781 075 octets, smoke et revue huit fonctions verts. | `M1-LOG-01` devient `ACTIVE`. |
| 2026-08-02 | `M1-LOG-01` | Le transport du bois repose désormais sur des tâches logistiques persistantes et indépendantes des chantiers, avec priorité, extrémités stock/bâtiment/site, réservation concurrente et annulation conservant la ressource. | 4/4 tests logistiques dédiés ; 34/34 EditMode et 1/1 PlayMode ; `Logs/editmode-m1-log-final-20260802.xml`, `Logs/playmode-m1-log-20260802.xml`. | `M1-TIME-01` devient `ACTIVE`. |
| 2026-08-02 | `M1-TIME-01` | Le snapshot porte un calendrier année/mois/jour/heure, quatre saisons, la pause, la vitesse de reprise et une file d'événements datés déclenchés dans un ordre stable. Le HUD consomme ce calendrier. | 3/3 tests ciblés ; 37/37 EditMode et 1/1 PlayMode ; `Logs/editmode-m1-time-final-20260802.xml`, `Logs/playmode-m1-time-20260802.xml`. | `M1-TEST-01` devient `ACTIVE`. |
| 2026-08-02 | `M1-TEST-01` | Un harnais sans GameObject exécute la simulation de référence pendant 30 jours, contrôle chaque tick les inventaires et les déplacements, puis compare deux snapshots et un hash épinglé. Il a détecté et fait corriger deux blocages logistique/chantier. | `CITYLAB_LONG_RUN_OK`, graine 140001, 35 989 ticks, hash courant `2b9da7af...62c0ab`, min 0, navigation 0, bloqués 0 ; 42/42 Editor et 1/1 PlayMode. | `M1-PERF-01` devient `ACTIVE`. |
| 2026-08-02 | `M1-PERF-01`, `M1` | Le profil player strict atteint le budget avec 100 habitants et 1 800 frames ; la boucle centrale ne provoque aucune collecte gen0 sur une journée mesurée. Toutes les portes fonctionnelles M1 sont vertes ; la revue artistique humaine bloquée reste isolée. | Build 308 796 131 octets ; 60,1 FPS, p95 16,650 ms ; 39/39 Editor et 1/1 PlayMode ; `CITYLAB_PERF_OK`, `CITYLAB_CORE_ALLOC_OK`. | `M2-RES-01` devient `ACTIVE`. |
| 2026-08-02 | `M2-RES-01` | Le snapshot dispose d'un registre complet bois/planches/pierre/nourriture/outils/textile, de capacités explicites, de réservations atomiques et de pertes journalières entières. Le miroir bois préserve la compatibilité du vertical slice. | 3/3 tests registre ; 42/42 Editor et 1/1 PlayMode ; hash 30 jours `2b9da7af...62c0ab`. | `M2-FOOD-01` devient `ACTIVE`. |
| 2026-08-02 | `M2-FOOD-01` | Des cueilleurs et chasseurs rejoignent physiquement des sources accessibles, rapportent les rations au stock et les foyers consomment chaque jour ou accumulent une pénurie persistante. | 3/3 tests alimentation ; 45/45 Editor et 1/1 PlayMode ; hash 30 jours `1691010e...2c6ec`. | `M2-FARM-01` devient `ACTIVE`. |
| 2026-08-02 | `M2-FARM-01` | Deux champs persistants suivent fertilité, labour, semis, croissance, récolte et jachère ; la météo quotidienne déterministe modifie la croissance et le HUD expose saison, météo et récoltes. | 3/3 tests agriculture ; 48/48 Editor et 1/1 PlayMode ; 35 989 ticks, hash 30 jours `b2ea7cbf...a81c6c`, minimum 0, navigation 0, bloqués 0. | `M2-CHAIN-01` devient `ACTIVE`. |
| 2026-08-02 | `M2-CHAIN-01` | Le transport est réellement générique pour les six ressources. Sept ateliers persistants exécutent leurs recettes, conservent entrées/sorties locales et créent des tâches physiques stock↔atelier ; leurs variantes visuelles sont instanciées dans le monde. | 3/3 tests chaînes ; 51/51 Editor et 1/1 PlayMode ; 35 989 ticks, hash `dc731cc9...726292`, minimum 0, navigation 0, bloqués 0. | `M2-STOCK-01` devient `ACTIVE`. |
| 2026-08-02 | `M2-STOCK-01` | Greniers et entrepôts possèdent des inventaires locaux catégorisés, un rayon de service et une capacité totale non doublée ; leurs employés déclenchent puis réalisent les transports de remplissage et de rééquilibrage. | 3/3 tests stockage ; 54/54 Editor et 1/1 PlayMode ; hash 30 jours `20cd6069...cbca08`, minimum 0, navigation 0, bloqués 0. | `M2-MARKET-01` devient `ACTIVE`. |
| 2026-08-02 | `M2-MARKET-01` | Les marchés couverts de commerçants créent des transports depuis les dépôts, stockent nourriture/outils/textile et publient couverture, rareté, prix et jours de pénurie ; les foyers couverts consomment aux étals. | 3/3 tests marché ; 57/57 Editor et 1/1 PlayMode ; hash 30 jours `da88d68d...c08e8c`, minimum 0, navigation 0, bloqués 0. | `M2-HOME-01` devient `ACTIVE`. |
| 2026-08-02 | `M2-HOME-01` | Chaque foyer consomme cinq besoins à cadence déterministe ; disponibilité et logement produisent satisfaction, niveau et compteurs de pénurie persistants visibles au HUD. Une consommation de combustible trop fréquente détectée par la régression de conservation a été corrigée en cadence mensuelle. | 3/3 tests besoins ; 60/60 Editor et 1/1 PlayMode ; 13/13 tests historiques simulation ; hash 30 jours `ed5a517e...18461d`. | `M2-TRADE-01` devient `ACTIVE`. |
| 2026-08-02 | `M2-TRADE-01`, `M2` | Import/export avec réservation, frais, délais, marchand et limites validé. La porte M2 tient 60 jours, soit deux heures de jeu simulé, puis build, smoke et profil graphique 100 habitants restent verts ; M2 passe à `DONE`. | 64/64 Editor, 1/1 PlayMode ; 71 954 ticks/60 jours, hash `dd38c163...daffcf3` ; build 308 826 739 octets ; 60,0 FPS, p95 16,683 ms, GC p95 0. | `M3-PLOT-01` devient `ACTIVE`. |
| 2026-08-03 | `M3-PLOT-01` | Le zoning crée des parcelles orientées, variables et non superposées ; cinq échantillons filtrent les terrains à plus de 180 ‰. Les maisons occupent la façade, les jardins s'activent à l'achèvement et les foyers établis/prospères gagnent une/deux extensions dans la capacité du lot. Le scénario automatique a été réespacé pour respecter les nouvelles profondeurs. | 3/3 ciblés, 67/67 Editor, 1/1 PlayMode ; hash 30 jours `8caf8646...1a5602` ; build 308 831 859 octets ; smoke 20/30/30, sauvegarde runtime et `CITYLAB_PERF_OK`. | `M3-BUILD-01` devient `ACTIVE`. |
| 2026-08-03 | `M3-BUILD-01` | Premier incrément physique : chaque nouveau chantier nivelle son emprise avant de demander ses ressources ; les bâtiments civiques consomment pierre, bois, planches et outils dans l'ordre des phases. Les bâtisseurs privilégient leur propre chantier, l'état passe le checksum/reload et le HUD/monde distinguent le terrassement. | 3/3 ciblés, 70/70 Editor, 1/1 PlayMode ; 35 989 ticks/hash `f5c411a9...753a82`, 60 jours/hash `2dc2bdb1...5541e8` ; build 308 836 067 octets ; smoke 20/30/30, sauvegarde et performance verts. | Poursuivre `M3-BUILD-01` avec échafaudages, réparation et démolition. |
