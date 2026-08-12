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

La cible de production est un city-builder médiéval premium haut de gamme : une
ville lisible sans HUD, une économie entièrement incarnée, une société dont les
familles ont une histoire et des batailles rares mais persistantes. Les œuvres
commerciales du genre servent uniquement de niveau d'exigence ; aucune règle,
interface, dénomination, composition visuelle ou propriété tierce ne doit être
copiée. La file « Sessions Codex ordonnées » transforme cette vision en lots
exécutables et constitue l'ordre de production par défaut.

Le projet est actuellement un vertical slice jouable. Il ne doit être qualifié
de jeu complet ou de qualité AAA que lorsque tous les critères de la section
« Définition de la version 1.0 » sont objectivement validés.

## État de pilotage

| Champ | Valeur |
|---|---|
| Dernière mise à jour | 12 août 2026 |
| Jalon actif | `M3` — ville organique et société |
| Dernier jalon validé | `M2` — économie de village jouable |
| Priorité immédiate | `M3-BUILD-01` — construction physique complète |
| Prochaine tâche prête | `M3-ART-01` — bible artistique et kit héroïque de production |
| Cible produit | City-builder médiéval premium original, campagne 20 h et bac à sable rejouable |
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
- au moins 10 familles de ressources, 30 chaînes de production et 70 bâtiments
  réellement différenciés par leur fonction ou leur évolution ;
- 500 habitants simulés à 60 FPS sur la machine cible recommandée, avec un p95
  CPU inférieur à 16,7 ms dans le scénario de référence ;
- sauvegarde/chargement versionné, autosave, récupération d'erreur et migrations ;
- au moins 6 régions jouables, revendication territoriale, 3 seigneurs IA,
  diplomatie et conflits résolus dans le monde de jeu ;
- armées, équipement, formations, moral, fatigue, pertes et conséquences
  économiques persistantes, avec une bataille de référence d'au moins
  200 combattants dans le budget de performance ;
- saisons, météo, agriculture, maladies, incendies et pénuries ayant un effet
  lisible et équilibré ;
- tutoriel, objectifs, options graphiques/audio/commandes, accessibilité et
  localisation française/anglaise ;
- direction artistique cohérente et originale, sans primitives de secours dans
  les scènes de production ni dépendance visuelle à une propriété tierce connue ;
- zéro erreur bloquante connue, zéro corruption de sauvegarde connue, tests de
  régression verts et campagne complète validée par QA ;
- build Windows signé et reproductible, crash reporting, crédits et licences.

## Cibles d'ambition premium

Ces cibles guident la conception et les budgets. Elles ne permettent jamais de
qualifier le jeu de « AAA », « alpha », « bêta » ou « 1.0 » avant le passage des
portes correspondantes :

- 70 bâtiments fonctionnels ou évolutifs, 30 chaînes de production et au moins
  10 familles de ressources, sans variantes purement cosmétiques comptées comme
  fonctions distinctes ;
- 8 à 12 régions présentant des sols, ressources, risques, routes et enjeux
  politiques propres ;
- 3 à 5 seigneurs IA aux stratégies économiques et diplomatiques différenciées ;
- 500 habitants simulés à 60 FPS comme porte ferme de livraison, avec 800 à
  1 000 habitants comme cible d'optimisation non bloquante si le budget matériel
  le permet ;
- batailles lisibles de 200 à 400 combattants, issues des foyers et des stocks du
  monde, dont les morts, blessés, captifs et équipements perdus persistent ;
- campagne d'au moins 20 heures, mode libre rejouable et objectifs proposant
  plusieurs trajectoires économiques, sociales ou militaires ;
- direction artistique originale et cohérente aux trois zooms, avec bâtiments
  héroïques, matériaux PBR, population variée, animations contextuelles, VFX et
  paysage sonore sans placeholder de production.

## Piliers non négociables

