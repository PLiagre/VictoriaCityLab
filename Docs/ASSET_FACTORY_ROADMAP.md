# Roadmap Asset Factory intégrée

Cette roadmap détaille `M1-ASSET-*` dans `Docs/ROADMAP.md`. L'objectif est de
faire de Victoria CityLab le projet de jeu **et** la source de vérité de son
usine visuelle, sans ouvrir Unity pendant la génération, la normalisation, le
rendu de revue ou la QA.

## Décision d'architecture

Le cœur de production est **Blender 5.2 LTS en ligne de commande, piloté par des
scripts Python versionnés**. Un MCP Blender peut servir à une exploration ou à
une revue interactive, mais il ne devient pas le moteur de build : une suite
d'appels conversationnels est plus difficile à rejouer, comparer et tester
qu'une recette et une graine.

Pour les textures, l'ordre retenu est :

1. matériaux procéduraux Blender et baking PBR pour démarrer sans coût ni nouvel
   outil ;
2. Material Maker pour les graphes de matériaux réutilisables et les exports
   PBR automatisables vers Unity ;
3. Substance 3D Designer/Painter seulement si le volume de production et le
   budget justifient une chaîne payante.

La Factory ne demande jamais à une IA distante de transformer un asset Unity
Store. Les sources Store peuvent être modifiées localement pour un produit sous
réserve de leur licence, mais leurs fichiers sources ou dérivés ne doivent pas
être redistribués comme une bibliothèque extractible.

## Architecture cible

```text
Assets/<éditeur>/<pack>/                 sources Unity Store immuables
        |
        v
AssetFactory/Recipes/<asset>.json        fonction + style + graine + composants
        |
        v
Blender headless                         assemblage, UV, bake, LOD, collision
        |
        +--> AssetFactory/Workbench/     FBX/GLB, textures et previews jetables
        +--> AssetFactory/Manifests/     provenance, hashes, métriques, revue
        |
        v
Assets/CityLabHost/Adapted/Factory/      publication approuvée seulement
```

La simulation dans `Packages/com.victoria.citymode` ne référence jamais un pack
Vendor. Le catalogue hôte associe un identifiant fonctionnel aux variantes
publiées.

## Stratégie de production efficace

### Géométrie

- Construire un **kit modulaire** (socles, murs, angles, pans de bois, portes,
  fenêtres, toits, cheminées, auvents, accessoires) au lieu d'un script par
  bâtiment.
- Séparer la **fonction** (scierie, ferme, grenier, marché) du **style**
  (frontière, village, religieux, fortifié) et de la **variation** (graine).
- Employer les packs Store comme banque de composants et référence de
  proportions ; chaque composant retenu est épinglé par chemin et SHA-256.
- Générer une source GLB stable, puis des dérivés FBX/LOD. Les conteneurs FBX ne
  sont pas utilisés comme preuve de déterminisme binaire.
- Produire automatiquement pivot au sol, échelle métrique, orientation Unity,
  UV, trois LOD, collision simple et preview aux distances réelles de la caméra.
- Exporter chaque bâtiment en quatre couches cumulatives obligatoires : base en
  pierre, ossature, toiture, puis équipement et décor. Chaque couche possède
  ses propres LOD0/1/2 et peut être activée par le progrès de construction.
- Générer au moins trois variantes par famille à finition constante : palettes
  bois/toiture, contreventements, côté des annexes et piles, enseigne et
  cheminée sont pilotés par la recette et la graine.

### Textures

- Partager un **trim sheet** et un petit atlas par famille architecturale plutôt
  qu'une texture unique par maison.
- Baker `BaseColor`, `Normal`, `AO`, `Roughness/Smoothness`, `Metallic` et un
  masque de variation ; créer les matériaux URP seulement à la publication.
- Utiliser des masques pour teinte, humidité, suie, mousse et usure afin de
  produire beaucoup de variantes avec les mêmes textures.
- Réserver les textures uniques aux éléments héroïques. Cible initiale : 1024
  à 2048 px par atlas partagé, à confirmer par captures et mesures.

## Lots ordonnés

