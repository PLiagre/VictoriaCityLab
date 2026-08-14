# Audit des assets Vendor — CityLab

Généré par l'outil d'admission CityLab. Les sources restent intactes dans leurs dossiers Unity Store ; seuls des prefabs adaptés sont utilisés par le prototype.

L'inventaire machine lisible de l'Asset Factory intégrée est
`AssetFactory/Reports/source_inventory.json`. Il épingle les candidats 3D et
textures par SHA-256 sans ouvrir Unity ; `scan --check` refuse un inventaire
périmé.

| Pack | Fichiers | Prefabs | Modèles | Scripts | Shaders | Décision |
|---|---:|---:|---:|---:|---:|---|
| `Assets/DoubleL` | 378 | 1 | 147 | 1 | 0 | Réserve animation (non activé dans le slice) |
| `Assets/EmaceArt` | 590 | 238 | 213 | 0 | 1 | Admis via variante CityLab |
| `Assets/Kevin Iglesias` | 191 | 2 | 121 | 2 | 1 | Admis via variante CityLab |
| `Assets/Polytope Studio` | 186 | 36 | 30 | 3 | 7 | Admis via variante CityLab |
| `Assets/URP GanzSe Free Modular Character Pack` | 459 | 217 | 217 | 1 | 0 | Admis via variante CityLab |

## Sélection active

- Catalogue runtime valide : **oui**.
- Décor runtime admis complet (2 buissons / 2 rochers / 2 herbes / 3 accessoires) : **oui**.
- EmaceArt : deux maisons composites actives, une troisième variante admise mais écartée visuellement, un bâtiment central, un tas de bois, deux buissons, deux rochers, deux herbes et trois accessoires médiévaux.
- GanzSe : personnage modulaire normalisé et réduit à 11 pièces visibles (contre 216 renderers dans la source), débarrassé des scripts de démonstration ; la source Vendor reste intacte.
- Kevin Iglesias : idle et marche Humanoid sans root motion pilotés par CityLab.
- Polytope : deux arbres normalisés, distribués de façon déterministe en périphérie.
- DoubleL : pack conservé pour une future action de chantier ; aucun asset DoubleL n'est requis par le slice actuel.

## Dérivé Asset Factory publié

| Identifiant | Sources Vendor immuables | Variante CityLab | État |
|---|---|---|---|
| `building_sawmill_frontier_01` | `EA03_Village_OutBuilding_WoodRoof_01b.fbx` et `EA03_Prop_Forester_wooden_01d.fbx`, pack EmaceArt, hashes consignés dans le manifest | `building_sawmill_frontier_01_a.fbx`, `_b.fbx` et `_c.fbx` sous `Assets/CityLabHost/Adapted/Factory/Models` | Trois variantes, quatre phases et trois LOD validés hors Unity ; import et validation en jeu en attente |
| Pilote architectural (7 familles) | Porche, échelle, caisse, banc, poêle, hovel et enseigne EmaceArt, chemins et SHA-256 dans `building_pilot.json` | résidence, grenier, entrepôt, marché, forge, grange et chapelle, variantes A/B/C sous `Assets/CityLabHost/Adapted/Factory/Models` | 21 FBX, quatre phases et trois LOD validés hors Unity ; import et validation en jeu en attente |
| Population modulaire (32 sorties) | `GanzSe Free Modular Character 1_1.fbx`, SHA-256 `0ecbc9e4...a02b`, source Vendor immuable | 24 corps et 8 rôles sous `Assets/CityLabHost/Adapted/Factory/Characters` | 52 os, trois LOD, 32/32 FBX et déterminisme validés hors Unity ; import Humanoid et animations en attente |
| Trim PBR CityLab v1 | Production procédurale originale, sans source Vendor | six cartes 2048² sous `Assets/CityLabHost/Adapted/Factory/Textures/CityLabTrimV1` | Bois, pierre et toiture ; déterminisme et lisibilité 512/256/128 validés hors Unity |

La famille est une composition procédurale originale issue d'une recette
versionnée. Les deux fichiers Vendor restent inchangés ; le manifest
`AssetFactory/Manifests/building_sawmill_frontier_01.json` conserve leur
provenance, leurs SHA-256, la graine, les métriques et le hash publié.
Le manifest `AssetFactory/Manifests/building_pilot.json` assure la même
traçabilité pour les sept autres familles et leurs 21 variantes.
`AssetFactory/Manifests/character_factory.json` conserve la provenance, les
hashes FBX et canoniques, les budgets et les portes Unity encore ouvertes des
32 personnages dérivés.

## Port City Mode v1

Le package privé `com.victoria.citymode.assets` admet onze copies de production
sans modifier ni déplacer les sources : six textures PBR originales, les deux
textures originales prairie/route et les trois FBX de scierie déjà approuvés.
Unity a généré onze GUID cible distincts ; le manifeste
`Docs/Integration/city-mode-asset-port-v1.json` conserve chaque paire de GUID,
le SHA-256 identique source→cible, la taille, la partition, la provenance, la
licence et le marquage LFS.