1. **Ville organique lisible** — les routes, parcelles, chantiers, extensions,
   métiers, niveaux de richesse, dégâts et saisons racontent l'état de la ville
   directement dans le monde.
2. **Simulation physique et déterministe** — toute ressource importante est
   produite, réservée, transportée, consommée et persistée ; toute exception au
   déterminisme est documentée et testée.
3. **Société persistante** — chaque habitant appartient à un foyer, travaille,
   consomme, vieillit, migre, peut être blessé ou mourir, et transmet ces effets
   à l'économie et à la stabilité.
4. **Région stratégique systémique** — territoire, influence, commerce,
   diplomatie et guerre reposent sur le même état de simulation, pas sur des
   mini-jeux isolés.
5. **Guerre à coût humain** — lever une armée retire de vrais travailleurs ; les
   équipements proviennent des ateliers et les pertes changent durablement les
   familles, la production et le pouvoir.
6. **Qualité prouvée** — aucune fonctionnalité ne passe à `DONE` sans preuve
   automatique et, lorsqu'elle est visuelle ou ergonomique, sans revue player.

## Jalons

| ID | Jalon | État | Porte de sortie |
|---|---|---|---|
| `M0` | Vertical slice forêt et construction | DONE | Routes, parcelles, maisons, transport du bois, camp forestier, HUD, build Windows et tests validés. |
| `M1` | Fondations de production | DONE | Sauvegarde fiable, données versionnées, navigation robuste, emplois physiques et tests déterministes. |
| `M2` | Économie de village jouable | DONE | Six ressources, alimentation, agriculture, sept chaînes, stockage local, marché, besoins, commerce et simulation 60 jours/2 heures sans invariant cassé. |
| `M3` | Ville organique et société | ACTIVE | Parcelles évolutives, familles, santé, foi, ordre, fiscalité et croissance jusqu'à 250 habitants. |
| `M4` | Région stratégique | BACKLOG | Six régions, revendication, commerce régional, diplomatie et au moins trois seigneurs IA fonctionnels. |
| `M5` | Guerre tactique | BACKLOG | Levées, suite, équipement, formations, moral, bataille de référence à 200 combattants et conséquences persistantes. |
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
| `META-CODEX-01` | Plan de production exécutable par sessions Codex | DONE | Cible premium, contrat de session, file ordonnée M3→M9 et prompt de lancement consignés ; `AGENTS.md` sélectionne l'unique incrément `EN_COURS` ; le vérificateur exige les nouvelles sections, compte 41 incréments et contrôle ordre, définition et concordance avec la tâche active ; `CITYLAB_ROADMAP_OK` et `git diff --check`. |
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

## M3 — ville, population et société

