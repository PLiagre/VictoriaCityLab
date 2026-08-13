# Roadmap de production Victoria CityLab

<!-- CITYLAB_ROADMAP
schema: 1
last_updated: 2026-08-13
active_milestone: M3
roadmap_status: ACTIVE
-->

Ce document est la source de vérité du développement de CityLab. Le produit
cible n'est plus un jeu autonome : CityLab doit devenir la vue **ville jouable**
ouverte depuis la carte principale de ForgeHistory. Le dépôt autonome reste un
laboratoire, un harnais de validation et la source du package de présentation ;
le runtime de production est `PLiagre/ForgeHistory/unity/game_unity`.

ForgeHistory est la source de vérité du monde, du temps, de la simulation et de
la sauvegarde. CityLab rend cet état, collecte des intentions joueur et présente
leurs résultats ; il ne possède jamais une seconde simulation de production. Le
dépôt ForgeHistory est une dépendance amont en lecture seule pour CityLab.
L'audit initial est épinglé à `PLiagre/ForgeHistory@268e8aab151452b0c740a44a7cc97ca3fd37e311`
(`master`, 13 août 2026). Toute évolution nécessaire côté ForgeHistory doit
faire l'objet d'une demande à son propriétaire Hermes, jamais d'une modification
implicite depuis ce dépôt.

Les deux projets utilisent Unity `6000.0.43f1`, ce qui rend le portage de code
possible sans upgrade moteur. En revanche, le chargement de scènes, le pipeline
de rendu, les packages, l'autorité de simulation, les identifiants, l'horloge et
la persistance doivent converger avant tout ajout majeur de gameplay. La file
« Sessions Codex ordonnées » place désormais cette intégration avant la suite
des fonctionnalités locales.

Le projet actuel reste un vertical slice autonome jouable et validé. Il sert de
preuve et de banc d'essai, pas de second jeu ni de seconde source de vérité.

## État de pilotage

| Champ | Valeur |
|---|---|
| Dernière mise à jour | 13 août 2026 |
| Jalon actif | `M3` — intégration ForgeHistory et première ville chargeable |
| Dernier jalon validé | `M2` — économie de village jouable dans le laboratoire autonome |
| Priorité immédiate | `M3-FH-01` — contrat d'autorité, de chargement et de portage |
| Prochaine tâche prête | `M3-FH-02` — découpage package/hôte et bootstrap explicite |
| Cible produit | Vue ville jouable de ForgeHistory, issue de sa carte principale et de sa simulation unique |
| Hôte de production | `PLiagre/ForgeHistory/unity/game_unity` — dépendance amont en lecture seule |
| Hôte laboratoire | `PLiagre/VictoriaCityLab` — branche `main` |
| Build de référence actuel | `Builds/Windows/VictoriaCityLab.exe` — preuve autonome, non build intégré |
| Preuves de référence | `Docs/VALIDATION.md`, `Logs/` et futurs tests de contrat d'intégration |

### États autorisés

- `DONE` : critères d'acceptation remplis et preuve locale enregistrée ;
- `ACTIVE` : travail prioritaire actuellement autorisé ;
- `NEXT` : prochain travail ordonné, prêt à démarrer ;
- `BACKLOG` : nécessaire à la version 1.0 mais non prioritaire ;
- `BLOCKED` : dépendance explicite renseignée dans la ligne concernée.

Une tâche ne passe jamais à `DONE` sur la seule base d'une impression visuelle
ou d'une compilation réussie. Sa colonne « Sortie / preuve » doit être satisfaite.

## Définition de la version 1.0

La version 1.0 de CityLab est atteinte uniquement si les conditions suivantes
sont toutes remplies dans le build ForgeHistory :

- depuis `Assets/Scenes/Main.unity`, le joueur sélectionne une ville réelle de
  la carte puis choisit « Entrer dans la ville » ;
- le chargement asynchrone affiche progression, erreur et possibilité de retour,
  sans écran figé ni création implicite du runtime CityLab dans la scène carte ;
- la ville ouverte est identifiée par le `city_id` ForgeHistory, son
  `cell_id`/territoire parent, le tick monde, la révision d'état et une graine
  dérivée du monde ; aucun identifiant `1001` n'est codé en dur en production ;
- la simulation ForgeHistory est l'unique autorité : CityLab consomme des
  snapshots et émet des intentions versionnées, corrélées, idempotentes et
  explicitement acceptées ou refusées ;
- `LocalCitySimulation`, les fixtures et `CitySaveService` ne servent qu'au
  laboratoire et aux tests ; le build intégré ne lance ni horloge, ni autosave,
  ni économie parallèle ;
- l'entrée et la sortie de la ville conservent exactement le viewport, la
  sélection, le temps monde et les conséquences économiques ; le retour à la
  carte reflète les changements validés par le backend ;
- agrégation et désagrégation ville/monde sont conservatives : personnes,
  familles, stocks, bâtiments, emplois et ordres ne sont ni créés ni perdus par
  un changement de vue ;
- une politique explicite décide si le monde continue, ralentit ou se met en
  pause pendant la vue ville ; une seule horloge applique cette décision ;
- la sauvegarde/charge ForgeHistory restaure carte et ville au même tick et à la
  même révision, avec migrations, checksum et refus propre des données
  incompatibles ;
