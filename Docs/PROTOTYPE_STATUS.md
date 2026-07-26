# Etat du prototype CityLab

## Vertical slice actuel

- camera RTS et terrain 512 x 512 m sous URP ;
- terrain texture par trois couches procedurales, relief doux et foret Vendor deterministe ;
- trace de route en deux clics avec apercu valide/invalide ;
- zoning residentiel, parcelles des deux cotes et orientation vers la route ;
- reservation, transport, livraison et conservation du bois ;
- fondations, ossature en bois et maison Vendor terminee ;
- occupation visible des maisons par une lumiere de foyer et une fumee de cheminee ;
- habitants GanzSe animes par les clips Humanoid Kevin Iglesias : idle, marche,
  transport avec faisceau de buches et gestes actifs de chantier ;
- HUD 1080p affichant ressources, population, foyers et chantiers ;
- selection d'un chantier dans le monde, surlignage et priorite basse/normale/haute ;
- fallbacks primitifs si le catalogue visuel hote est absent.

## Architecture

La simulation deterministe et les contrats `ICityStateSource` / `ICityCommandSink`
restent dans `Packages/com.victoria.citymode`. La scene, le catalogue visuel et les
adaptateurs d'assets tiers restent dans `Assets/CityLabHost`. Aucun code du package
ne reference directement un dossier Vendor.

## Validation

Les jalons visuels sont captures depuis le player Windows a 1920 x 1080 dans
`Logs/Captures`. Le smoke test charge une fixture hote de 20 foyers, 30 batiments
et 30 habitants et refuse de valider un scenario incomplet. Le jalon du 26 juillet
2026 mesure 60,0 FPS moyens et 16,683 ms au p95 sur la machine de validation.
Les snapshots metier sont rafraichis a 10 Hz et les vues d'habitants interpolent
leur position a chaque frame, ce qui evite une serialisation JSON complete dans
chaque `Update` sans sacrifier la fluidite visuelle.

## Limites connues et prochaines priorites

- densifier les details proches : herbes, pierres, clotures et traces de roues ;
- ameliorer la variation des facades et la composition du noyau initial.