| ID | Travail | État | Critères d'acceptation | Sortie / preuve minimale |
|---|---|---|---|---|
| `M3-PLOT-01` | Parcelles organiques | DONE | Parcelles orientées sur la route à frontage/profondeur variables ; pente maximale 180 ‰ et chevauchements refusés ; jardins persistants et jusqu'à deux extensions selon le niveau du foyer. | 3/3 tests dédiés, 67/67 Editor, 1/1 PlayMode, build et smoke player verts. |
| `M3-BUILD-01` | Construction physique complète | ACTIVE | Échafaudages synchronisés avec les quatre phases ; usure et réparation consommant matériaux/temps ; démolition progressive, sûre pour les réservations, stocks et habitants ; les trois axes survivent au save/reload. | Échafaudages prouvés par 4/4 ciblés, 71/71 Editor, 1/1 PlayMode, build/smoke et capture des quatre phases ; réparation et démolition restent ouvertes avant `DONE`. |
| `M3-ART-01` | Bible artistique et kit héroïque de production | NEXT | Bible originale aux trois zooms ; palette, silhouettes, matériaux, densité de détail et budgets validés ; huit fonctions héroïques adaptées sans prefab Vendor direct ; provenance/licence/hash complets ; quatre phases et trois LOD. | Planche avant/après, captures player jour/nuit et trois zooms, QA Factory verte et approbation artistique humaine explicite ; peut fermer `M1-ASSET-05`. |
| `M3-FAMILY-01` | Foyers et cycle de vie | BACKLOG | Âge, couples, parenté, naissances, décès, migration, compétences et taille de foyer ; règles bornées, déterministes et persistantes ; croissance stable jusqu'à 250 habitants. | Tests d'événements de vie et reload, simulation longue un an, PlayMode, profil 250 habitants et inspection de foyers dans le HUD. |
| `M3-HEALTH-01` | Santé, maladies et blessures | BACKLOG | Risques liés au travail/logement/saison, contagion locale, soins, incapacité, récupération et mortalité ; aucun décès arbitraire non explicable. | Tests propagation/soins/reload, scénario de 90 jours, HUD lisible et capture d'un service de soin en activité. |
| `M3-FAITH-01` | Foi et sépulture | BACKLOG | Offices planifiés, capacité et couverture d'église, cimetière physique, sépultures persistantes, besoin de foi et effets sociaux bornés. | Tests calendrier/capacité/reload, trajet réel vers office ou sépulture, PlayMode et capture player. |
| `M3-ORDER-01` | Ordre et criminalité | BACKLOG | Mécontentement produit des délits explicables ; vol conserve les ressources ; milice, arrestation et sanctions graduées influencent les foyers sans boucle punitive incontrôlée. | Tests déterministes vol/intervention/reload, scénario de crise, télémétrie et revue HUD/monde. |
| `M3-TAX-01` | Fiscalité et trésor | BACKLOG | Impôts, dîme, salaires et dépenses sont comptabilisés ; politiques persistantes ; insolvabilité et pression fiscale provoquent des réactions lisibles. | Livre de comptes conservatif, tests politiques/reload, simulation 180 jours et panneau financier inspecté. |
| `M3-FIRE-01` | Incendies et catastrophes | BACKLOG | Départ et propagation liés aux matériaux, météo et densité ; alerte, eau, intervention, dégâts, victimes, réparation et reconstruction persistants. | Tests propagation/intervention/reload, invariants de ressources, PlayMode et capture d'un cycle incendie→reconstruction. |
| `M3-ART-02` | Environnement, population et animation de ville | BACKLOG | Variantes de façades/toits, accessoires sociaux, végétation, métiers, animaux, saisons et animations contextuelles cohérents avec `M3-ART-01`, budgets LOD/occlusion respectés. | Audit de diversité, captures des trois zooms et quatre saisons, profil 250 habitants et revue artistique humaine. |

## M4 — région stratégique et IA

| ID | Travail | État | Critères d'acceptation | Sortie / preuve minimale |
|---|---|---|---|---|
| `M4-MAP-01` | Carte en plusieurs régions | BACKLOG | Au moins six régions au jalon, frontières et ressources propres, chargement/transition sans perte d'état, architecture extensible vers 8–12 régions. | Tests de transition/reload, parcours player de trois régions et profil mémoire/chargement. |
| `M4-CLAIM-01` | Influence et revendication | BACKLOG | Influence produite/consommée, coût, progression, contestation et changement de contrôle avec historique persistant. | Tests de concurrence/reload et scénario player de revendication contestée. |
| `M4-TRADE-01` | Routes commerciales régionales | BACKLOG | Offre/demande, prix locaux, convois physiques, distance, risque, saison et contrôle territorial ; conservation exacte des biens et monnaies. | Tests économiques 365 jours, disparition de convoi sûre, PlayMode et télémétrie des marchés. |
| `M4-AI-01` | Seigneurs IA économiques | BACKLOG | Au moins trois seigneurs aux priorités distinctes bâtissent un village viable, réagissent aux pénuries, commercent, revendiquent et poursuivent des objectifs sans information cachée illégitime. | Trois seigneurs × trois graines × 365 jours sans invariant cassé, comparaison déterministe, télémétrie de décision et partie observée. |
| `M4-DIPLO-01` | Diplomatie | BACKLOG | Relations, demandes, accords, menaces, paix et mémoire des actions ; décisions IA explicables par état et personnalité. | Tests de mémoire/accord/reload, journal diplomatique et scénario player multi-issue. |
| `M4-EVENT-01` | Événements et objectifs | BACKLOG | Événements data-driven à conditions/choix/conséquences, objectifs économiques, sociaux et territoriaux, victoire/défaite persistantes. | Validation de catalogue, tests de branches/reload et partie guidée de deux heures. |