- le portage préserve les GUID et licences, ne référence aucun prefab Vendor
  direct et résout les écarts de packages/rendu sans régression de la carte
  principale ;
- cinquante cycles carte→ville→carte et un soak de deux heures passent sans fuite
  mémoire croissante, double bootstrap, état dupliqué ni corruption ;
- une ville de 250 habitants tient 60 FPS en 1080p avec un p95 CPU inférieur à
  16,7 ms sur la machine cible ; 500 habitants reste la porte d'optimisation
  finale ;
- tutoriel d'entrée/sortie, remapping, accessibilité, français/anglais, gestion
  d'erreur, crédits et licences sont validés dans le build intégré ;
- zéro erreur bloquante connue, zéro corruption connue et toutes les régressions
  carte, simulation, transition et ville sont vertes.

## Cibles d'ambition premium

Ces cibles guident CityLab sans lui redonner l'autorité du monde :

- plusieurs villes data-driven, ouvertes depuis leurs marqueurs réels, avec
  terrain, ressources, population, richesse et culture dérivés de ForgeHistory ;
- ville lisible aux trois zooms, 70 bâtiments ou évolutions fonctionnelles et
  30 chaînes à terme, sans compter les variantes purement cosmétiques ;
- 500 habitants comme porte ferme de livraison et 800 à 1 000 comme cible
  d'optimisation non bloquante si le budget matériel le permet ;
- chargement froid inférieur à 10 s, retour carte inférieur à 5 s et transition
  chaude inférieure à 3 s sur la machine cible, budgets à confirmer par le
  prototype d'intégration ;
- contenus visuels partitionnés par socle commun, culture/biome et ville, avec
  cache borné et libération mesurée ;
- conséquences des décisions, crises et guerres ForgeHistory visibles dans la
  ville, sans implémenter dans CityLab une seconde diplomatie, IA stratégique ou
  bataille ;
- direction artistique originale, bâtiments héroïques, matériaux cohérents,
  population variée, animations contextuelles, VFX et paysage sonore sans
  placeholder de production.

## Piliers non négociables

1. **Une simulation, plusieurs vues** — ForgeHistory possède le monde ; carte,
   ville, quartier et bataille observent les mêmes identités et le même tick.
2. **Unity client mince** — CityLab rend, anime et collecte des intentions. La
   logique métier de production vit dans le backend ForgeHistory et fonctionne
   sans Unity.
3. **Transition jouable et réversible** — charger, entrer, jouer, sortir et
   reprendre la carte est un parcours testé, mesuré et récupérable.
4. **Ville physique et lisible** — routes, parcelles, chantiers, ressources,
   familles et saisons racontent l'état validé par le backend dans le monde.
5. **LOD conservatif et persistance unique** — aucun changement de vue ne
   duplique ni ne détruit l'état ; la sauvegarde ForgeHistory est autoritaire.
6. **Portage explicite** — bootstrap, dépendances, rendu, assets, protocoles et
   budgets sont décidés et prouvés ; aucun couplage implicite entre deux projets.
7. **Qualité prouvée** — aucune fonctionnalité ne passe à `DONE` sans preuve
   automatique et, lorsqu'elle est visuelle ou ergonomique, sans revue player.

## Jalons

| ID | Jalon | État | Porte de sortie |
|---|---|---|---|
| `M0` | Vertical slice forêt et construction | DONE | Routes, parcelles, maisons, transport du bois, camp forestier, HUD, build Windows et tests validés dans le laboratoire. |
| `M1` | Fondations de production | DONE | Sauvegarde de laboratoire, données versionnées, navigation, emplois physiques et tests déterministes. |
| `M2` | Économie de village jouable | DONE | Six ressources, agriculture, sept chaînes, stockage, marché, besoins et commerce validés dans le laboratoire. |
| `M3` | Intégration ForgeHistory et première ville | ACTIVE | Contrats d'autorité, package portable, rendu compatible, transition asynchrone et premier aller-retour carte→ville→carte. |
| `M4` | Synchronisation, LOD et multi-ville | BACKLOG | Agrégation conservative, sauvegarde monde, streaming borné et plusieurs villes sans identifiant codé en dur. |
| `M5` | Société urbaine intégrée | BACKLOG | Familles, santé, foi, ordre, fiscalité et catastrophes pilotés par le backend et lisibles dans la vue ville. |
| `M6` | Parcours ForgeHistory de bout en bout | BACKLOG | Partie intégrée de huit heures avec transitions, reprise, erreurs récupérables et aucune divergence d'état. |
| `M7` | Contenu et qualité de production | BACKLOG | Art, audio, VFX, animations, variations et UX sans placeholders dans l'hôte ForgeHistory. |
| `M8` | Performance et QA intégrées | BACKLOG | 500 habitants, budgets de chargement/mémoire, soak et régressions carte/ville verts. |
| `M9` | Release Candidate City Mode | BACKLOG | Accessibilité, localisation, packaging ForgeHistory et critères 1.0 remplis. |

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
| `META-CODEX-01` | Plan de production exécutable par sessions Codex | DONE | Cible premium, contrat de session, file ordonnée et prompt de lancement consignés ; `AGENTS.md` sélectionne l'unique incrément `EN_COURS` ; le vérificateur contrôle ordre, définition et concordance avec la tâche active ; `CITYLAB_ROADMAP_OK` et `git diff --check`. |
| `META-AUTO-01` | Architecture full-auto adaptée de ForgeHistory | DONE | Runner `citylab-full-auto-pe` en ligne ; run #31606929060 ; PR #14 auditée `PASS` puis fusionnée automatiquement ; audit #15, archive terminale #16 et dashboard Hermes #18 ; 20/20 tests Python et trois cycles refusés conservés comme preuves fail-closed. |

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

