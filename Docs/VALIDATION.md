# Validation du vertical slice

Derniere validation complete : 12 août 2026, Unity `6000.0.43f1`, Windows 11,
player URP en 1920 x 1080. Toutes les commandes Unity passent par le verrou propre
a CityLab : `py Tools/run_unity_locked.py -- <Unity.exe> ...`.

Cette validation porte sur un vertical slice jouable. Elle ne constitue pas une
certification qu'un jeu AAA complet est termine.

## Architecture full-auto — 12 août 2026

Cette validation structurelle ne remplace aucune preuve Unity ou player.

| Porte | Résultat | Preuve |
|---|---|---|
| Harnais | 20/20 tests Python verts | `py -m unittest discover -s harness/tests -v` |
| Workflows | Six documents YAML chargés sans erreur | parse `yaml.safe_load` |
| Roadmap | Contrôle structurel vert avec un incrément `EN_COURS` | `CITYLAB_ROADMAP_OK` |
| Hermes | Profil isolé invoqué sur le modèle local | sortie `CITYLAB_HERMES_OK` |
| Runner | `citylab-full-auto-pe` en ligne, labels Windows/X64/citylab-full-auto | API GitHub Actions runners |
| Orchestration | Hermes, Codex et Claude exécutés ; CI verte ; PR témoin créée | [run 31606929060](https://github.com/PLiagre/VictoriaCityLab/actions/runs/31606929060) |
| Audit | Cursor `PASS`, Claude `PASS`, décision au SHA `485847d...` | [audit record #15](https://github.com/PLiagre/VictoriaCityLab/pull/15) |
| Fusion automatique | PR témoin fusionnée par le merge bot au SHA `5f2421a...` | [PR #14](https://github.com/PLiagre/VictoriaCityLab/pull/14) |
| Archivage | FSM complète jusqu'à `AUDIT_ARCHIVED`, archive fusionnée | [PR #16](https://github.com/PLiagre/VictoriaCityLab/pull/16) |
| Hermes | Dashboard calculé et fusionné automatiquement | [PR #18](https://github.com/PLiagre/VictoriaCityLab/pull/18) |
| Échec fermé | Trois preuves refusées n'ont pas été fusionnées | PR #5, #8 et #11 fermées ; événements `AUDIT_REJECTED` conservés |
| Sortie Unicode Windows | Un flux acteur contenant `→` est journalisé en UTF-8 et rendu sans erreur sur une console CP1252 | `test_console_output_survives_legacy_windows_encoding` ; le run #31611611471 a reproduit l'ancien échec fermé |
| Outil acteur détaché | Un `CTRL_C` reçu après la fin de l'acteur devient une itération refusée et récupérable ; une interruption pendant que l'acteur vit encore reste propagée | `test_post_exit_console_interrupt_becomes_retryable_actor_failure`, `test_live_actor_console_interrupt_is_not_swallowed` ; run refusé #31615708173 |

## Resultats

| Porte | Resultat | Preuve locale |
|---|---|---|
| EditMode | 71/71 réussis | `Logs/editmode-m3-scaffolding-final-integration.xml` |
| PlayMode | 1/1 réussi | `Logs/playmode-m3-scaffolding-integration.xml` |
| Échafaudages | 4/4 : niveaux 1→4, attente du terrassement, retrait final, sélection et reconstruction après reload | `Logs/editmode-m3-scaffolding-integration.xml` |
| Construction physique | 3/3 : terrassement avant livraison, quatre matériaux séquencés et reload exact | `Logs/editmode-m3-build-targeted-final-20260803.xml` |
| Parcelles organiques | 3/3 : variation, orientation, pente, jardins, extensions, démolition et reload | `Logs/editmode-m3-plot-targeted-final-20260803.xml` |
| Commerce extérieur | 3/3 : import/export, frais, délai, marchand, limites et reload | `Logs/editmode-m2-trade-targeted-v2-20260802.xml` |
| Porte M2 deux heures | 60 jours, 71 954 ticks, hash `2dc2bdb1...5541e8`, minimum 0, navigation 0, bloqués 0 | `Logs/editmode-m3-build-final-20260803.log` |
| Besoins des foyers | 3/3 : cinq besoins, pénuries, satisfaction, niveau et reload | `Logs/editmode-m2-home-targeted-20260802.xml` |
| Marché local | 3/3 : approvisionnement, couverture, consommation, rareté/prix et pénurie | `Logs/editmode-m2-market-targeted-v2-20260802.xml` |
| Stockage local | 3/3 : catégories, zones, gardiens et rééquilibrage | `Logs/editmode-m2-stock-targeted-20260802.xml` |
| Chaînes de production | 3/3 : sept recettes, consommation/production et transport physique | `Logs/editmode-m2-chain-targeted-v2-20260802.xml` |
| Agriculture | 3/3 : phases, fertilité, météo et déterminisme | `Logs/editmode-m2-farm-final-20260802.xml` |
| Simulation 30 jours | 35 989 ticks, hash `f5c411a9...753a82`, minimum 0, navigation 0, bloqués 0 | `Logs/editmode-m3-build-final-20260803.log` |
| Build Windows x64 | réussi, 308 842 899 octets | `Logs/build-m3-scaffolding-integration-v2.log` |
| Smoke final | sauvegarde runtime, 20 foyers, 30 bâtiments, 30 habitants et porte fonctionnelle réussie | `Logs/player-smoke-m3-scaffolding-integration.log` |
| Performance | 100 habitants, 60,0 FPS moyens, p95 16,683 ms sur 1 800 frames, GC p95 0 | `Logs/player-perf-m2-final-20260802.log` |
| Navigation | 100 habitants pendant 20 minutes, zéro échec, hash identique | `DeterministicNavigationGridTests.cs` dans la suite 30/30 |
| Emplois | exclusivité, horaires, trajets, absence/remplacement et reload exact | `EmploymentSimulationTests.cs`, 4/4 |
| Capture player | huit fonctions achevées, 14 employés et 4 remplacements | `Logs/player-building-review-m1-jobs-final-20260801.log` |
| Capture échafaudages | quatre phases visibles, niveaux cumulatifs 1/2/3/4 et chantier sélectionné | `Logs/Captures/Buildings/m3-scaffolding-four-phases-20260811.png` |

## Couverture fonctionnelle

- route en deux clics avec apercu vert/rouge et refus localise ;
- parcelles orientées des deux côtés, frontage/profondeur variables, maisons en
  façade, jardins et extensions persistants, pente et chevauchement contrôlés ;
- placement d'un camp forestier avec cout, limites, espacement et refus stables ;
- affectation physique de deux bûcherons au maximum, trajet réel, production
  pendant la présence et épuisement déterministe de la réserve locale ;
- A* quatre voisins déterministe, cibles bloquées récupérées et NavMesh Unity
  reconstruit après l'apparition des obstacles ;
- huit métiers exclusifs, horaires 08h–18h, trajet domicile-travail, absence et
  remplacement déterministes visibles dans le HUD ;
- revendication par les foyers, reservation et conservation du bois ;
- transport visible, fondations, ossature, maison achevee et foyer visible ;
- selection et priorite de chantier reliees a `ICityCommandSink` ;
- idle, marche, port de bois et activite de chantier differencies ;
- commandes `R`, `Z`, `B`, `Echap`, pause `Espace` et vitesses x1/x2/x4 avec
  `1`/`2`/`3` ;
- camera RTS, HUD 1080p et retours de succes, refus et manque de bois ;
- textures originales de prairie et route, dressing medieval dark-fantasy,
  post-traitement, particules et ambiance procedurale ;
- fallbacks visuels conserves si le catalogue hote est absent.

## Commandes de reproduction

```powershell
py Tools/run_unity_locked.py -- `
  'C:\Program Files\Unity\Hub\Editor\6000.0.43f1\Editor\Unity.exe' `
  -batchmode -nographics -projectPath 'C:\Users\liagr\VictoriaCityLab' `
  -runTests -testPlatform EditMode -testResults 'Logs/editmode-delivery.xml' `
  -logFile 'Logs/editmode-delivery.log'

py Tools/run_unity_locked.py -- `
  'C:\Program Files\Unity\Hub\Editor\6000.0.43f1\Editor\Unity.exe' `
  -batchmode -nographics -projectPath 'C:\Users\liagr\VictoriaCityLab' `
  -runTests -testPlatform PlayMode -testResults 'Logs/playmode-delivery.xml' `
  -logFile 'Logs/playmode-delivery.log'

py Tools/run_unity_locked.py -- `
  'C:\Program Files\Unity\Hub\Editor\6000.0.43f1\Editor\Unity.exe' `
  -batchmode -quit -projectPath 'C:\Users\liagr\VictoriaCityLab' `
  -executeMethod Victoria.CityLab.Editor.CityLabProjectSetup.BuildWindows `
  -logFile 'Logs/build-final-lighting.log'

& 'Builds\Windows\VictoriaCityLab.exe' -screen-width 1920 -screen-height 1080 `
  -screen-fullscreen 0 -citylabSmoke -logFile 'Logs/player-smoke-delivery-2.log'
```

## Isolation

Les seules ecritures de cette mission sont dans `C:\Users\liagr\VictoriaCityLab`.
Les sources Vendor ne sont jamais modifiees par l'admission ; les variantes sont
generees sous `Assets/CityLabHost/Adapted`. Aucun push distant n'est effectue.

## Validation Asset Factory hors Unity — 1er août 2026

Cette validation ne relance ni l'Editor ni le player Unity :

| Porte | Résultat | Preuve locale |
|---|---|---|
| Tests Python | 5/5 réussis | `py -m unittest Tools.AssetFactory.test_citylab_factory -v` |
| Environnement | Blender 5.2.0 LTS et Python 3.13.14 détectés | `py Tools/AssetFactory/citylab_factory.py doctor --json` |
| Inventaire | 5 sources enregistrées, 728 modèles et 55 textures | `AssetFactory/Reports/source_inventory.json` |
| Reproductibilité catalogue | inventaire à jour | `py Tools/AssetFactory/citylab_factory.py scan --check` |
| Recette pilote | 6 composants EmaceArt épinglés par SHA-256 | `py Tools/AssetFactory/citylab_factory.py recipe-check` |

Les cinq packs existants reprennent la provenance et la licence déjà consignées
dans `Assets/Vendor/THIRD_PARTY_ASSETS.md`. Aucun modèle, prefab, matériau ou
`.meta` Vendor n'a été modifié.

## Famille de scieries modulaires Factory — 1er août 2026

Cette porte valide la génération, les variantes et le schéma de construction
hors Unity. Elle ne valide pas encore l'import ni le rendu dans le player :

| Porte | Résultat | Preuve locale |
|---|---|---|
| Contrat de recette | 3 variantes minimum, 4 phases contiguës et palettes valides | 3/3 tests `Tools.AssetFactory.test_citylab_factory` |
| Sources | deux FBX EmaceArt immuables épinglés | `AssetFactory/Recipes/building_sawmill_frontier_01.json` |
| Phases FBX | base, ossature, toiture, détails ; un LOD0/1/2 par phase | `CITYLAB_BUILDING_FBX_OK`, 12 meshes pour A, B et C |
| Variante A | 39 980 / 19 990 / 7 992 triangles | mesh `ab84c55b...`, GLB `bf061342...`, FBX `5c750679...` |
| Variante B | 41 940 / 20 970 / 8 384 triangles | mesh `5c462f92...`, GLB `a21d9e48...`, FBX `0912a3b1...` |
| Variante C | 41 952 / 20 976 / 8 386 triangles | mesh `ea445f0a...`, GLB `757de673...`, FBX `184f7e4d...` |
| Déterminisme | deux GLB successifs bit-identiques pour chaque variante | hashes complets dans `AssetFactory/Manifests/building_sawmill_frontier_01.json` |
| Revue visuelle | A/B/C et les quatre étapes A inspectées | previews sous `AssetFactory/Workbench/Previews` |
| Publication | trois copies FBX conformes au workbench | `Assets/CityLabHost/Adapted/Factory/Models` |
| Import et tests Unity | différés | aucun lancement du projet Unity CityLab ; une autre session Unity indépendante est restée intacte |

Le code d'intégration prépare trois prefabs, leurs matériaux URP et quatre
`LODGroup` par prefab, puis affecte `CityVisualLibrary.lumberCampPrefabs`.
La variante est choisie de façon déterministe par l'identifiant du site. La
simulation expose une progression de construction déterministe aux seuils
0,25 / 0,55 / 0,80 / 1,00 et interdit la production avant la fin des quatre
phases. Le camp procédural historique reste le fallback.

La compilation, les tests EditMode/PlayMode et l'inspection en jeu de cette
nouvelle famille sont explicitement en attente d'une session Unity CityLab
autorisée ; la fonction n'est donc pas déclarée validée en jeu dans
`Docs/PROTOTYPE_STATUS.md`.

## Pilote de huit familles de bâtiments — 1er août 2026

Cette porte complète la scierie avec résidence, grenier, entrepôt, marché,
forge, grange et chapelle. Elle valide les artefacts hors Unity, pas encore leur
compilation ni leur jouabilité dans CityLab :

| Porte | Résultat | Preuve locale |
|---|---|---|
| Couverture | 8 familles × 3 variantes = 24 FBX publiés | `AssetFactory/Manifests/building_pilot.json` et manifest scierie |
| Construction | 4 phases cumulatives × 3 LOD dans chaque FBX, soit 288 meshes | 21 sorties `CITYLAB_BUILDING_FBX_OK` plus les 3 scieries déjà validées |
| Budgets | 21/21 nouvelles variantes sous 60 000 / 30 000 / 12 000 triangles | métriques du manifest pilote |
| Déterminisme | 21/21 GLB bit-identiques après régénération à graine constante | contrôle A/B/C : 7/7 par variante |
| Revue visuelle | vues héroïques des sept familles et quatre phases de la résidence inspectées ; chapelle corrigée puis revue | `AssetFactory/Workbench/Previews` |
| Publication | hashes des copies conformes aux sorties du workbench | `CITYLAB_BUILDING_PILOT_OK`, statut `published_pending_unity_import_validation` |
| Unity | différé à la demande de l'utilisateur | aucun lancement du projet Unity CityLab |

La résidence est préparée pour `CityVisualLibrary.housePrefabs`. Les six autres
familles disposent de champs de catalogue et d'un importeur Editor, mais ne
sont pas déclarées jouables : leurs définitions de simulation, l'import des
prefabs, la compilation et l'inspection en jeu restent à réaliser dans une
session Unity autorisée.

### Révision artistique murs et identité fonctionnelle

Le premier rendu a été refusé par l'utilisateur car les façades paraissaient
ouvertes, trop lisses et insuffisamment différenciées. La seconde passe remplace
les mêmes artefacts publiés et ajoute une porte artistique vérifiable :

| Porte | Résultat | Preuve locale |
|---|---|---|
| Murs | quatre panneaux structurels fermés par bâtiment | étape `frame` du générateur générique |
| Matériaux | résidence/marché/entrepôt pierre-bois ; grenier/grange planches ; forge brique-pierre ; chapelle pierre taillée et ardoise | champs `wall_system` et `roof_system` du catalogue/manifest |
| Détail | 5 à 6 marqueurs par fonction : sacs et treuil, quai et caisses, étals et denrées, forge et enseigne, foin et enclos, contreforts et vitraux | `identity_markers` et previews héroïques |
| Budgets révisés | LOD0 30 491–50 671 ; LOD1 15 244–25 335 ; LOD2 6 094–10 128 | 21 rapports métriques, tous sous 60k/30k/12k |
| Structure | 21/21 FBX, 12 meshes chacun | trois lots `CITYLAB_BUILDING_FBX_BATCH_OK`, 7/7 A/B/C |
| Déterminisme | 21/21 GLB bit-identiques après seconde génération | `CITYLAB_BUILDING_DETERMINISM_OK` |
| Publication | 21/21 hashes publiés conformes | `PUBLISHED_HASHES_OK` et manifest réécrit atomiquement |
| Unity | non lancé | `unity_launched=false` ; import et inspection en jeu toujours différés |

## Audit de population modulaire — 1er août 2026

| Porte | Résultat | Preuve locale |
|---|---|---|
| Source GanzSe | 217 FBX, dont 2 corps complets de la même lignée | `AssetFactory/Reports/character_modularity.json` |
| Modularité disponible | 25 cheveux, 25 barbes, 25 yeux, 25 sourcils, 5 nez, 2 oreilles | `CITYLAB_CHARACTER_AUDIT_OK` |
| Tenues disponibles | 18 pièces pour chacune des catégories torse, bras, jambes, pieds, ceinture et tête | audit par préfixes et chemins source |
| Contrat cible | 2 genres × 3 âges × 4 morphologies × 8 rôles sociaux, sélection déterministe | `AssetFactory/Catalogs/character_population.json` |
| Limite vérifiée | aucun corps féminin, enfant ou vieux distinct n'est validé dans la source | statut `source_components_ready_morphology_generation_required` |

Le chiffre de 4 800 combinaisons théoriques avant variations du visage mesure
la capacité combinatoire du catalogue, pas un nombre de personnages générés.
Les nouvelles silhouettes devront conserver un rig Humanoid commun, puis
passer une porte d'animation, de clipping des vêtements et de LOD avant toute
publication ou déclaration de jouabilité.

### Propositions visuelles de population

Huit assemblages de rôle ont été rendus hors Unity à partir du FBX GanzSe
immuable : ouvrier, riche, paysan, religieux, soldat, noble, bourgeois et
mendiant. Chaque rapport conserve les douze ou treize pièces réellement
sélectionnées et le SHA-256 source.

La revue accepte ces sorties comme directions de palette et de modularité, pas
comme personnages de production. Ouvrier, soldat, bourgeois, riche et noble
offrent une base exploitable. Les silhouettes féminines, enfant et vieille
restent des essais d'échelle ; religieux et mendiant exigent des vêtements
spécifiques. La proposition retenue est de produire six corps partagés
(deux genres × trois âges), d'appliquer ensuite quatre morphologies, puis huit
capsules de vêtements/accessoires. Preuves :
`AssetFactory/Catalogs/character_proposals.json`, huit rapports sous
`AssetFactory/Reports/Characters` et
`AssetFactory/Reports/character_proposal_review.json`.

## Production de population modulaire Factory — 1er août 2026

| Porte | Résultat | Preuve locale |
|---|---|---|
| Matrice corporelle | 6 bases genre/âge × 4 morphologies = 24 FBX | `AssetFactory/Catalogs/character_factory.json` |
| Capsules sociales | 8/8 : ouvrier, riche, paysan, religieux, soldat, noble, bourgeois, mendiant | 8 previews et 8 rapports de production |
| Rig | 52 os partagés ; 100 % des sommets exportés pondérés | 32 sorties `CITYLAB_CHARACTER_FBX_OK` |
| LOD corps | LOD0 2 272–2 576 ; LOD1 1 178–1 336 ; LOD2 540–612 | 24 rapports JSON |
| LOD rôles | LOD0 6 553–9 343 ; LOD1 3 400–4 849 ; LOD2 1 584–2 232 | tous sous 18k/9,5k/4,2k |
| Déterminisme | 32/32 empreintes canoniques identiques après régénération | `CITYLAB_CHARACTER_DETERMINISM_OK matched=32/32` |
| Publication | 32 FBX, 50 774 256 octets, hashes conformes | `AssetFactory/Manifests/character_factory.json` |
| Tests Python | 6/6 réussis | `py -3 -m unittest Tools.AssetFactory.test_citylab_factory -v` |
| Préparation CityLab | importeur Humanoid, prefabs LOD, palette URP et sélection par identifiant ajoutés | `CityLabFactoryCharacterIntegration.cs`, non exécuté |
| Unity | non lancé | import Humanoid, compilation, animation et clipping restent non validés |

La validation Blender recharge chaque FBX publié, recompte les os et les meshes,
contrôle les budgets, les valeurs finies, la couverture des poids et applique
une pose des deux bras. Elle ne remplace pas la porte Unity Humanoid ni la revue
des intersections avec les animations du jeu ; `M1-CHAR-01` reste donc ouvert
en `NEXT` derrière la porte QA Factory active.

## Admission Vendor et publication atomique — 1er août 2026

| Porte | Résultat | Preuve locale |
|---|---|---|
| Profil d'admission | provenance, licence vérifiée et SHA du FBX GanzSe concordent avec l'inventaire | `AssetFactory/AdmissionProfiles/ganzse_free_modular_character.json` |
| Découverte | un pack factice non enregistré est détecté avec alerte licence | test `test_admission_discovers_new_pack_and_validates_production_profile` |
| Dry-run | 32 sorties personnages et 50 774 256 octets vérifiés sans copie | `ASSET_FACTORY_PUBLICATION_OK mode=dry-run` |
| Copie | destination Adapted, hash et écriture atomique `.tmp` contrôlés | test `test_generic_publication_is_dry_run_by_default_and_atomic_when_explicit` |
| Unity | non lancé | `unity_launched=false` |

## Laboratoire PBR CityLab Trim v1 — 1er août 2026

| Porte | Résultat | Preuve locale |
|---|---|---|
| Cartes | BaseColor, Normal, AO, Roughness, Metallic et VariationMask en 2048² | `AssetFactory/Manifests/citylab_trim_v1.json` |
| Cohérence | bois, pierre et toiture issus du même champ de hauteur/masques | `AssetFactory/Graphs/citylab_trim_pbr_graph.json` |
| Déterminisme | 6/6 hashes identiques après régénération | `CITYLAB_PBR_TRIM_OK determinism=true` |
| Résolution | contraste minimal passé à 512, 256 et 128 px | `AssetFactory/Reports/Textures/citylab_trim_v1.json` |
| Publication | 6 537 776 octets, copies Workbench/Adapted identiques | test `test_pbr_trim_has_six_coherent_published_maps_and_three_review_scales` |
| Unity | non lancé | validation du matériau URP différée |

## QA Factory transversale — 1er août 2026

| Porte | Résultat | Preuve locale |
|---|---|---|
| Couverture | 24 bâtiments + 32 personnages + 6 textures | `AssetFactory/Reports/factory_qa.json` |
| Géométrie | 56/56 FBX, 1 038 meshes finis et 1 038 jeux d'UV valides | rapports sous `AssetFactory/Reports/QA/Fbx` |
| Structure | quatre phases/trois LOD par bâtiment ; trois LOD et rig 52 os par personnage | `Tools/AssetFactory/Blender/qa_factory_fbx.py` |
| Colliders | aucun collider embarqué dans les FBX | `embedded_colliders=0` |
| Traçabilité | cinq licences, noms, budgets et hashes Workbench/Adapted passés | `CITYLAB_FACTORY_QA_OK` |
| Tests | 10/10 réussis | `py -3 -m unittest Tools.AssetFactory.test_citylab_factory -v` |
| Revue | planche produite, approbation utilisateur en attente | `AssetFactory/Reports/QA/factory_review_board.png` |
| Unity | non lancé à la demande de l'utilisateur | import et rendu runtime non couverts par cette porte |

La QA a détecté l'absence d'UV sur plusieurs accessoires de personnages
procéduraux. Le générateur applique maintenant une projection planaire
déterministe par axe dominant ; les 32 personnages ont été régénérés, leur
déterminisme canonique UV inclus a été revérifié, puis ils ont été republiés.
`M1-ASSET-05` reste `ACTIVE` uniquement pour les portes artistique et Unity.

## Validation de sauvegarde versionnée — 1er août 2026

La prévalidation hors éditeur a été complétée par les portes Unity et player.

| Porte | Résultat | Preuve locale |
|---|---|---|
| Schéma | enveloppe v1, snapshot v1 et documentation versionnée | `Docs/SAVE_SCHEMA.md` |
| Intégrité | SHA-256 du payload vérifié avant désérialisation | `CitySaveService.TryDeserialize` |
| Atomicité | `.tmp` même dossier, flush disque, `File.Move`/`File.Replace`, nettoyage final | `CitySaveService.SaveAtomic` |
| Migration | fixture snapshot v0 avec checksum valide, migration vers v1 | `Packages/com.victoria.citymode/Tests/Fixtures/city_save_v0.json` |
| Couverture | aller-retour complet, manuel/autosave, corruption et migration | 4/4 tests dans `CitySaveServiceTests.cs` |
| Compilation ciblée | contrats et service compilés en DLL avec Roslyn et références Unity 6 | `Victoria.CityMode.SaveCompile.dll`, artefact temporaire hors dépôt |
| Contrôle hors Unity | fixture/hash et axes d'état vérifiés | suite Python 11/11 |
| Unity | validé | `Logs/editmode-m1-integration-20260801.xml`, 19/19 avec la suite complète |
| Player | round-trip exact, écriture atomique et checksum | `CITYLAB_SAVE_RUNTIME_OK` dans `Logs/player-smoke-m1-20260801.log` |

## Import Unity Asset Factory — 1er août 2026

| Porte | Résultat | Preuve locale |
|---|---|---|
| Bâtiments | 8 familles × 3 variantes importées, 4 phases × 3 LOD | `Assets/CityLabHost/Adapted/Factory/Prefabs` |
| Personnages | 8/8 rôles, avatars Humanoid valides, un LODGroup par prefab | `FactoryUnityIntegrationTests.CharacterRoles_HaveHumanoidAvatarSingleLodAuthorityAndController` |
| Catalogue | scieries, résidences, six familles futures et huit rôles renseignés | `Assets/CityLabHost/Resources/CityLabVisualLibrary.asset` |
| Régression corrigée | groupes LOD FBX retirés avant création des groupes autoritaires ; composant du camp ajouté au runtime | 19/19 EditMode et 1/1 PlayMode |
| Build | Windows x64, 308 751 907 octets | `CITYLAB_BUILD_RELEASE_OK` dans `Logs/build-m1-final-20260801.log` |
| Smoke | 20 foyers, 30 bâtiments, 30 habitants, 600 frames | `CITYLAB_PERF_OK`, 60,0 FPS et p95 16,683 ms |
| Capture | rendu hors écran non noir, inspecté sans shader rose | `Logs/Captures/m1-factory-runtime-20260801.png` |

L'import Unity ferme la porte technique de `M1-ASSET-05`, mais son approbation
artistique reste explicitement humaine. Les revues player ultérieures ont fermé
`M1-CHAR-01` et `M1-ASSET-06` : animations inspectées et huit fonctions reliées
au catalogue de simulation.

## Navigation robuste et emplois physiques — 1er août 2026

| Porte | Résultat | Preuve locale |
|---|---|---|
| Routage | grille 128² de cellules de 4 m, A* quatre voisins et départage stable | `DeterministicNavigationGrid.cs` |
| Obstacles | emprises des bâtiments et camps interdites ; cible occupée ramenée à la première cellule accessible | 2 tests unitaires de chemin |
| Stress | 100 habitants, 18 obstacles, 20 minutes simulées, zéro échec et deux snapshots identiques | `HundredVillagers_RunTwentyMinutesWithoutNavigationFailureDeterministically` |
| NavMesh Unity | construction initiale puis mise à jour asynchrone après camp ou bâtiment achevé | `CityLabGame.UpdateNavigationMesh` |
| Exclusivité | un seul enum métier et un seul lieu bâtiment/camp par habitant | `Employment_AssignsExclusivePhysicalSlotsWithoutDoubleCounting` |
| Horaires et présence | journée de 120 s, poste 08h–18h, trajet physique aller/retour ; une livraison engagée est terminée | `ScheduledWorkers_CommuteWorkAndReturnHome` |
| Absence/remplacement | décision par seed, identifiant et jour ; remplacement du poste vacant stable | `DailyAbsence_IsReplacedAndEntireRunRemainsDeterministic` |
| Persistance | affectations, chemins, présence, absences et horloge identiques après reconstruction | `EmploymentState_ReloadsWithoutReassignmentOrClockDrift` |
| Suite Unity | 30/30 EditMode et 1/1 PlayMode | `Logs/editmode-m1-jobs-final-20260801.xml`, `Logs/playmode-m1-jobs-20260801.xml` |
| Player | six bâtiments civiques terminés, capacités 120/160/24/2/12/32, 14 employés et 4 remplacements | `CITYLAB_BUILDING_REVIEW_OK` dans `Logs/player-building-review-m1-jobs-final-20260801.log` |
| Build/smoke | 308 781 075 octets ; sauvegarde runtime et scénario 20/30/30 verts | `Logs/build-m1-jobs-final-20260801.log`, `Logs/player-smoke-m1-jobs-final-20260801.log` |

Le smoke final a été lancé sans synchronisation verticale et sert ici de porte
fonctionnelle. La référence performance visible/throttlée reste la mesure p95
16,683 ms déjà consignée plus haut ; elle n'est pas remplacée par la valeur
headless artificiellement rapide.

## Logistique générique — 2 août 2026

| Porte | Résultat | Preuve locale |
|---|---|---|
| Contrat | Tâche persistante avec ressource, priorité, source, destination, quantités réservée/en transit/livrée et statut | `LogisticsTaskState` dans `CityContracts.cs` |
| Extrémités | Stock global, bâtiment et site de production ; livraison non liée à un chantier | `EnqueueLogisticsTask` et test vers un `ProductionSiteState` |
| Priorité | La tâche haute est réservée avant une tâche basse créée plus tôt | `Priority_SelectsHighestTaskBeforeCreationOrder` |
| Concurrence | Deux travailleurs ne réservent ensemble que les cinq unités disponibles | `ConcurrentWorkers_ReserveEachUnitAtMostOnce` |
| Pénurie | Cinq unités sur dix livrées, reste actif, aucun inventaire négatif et conservation exacte | `SourceShortage_LeavesRemainderPendingWithoutNegativeInventory` |
| Destruction | Destination supprimée pendant le trajet ; tâche annulée, réservations et cargaisons restituées | `DestroyedDestination_CancelsTaskAndReturnsReservationsAndCargo` |
| Régression Unity | 34/34 EditMode et 1/1 PlayMode | `Logs/editmode-m1-log-final-20260802.xml`, `Logs/playmode-m1-log-20260802.xml` |

## Calendrier et saisons persistants — 2 août 2026

| Porte | Résultat | Preuve locale |
|---|---|---|
| Calendrier | 120 s/jour, 30 jours/mois, 12 mois/an ; date et heure dérivées de l'horloge déterministe | `CityCalendarState`, `Calendar_CrossesMonthSeasonAndYearDeterministically` |
| Saisons | hiver, printemps, été et automne par trimestres ; franchissements mois 4 et nouvel an validés | test ciblé 1/3 |
| Événements | planification par temps absolu, ordre date puis identifiant, statuts pending/triggered/cancelled | `ScheduledEvents_TriggerInStableTimeThenIdOrder` |
| Persistance | pause, vitesse de reprise x4, date et événement en attente identiques après checksum et reconstruction | `ClockPauseCalendarAndPendingEvents_ReloadExactly` |
| HUD | date complète, heure/minute, saison et vitesse issues du snapshot | `CityLabHud.Refresh` |
| Régression Unity | 37/37 EditMode et 1/1 PlayMode | `Logs/editmode-m1-time-final-20260802.xml`, `Logs/playmode-m1-time-20260802.xml` |

## Harnais de simulation longue — 2 août 2026

| Porte | Résultat | Preuve locale |
|---|---|---|
| Scénario | 30 habitants, 8 chantiers, 6 fonctions civiques, scierie et 30 jours simulés sans GameObject | `LongRunSimulationTests.CreateReferenceSnapshot` |
| Déterminisme | deux exécutions et leurs JSON finaux identiques | hash SHA-256 courant `1691010ea6b399f86f7c370a946bff960376711c4135275da2e48d85cde2c6ec` |
| Ressources | contrôle à chaque tick du stock, des porteurs, sites et tâches logistiques | minimum observé 0, aucune valeur négative |
| Agents | surveillance des activités de déplacement et des échecs A* | `navigationFailures=0`, `blockedAgents=0` |
| Durée | 35 989 ticks fixes de 0,1 s | `CITYLAB_LONG_RUN_OK seed=140001 days=30` |
| Régressions corrigées | oscillation réservation/emploi et trajet de chantier sans tâche logistique | suite historique six bâtiments à nouveau verte |
| Suite Unity | 38/38 EditMode et 1/1 PlayMode | `Logs/editmode-m1-longrun-final-20260802.xml`, `Logs/playmode-m1-longrun-20260802.xml` |

## Budget performance 100 habitants — 2 août 2026

| Porte | Résultat | Preuve locale |
|---|---|---|
| Build | Windows x64, 308 796 131 octets | `CITYLAB_BUILD_RELEASE_OK` dans `Logs/build-m1-perf-20260802.log` |
| Scénario | 20 foyers, 30 bâtiments et 100 habitants | `CITYLAB_SMOKE_SCENARIO` dans le log player |
| Mesure | chauffe 120 frames puis 1 800 frames en 1920×1080 | drapeau player `-citylabPerf` |
| CPU/frame | moyenne 16,650 ms, p95 16,650 ms, 60,1 FPS | `CITYLAB_PERF_OK` dans `Logs/player-perf-m1-20260802.log` |
| Boucle centrale | 100 habitants et 1 200 ticks après chauffe, 0 collecte gen0 | `CITYLAB_CORE_ALLOC_OK` dans `Logs/editmode-m1-perf-final-20260802.log` |
| Régression | 39/39 EditMode et 1/1 PlayMode | `Logs/editmode-m1-perf-final-20260802.xml`, `Logs/playmode-m1-perf-20260802.xml` |

## Registre de six ressources — 2 août 2026

| Porte | Résultat | Preuve locale |
|---|---|---|
| Couverture | bois, planches, pierre, nourriture, outils et textile, chacun avec clé et unité | `CityResourceRegistry.CreateDefault`, 6/6 uniques |
| Persistance | un `ResourceStockState` par ressource dans le snapshot ; miroir bois compatible | `DefaultRegistry_DefinesSixUniqueResourcesWithUnitsAndStorage` |
| Stockage | capacité explicite, ajout partiel et refus du débordement | `Storage_ClampsOverflowAndReservationsCannotDuplicateOrOverconsume` |
| Réservations | disponibilité non dupliquée, consommation réservée atomique, libération sûre | même test |
| Pertes | taux journalier en millièmes, reste entier persistant, quantité réservée protégée | `DailyLosses_ProtectReservationsAndReloadDeterministically` |
| Régression | 42/42 EditMode et 1/1 PlayMode | `Logs/editmode-m2-res-final-20260802.xml`, `Logs/playmode-m2-res-20260802.xml` |

## Collecte et consommation alimentaire — 2 août 2026

| Porte | Résultat | Preuve locale |
|---|---|---|
| Sources | bosquet et territoire de chasse finis, accessibles/inaccessibles, capacité de travailleurs | `FoodSourceState` |
| Collecte | métiers cueilleur/chasseur, trajet source, travail temporisé et retour physique au stock | `Forager_TravelsToAccessibleSourceAndPhysicallyReturnsFood` |
| Consommation | ration quotidienne selon taille du foyer, consommation partielle et jours de faim persistants | `Households_ConsumeDailyChooseAccessibleSourceAndPersistShortage` |
| Accessibilité | source inaccessible ignorée au profit de la source valide la plus proche, départage par ID | même test |
| Persistance | collecte, portage, stock et consommation identiques après reconstruction | `FoodCollectionAndConsumption_ReloadDeterministically` |
| Player | deux sources initiales, quatre postes et nourriture/faim visibles dans le HUD | `CityLabGame.EnsureInitialFoodSources`, `CityLabHud.Refresh` |
| Régression | 45/45 EditMode et 1/1 PlayMode | `Logs/editmode-m2-food-final-20260802.xml`, `Logs/playmode-m2-food-20260802.xml` |

## Parcelles organiques — 3 août 2026

| Porte | Résultat | Preuve locale |
|---|---|---|
| Forme | jusqu'à 16 lots par route, largeur 10,5–14,5 m et profondeur 22–36 m, orientation sur chaque côté | `ZoningCreatesVariableOrientedGardenParcelsDeterministically` |
| Terrain | cinq échantillons par lot, valeurs non finies refusées, pente maximale 180 ‰ et limites de carte contrôlées | `TerrainSlopeRejectsOnlyInvalidLotsWithStableOutcome` |
| Occupation | test SAT de rectangles orientés empêchant les chevauchements entre routes et parcelles existantes | `LocalCitySimulation.OverlapsExistingParcel` |
| Évolution | maison placée en façade ; jardin actif après achèvement ; 0/1/2 extensions selon niveau et capacité du lot | `CompletedHomesActivateGardensAndLevelBoundedExtensionsAcrossReload` |
| Persistance | profondeur, orientation, pente, jardin, capacité et niveau d'extension passent le checksum/reload | test dédié 3/3 |
| Navigation/UI | profondeur bloquée étendue avec la maison ; jardin/extensions visibles et compteurs HUD | `DeterministicNavigationGrid`, `CityLabGame.SyncParcel`, `CityLabHud.Refresh` |
| Régression | 67/67 EditMode, hash 30 jours inchangé `8caf8646...1a5602`, 1/1 PlayMode | logs M3 du 3 août 2026 |
| Player | build 308 831 859 octets ; smoke 20 foyers/30 bâtiments/30 habitants, sauvegarde et performance verts | `Logs/build-m3-plot-final-20260803.log`, `Logs/player-smoke-m3-plot-final-20260803.log` |

## Construction physique, premier incrément — 3 août 2026

| Porte | Résultat | Preuve locale |
|---|---|---|
| Terrassement | cinq hauteurs sous l'emprise, altitude moyenne, dénivelé persistant en millimètres et travail achevé avant toute livraison | `NewSite_PreparesSampledTerrainBeforeRequestingMaterials` |
| Matériaux | bâtiments civiques séquencés fondations/pierre, charpente/bois, couverture/planches et finitions/outils | `CivicSite_ConsumesPhaseSpecificMaterialsInOrder` |
| Ouvriers/logistique | l'équipe affectée priorise les transports de son chantier sans casser la logistique générique des sauvegardes historiques | suite logistique 4/4 et scénario six fonctions |
| Persistance | terrassement, manifeste des matériaux, livraisons et phase passent le checksum puis continuent bit à bit | `MidConstruction_SaveReloadRemainsBitExact` |
| Monde/HUD | terrassement visible avant les fondations ; phase et matériau courant affichés dans la fiche de chantier | `CityLabGame.SyncBuilding`, `CityLabHud.Refresh` |
| Régression | 70/70 EditMode ; 35 989 ticks/hash `f5c411a9...753a82` ; 60 jours/hash `2dc2bdb1...5541e8` ; 1/1 PlayMode | `Logs/editmode-m3-build-final-20260803.xml`, `Logs/playmode-m3-build-final-v2-20260803.xml` |
| Player | build 308 836 067 octets ; smoke 20 foyers/30 bâtiments/30 habitants, sauvegarde et `CITYLAB_PERF_OK` | `Logs/build-m3-build-final-v2-20260803.log`, `Logs/player-smoke-m3-build-final-v2-20260803.log` |
| Suite au 3 août pour `M3-BUILD-01` | Échafaudages, réparation et démolition restaient requis ; la tâche demeurait `ACTIVE` | `Docs/ROADMAP.md` |

## Construction physique, échafaudages — 11 août 2026

| Porte | Résultat | Preuve locale |
|---|---|---|
| Synchronisation | Quatre niveaux cumulatifs suivent fondations, charpente, couverture et finitions ; aucun échafaudage avant terrassement et retrait complet à l'achèvement | `Scaffolding_FollowsFourPersistedPhasesAndSelection` |
| Persistance | La vue est reconstruite depuis `phase` et `terrainPrepared` déjà versionnés ; le round-trip conserve exactement le niveau 3/4 de la phase couverture | `Logs/editmode-m3-scaffolding-integration.xml` |
| Sélection/HUD | Le clic reste porté par l'emprise du chantier, active contour et fanions dorés ; le reload réapplique le surlignage et le HUD affiche `Échafaudage n/4` | `Logs/playmode-m3-scaffolding-integration.xml`, `CityLabHud.Refresh` |
| Régression | 71/71 EditMode ; 35 989 ticks/hash `f5c411a9...753a82` ; 60 jours/hash `2dc2bdb1...5541e8` ; zéro allocation centrale | `Logs/editmode-m3-scaffolding-final-integration.xml` |
| Player | Build 308 842 899 octets ; smoke 20 foyers/30 bâtiments/30 habitants et sauvegarde runtime verts | `Logs/build-m3-scaffolding-integration-v2.log`, `Logs/player-smoke-m3-scaffolding-integration.log` |
| Revue visuelle | Quatre chantiers cadrés ensemble avec niveaux visibles 1/2/3/4 et marqueurs de sélection sur la phase finitions | `CITYLAB_SCAFFOLDING_REVIEW_OK`, `Logs/Captures/Buildings/m3-scaffolding-four-phases-20260811.png` |
| Suite de `M3-BUILD-01` | L'incrément 01 passe à `PROUVÉ` ; usure, panne et réparation deviennent `EN_COURS` | `Docs/ROADMAP.md` |