## M5 — guerre tactique

| ID | Travail | État | Critères d'acceptation | Sortie / preuve minimale |
|---|---|---|---|---|
| `M5-LEVY-01` | Levées issues des foyers | BACKLOG | Mobilisation retire de vrais adultes aptes des emplois, conserve identité/foyer/compétences et produit un coût économique mesurable. | Tests mobilisation/retour/reload et comparaison économique ville mobilisée/témoin. |
| `M5-EQUIP-01` | Fabrication et distribution d'équipement | BACKLOG | Armes, boucliers, armures et munitions sont fabriqués, stockés, attribués, usés, récupérés ou perdus sans duplication. | Tests de conservation/reload et inspection d'une unité équipée depuis la forge. |
| `M5-RETINUE-01` | Suite professionnelle | BACKLOG | Recrutement, solde, entretien, expérience, blessures et équipement persistent ; l'insolvabilité a des conséquences graduées. | Tests de paie/progression/reload et simulation 180 jours. |
| `M5-FORM-01` | Formations et commandes | BACKLOG | Déplacement groupé, ligne/colonne, orientation, cohésion, obstacles et collisions stables pour 200 à 400 combattants. | Tests déterministes de formation, stress pathfinding et profil player bataille cible. |
| `M5-COMBAT-01` | Combat, moral et fatigue | BACKLOG | Portée, impact, défense, terrain, moral, fuite, poursuite, fatigue, blessures et mort sont lisibles et bornés. | Replays à hash stable, tests d'équilibrage statistique, PlayMode et capture d'une bataille complète. |
| `M5-AI-01` | IA tactique | BACKLOG | L'IA choisit terrain, formation, flanc, réserve, objectifs et retraite à partir d'informations autorisées. | Batterie de scénarios, télémétrie de décision et absence de blocage sur 20 batailles. |
| `M5-CONSEQ-01` | Conséquences persistantes | BACKLOG | Blessés, morts, captifs, butin et équipement retournent au monde ; familles, production, ordre et diplomatie reflètent les pertes. | Round-trip bataille→ville→reload, invariants économiques et revue d'après-bataille. |

## M6 à M9 — finition et sortie