## M3 — intégration ForgeHistory et première ville

| ID | Travail | État | Critères d'acceptation | Sortie / preuve minimale |
|---|---|---|---|---|
| `M3-FH-01` | Contrat d'autorité, de chargement et de portage | ACTIVE | Épingler le commit ForgeHistory audité ; cartographier carte, sélection de ville, scène, packages, rendu, données, horloge, commandes, sauvegarde et LOD ; définir `CityLaunchContext`, snapshot, intention/accusé, révision, erreurs et propriétaire de chaque donnée ; consigner les changements amont comme demandes Hermes sans écrire dans ForgeHistory. | Document d'architecture versionné, matrice d'autorité exhaustive, schémas JSON/C# versionnés, tests de contrat rouge/vert hors Unity et aucune ambiguïté sur l'autorité. |
| `M3-FH-02` | Package portable et bootstrap explicite | NEXT | Séparer contrats, présentation, assets et adaptateur de laboratoire ; supprimer le bootstrap global `AfterSceneLoad` du chemin de production ; l'hôte crée et détruit explicitement une instance par contexte de ville ; aucune dépendance à une scène ou fixture CityLab. | Import du package dans un hôte minimal, tests zéro auto-démarrage/double instance, API publique documentée et laboratoire autonome toujours vert. |
| `M3-FH-03` | Convergence rendu et dépendances | BACKLOG | Comparer Built-in actuel de ForgeHistory et URP CityLab ; choisir une stratégie unique après prototype ; résoudre Input System, AI Navigation, Entities/Burst/Collections et shaders sans casser la carte. | Matrice de packages, build test, captures dorées carte avant/après, capture ville sans shader rose et profil CPU/GPU. |
| `M3-FH-04` | Shell de transition asynchrone | BACKLOG | Depuis un hôte miroir de `Main.unity`, sélectionner une ville puis charger la scène ville de façon asynchrone ; progression, timeout, annulation, erreur et retour ; préserver viewport/sélection et empêcher les entrées concurrentes. | Tests PlayMode succès/annulation/échec/double clic, 50 transitions, budgets froid/chaud/retour et mémoire enregistrés. |
| `M3-FH-05` | Adaptateur ForgeHistory snapshot/intention | BLOCKED | Remplacer `LocalCitySimulation` en production par un adaptateur au backend ForgeHistory ; commandes corrélées/idempotentes, ordre par tick/révision, refus explicites, reconnexion et resynchronisation. Dépend de la couche villes et du transport runtime décidés par Hermes dans ForgeHistory. | Tests de contrat avec backend factice puis réel, latence/ordre/perte simulés, aucune écriture métier Unity et demande amont ForgeHistory acceptée. |
| `M3-FH-06` | Portage des assets et catalogues | BACKLOG | Porter uniquement les assets approuvés avec GUID, LFS, provenance et licences ; retirer les `Resources.Load` structurants du chemin intégré ; partitionner socle/biome/ville et charger/libérer selon le budget mesuré. | Manifeste de portage, hash source→cible, build sans asset manquant, captures trois zooms et profil mémoire. |
| `M3-FH-07` | Première ville intégrée jouable | BLOCKED | Entrer depuis un marqueur ForgeHistory réel, recevoir l'état autoritaire, jouer une boucle construction/logistique via intentions, sortir et observer les conséquences sur la carte ; aucune sauvegarde ou horloge parallèle. | Parcours player enregistré, tests carte→ville→carte, restart/reload, hash d'état avant/après et régressions carte/sim/ville vertes. |
| `M3-PLOT-01` | Parcelles organiques de laboratoire | DONE | Preuve historique conservée ; le comportement de production devra être réémis par le backend ForgeHistory avant intégration. | 3/3 tests dédiés, 67/67 Editor, 1/1 PlayMode, build et smoke player verts. |
| `M3-BUILD-01` | Construction physique complète | BACKLOG | Échafaudages déjà prouvés ; usure, réparation et démolition reprennent après `M3-FH-01..07` et doivent être pilotées par l'autorité ForgeHistory, pas par une nouvelle logique locale. | Tests de contrat backend, conservation/reload monde, HUD, capture et régression intégrée. |
| `M3-ART-01` | Bible artistique et kit héroïque | BACKLOG | Bible compatible carte→ville, matériaux selon le pipeline choisi, silhouettes et budgets de streaming ; approbation humaine. | Document, planches, huit fonctions, captures dans l'hôte intégré et QA licences/LOD. |
| `M3-ART-02` | Environnement, population et animation de ville | BACKLOG | Variations, saisons et animations contextuelles cohérentes, chargées par catalogue et compatibles avec les budgets intégrés. | Audit de diversité, quatre saisons, profil et revue artistique humaine. |

## M4 — synchronisation, LOD et multi-ville

