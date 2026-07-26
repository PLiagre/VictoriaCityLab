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
- EmaceArt : trois maisons composites, un bâtiment central et un tas de bois.
- GanzSe : personnage complet copié, normalisé à 1,75 m et débarrassé des scripts de démonstration.
- Kevin Iglesias : idle et marche Humanoid sans root motion pilotés par CityLab.
- Polytope : deux arbres normalisés, distribués de façon déterministe en périphérie.
- DoubleL : pack conservé pour une future action de chantier ; aucun asset DoubleL n'est requis par le slice actuel.

## Risques et garde-fous

- Les shaders Vendor ne sont jamais chargés par le code métier ; leur rendu URP est validé dans le build CityLab.
- Les scripts de démo ne sont pas utilisés. Le contrôleur GanzSe est isolé Editor-only car son fichier importe `UnityEditor`.
- Les colliders Vendor sont supprimés des variantes visuelles afin de ne pas perturber les routes, le NavMesh ou la sélection.
- Toute publication du dépôt contenant les sources Unity Store doit rester privée et respecter l'EULA Unity Asset Store.
