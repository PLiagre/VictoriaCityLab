# Victoria CityLab

Vertical slice Unity autonome du mode ville de Victoria. Ce depot ne partage ni
`Library/`, ni caches d'import, ni verrou avec les projets Victoria. Il valide
une boucle de construction et une premiere economie forestiere ; ce n'est pas
encore un jeu AAA complet.

La source de vérité du développement est `Docs/ROADMAP.md`. Toute session doit
la lire et exécuter `Tools/check_roadmap.ps1` avant de modifier le projet ; les
règles permanentes correspondantes sont définies dans `AGENTS.md`.

## Ouvrir le projet

- Unity : `6000.0.43f1`
- Pipeline : URP `17.0.4`
- Entree : `Assets/CityLabHost/Scenes/CityLab.unity`

Au premier import, lancer `Victoria > CityLab > Configure Project` si la scene
ou l'asset URP n'ont pas encore ete generes. En batch :

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.0.43f1\Editor\Unity.exe' `
  -batchmode -quit -projectPath C:\Users\liagr\VictoriaCityLab `
  -executeMethod Victoria.CityLab.Editor.CityLabProjectSetup.Configure
```

Pour une execution automatisee, passer par `Tools/run_unity_locked.py`. Son
verrou est stocke dans le `Library` CityLab et n'interagit jamais avec le
verrou du projet Victoria. Un Editor ouvert manuellement reste volontairement
hors de ce verrou advisory.

## Commandes du prototype

- `WASD` ou bords de l'ecran : deplacer la camera
- molette : zoomer
- bouton droit + mouvement : rotation et inclinaison
- `F` : recentrer la camera
- `R`, puis deux clics au sol : tracer une route
- l'apercu de route devient vert si le trace est valide et rouge sinon
- `Z`, puis clic sur une route : creer des parcelles residentielles
- `B`, puis clic au sol : fonder un camp de bucherons
- `Echap` : annuler l'outil actif
- `Espace` : mettre la simulation en pause ou la reprendre
- `1`, `2`, `3` : regler la vitesse de simulation sur x1, x2 ou x4
- en mode inspection, cliquer un chantier puis choisir sa priorite dans le HUD

Le village commence avec huit habitants, six foyers sans logement et un stock
de bois. Les habitants transportent physiquement le bois jusqu'aux chantiers.
Le bois porte est materialise par un petit faisceau de buches. Les chantiers
passent des fondations a l'ossature en bois, puis au prefab final.

Le camp forestier coute huit unites de bois disponible. Son placement est
limite a la couronne forestiere autour du bourg et respecte un espacement entre
camps. Il affecte de maniere deterministe jusqu'a deux habitants et transforme
progressivement une reserve locale finie de bois en stock utilisable. Cette
boucle permet de relancer la construction apres epuisement du stock initial,
tant que la reserve locale du camp n'est pas epuisee.

## Direction artistique

La cible actuelle est un medieval dark-fantasy stylise : silhouettes lisibles,
palette terre, bois, bronze et braises, vegetation peinte et interface de
chronique seigneuriale. Les references d'intention sont la lisibilite de
`World of Warcraft` et l'atmosphere de `Warhammer`, sans reprendre leurs assets.

Deux textures originales propres a CityLab sont integrees au rendu :
`StylizedMeadow_Albedo.png` et `StylizedRoad_Albedo.png`, toutes deux en
1254 x 1254. Elles accompagnent les materiaux URP, le relief, les chemins fondus
au terrain, le couvert vegetal, les clotures, le puits, le marche, le feu, les
particules et l'ambiance sonore procedurale. Le rendu constitue une base
artistique coherente de vertical slice ; les contenus, animations, effets et
variations requis pour une production AAA restent a developper.

## Validation locale