| ID | Travail | État | Critères d'acceptation | Sortie / preuve minimale |
|---|---|---|---|---|
| `M4-FH-LOD-01` | Agrégation/désagrégation conservative | BACKLOG | Personnes, foyers, bâtiments, stocks, emplois et ordres passent du LOD monde au LOD ville sans création, perte ni double comptage. | Invariants, hashes aller-retour multi-graines et soak de transitions. |
| `M4-FH-SYNC-01` | Synchronisation carte↔ville | BACKLOG | Les changements validés en ville apparaissent sur la carte au tick attendu ; changements monde concurrents resynchronisés sans écrasement silencieux. | Tests de concurrence/révision, latence simulée et capture avant/après. |
| `M4-FH-MULTI-01` | Plusieurs villes data-driven | BACKLOG | Aucun `city_id` codé en dur ; contexte, terrain, ressources, culture et population varient par ville réelle. | Trois villes contrastées, transitions croisées, reload et profils comparés. |
| `M4-FH-STREAM-01` | Streaming et cache bornés | BACKLOG | Socle partagé et contenus spécifiques chargés à la demande ; préchauffage, annulation et libération mesurés ; aucune fuite après 50 transitions. | Budgets temps/mémoire, profils froid/chaud et test d'endurance. |
| `M4-FH-SAVE-01` | Sauvegarde monde autoritaire | BACKLOG | La sauvegarde ForgeHistory inclut l'état urbain et le contexte de vue ; le save autonome CityLab reste une fixture de migration, jamais une sauvegarde parallèle en production. | Matrice de migrations, corruption refusée, round-trip carte/ville exact. |

### Capacités stratégiques transférées à ForgeHistory

Les identifiants historiques restent traçables mais ne sont plus des lots CityLab.
Ils ne peuvent être débloqués que par une décision de propriété explicite côté
ForgeHistory.

| ID | Ancien périmètre | État | Nouveau propriétaire |
|---|---|---|---|
| `M4-MAP-01` | Carte en plusieurs régions | BLOCKED | ForgeHistory carte/monde ; CityLab ne fournit que la vue ville et la transition. |
| `M4-CLAIM-01` | Influence et revendication | BLOCKED | ForgeHistory `sim/`. |
| `M4-TRADE-01` | Commerce régional | BLOCKED | ForgeHistory `sim/`. |
| `M4-AI-01` | Seigneurs IA | BLOCKED | ForgeHistory `sim/`. |
| `M4-DIPLO-01` | Diplomatie | BLOCKED | ForgeHistory `sim/`. |
| `M4-EVENT-01` | Événements et objectifs monde | BLOCKED | ForgeHistory `sim/` et campagne. |

## M5 — société urbaine intégrée

Les identifiants CityLab historiques sont conservés. Chaque lot exige désormais
un modèle autoritaire ForgeHistory, un adaptateur de contrat et une présentation
CityLab ; une implémentation uniquement dans `LocalCitySimulation` ne ferme
aucune tâche.

| ID | Travail | État | Critères d'acceptation | Sortie / preuve minimale |
|---|---|---|---|---|
| `M3-FAMILY-01` | Foyers et cycle de vie | BACKLOG | Identités monde, parenté, âges, migrations et compétences conservés entre LOD. | Simulation backend, tests de contrat/reload, profil 250 habitants et inspection ville. |
| `M3-HEALTH-01` | Santé, maladies et blessures | BACKLOG | Risques et soins issus du backend, lisibles sans décès arbitraire client. | Scénario backend 90 jours, resynchronisation et revue player. |
| `M3-FAITH-01` | Foi et sépulture | BACKLOG | Offices, capacité, sépultures et effets sociaux partagés avec le monde. | Tests calendrier/LOD/reload et capture intégrée. |
| `M3-ORDER-01` | Ordre et criminalité | BACKLOG | Délits et sanctions émergent de l'état monde ; conservation des biens et identités. | Scénario de crise, télémétrie, contrats et revue player. |
| `M3-TAX-01` | Fiscalité et trésor | BACKLOG | Livre de comptes commun avec ForgeHistory ; aucune monnaie locale parallèle. | Invariants monde/ville, simulation 180 jours et panneau inspecté. |
| `M3-FIRE-01` | Incendies et catastrophes | BACKLOG | Propagation, intervention, victimes et reconstruction persistent dans l'état monde. | Cycle complet, reload, retour carte et invariants de ressources. |

### Capacités militaires transférées à ForgeHistory

| ID | Ancien périmètre | État | Nouveau propriétaire |
|---|---|---|---|
| `M5-LEVY-01` | Levées | BLOCKED | ForgeHistory population/armées. |
| `M5-EQUIP-01` | Équipement | BLOCKED | ForgeHistory économie/armées. |
| `M5-RETINUE-01` | Suite professionnelle | BLOCKED | ForgeHistory armées. |
| `M5-FORM-01` | Formations | BLOCKED | ForgeHistory bataille tactique. |
| `M5-COMBAT-01` | Combat | BLOCKED | ForgeHistory bataille tactique. |
| `M5-AI-01` | IA tactique | BLOCKED | ForgeHistory bataille tactique. |
| `M5-CONSEQ-01` | Conséquences persistantes | BLOCKED | ForgeHistory, exposées ensuite par CityLab. |

## M6 à M9 — finition et sortie

