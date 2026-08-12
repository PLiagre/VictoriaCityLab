# Etat du prototype CityLab

## Vertical slice actuel

- camera RTS et terrain 512 x 512 m sous URP ;
- direction medievale dark-fantasy stylisee, palette terre/bronze/braises et
  post-traitement URP ;
- deux textures originales CityLab de 1254 x 1254 pour la prairie peinte et les
  routes, completees par des couches de terrain procedurales ;
- relief doux, foret Vendor deterministe, herbes, pierres, clotures, puits,
  marche, feu, particules, vent et ambiance sonore procedurale ;
- trace de route en deux clics avec apercu valide/invalide ;
- zoning residentiel, parcelles des deux cotes et orientation vers la route ;
- parcelles organiques orientées à largeur/profondeur variables, contraintes par
  les limites, les chevauchements et une pente maximale de 180 ‰ ; maisons en
  façade, jardins actifs à l'achèvement et une/deux extensions pour les foyers
  établis/prospères selon la capacité persistante du lot ;
- camp forestier placable avec `B`, coût piloté par catalogue, contraintes de distance
  et espacement, deux bucherons au maximum et reserve locale finie ;
- huit fonctions constructibles pilotées par données : résidence, scierie,
  grenier, entrepôt, marché, forge, grange et chapelle, chacune en trois variantes ;
- nouveaux chantiers précédés d'un terrassement calculé sur cinq points de
  l'emprise ; bâtiments civiques alimentés physiquement en pierre, bois,
  planches puis outils selon leur phase, avec progression et matériau visibles ;
- échafaudages déterministes à quatre niveaux cumulatifs, absents avant le
  terrassement et retirés à l'achèvement ; leur niveau `1/4` à `4/4` apparaît
  dans la fiche du chantier, et sélection comme phase sont restaurées au reload ;
- production forestiere deterministe et suivi du bois en stock, reserve, en
  transit et livre aux chantiers ;
- tâches logistiques persistantes pilotées par priorité, avec sources et
  destinations stock/bâtiment/site, réservation anti-duplication, pénurie
  stable et restitution sûre si une destination disparaît ;
- registre persistant de bois, planches, pierre, nourriture, outils et textile,
  avec unités, capacités, réservations, débordements refusés et pertes journalières ;
- cueillette et chasse avec sources accessibles, trajets physiques, retour de
  rations au stock, consommation quotidienne et faim persistante par foyer ;
- deux champs agricoles persistants avec fertilité, labour, semis, croissance
  affectée par la météo quotidienne, récolte en nourriture et retour en jachère ;
- sept ateliers persistants — scierie, carrière, forge, moulin, four, tissage
  et artisanat — avec recettes, tampons locaux et transports physiques multi-ressource ;
- greniers et entrepôts à inventaires locaux catégorisés, capacité non doublée,
  rayon de service, gardiens actifs et rééquilibrage physique vers 50 % ;
- marchés à étals physiques nourriture/outils/textile, couverture des foyers,
  rareté, prix et jours de pénurie visibles ;
- besoins des foyers en nourriture, combustible, vêtements, outils et logement,
  avec satisfaction 0–1000, quatre niveaux et pénuries persistantes ;
- commerce extérieur persistant par import/export, frais de 10 %, délai 2–3
  jours, marchand en transit, limite de volume et réservation sûre ;
- fondations, ossature en bois et maison Vendor terminee ;
- occupation visible des maisons par une lumiere de foyer et une fumee de cheminee ;
- habitants GanzSe animes par les clips Humanoid Kevin Iglesias : idle, marche,
  transport avec faisceau de buches et gestes actifs de chantier ;
- grille de navigation A* déterministe 128 x 128, contournement des emprises,
  récupération d'une cible bloquée et NavMesh Unity actualisé après construction ;
- emplois exclusifs de bâtisseur, bûcheron, grenetier, magasinier, marchand,
  forgeron, éleveur et clerc ; horaires 08h–18h, trajets physiques, absences et
  remplacements reproductibles par graine ;
- HUD 1080p de chronique seigneuriale affichant ressources, forêt, emplois,
  présents, absents, population, foyers, chantiers, services, heure et vitesse ;
