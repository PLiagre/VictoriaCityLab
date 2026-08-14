# Gouvernance CityLab

CityLab devient le périmètre intégré du prototype et de son Asset Factory. Le
worktree historique `C:/Users/liagr/VictoriaProject-assets` reste une référence
en lecture seule pendant la migration ; les nouveaux contrats, recettes et
manifests CityLab vivent dans ce dépôt.

## Propriété des répertoires

- la simulation déterministe reste dans `Packages/com.victoria.citymode` ;
- les scènes, catalogues hôtes et adaptateurs restent sous `Assets/CityLabHost` ;
- les copies de production approuvées vivent dans
  `Packages/com.victoria.citymode.assets/Runtime/Content`, séparées en
  `Common`, `Biome` et `City` ;
- les imports Unity Store restent intacts dans leur dossier racine sous
  `Assets/<éditeur>` ;
- le code de l'usine vit sous `Tools/AssetFactory` ;
- recettes, manifests, rapports et workbench vivent sous `AssetFactory` ;
- seules les sorties approuvées sont copiées sous
`Assets/CityLabHost/Adapted/Factory`.

Tout passage du laboratoire vers le package portable est décrit dans
`Docs/Integration/city-mode-asset-port-v1.json`. Le fichier source et la cible
doivent être bit-identiques, leurs GUID source/cible doivent être explicites et
distincts, et chaque binaire doit suivre Git LFS. Les `.meta` cible sont générés
par Unity puis versionnés ; ils ne sont jamais fabriqués à la main. Cette règle
évite une collision de GUID tant que le laboratoire et le package coexistent
dans le même dépôt.

Le package métier ne référence jamais directement une source Vendor ou un
artefact de workbench. La génération, les previews et la QA s'exécutent sans
Unity. Aucun `.meta` n'est créé à la main et aucun import Store n'est envoyé à
un service génératif distant sans droit explicite.

L'admission d'une nouvelle source exige un profil versionné avec provenance,
licence vérifiée et hashes des composants retenus. La publication générique est
un dry-run par défaut ; une copie explicite est atomique, refuse toute sortie
hors `Assets/CityLabHost/Adapted/Factory` et ne lance jamais Unity.

Le portage vers le package de production est une seconde admission explicite,
après publication Factory : licence, provenance, hash source→cible, budget de
partition, build et capture player sont obligatoires. Il ne donne au package
aucune autorité de simulation, d'horloge ou de sauvegarde.

Les textures partagées sont produites comme un ensemble PBR cohérent :
`BaseColor`, `Normal`, `AO`, `Roughness`, `Metallic` et masque de variation.
Leur recette, leur graph, leur graine et leurs hashes restent versionnés ; la
QA doit prouver leur lisibilité aux résolutions de revue avant publication.

Tout nouveau bâtiment Factory respecte le contrat
`AssetFactory/Schemas/building_construction.schema.json` : au moins trois
variantes déterministes et quatre couches cumulatives `foundation`, `frame`,
`roof`, `details`. Chaque couche exporte exactement trois LOD. Une variation
peut modifier palette, contreventement, implantation des accessoires et
présence d'une cheminée, mais jamais supprimer la lecture fonctionnelle ni
abaisser le niveau de finition de la famille approuvée.

Un bâtiment achevé doit présenter une enveloppe fermée et lisible : quatre murs
ou une ouverture fonctionnelle volontaire, jamais une façade absente masquée
par une porte décorative. Le catalogue déclare son système de murs et de
toiture ainsi qu'au moins quatre marqueurs d'usage visibles à distance RTS.
Pierre, brique, planches et colombage sont choisis selon la fonction du bâtiment
et non appliqués uniformément à tout le pilote.

Tout personnage Factory respecte le contrat
`AssetFactory/Schemas/character_modular.schema.json`. Genre, âge, morphologie,
visage, cheveux et rôle social sont des axes de données déterministes, pas des
prefabs Vendor directs. Une simple mise à l'échelle uniforme ne constitue pas
une morphologie enfant, féminine ou âgée validée : les déformations doivent
conserver un squelette Humanoid partagé, des proportions articulaires sûres et
passer les contrôles d'animation, d'intersection des vêtements et de LOD avant
publication.

`C:/Users/liagr/VictoriaProject` reste séparé. Aucune fusion avec son `main` et
aucune publication distante ne sont implicites dans cette intégration.