| ID | Jalon | Domaine | État | Porte de sortie vérifiable |
|---|---|---|---|---|
| `M6-GAME-01` | M6 | Boucle de partie de bout en bout | BACKLOG | Départ nu, croissance, crise, revendication, conflit et victoire/défaite jouables pendant huit heures, avec sauvegarde à tout moment et aucun blocage. |
| `M6-ONBOARD-01` | M6 | Tutoriel, objectifs et encyclopédie | BACKLOG | Un nouveau joueur termine le tutoriel sans aide externe et comprend les causes d'au moins 80 % des alertes de test utilisateur. |
| `M6-SAVE-01` | M6 | Robustesse campagne | BACKLOG | Autosave rotatif, récupération après interruption, migrations de toutes les versions publiées et zéro corruption connue sur la matrice QA. |
| `REL-CONTENT` | M7 | 70 bâtiments, 30 chaînes, régions et événements | BACKLOG | Contenu fonctionnel équilibré et sans placeholder ; chaque entrée a une utilité, un coût, un visuel et une preuve player. |
| `REL-ART` | M7 | Environnements, architecture, personnages, UI et VFX | BACKLOG | Cohérence finale aux trois zooms et quatre saisons ; provenance/licence, LOD, occlusion, budgets et diversité validés ; aucun prefab Vendor direct. |
| `REL-ANIM` | M7 | Locomotion, métiers, construction et combat | BACKLOG | Transitions propres, IK, variations, réactions contextuelles et budgets CPU validés sur foule cible. |
| `REL-AUDIO` | M7 | Musique, ambiances et sound design | BACKLOG | Mix dynamique, spatialisation, variations, lisibilité des alertes et options complètes validés en partie longue. |
| `REL-UX` | M7 | Interface et lisibilité de production | BACKLOG | Toutes les actions critiques sont découvrables, annulables ou confirmées ; UI testée à 1080p/1440p/4K et à trois tailles. |
| `REL-PERF` | M8 | 500 habitants et grandes villes | BACKLOG | 60 FPS et p95 CPU < 16,7 ms sur machine cible, mémoire stable, zéro allocation majeure récurrente et budgets de chargement/sauvegarde respectés. |
| `M8-BALANCE-01` | M8 | Équilibrage et télémétrie | BACKLOG | Au moins 30 parties automatisées multi-graines et 10 parties humaines terminables ; aucune stratégie unique dominante ni spirale inévitable non signalée. |
| `REL-QA` | M8 | Tests et campagne de régression | BACKLOG | Zéro critique/majeur ouvert, soak test, matrice graphique, campagne 20 heures et migrations validés par QA. |
| `REL-ACCESS` | M9 | Accessibilité | BACKLOG | Remapping complet, navigation clavier, sous-titres, tailles UI, contrastes, daltonisme, réduction des effets et options de confort validés. |
| `REL-LOC` | M9 | Français et anglais | BACKLOG | Aucun texte codé en dur, pluriels/variables corrects, débordements contrôlés et relecture humaine terminée. |
| `REL-SHIP` | M9 | Packaging Windows | BACKLOG | Build signé/reproductible, installateur, crédits/licences, crash reporting, confidentialité, sauvegarde cloud si retenue et procédure de mise à jour/rollback testée. |

## Contrat d'une session Codex

Une session Codex de production doit livrer une tranche verticale, pas seulement
un squelette de code. Ce contrat complète `AGENTS.md` :

1. prendre la tâche `ACTIVE`, puis l'unique incrément `EN_COURS` dans la table
   « Sessions Codex ordonnées » ; si elle est `DONE` ou réellement bloquée,
   promouvoir la première tâche `NEXT` et le premier incrément `À_FAIRE` associé ;
2. annoncer l'identifiant de roadmap et l'incrément visé avant toute modification ;
3. inspecter les contrats, tests, scènes, assets et changements Git concernés
   avant de concevoir l'implémentation ; préserver tout travail utilisateur ;
4. pour une mécanique, livrer ensemble données versionnées, simulation,
   persistance, représentation monde, HUD/feedback et tests déterministes ;
5. pour un asset, conserver la source Vendor immuable, enregistrer provenance,
   licence et SHA-256, publier seulement une variante sous
   `Assets/CityLabHost/Adapted`, puis valider UV, matériaux, LOD, colliders,
   pivots, budgets, import Unity et rendu player ;
6. exécuter d'abord les tests ciblés, puis la régression proportionnée au risque ;
   toute modification visuelle exige au moins une capture player inspectable ;
7. ne jamais réduire silencieusement un critère pour terminer dans une session :
   garder la tâche et l'incrément `EN_COURS`, documenter la preuve acquise et
   reprendre le même ID à la session suivante ; quand la preuve est complète,
   passer l'incrément à `PROUVÉ` et le suivant à `EN_COURS` ;