| ID | Jalon | Domaine | État | Porte de sortie vérifiable |
|---|---|---|---|---|
| `M6-GAME-01` | M6 | Parcours intégré de bout en bout | BACKLOG | Huit heures depuis la carte ForgeHistory avec entrées/sorties de villes, crises, reprise et aucune divergence d'état. |
| `M6-ONBOARD-01` | M6 | Tutoriel carte→ville | BACKLOG | Un nouveau joueur entre, agit, comprend les retours backend et revient à la carte sans aide externe. |
| `M6-SAVE-01` | M6 | Robustesse de la sauvegarde monde | BACKLOG | Autosaves ForgeHistory, récupération, migrations ville/monde et zéro corruption sur la matrice QA. |
| `REL-CONTENT` | M7 | Contenu urbain | BACKLOG | Bâtiments, chaînes et variations utiles, data-driven et compatibles avec plusieurs villes ForgeHistory. |
| `REL-ART` | M7 | Environnements, architecture, personnages, UI et VFX | BACKLOG | Cohérence carte→ville, provenance/licence, LOD, occlusion, budgets et diversité validés. |
| `REL-ANIM` | M7 | Locomotion, métiers et construction | BACKLOG | Transitions, IK, variations et budgets CPU validés sur foule cible. |
| `REL-AUDIO` | M7 | Musique, ambiances et sound design | BACKLOG | Mix dynamique lors des transitions et en ville, options et variations validées. |
| `REL-UX` | M7 | Interface et lisibilité | BACKLOG | Actions critiques découvrables et retours backend explicites à 1080p/1440p/4K. |
| `REL-PERF` | M8 | 500 habitants et transitions | BACKLOG | 60 FPS, p95 < 16,7 ms, mémoire stable et budgets froid/chaud/retour tenus. |
| `M8-BALANCE-01` | M8 | Équilibrage observable | BACKLOG | Simulation équilibrée côté ForgeHistory et présentation CityLab sans règle compensatoire locale. |
| `REL-QA` | M8 | Régression intégrée | BACKLOG | Soak, 50 transitions, matrice graphique, sauvegardes et carte/ville sans critique ouvert. |
| `REL-ACCESS` | M9 | Accessibilité | BACKLOG | Remapping, navigation clavier, tailles UI, contrastes, daltonisme et réduction des effets validés. |
| `REL-LOC` | M9 | Français et anglais | BACKLOG | Aucun texte codé en dur, pluriels/variables et débordements validés. |
| `REL-SHIP` | M9 | Packaging ForgeHistory | BACKLOG | City Mode inclus dans le build ForgeHistory signé/reproductible avec crédits, licences, crash reporting et rollback. |

## Contrat d'une session Codex

Une session Codex de production doit livrer une tranche verticale, pas seulement
un squelette de code. Ce contrat complète `AGENTS.md` :

1. prendre la tâche `ACTIVE`, puis l'unique incrément `EN_COURS` ;
2. traiter ForgeHistory comme une dépendance amont en lecture seule ; toute
   évolution amont devient une demande Hermes documentée ;
3. distinguer explicitement code portable, présentation, adaptateur de
   laboratoire et contrat d'hôte ;
4. ne jamais ajouter de logique métier de production dans Unity : les fixtures
   et `LocalCitySimulation` peuvent prouver le protocole, pas devenir l'autorité ;
5. pour une intégration, livrer ensemble schéma versionné, gestion du cycle de
   vie, erreurs, annulation, observabilité, tests de contrat et budget mesuré ;
6. pour un asset, conserver source immuable, provenance, licence et SHA-256,
   publier seulement une variante approuvée puis valider import, rendu, LOD,
   mémoire et déchargement ;
7. exécuter tests ciblés puis régression proportionnée ; toute modification
   visuelle ou de transition exige une capture ou une vidéo player inspectable ;
8. ne jamais réduire silencieusement un critère ; conserver `EN_COURS` tant
   que la preuve manque, puis promouvoir l'incrément suivant ;
9. synchroniser roadmap, statut et validation uniquement avec des capacités
   réellement prouvées ; terminer par le contrôle de roadmap et
   `git diff --check`. Aucun commit, push, achat ou changement ForgeHistory
   n'est implicite.

### Prompt de lancement recommandé

```text
Continue Victoria CityLab depuis la roadmap. Respecte AGENTS.md et prends la
tâche ACTIVE puis l'unique incrément EN_COURS. La cible est une ville jouable de
ForgeHistory, chargée depuis sa carte principale. ForgeHistory reste en lecture
seule et possède simulation, horloge et sauvegarde ; CityLab est un client de
présentation et un laboratoire. Livre contrats versionnés, cycle de chargement,
adaptateur/mocks, tests et preuves sans créer de seconde autorité. Mets à jour
les documents uniquement selon ce qui est réellement validé.
```

## Sessions Codex ordonnées

Cette table est la file autoritaire. Le nouveau paradigme d'intégration précède
la suite des mécaniques locales. `BLOQUÉ` nomme une dépendance ForgeHistory
explicite ; il n'autorise aucune écriture amont implicite.

