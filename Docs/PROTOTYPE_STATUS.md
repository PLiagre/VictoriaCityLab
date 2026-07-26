# Etat du prototype CityLab

## Vertical slice actuel

- camera RTS et terrain 512 x 512 m sous URP ;
- terrain texture par trois couches procedurales, relief doux et foret Vendor deterministe ;
- trace de route en deux clics avec apercu valide/invalide ;
- zoning residentiel, parcelles des deux cotes et orientation vers la route ;
- reservation, transport, livraison et conservation du bois ;
- fondations, ossature en bois et maison Vendor terminee ;
- habitants GanzSe animes par les clips Humanoid Kevin Iglesias ;
- HUD 1080p affichant ressources, population, foyers et chantiers ;
- fallbacks primitifs si le catalogue visuel hote est absent.

## Architecture

La simulation deterministe et les contrats `ICityStateSource` / `ICityCommandSink`
restent dans `Packages/com.victoria.citymode`. La scene, le catalogue visuel et les
adaptateurs d'assets tiers restent dans `Assets/CityLabHost`. Aucun code du package
ne reference directement un dossier Vendor.

## Validation

Les jalons visuels sont captures depuis le player Windows a 1920 x 1080 dans
`Logs/Captures`. Les suites EditMode et PlayMode, le build Windows et le smoke test
de performance doivent etre relances avant chaque livraison.

## Limites connues et prochaines priorites

- ajouter la selection d'un chantier et le reglage de sa priorite dans le HUD ;
- enrichir l'animation de chantier (l'ossature est lisible, mais l'animation dediee
  reste a choisir dans le pack DoubleL) ;
- densifier les details proches : herbes, pierres, clotures et traces de roues ;
- etendre le smoke test a 20 foyers et 30 batiments visibles ;
- ameliorer la variation des facades et la composition du noyau initial.