8. ne marquer `DONE` que lorsque chaque critère de la ligne est prouvé. Mettre
   alors la prochaine tâche de la file en `NEXT` ou `ACTIVE`, synchroniser les
   trois documents de pilotage concernés et ajouter une entrée au journal ;
9. terminer par `Tools/check_roadmap.ps1`, `git diff --check` et un résumé des
   fichiers, preuves, limites et prochaine action. Aucun commit, push, achat ou
   publication externe n'est implicite.

### Prompt de lancement recommandé

```text
Continue Victoria CityLab depuis la roadmap. Respecte AGENTS.md, prends la
tâche ACTIVE puis l'unique incrément EN_COURS de « Sessions Codex ordonnées ».
Livre une tranche verticale complète et déterministe : données, simulation,
sauvegarde, monde, HUD, tests et preuve player selon le contrat de session.
Ne réduis aucun critère, ne modifie aucune source Vendor et ne marque DONE
qu'avec toutes les preuves. Mets à jour ROADMAP, PROTOTYPE_STATUS et VALIDATION
uniquement selon ce qui est réellement validé, puis exécute les contrôles finaux.
```

## Sessions Codex ordonnées

Cette table est la file de production autoritaire. Un incrément peut demander
plusieurs sessions : tant que sa preuve manque, il reste `EN_COURS`. Une fois
prouvé, il passe à `PROUVÉ` et le premier `À_FAIRE` devient `EN_COURS`.
`BLOQUÉ` exige une dépendance explicite dans le journal. Lorsqu'une tâche se
ferme, la session qui la ferme promeut la prochaine tâche de cette table à
`NEXT` ou `ACTIVE`. `M1-ASSET-05` reste une approbation
humaine isolée ; s'il bloque `M3-ART-01`, consigner le blocage et poursuivre
`M3-FAMILY-01` sans déclarer la porte artistique acquise.