| ID | État | Livrable | Porte de sortie |
|---|---|---|---|
| `M1-ASSET-01` | DONE | Noyau CityLab hors Unity | `doctor`, inventaire SHA-256, test déterministe et `scan --check` verts. |
| `M1-ASSET-02` | DONE | Admission Vendor et recettes | Profil versionné, provenance/licence et SHA obligatoires, découverte testée et copie atomique en dry-run par défaut. |
| `M1-ASSET-03` | DONE | Kit architectural et grammaire Blender | Maison, scierie et grenier A/B/C produits avec fonction/style/graine séparés, quatre phases, trois LOD et déterminisme vérifié. |
| `M1-ASSET-04` | DONE | Laboratoire textures PBR | Six cartes 2048², graph/recette, déterminisme et comparaison 512/256/128 publiés hors Unity. |
| `M1-ASSET-05` | ACTIVE | QA et publication | Porte technique passée sur 56 FBX/1 038 meshes et 6 textures ; approbation artistique et import Unity restent ouverts. |
| `M1-ASSET-06` | NEXT | Pilote de contenu | Seconde passe publiée avec murs fermés, systèmes pierre/bois/brique/planches et marqueurs métier ; approbation artistique, import Unity et raccord fonctionnel restent à valider. |
| `M1-CHAR-01` | NEXT | Population modulaire | 24 corps et 8 capsules de rôle publiés sur 52 os et trois LOD ; déterminisme et FBX validés hors Unity. Approbation, import Humanoid et test animation/clipping restent ouverts. |

## Pilote généré

`building_sawmill_frontier_01` est la première famille de production de la
grammaire. Ses variantes A, B et C combinent deux sources EmaceArt immuables
avec une structure, une scie, un chariot, un volant et une toiture générés.
Elles diffèrent par palette, ossature, implantation, enseigne et cheminée sans
perdre les marqueurs fonctionnels. La recette, les graines, les hashes, les
métriques et les portes QA sont consignés dans
`AssetFactory/Manifests/building_sawmill_frontier_01.json`.

Les trois FBX approuvés sont publiés sous
`Assets/CityLabHost/Adapted/Factory/Models`. Chacun contient douze meshes :
quatre phases et trois LOD. Le script Editor CityLab créera les trois prefabs,
leurs matériaux URP, quatre `LODGroup` par prefab et leur affectation au
catalogue lors du prochain import CityLab autorisé.

La grammaire générique couvre aussi résidence, grenier, entrepôt, marché,
forge, grange et chapelle. Ces sept familles ajoutent 21 FBX publiés, chacun
composé de quatre phases cumulatives et trois LOD. La seconde passe ferme les
quatre murs et applique un système data-driven : résidence et marché en
pierre/bois, grenier et grange en planches, forge en brique sur soubassement de
pierre et chapelle en pierre taillée avec contreforts et toiture d'ardoise.
Chaque famille expose cinq à six marqueurs fonctionnels. Le catalogue source et
le manifest de publication sont `AssetFactory/Catalogs/building_pilot.json` et
`AssetFactory/Manifests/building_pilot.json`.

## Population modulaire produite hors Unity

`AssetFactory/Catalogs/character_population.json` définit une sélection
déterministe par graine et identifiant d'habitant : masculin/féminin,
enfant/adulte/vieux, quatre morphologies, cheveux, visage et huit rôles
sociaux. Le pack GanzSe fournit une banque utile de pièces, mais seulement une
lignée de corps/rig de départ. Les silhouettes féminines, enfants et âgées ne
sont donc pas déclarées prêtes : elles doivent être produites par déformation
contrôlée sur un squelette commun, puis testées avec les animations et les
tenues. L'audit vérifiable se trouve dans
`AssetFactory/Reports/character_modularity.json`.

Huit propositions visuelles initiales ont été générées dans
`AssetFactory/Workbench/Previews/Characters`. Elles sélectionnent réellement
les pièces GanzSe et ajoutent quelques marqueurs de rôle, mais ne sont ni des
prefabs publiés ni des rigs approuvés. La revue recommande six corps de base
(masculin/féminin × enfant/adulte/vieux), puis huit capsules de vêtements ; les
tenues religieuses et de mendiant doivent être créées spécifiquement. La synthèse
est `AssetFactory/Reports/character_proposal_review.json`.