| Ordre | Suivi | Tâche | Incrément de session | Preuve de fermeture de l'incrément |
|---:|---|---|---|---|
| 01 | PROUVÉ | `M3-BUILD-01` | Échafaudages synchronisés aux quatre phases, sélection et reload dans le laboratoire. | 4/4 ciblés, 71/71 Editor, 1/1 PlayMode, build/smoke et capture player. |
| 02 | EN_COURS | `M3-FH-01` | Audit lecture seule ForgeHistory et contrat d'autorité/chargement/portage épinglé au commit observé. | Matrice exhaustive, schémas versionnés, décisions d'autorité et tests de contrat initiaux. |
| 03 | À_FAIRE | `M3-FH-02` | Découper package, présentation et adaptateur laboratoire ; remplacer le bootstrap global par un démarrage hôte explicite. | Hôte minimal, zéro auto-démarrage/double instance, laboratoire vert. |
| 04 | À_FAIRE | `M3-FH-03` | Prototype de convergence Built-in/URP et matrice de packages Unity. | Captures dorées carte/ville, build et profil sans shader cassé. |
| 05 | À_FAIRE | `M3-FH-04` | Transition asynchrone carte→ville→carte avec progression, annulation, erreur et restauration du viewport. | PlayMode, 50 transitions et budgets froid/chaud/retour. |
| 06 | BLOQUÉ | `M3-FH-05` | Adaptateur snapshot/intention vers le backend ForgeHistory. | Dépendance : contrat runtime et couche villes acceptés par Hermes côté ForgeHistory. |
| 07 | À_FAIRE | `M3-FH-06` | Manifeste de portage assets/catalogues et chargement borné. | Hashes, licences, build, captures et profil mémoire. |
| 08 | BLOQUÉ | `M3-FH-07` | Première ville réelle jouable depuis un marqueur ForgeHistory. | Dépend de 03–07 et d'un hôte ForgeHistory modifiable par son propriétaire. |
| 09 | À_FAIRE | `M3-BUILD-01` | Reprendre usure, panne et réparation via le backend autoritaire. | Contrats, conservation/reload monde, HUD et capture intégrée. |
| 10 | À_FAIRE | `M3-BUILD-01` | Démolition progressive et récupération bornée via intentions. | Concurrence, retour carte, régression complète et fermeture de la tâche. |
| 11 | À_FAIRE | `M3-ART-01` | Bible visuelle carte→ville et kit héroïque compatible pipeline choisi. | Planche, approbation humaine, QA et captures intégrées. |
| 12 | À_FAIRE | `M4-FH-LOD-01` | Agrégation/désagrégation conservative ville↔monde. | Hashes aller-retour et invariants multi-graines. |
| 13 | À_FAIRE | `M4-FH-SYNC-01` | Synchronisation révisionnée des changements concurrents. | Tests latence/ordre/conflit et capture carte/ville. |
| 14 | À_FAIRE | `M4-FH-MULTI-01` | Trois villes data-driven sans identifiant codé en dur. | Transitions croisées, reload et profils. |
| 15 | À_FAIRE | `M4-FH-STREAM-01` | Streaming/cache bornés pour socle, biome et ville. | 50 transitions, mémoire stable et budgets. |
| 16 | À_FAIRE | `M4-FH-SAVE-01` | Sauvegarde ForgeHistory autoritaire incluant le contexte de vue. | Migrations et round-trip exact carte/ville. |
| 17 | À_FAIRE | `M3-FAMILY-01` | Familles et cycle de vie partagés entre LOD. | Backend, contrats, reload et profil 250 habitants. |
| 18 | À_FAIRE | `M3-HEALTH-01` | Santé et soins autoritaires, présentation ville. | Scénario 90 jours et revue player. |
| 19 | À_FAIRE | `M3-FAITH-01` | Foi, offices et sépultures partagés avec le monde. | Tests calendrier/LOD et capture. |
| 20 | À_FAIRE | `M3-ORDER-01` | Ordre et criminalité sans règle compensatoire client. | Scénario, télémétrie et contrats. |
| 21 | À_FAIRE | `M3-TAX-01` | Fiscalité et trésor communs avec ForgeHistory. | Livre conservatif et panneau inspecté. |
| 22 | À_FAIRE | `M3-FIRE-01` | Incendie, intervention et reconstruction persistants. | Cycle complet et retour carte. |
| 23 | À_FAIRE | `M3-ART-02` | Variations, saisons et animations de ville. | Audit de diversité, profil et revue humaine. |
| 24 | À_FAIRE | `M6-GAME-01` | Parcours intégré de huit heures. | QA sans blocage ni divergence. |
| 25 | À_FAIRE | `M6-ONBOARD-01` | Tutoriel carte→ville et retours backend. | Tests utilisateurs. |
| 26 | À_FAIRE | `M6-SAVE-01` | Autosaves/migrations monde et récupération. | Matrice zéro corruption. |
| 27 | À_FAIRE | `REL-CONTENT` | Contenu urbain multi-ville. | Catalogue, équilibrage et revue player. |
| 28 | À_FAIRE | `REL-ART` | Art final carte→ville. | Revue trois zooms/quatre saisons. |
| 29 | À_FAIRE | `REL-ANIM` | Animations de production. | Transitions/IK/variations et profil. |
| 30 | À_FAIRE | `REL-AUDIO` | Audio de transition et de ville. | Mix long et options. |
| 31 | À_FAIRE | `REL-UX` | UX finale et erreurs récupérables. | Résolutions et tailles UI. |
| 32 | À_FAIRE | `REL-PERF` | 500 habitants et budgets de transition. | 60 FPS, p95, mémoire et chargements. |
| 33 | À_FAIRE | `M8-BALANCE-01` | Équilibrage backend observable. | Parties automatiques/humaines sans compensation locale. |
| 34 | À_FAIRE | `REL-QA` | Soak et régression intégrée. | Zéro critique/majeur. |
| 35 | À_FAIRE | `REL-ACCESS` | Accessibilité complète. | Checklist et tests utilisateurs. |
| 36 | À_FAIRE | `REL-LOC` | Français et anglais. | Pseudo-localisation et relecture. |
| 37 | À_FAIRE | `REL-SHIP` | Packaging City Mode dans ForgeHistory. | RC reproductible, licences et rollback. |

