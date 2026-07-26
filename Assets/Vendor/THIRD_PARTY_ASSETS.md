# Registre des assets tiers

Les packs Unity Store peuvent rester durablement, mais aucun pack ne doit etre
utilise avant d'avoir une entree complete dans ce registre.

| Editeur | Pack | URL | Version | Acquisition | Licence | Pipelines | Contenu utilise | Scripts audites | Budget valide |
|---|---|---|---|---|---|---|---|---|---|
| EmaceArt | Slavic Medieval Village | https://assetstore.unity.com/packages/3d/environments/fantasy/slavic-medieval-environment-town-interior-and-exterior-167010 | 1.7 | 2026-07-26 | Standard Unity Asset Store EULA (Extension Asset) | URP | 3 maisons, centre, bois | Oui : 0 script, 1 shader legacy isolé | Oui, build 1080p |
| GanzSe | Free Modular Character Pack — URP | https://assetstore.unity.com/packages/3d/characters/humanoids/fantasy/free-modular-character-pack-urp-medieval-fantasy-307385 | 1.12 | 2026-07-26 | Standard Unity Asset Store EULA (Extension Asset) | URP | 1 personnage complet adapté | Oui : contrôleur de démo retiré, assembly Editor-only | Oui, build 1080p |
| Kevin Iglesias | Human Basic Motions FREE | https://assetstore.unity.com/packages/3d/animations/human-basic-motions-free-154271 | 2.4.2 | 2026-07-26 | Standard Unity Asset Store EULA (Extension Asset) | Built-in / URP | Idle + marche Humanoid | Oui : 2 scripts de démo non utilisés | Oui, build 1080p |
| Polytope Studio | Low Poly Nature — FREE Vegetation, Rocks and Water | https://assetstore.unity.com/packages/3d/environments/landscapes/low-poly-nature-free-vegetation-rocks-and-water-162124 | 1.1.2 | 2026-07-26 | Standard Unity Asset Store EULA (Extension Asset) | URP | 2 arbres adaptés | Oui : helpers démo ignorés, shaders convertis en variantes URP | Oui, build 1080p |
| DoubleL | RPG Animations Pack FREE | https://assetstore.unity.com/packages/3d/animations/rpg-animations-pack-free-288783 | 1.9 | 2026-07-26 | Standard Unity Asset Store EULA (Extension Asset) | Built-in / URP | Réserve, non utilisé actuellement | Oui : 1 contrôleur caméra de démo ignoré | Non applicable au slice |

Règles : lorsqu'un import Unity Store impose son propre dossier racine, le conserver
intact pour préserver les mises à jour. Les variantes consommées par CityLab vivent
dans `Assets/CityLabHost/Adapted`. Ne jamais présenter un pack tiers comme une
production AssetFactory, ni redistribuer ses sources hors des conditions de licence.
