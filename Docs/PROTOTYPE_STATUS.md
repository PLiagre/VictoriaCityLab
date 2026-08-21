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

Le contrat d'intégration v1 ForgeHistory ↔ City Mode est maintenant défini en
C# pur et JSON versionné. ForgeHistory possède le monde, le tick, la simulation
et la sauvegarde ; City Mode reçoit un `CityLaunchContext`, consomme un snapshot
révisionné et émet des intentions corrélées. `LocalCitySimulation`, le save local
et les fixtures restent des outils de laboratoire, pas une autorité de
production. Le contrat vit dans un package UPM sans dépendance Unity. Le
lifecycle `com.victoria.citymode.presentation` est un second package portable,
sans URP, Input System, AI Navigation, scène, fixture, simulation ni save ;
l'hôte crée et détruit explicitement une présentation par session. Le bundle
historique `com.victoria.citymode` est identifié comme laboratoire uniquement et
reste composé avec `Assets/CityLabHost`. Un hôte Unity minimal distinct importe
seulement contrats et présentation. Les vues urbaines riches doivent encore être
branchées après convergence rendu ; aucune intégration player ForgeHistory n'est
revendiquée.

La convergence rendu `M3-FH-03` retient URP `17.0.4` pour le futur hôte intégré.
Sur deux extractions jetables du même commit ForgeHistory, les trois captures de
carte Built-in et URP sont bit-identiques ; le player URP de `Main.unity` se
construit et produit un framebuffer complet sans pixel magenta. Entities
`1.3.15`, Burst `1.8.19`, Collections `2.5.7` et Mathematics `1.3.2` restent
épinglés par l'hôte. Input System et AI Navigation demeurent des adaptateurs de
laboratoire/contenu, absents du cœur de présentation. Cette décision est une
preuve de portabilité, pas une modification ni une intégration livrée dans le
dépôt ForgeHistory.

Le shell `M3-FH-04` est également portable : `CityModeTransitionShell` orchestre
progression, timeout, annulation, erreur, entrée et retour via
`ICityModeTransitionHost`, tandis que le `SceneManager`, les scènes et la
restauration du viewport restent entièrement côté hôte. Le miroir de carte
charge `CityModeView` en additif puis restaure cellule et viewport ; il ne
contient aucune carte, simulation ou sauvegarde ForgeHistory.

Le port d'assets `M3-FH-06` est fermé dans un troisième package portable,
`com.victoria.citymode.assets`. Onze binaires approuvés sont répartis en socle
commun, biome et ville avec GUID cible versionnés, hashes source→cible
identiques, LFS, provenance et licences. `CityModeAssetPartitionLoader` impose
le chargement `Common→Biome→City`, le déchargement inverse et le rollback sous
budget via un port hôte. Le package ne contient ni `Resources.Load`, ni scène,
ni simulation, ni horloge, ni sauvegarde. Le player miroir URP produit trois
zooms lisibles et dix cycles mémoire bornés ; il ne constitue toujours pas une
intégration dans le dépôt ForgeHistory.

La production est pilotée par Hermes (ADR-0002) : propositions sous
`hermes/`, briefs sous `harness/queue/briefs/`, exécution Cursor dans
`agent/*`, worker Unity Windows en `workflow_dispatch`. L'ancien
full-auto Codex + merge bot est archivé (`mode: manual`). `LocalCitySimulation`
reste l'adaptateur de laboratoire ; la simulation de production appartient
à ForgeHistory `sim/`.

L'économie forestière est exposée par `PlaceLumberCamp` et
`ProductionSiteState`. L'affectation est physique : seuls les bûcherons présents
au camp construisent puis produisent. Les huit emplois, leurs lieux de travail,
horaires, absences, chemins et tâches logistiques font partie du snapshot
sauvegardé. L'abattage individuel de chaque arbre reste à produire.

## Validation