Les scieries restent soumises à la Unity Asset Store EULA et ne sont
distribuables que comme contenu embarqué du projet privé. Aucune arborescence
Vendor, prefab Vendor direct ou script de démonstration n'entre dans le package.
Les scènes de preuve référencent uniquement les adaptations portées et des
matériaux URP transitoires détenus par l'hôte.

## Audit modulaire GanzSe

L'audit hors Unity recense 217 FBX : 2 corps complets issus de la même lignée,
25 cheveux, 25 barbes, 25 yeux, 25 sourcils, 5 nez, 2 oreilles et 18 pièces pour
chacune des six catégories principales de tenue/armure. Cette banque permet des
variations d'apparence et de rôle, mais ne prouve pas l'existence de corps
féminin, enfant ou âgé distincts. Ces silhouettes ont donc été produites comme
variantes adaptées sous `Assets/CityLabHost/Adapted/Factory`, jamais comme
modifications Vendor. Leur rig et leurs LOD sont validés hors Unity ; la porte
Humanoid et les animations restent ouvertes. Le rapport source complet est
`AssetFactory/Reports/character_modularity.json`.

Huit propositions statiques sélectionnent 11 à 13 pièces du FBX complet sans
modifier la source. Elles restent dans le workbench et ne sont pas publiées
comme prefabs. La revue confirme que les tenues d'armure couvrent une base
ouvrier/soldat/notable, mais qu'elles ne remplacent pas de vrais corps
féminin/enfant/vieux ni les vêtements religieux et de mendiant. Le registre est
`AssetFactory/Catalogs/character_proposals.json` et la décision détaillée
`AssetFactory/Reports/character_proposal_review.json`.

## Variantes de décor admises

Les dimensions sont des plafonds de normalisation en mètres. Le ratio d'aspect reste inchangé : l'outil retient le plus petit facteur entre hauteur et empreinte horizontale, puis conserve l'ancrage au sol.

| Catégorie | Source Vendor intacte | Variante CityLab | Hauteur max. | Empreinte max. |
|---|---|---|---:|---:|
| Buisson 1 | `Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Bushes/EA03_Nature_Bush_03a_PRE.prefab` | `Assets/CityLabHost/Adapted/Prefabs/CityLab_Bush_1.prefab` | 1.25 m | 2.2 m |
| Buisson 2 | `Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Bushes/EA03_Nature_Bush_04a_PRE.prefab` | `Assets/CityLabHost/Adapted/Prefabs/CityLab_Bush_2.prefab` | 1.25 m | 2 m |
| Rocher 1 | `Assets/EmaceArt/Slavic World Free/Prefabs/Environment/Rock/EA03_Environment_Rock_Mini_Head_01a_PRE.prefab` | `Assets/CityLabHost/Adapted/Prefabs/CityLab_Rock_1.prefab` | 0.8 m | 1.6 m |
| Rocher 2 | `Assets/EmaceArt/Slavic World Free/Prefabs/Environment/Rock/EA03_Env_Rock_Slice_01a_PRE.prefab` | `Assets/CityLabHost/Adapted/Prefabs/CityLab_Rock_2.prefab` | 1.8 m | 2.8 m |
| Herbe 1 | `Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Grass/EA03_Plant_Grass_01c_PRE.prefab` | `Assets/CityLabHost/Adapted/Prefabs/CityLab_Grass_1.prefab` | 0.65 m | 0.85 m |
| Herbe 2 | `Assets/EmaceArt/Slavic World Free/Prefabs/Nature/Grass/EA03_Plant_Grass_02a_PRE.prefab` | `Assets/CityLabHost/Adapted/Prefabs/CityLab_Grass_2.prefab` | 0.55 m | 0.75 m |
| Accessoire 1 | `Assets/EmaceArt/Slavic World Free/Prefabs/Fence/Plank2/EA03_Village_Fence_01a_PRE.prefab` | `Assets/CityLabHost/Adapted/Prefabs/CityLab_Prop_1.prefab` | 1.25 m | 3.6 m |
| Accessoire 2 | `Assets/EmaceArt/Slavic World Free/Prefabs/Prop/Container/EA03_Prop_Container_Barrel_01d_PRE.prefab` | `Assets/CityLabHost/Adapted/Prefabs/CityLab_Prop_2.prefab` | 1.05 m | 1.2 m |
| Accessoire 3 | `Assets/EmaceArt/Slavic World Free/Prefabs/Prop/Container/EA03_Prop_Container_Crate_01a_PRE.prefab` | `Assets/CityLabHost/Adapted/Prefabs/CityLab_Prop_3.prefab` | 0.75 m | 1.1 m |

## Risques et garde-fous

- Les shaders Vendor ne sont jamais chargés par le code métier ; leurs matériaux sont copiés en URP/Lit dans `Assets/CityLabHost/Adapted/Materials`.
- Les cartes d'herbe sont admises en alpha clipping double face afin de conserver silhouettes, ombres et profondeur sous URP.
- Les scripts de démo ne sont pas utilisés. Le contrôleur GanzSe est isolé Editor-only car son fichier importe `UnityEditor`.
- Les colliders Vendor sont supprimés des variantes visuelles afin de ne pas perturber les routes, le NavMesh ou la sélection.
- Toute publication du dépôt contenant les sources Unity Store doit rester privée et respecter l'EULA Unity Asset Store.