| Ordre | Suivi | Tâche | Incrément de session | Preuve de fermeture de l'incrément |
|---:|---|---|---|---|
| 01 | PROUVÉ | `M3-BUILD-01` | Échafaudages synchronisés aux quatre phases, sélection et reload. | 4/4 ciblés, 71/71 Editor, 1/1 PlayMode, build/smoke et capture player des quatre phases. |
| 02 | EN_COURS | `M3-BUILD-01` | Usure, panne et réparation physique consommant matériaux et travail. | Tests conservation/reload, HUD et capture avant/après. |
| 03 | À_FAIRE | `M3-BUILD-01` | Démolition progressive avec récupération bornée et nettoyage des contrats. | Tests destruction en concurrence, régression complète, build/smoke ; fermeture de `M3-BUILD-01`. |
| 04 | À_FAIRE | `M3-ART-01` | Bible visuelle originale : références, palette, matériaux, silhouettes, densité aux trois zooms et budgets. | Document versionné, planche de direction et approbation humaine. |
| 05 | À_FAIRE | `M3-ART-01` | Kit héroïque des huit fonctions, adaptation/licences, phases, LOD et rendu Unity. | QA Factory, captures jour/nuit et trois zooms ; fermeture ou blocage humain explicite. |
| 06 | À_FAIRE | `M3-FAMILY-01` | Contrats familiaux : âge, sexe, parenté, foyer, compétences, migration et migration de sauvegarde. | Tests de création/reload/migration et invariants. |
| 07 | À_FAIRE | `M3-FAMILY-01` | Couples, naissances, vieillissement, décès, héritage du logement et croissance. | Simulation un an multi-graines sans incohérence. |
| 08 | À_FAIRE | `M3-FAMILY-01` | Représentation player, inspection HUD et performance de 250 habitants. | PlayMode, build/smoke, profil 250 habitants ; fermeture de la tâche. |
| 09 | À_FAIRE | `M3-HEALTH-01` | Risques, maladies, contagion, blessures, soins, incapacité et mortalité. | Scénario 90 jours, tests/reload et revue player. |
| 10 | À_FAIRE | `M3-FAITH-01` | Offices, couverture, cimetière, sépultures et effets sociaux. | Tests calendrier/capacité/reload et revue player. |
| 11 | À_FAIRE | `M3-ORDER-01` | Mécontentement, délits, vol conservatif, milice et sanctions. | Scénario de crise, télémétrie, tests/reload et revue player. |
| 12 | À_FAIRE | `M3-TAX-01` | Trésor, impôts, dîme, dépenses et politiques avec réactions des foyers. | Livre conservatif, simulation 180 jours et panneau inspecté. |
| 13 | À_FAIRE | `M3-FIRE-01` | Incendie déterministe, intervention, victimes, dégâts et reconstruction. | Cycle complet capturé, tests/reload et invariants. |
| 14 | À_FAIRE | `M3-ART-02` | Variations environnement/population, animations de métiers, saisons et optimisation 250 habitants. | Audit de diversité, quatre saisons, profil et fermeture de M3. |
| 15 | À_FAIRE | `M4-MAP-01` | Modèle régional, six régions initiales, streaming/transition et sauvegarde. | Tests transition/reload, profil mémoire et parcours player. |
| 16 | À_FAIRE | `M4-CLAIM-01` | Influence, revendication, contestation et contrôle persistant. | Scénario contesté déterministe et HUD régional. |
| 17 | À_FAIRE | `M4-TRADE-01` | Prix régionaux, routes, convois, risques et conservation économique. | Simulation 365 jours, disparition sûre et télémétrie. |
| 18 | À_FAIRE | `M4-AI-01` | Trois seigneurs économiques différenciés utilisant les mêmes règles que le joueur. | Trois seigneurs × trois graines × 365 jours et journal de décisions. |
| 19 | À_FAIRE | `M4-DIPLO-01` | Relations, mémoire, accords, menaces, guerre et paix. | Tests de branches/reload et scénario multi-issue. |
| 20 | À_FAIRE | `M4-EVENT-01` | Catalogue d'événements, objectifs et victoire/défaite. | Partie guidée deux heures ; fermeture de M4. |
| 21 | À_FAIRE | `M5-LEVY-01` | Mobilisation de vrais habitants et coût économique. | Comparaison ville mobilisée/témoin et reload. |
| 22 | À_FAIRE | `M5-EQUIP-01` | Production, attribution, usure, récupération et perte d'équipement. | Tests de conservation et inspection d'unité. |
| 23 | À_FAIRE | `M5-RETINUE-01` | Suite professionnelle, solde, expérience, blessures et entretien. | Simulation 180 jours et reload exact. |
| 24 | À_FAIRE | `M5-FORM-01` | Commandes et formations stables pour 200 à 400 combattants. | Stress pathfinding, déterminisme et profil player. |
| 25 | À_FAIRE | `M5-COMBAT-01` | Combat, terrain, fatigue, moral, fuite, blessures et mort. | Replays stables, tests statistiques et bataille capturée. |
| 26 | À_FAIRE | `M5-AI-01` | IA tactique : terrain, flancs, réserve, objectifs et retraite. | Batterie de 20 batailles et télémétrie explicable. |
| 27 | À_FAIRE | `M5-CONSEQ-01` | Retour des pertes, captifs, butin et effets familiaux/économiques. | Round-trip bataille→ville→reload ; fermeture de M5. |
| 28 | À_FAIRE | `M6-GAME-01` | Partie complète de huit heures, crises et fins de partie. | Campagne QA sans blocage et sauvegardes valides. |
| 29 | À_FAIRE | `M6-ONBOARD-01` | Tutoriel, objectifs, encyclopédie et explication des alertes. | Tests utilisateurs et taux de compréhension documenté. |
| 30 | À_FAIRE | `M6-SAVE-01` | Autosaves rotatifs, récupération et matrice de migrations. | Matrice QA zéro corruption ; fermeture de M6. |
| 31 | À_FAIRE | `REL-CONTENT` | Monter progressivement à 70 bâtiments, 30 chaînes et contenu régional. | Catalogue validé, équilibrage et revue player sans placeholder. |
| 32 | À_FAIRE | `REL-ART` | Passe finale environnement, architecture, personnages, UI et VFX. | Revue aux trois zooms/quatre saisons et budgets verts. |
| 33 | À_FAIRE | `REL-ANIM` | Locomotion, métiers, construction et combat de production. | Revue de transitions/IK/variations et profil foule. |
| 34 | À_FAIRE | `REL-AUDIO` | Musique, ambiances, spatialisation et alertes. | Mix de partie longue, variations et options validés. |
| 35 | À_FAIRE | `REL-UX` | Interface finale, lisibilité et résolutions cibles. | Tests 1080p/1440p/4K et trois tailles d'UI ; fermeture de M7. |
| 36 | À_FAIRE | `REL-PERF` | Optimisation 500 habitants et grande ville de référence. | 60 FPS, p95 < 16,7 ms, mémoire/GC/chargements dans le budget. |
| 37 | À_FAIRE | `M8-BALANCE-01` | Équilibrage automatisé et humain multi-stratégies. | 30 parties automatiques et 10 humaines terminables. |
| 38 | À_FAIRE | `REL-QA` | Soak, régression, configurations et campagne 20 heures. | Zéro critique/majeur et fermeture de M8. |
| 39 | À_FAIRE | `REL-ACCESS` | Accessibilité et options de confort complètes. | Checklist et tests utilisateurs ciblés. |
| 40 | À_FAIRE | `REL-LOC` | Localisation et relecture française/anglaise. | Audit zéro texte dur, pseudo-localisation et relecture humaine. |
| 41 | À_FAIRE | `REL-SHIP` | Signature, installateur, licences, crash reporting et rollback. | RC reproductible et campagne finale ; fermeture de M9/1.0. |

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
| Simulation locale trop liée au vertical slice | Élevé | Stabiliser les contrats et les données avant d'ajouter beaucoup de contenu. |
| Extension des flux au-delà du bois | Moyen | Réutiliser le contrat de ressources et d'extrémités de `M1-LOG-01` sans recréer de transport spécifique. |
| Coût de navigation à plus de 100 habitants | Moyen | Conserver le test 20 minutes et profiler à nouveau dans `M1-PERF-01`. |
| Accumulation d'assets hétérogènes | Moyen | Conserver l'adaptation sous `Assets/CityLabHost/Adapted` et auditer chaque source. |
| Dérivés Store non traçables ou redistribuables | Élevé | Provenance, licence, hashes d'entrée, workbench non publié et sortie uniquement intégrée au jeu. |
| Pipeline conversationnel non reproductible | Élevé | Blender headless et recettes versionnées sont la source de vérité ; MCP réservé à l'exploration. |
| Ambition AAA sans budget mesuré | Élevé | Utiliser les portes M1–M9 ; ne jamais remplacer une preuve par un pourcentage subjectif. |
| Régression performance par ajout de contenu | Élevé | Rejouer le scénario de référence à chaque jalon. |
| Fonctionnalités larges livrées à moitié | Élevé | Appliquer le contrat vertical données→simulation→save→monde→HUD→tests et reprendre le même ID tant que la preuve manque. |
| Art final repoussé après les systèmes | Élevé | Exécuter `M3-ART-01` dès la fermeture de la construction, puis imposer la bible à chaque nouveau contenu. |
| Achats ou commandes non maîtrisés | Élevé | Codex prépare spécifications et audits, mais toute dépense, licence non standard ou engagement externe exige une autorisation utilisateur explicite. |
| File Codex devenue obsolète | Moyen | La session qui ferme une tâche promeut la suivante, met à jour la file et documente toute dépendance nouvellement découverte. |

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