## Stratégie d'assets 3D de qualité

La production visuelle suit un modèle hybride : bases commerciales ou libres
strictement licenciées pour les éléments génériques, adaptation systématique par
l'Asset Factory, et création/commande sur mesure pour les silhouettes héroïques.

- **Générique adaptable** : végétation, rochers, petits accessoires, outils et
  matériaux sources peuvent venir de packs admis, sans intégration directe.
- **Identité CityLab** : architecture, personnages sociaux, héraldique, UI,
  machines, chantiers, dégâts et éléments vus de près utilisent la bible
  `M3-ART-01` et des variantes originales sous `Adapted`.
- **Sur mesure prioritaire** : manoir, chapelle, marché, portes, soldats,
  personnages remarquables, VFX signature et iconographie reçoivent le budget
  humain en premier. Codex ne déclenche jamais un achat ou une commande sans
  autorisation explicite de l'utilisateur.
- **IA générative** : autorisée pour concepts, moodboards, masques et blockouts
  dont les droits sont vérifiés ; aucun mesh final n'est admis sans les mêmes
  portes de topologie, UV, rig, LOD, provenance et revue que toute autre source.
- **Revue** : chaque famille possède une planche avant/après, une vue proche,
  ville et carte, puis une capture Unity éclairée. Une QA technique verte ne
  remplace pas l'approbation artistique humaine.

## Risques actifs

| Risque | Niveau | Réponse obligatoire |
|---|---|---|
| Deux simulations ou deux horloges actives | Critique | ForgeHistory seul autoritaire ; adapter/mocks CityLab exclus du build intégré et test zéro double tick. |
| Bootstrap `AfterSceneLoad` injecté dans `Main.unity` | Critique | Démarrage explicite par l'hôte, une instance par `CityLaunchContext`, test zéro auto-démarrage. |
| `cityId = 1001` et fixture locale en production | Élevé | Identifiants fournis par ForgeHistory, tests multi-ville et refus des contextes incomplets. |
| Sauvegardes concurrentes CityLab/ForgeHistory | Critique | Sauvegarde monde unique ; service CityLab limité au laboratoire et aux migrations. |
| Écart Built-in/URP et shaders carte | Élevé | Prototype avant portage, stratégie unique, captures dorées et régression GPU. |
| Conflits de packages Entities/Input/Navigation/URP | Élevé | Matrice de versions épinglée, hôte minimal puis build ForgeHistory avant migration d'assets. |
| Chargement synchrone et `Resources.Load` massif | Élevé | Transition asynchrone, catalogues partitionnés, annulation et budgets froid/chaud/mémoire. |
| LOD monde/ville non conservatif | Critique | Invariants de masse/identité, hashes aller-retour et conflits de révision testés. |
| Couche villes ForgeHistory non commencée | Élevé | Contrat CityLab d'abord, demande Hermes explicite, tâches dépendantes `BLOCKED` sans écriture amont. |
| Dérive de la dépendance ForgeHistory | Moyen | Audit épinglé à un commit et revalidation du delta avant chaque lot d'intégration. |
| Assets hétérogènes ou non redistribuables | Élevé | Provenance, licences, hashes, GUID/LFS et portage par manifeste. |
| Régression performance par transition/contenu | Élevé | 50 transitions, soak deux heures, profil 250 puis 500 habitants. |
| File Codex devenue obsolète | Moyen | La session qui ferme une tâche promeut la suivante et documente toute dépendance nouvelle. |

## Protocole de mise à jour

Au début de chaque session de travail :

1. lire `AGENTS.md`, ce document, `Docs/PROTOTYPE_STATUS.md` et
   `Docs/VALIDATION.md` ;
2. exécuter `powershell -ExecutionPolicy Bypass -File Tools/check_roadmap.ps1` ;
3. vérifier `git status --short` et préserver les changements existants ;
4. sélectionner la première tâche `ACTIVE`, ou la première `NEXT` si elle est
   terminée ou réellement bloquée, puis l'unique incrément `EN_COURS` dans
   « Sessions Codex ordonnées » ;
5. annoncer l'identifiant de la tâche et l'incrément choisis avant de modifier
   le projet ;
6. conserver la portée complète de l'incrément ; si elle dépasse une session,
   documenter les preuves acquises et reprendre le même ID à la suivante.

Avant de terminer une session qui a modifié le projet :

1. mettre à jour l'état des tâches touchées sans réécrire leur identifiant ;
2. renseigner la date `last_updated` et le tableau « État de pilotage » ;
3. ajouter les preuves exactes dans la ligne concernée ou `Docs/VALIDATION.md` ;
4. mettre à jour `Docs/PROTOTYPE_STATUS.md` uniquement pour les fonctions
   réellement jouables ;
5. ajouter une entrée au journal ci-dessous ;
6. passer l'incrément achevé à `PROUVÉ`, promouvoir le suivant à `EN_COURS` et,
   si une tâche passe à `DONE`, promouvoir la prochaine tâche de la file à
   `NEXT` ou `ACTIVE` puis vérifier que l'état de pilotage la désigne ;