Les jalons visuels sont captures depuis le player Windows a 1920 x 1080 dans
`Logs/Captures`. Le smoke test charge une fixture hote de 20 foyers, 30 batiments
et 30 habitants et refuse de valider un scenario incomplet. La validation du
13 août 2026 passe 99/99 tests EditMode et 6/6 tests PlayMode ; le build Windows
de référence pèse 308 862 268 octets. L'hôte Unity minimal passe séparément 3/3
tests sans charger le laboratoire. Le stress déterministe couvre 100 habitants pendant
20 minutes sans échec de navigation. Le smoke valide aussi un round-trip de
sauvegarde dans le player ; la mesure visible de référence reste 60,0 FPS et
16,683 ms au p95 sur 1 800 frames. L'économie M2 progresse aussi 60 jours,
soit deux heures de jeu simulé, sans quantité négative ni agent bloqué.
Les snapshots metier sont rafraichis a 10 Hz et les vues d'habitants interpolent
leur position a chaque frame, ce qui evite une serialisation JSON complete dans
chaque `Update` sans sacrifier la fluidite visuelle.

Le prototype rendu ForgeHistory compare également la carte Built-in et URP sur
le même commit : 3/3 captures SHA-256 identiques, six verdicts carte verts,
player URP de 178 175 782 octets, framebuffer carte et capture ville 1920×1080
avec 0 pixel magenta. Le chemin GPU câblé mesure 1,475 ms/image en Built-in et
0,266 ms/image sous URP sur la machine de session, sous le budget de 16,7 ms.
Le shell de transition passe 9/9 tests EditMode, 5/5 PlayMode dans l'hôte minimal
et 7/7 PlayMode dans le miroir. Cinquante scènes réelles mesurent 3,017 ms à
froid, 0,728 ms au pire à chaud, 2,159 ms au retour et +118 439 octets alloués ;
le player GPU répète 50 cycles en 15,783/1,222/3,882 ms.

L'hôte d'assets passe 4/4 EditMode et 2/2 PlayMode. Dix cycles réels chargent 30
partitions et en libèrent 30 avec un delta Editor de 19 731 octets. Le player GPU
mesure 16 779 984 octets pour `common`, 1 431 216 pour `biome` et 22 393 444
pour `city`, +3 449 975 octets après GC, 18,521 ms au pire pour charger et
3,083 ms pour libérer. Son build pèse 165 565 540 octets et les trois captures
1280×720 sont distinctes, sans pixel magenta.

La régression de fermeture ajoute 17/17 tests Python d'outils et conserve 30/30
tests du harnais full-auto.

Le 12 août 2026, les 20 tests Python du harnais passent et le cycle témoin de la
PR #14 a été fusionné automatiquement après CI, audit Cursor `PASS` et challenge
Claude `PASS`. L'audit #15, l'archive terminale #16 et le dashboard Hermes #18
sont fusionnés ; le ledger atteint `AUDIT_ARCHIVED`. Cette preuve porte sur
l'automatisation de production et ne remplace aucune porte Unity ou player.

## Limites connues et prochaines priorites

- le livrable est un vertical slice jouable et valide, pas un jeu AAA termine ;
- ajouter objectifs, tutoriel et options completes, puis étendre la sauvegarde
  aux futurs systèmes au fil de leur intégration ;
- attendre la couche 2 « Villes » dans ForgeHistory `sim/` qui débloque
  l'adaptateur autoritaire (`M3-FH-05`) et la première ville réelle
  (`M3-FH-07`) ; le worker `unity-windows` est prêt mais manuel ; shell,
  URP et portage borné des assets sont prouvés uniquement dans des hôtes miroir ;
- terminer la construction physique avec usure, réparation et démolition
  (`M3-BUILD-01`) seulement après fermeture de `M3-FH-07` ; terrassement,
  matériaux par étape, équipes affectées et échafaudages synchronisés sont déjà
  jouables dans le laboratoire ;
- augmenter la variation des facades, personnages, animations, effets, sons et
  compositions de village, puis valider plusieurs centaines d'habitants.