La passe de production est maintenant décrite par
`AssetFactory/Catalogs/character_factory.json`. Elle publie 24 corps (six bases
genre/âge × quatre morphologies) et huit capsules — ouvrier, riche, paysan,
religieux, soldat, noble, bourgeois et mendiant — sous
`Assets/CityLabHost/Adapted/Factory/Characters`. Chaque FBX conserve les 52 os
du rig commun, trois niveaux de LOD et des pièces séparées skinnées. Le manifest
`AssetFactory/Manifests/character_factory.json` épingle toutes les sorties.

`CityLabFactoryCharacterIntegration.cs` prépare l'import Humanoid, huit prefabs
avec `LODGroup` et leur affectation déterministe à `CityVisualLibrary`. Cette
intégration n'a pas encore été exécutée : l'utilisateur a interdit le lancement
d'Unity pendant cette session.

## Admission, textures et QA transversale

Le profil `AssetFactory/AdmissionProfiles/ganzse_free_modular_character.json`
épingle la provenance, la licence et le SHA-256 du FBX réellement exploité.
`admission-discover` signale tout nouveau dossier d'assets non enregistré ; le
test automatisé prouve la détection sans dépendre d'un nouvel import présent sur
la machine. `publication-check` vérifie manifest, hashes et frontière Adapted,
puis reste un dry-run sauf demande explicite `--publish`.

Le laboratoire `citylab_trim_v1` produit de façon déterministe un trim sheet
2048² en trois bandes — bois structurel, pierre taillée et tuiles — accompagné
de `BaseColor`, `Normal`, `AO`, `Roughness`, `Metallic` et `VariationMask`.
Recette, graph, rapport et manifest conservent la graine et les hashes. La
planche de résolution contrôle séparément la lecture à 512, 256 et 128 px.

La QA transversale recharge les 56 FBX publiés dans Blender et contrôle valeurs
finies, noms, structure LOD, UV, rig et absence de colliders embarqués. Elle
recoupe ensuite les sources, licences, hashes workbench/publiés, budgets, six
textures et previews. Le résultat est de 1 038 meshes, tous avec UV, zéro
collider et 101 474 304 octets publiés. La porte technique est passée ;
`M1-ASSET-05` reste `ACTIVE` jusqu'à l'approbation de la planche de revue et à
l'import Unity explicitement différé.

## Commandes disponibles aujourd'hui

```powershell
py Tools/AssetFactory/citylab_factory.py doctor
py Tools/AssetFactory/citylab_factory.py scan
py Tools/AssetFactory/citylab_factory.py scan --check
py Tools/AssetFactory/citylab_factory.py recipe-check
py Tools/AssetFactory/citylab_factory.py admission-discover
py Tools/AssetFactory/citylab_factory.py admission-check --write-report
py Tools/AssetFactory/citylab_factory.py publication-check AssetFactory/Manifests/character_factory.json
py -m unittest Tools.AssetFactory.test_citylab_factory -v
py Tools/AssetFactory/publish_character_factory.py --publish
py Tools/AssetFactory/qa_factory_release.py
# Pour chaque FBX publié :
blender --background --factory-startup --python `
  Tools/AssetFactory/Blender/validate_building_fbx.py -- --fbx <asset.fbx>
```

Ces commandes ne lancent pas Unity. `CITYLAB_BLENDER` peut remplacer le chemin
de Blender enregistré dans `AssetFactory/config.json`.

## Règles de publication

- Une source Store reste dans son dossier d'origine et n'est jamais écrasée.
- Toute sortie de travail reste hors de `Assets/` jusqu'à validation humaine.
- Seule une copie approuvée entre sous `Assets/CityLabHost/Adapted/Factory`.
- Aucun `.meta` n'est écrit à la main ; Unity les créera lors d'une future
  publication autorisée.
- L'outil de publication est en dry-run par défaut et ne peut jamais ouvrir
  Unity implicitement.