7. exécuter le contrôle de roadmap et `git diff --check`.

## Journal d'avancement

| Date | Tâches | Résultat | Preuves | Prochaine priorité |
|---|---|---|---|---|
| 2026-08-13 | `M3-FH-01` | Pivot de produit : CityLab devient la vue ville jouable de ForgeHistory, ouverte depuis sa carte principale. L'audit lecture seule confirme Unity 6000.0.43f1 commun, mais révèle une divergence d'autorité : CityLab simule/sauvegarde dans Unity et se bootstrap après chaque scène, tandis que ForgeHistory exige une simulation unique hors Unity. La roadmap place donc contrats, packaging, rendu, transition, adaptateur backend et portage d'assets avant la suite des mécaniques locales. | ForgeHistory `268e8aab...e311`, `VISION.md`, `unity/README.md`, `Main.unity`, `MapDisplaySystem`, `PilotMapProvider`, manifests Unity ; CityLab `CityLabBootstrap`, `CityLabGame`, `CityContracts`, manifests et schéma de sauvegarde. | Exécuter l'incrément 02 : documenter le contrat d'autorité/chargement/portage sans écrire dans ForgeHistory. |
| 2026-08-12 | `META-AUTO-01` | L'évaluation indépendante accepte désormais les lots réels volumineux sous Windows : l'invite complète est transmise à Claude par stdin ; Cursor reçoit un ordre borné et relit le diff complet du PR au SHA ciblé avec `gh pr diff`. L'absence de verdict reste un échec fermé. | `WinError 206` reproduit par le run #31617561462 ; test stdin avec une invite de 100 000 caractères ; test de relecture Cursor hors ligne de commande ; harnais et roadmap verts. | Relancer l'évaluation de `M3-BUILD-01`, incrément 02, depuis `main` corrigée. |
| 2026-08-12 | `META-AUTO-01` | Le superviseur Windows distingue désormais une vraie interruption d'un `CTRL_C` tardif émis après la fin de l'acteur par un outil détaché : la première arrête le cycle, le second produit un rejet mécanique borné et permet l'itération corrective suivante. | Run refusé #31615708173 après 8/8 tests ciblés ; deux tests de signal post-sortie/acteur vivant ; harnais et roadmap verts. | Relancer `M3-BUILD-01`, incrément 02, en conservant un comportement fail-closed. |
| 2026-08-12 | `META-AUTO-01` | Durcissement du flux acteur sous Windows : une sortie Unicode non représentable par la page de codes de la console ne peut plus interrompre le cycle après génération ; le journal UTF-8 intégral reste la preuve de référence. L'échec est resté fermé et aucune modification de production n'a été publiée. | Run refusé #31611611471 ; test de non-régression CP1252 ; suite du harnais et contrôle roadmap verts. | Relancer `M3-BUILD-01`, incrément 02, après fusion du correctif. |
| 2026-08-12 | `META-AUTO-01` | Boucle full-auto prouvée de bout en bout et passée à `DONE` : Hermes orchestre, Codex produit, Cursor audite, Claude challenge, la CI et le merge bot décident, puis l'audit est archivé. Trois cycles refusés restent enregistrés comme preuves fail-closed. | [run full-auto 31606929060](https://github.com/PLiagre/VictoriaCityLab/actions/runs/31606929060), [PR preuve #14](https://github.com/PLiagre/VictoriaCityLab/pull/14), [audit #15](https://github.com/PLiagre/VictoriaCityLab/pull/15), [archive #16](https://github.com/PLiagre/VictoriaCityLab/pull/16), [dashboard #18](https://github.com/PLiagre/VictoriaCityLab/pull/18), ledger à `AUDIT_ARCHIVED`. | `M3-BUILD-01` redevient `ACTIVE` et l'incrément 02 reste `EN_COURS`. |
| 2026-08-12 | `M3-BUILD-01` | L'incrément 01 est fermé : les chantiers civiques gagnent quatre niveaux cumulatifs d'échafaudage après terrassement, retirés à l'achèvement ; le HUD expose le niveau, la sélection active des marqueurs et le reload reconstruit la phase tout en réappliquant le surlignage. La tâche reste `ACTIVE`. | 4/4 ciblés, 71/71 Editor, 1/1 PlayMode ; hashes 30/60 jours inchangés ; build 308 842 899 octets ; smoke 20/30/30, sauvegarde runtime et performance verts ; `CITYLAB_SCAFFOLDING_REVIEW_OK` et capture `m3-scaffolding-four-phases-20260811.png`. | Incrément 02 : usure, panne et réparation physique. |
| 2026-08-11 | `META-CODEX-01` | La vision premium est traduite en portes mesurables, 41 incréments Codex ordonnés et un contrat vertical de session. La qualité 3D suit désormais un pipeline hybride licencié→Adapted→QA→revue humaine. Les tâches existantes sont conservées ; `M3-BUILD-01` reste active et `M3-ART-01` devient la prochaine tâche prête. | `Docs/ROADMAP.md`, `AGENTS.md`, `Tools/check_roadmap.ps1`, `CITYLAB_ROADMAP_OK codex_increments=41`, `git diff --check`. | Reprendre `M3-BUILD-01`, incrément 01 : échafaudages synchronisés aux phases. |
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