- pause avec `Espace` et vitesses x1/x2/x4 avec `1`/`2`/`3` ;
- calendrier persistant année/mois/jour/heure, saisons trimestrielles,
  pause/vitesse de reprise sauvegardées et événements datés déterministes ;
- sauvegarde manuelle avec `F5`, chargement vérifié avec `F9` et autosave
  atomique toutes les 120 secondes réelles ; checksum SHA-256, refus propre des
  corruptions et migration du schéma v0 vers v1 ;
- selection d'un chantier dans le monde, surlignage et priorite basse/normale/haute ;
- trois variantes déterministes de résidence et de scierie importées, avec
  quatre phases de construction et trois LOD par phase ;
- huit rôles visuels de population importés en Humanoid avec trois LOD et
  sélection déterministe ;
- fallbacks primitifs si le catalogue visuel hote est absent.

## Architecture

La simulation deterministe et les contrats `ICityStateSource` / `ICityCommandSink`
restent dans `Packages/com.victoria.citymode`. La scene, le catalogue visuel et les
adaptateurs d'assets tiers restent dans `Assets/CityLabHost`. Aucun code du package
ne reference directement un dossier Vendor.

La production est pilotable par le harnais full-auto sous `harness/`. Hermes
sélectionne l'unique incrément `EN_COURS`, Codex produit, Cursor audite et
Claude contredit dans des exécutions séparées. La fusion automatique exige la
CI verte, les deux verdicts `PASS`, une décision enregistrée sur `main` et la
politique de chemins fermée. Les workflows, l'orchestrateur, la gouvernance et
les sources Vendor restent protégés ; quatre coupe-circuits permettent un
retour immédiat au mode manuel.

L'économie forestière est exposée par `PlaceLumberCamp` et
`ProductionSiteState`. L'affectation est physique : seuls les bûcherons présents
au camp construisent puis produisent. Les huit emplois, leurs lieux de travail,
horaires, absences, chemins et tâches logistiques font partie du snapshot
sauvegardé. L'abattage individuel de chaque arbre reste à produire.

## Validation

Les jalons visuels sont captures depuis le player Windows a 1920 x 1080 dans
`Logs/Captures`. Le smoke test charge une fixture hote de 20 foyers, 30 batiments
et 30 habitants et refuse de valider un scenario incomplet. La validation du
12 août 2026 passe 71/71 tests EditMode et 1/1 test PlayMode ; le build Windows
de référence pèse 308 842 899 octets. Le stress déterministe couvre 100 habitants pendant
20 minutes sans échec de navigation. Le smoke valide aussi un round-trip de
sauvegarde dans le player ; la mesure visible de référence reste 60,0 FPS et
16,683 ms au p95 sur 1 800 frames. L'économie M2 progresse aussi 60 jours,
soit deux heures de jeu simulé, sans quantité négative ni agent bloqué.
Les snapshots metier sont rafraichis a 10 Hz et les vues d'habitants interpolent
leur position a chaque frame, ce qui evite une serialisation JSON complete dans
chaque `Update` sans sacrifier la fluidite visuelle.

Le 12 août 2026, les 20 tests Python du harnais passent et le cycle témoin de la
PR #14 a été fusionné automatiquement après CI, audit Cursor `PASS` et challenge
Claude `PASS`. L'audit #15, l'archive terminale #16 et le dashboard Hermes #18
sont fusionnés ; le ledger atteint `AUDIT_ARCHIVED`. Cette preuve porte sur
l'automatisation de production et ne remplace aucune porte Unity ou player.

## Limites connues et prochaines priorites

- le livrable est un vertical slice jouable et valide, pas un jeu AAA termine ;
- ajouter objectifs, tutoriel et options completes, puis étendre la sauvegarde
  aux futurs systèmes au fil de leur intégration ;
- terminer la construction physique avec usure, réparation et démolition
  (`M3-BUILD-01`) ; terrassement, matériaux par étape, équipes affectées et
  échafaudages synchronisés sont déjà jouables ;
- augmenter la variation des facades, personnages, animations, effets, sons et
  compositions de village, puis valider plusieurs centaines d'habitants.