Les tests et builds automatises passent toujours par `Tools/run_unity_locked.py`.
Les resultats XML, journaux de build et captures du player Windows sont conserves
sous `Logs/`; le build de travail est produit sous `Builds/Windows`.
La livraison du 31 juillet 2026 valide 12 tests EditMode, 1 test PlayMode, le
build Windows x64 et un smoke test a 60,0 FPS sur la machine de validation.
Le detail des portes, commandes et derniers resultats est conserve dans
`Docs/VALIDATION.md`.

La sauvegarde versionnee est preparee dans le runtime et documentee dans
`Docs/SAVE_SCHEMA.md`. Son execution F5/F9 et son autosave restent volontairement
hors de la liste des fonctions validees jusqu'a la prochaine session Unity
CityLab autorisee.

## Frontiere d'integration

Le package embarque `Packages/com.victoria.citymode` expose
`ICityStateSource` et `ICityCommandSink`. La simulation locale n'est qu'un
adaptateur de prototype et devra etre remplacee par un adaptateur Victoria.

## Assets tiers

Les imports Unity Store restent intacts dans leur dossier racine d'origine afin
de préserver les mises à jour. CityLab ne consomme que les copies normalisées
de `Assets/CityLabHost/Adapted`; les sources et leur licence sont répertoriées
dans `Assets/Vendor/THIRD_PARTY_ASSETS.md` et auditées dans
`Docs/VENDOR_AUDIT.md`.

L'Asset Factory est maintenant intégrée à CityLab : son code hors Unity vit dans
`Tools/AssetFactory`, et ses recettes, manifests, rapports et workbench dans
`AssetFactory`. Le worktree historique `VictoriaProject-assets` sert seulement
de référence pendant la migration.

```powershell
py Tools/AssetFactory/citylab_factory.py doctor
py Tools/AssetFactory/citylab_factory.py scan
py Tools/AssetFactory/citylab_factory.py scan --check
py Tools/AssetFactory/citylab_factory.py recipe-check
py Tools/AssetFactory/citylab_factory.py admission-discover
py Tools/AssetFactory/citylab_factory.py admission-check --write-report
py Tools/AssetFactory/citylab_factory.py publication-check AssetFactory/Manifests/character_factory.json
py Tools/AssetFactory/publish_building_pilot.py --publish
py Tools/AssetFactory/publish_character_factory.py --publish
py Tools/AssetFactory/qa_factory_release.py
```

Ces commandes ne lancent pas Unity. La roadmap de production 3D et textures est
documentée dans `Docs/ASSET_FACTORY_ROADMAP.md`.

Les recettes de bâtiment doivent respecter le schéma Factory commun : quatre
phases cumulatives (`foundation`, `frame`, `roof`, `details`), trois LOD par
phase et au moins trois variantes procédurales. Le pilote
`building_sawmill_frontier_01` publie les variantes A/B/C sous
`Assets/CityLabHost/Adapted/Factory/Models`.

Les sept autres familles du pilote sont pilotées par
`AssetFactory/Catalogs/building_pilot.json`. Les propositions de population et
leur revue se trouvent dans `AssetFactory/Catalogs/character_proposals.json` et
`AssetFactory/Reports/character_proposal_review.json`. La passe de production
publie 24 corps morphologiques et huit capsules de rôle via
`AssetFactory/Catalogs/character_factory.json` et
`AssetFactory/Manifests/character_factory.json`. Les FBX et le rig sont validés
hors Unity ; import Humanoid, animations et clipping restent à contrôler dans
une session Unity autorisée.

Le laboratoire PBR publie un trim sheet 2048² bois/pierre/toiture et ses six
cartes cohérentes sous `Assets/CityLabHost/Adapted/Factory/Textures/CityLabTrimV1`.
Le rapport transversal `AssetFactory/Reports/factory_qa.json` contrôle les 56
FBX, leurs UV/LOD/noms/budgets et les six textures. La copie générique reste en
dry-run tant que `publication-check` n'est pas appelé avec `--publish`.
