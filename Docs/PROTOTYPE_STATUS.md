# Etat du prototype CityLab

## Vertical slice actuel

- camera RTS et terrain 512 x 512 m sous URP ;
- direction medievale dark-fantasy stylisee, palette terre/bronze/braises et
  post-traitement URP ;
- deux textures originales CityLab de 1254 x 1254 pour la prairie peinte et les
  routes, completees par des couches de terrain procedurales ;
- relief doux, foret Vendor deterministe, herbes, pierres, clotures, puits,
  marche, feu, particules, vent et ambiance sonore procedurale ;
- trace de route en deux clics avec apercu valide/invalide ;
- zoning residentiel, parcelles des deux cotes et orientation vers la route ;
- camp forestier placable avec `B`, cout de huit bois, contraintes de distance
  et espacement, deux bucherons au maximum et reserve locale finie ;
- production forestiere deterministe et suivi du bois en stock, reserve, en
  transit et livre aux chantiers ;
- fondations, ossature en bois et maison Vendor terminee ;
- occupation visible des maisons par une lumiere de foyer et une fumee de cheminee ;
- habitants GanzSe animes par les clips Humanoid Kevin Iglesias : idle, marche,
  transport avec faisceau de buches et gestes actifs de chantier ;
- HUD 1080p de chronique seigneuriale affichant ressources, foret, bucherons,
  population, foyers, chantiers, camps, jour, saison et vitesse ;
- pause avec `Espace` et vitesses x1/x2/x4 avec `1`/`2`/`3` ;
- selection d'un chantier dans le monde, surlignage et priorite basse/normale/haute ;
- fallbacks primitifs si le catalogue visuel hote est absent.

## Architecture

La simulation deterministe et les contrats `ICityStateSource` / `ICityCommandSink`
restent dans `Packages/com.victoria.citymode`. La scene, le catalogue visuel et les
adaptateurs d'assets tiers restent dans `Assets/CityLabHost`. Aucun code du package
ne reference directement un dossier Vendor.

L'economie forestiere est exposee par `PlaceLumberCamp` et
`ProductionSiteState`. L'affectation des travailleurs et la production utilisent
le meme tick deterministe que la construction ; elles restent abstraites et ne
simulent pas encore l'abattage individuel de chaque arbre.

## Validation

Les jalons visuels sont captures depuis le player Windows a 1920 x 1080 dans
`Logs/Captures`. Le smoke test charge une fixture hote de 20 foyers, 30 batiments
et 30 habitants et refuse de valider un scenario incomplet. La livraison du
31 juillet 2026 passe 12/12 tests EditMode, 1/1 test PlayMode et le build Windows
x64. Le smoke test mesure 60,0 FPS moyens et 16,683 ms au p95 sur 600 frames.
Les snapshots metier sont rafraichis a 10 Hz et les vues d'habitants interpolent
leur position a chaque frame, ce qui evite une serialisation JSON complete dans
chaque `Update` sans sacrifier la fluidite visuelle.

## Limites connues et prochaines priorites

- le livrable est un vertical slice jouable et valide, pas un jeu AAA termine ;
- ajouter sauvegarde/chargement, objectifs, tutoriel et options completes ;
- etendre l'economie au bois coupe visible, a la nourriture, aux metiers, aux
  chaines de production, au commerce et aux saisons ayant un effet de jeu ;
- remplacer les deplacements directs par une navigation et une circulation
  robustes a grande population ;
- augmenter la variation des facades, personnages, animations, effets, sons et
  compositions de village, puis valider plusieurs centaines d'habitants.
