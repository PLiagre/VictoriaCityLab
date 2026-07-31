# Audit des assets Vendor — CityLab

Généré par l'outil d'admission CityLab. Les sources restent intactes dans leurs dossiers Unity Store ; seuls des prefabs adaptés sont utilisés par le prototype.

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
