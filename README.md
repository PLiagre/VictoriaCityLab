# Victoria CityLab

Prototype Unity autonome du mode ville de Victoria. Ce depot ne partage ni
`Library/`, ni caches d'import, ni verrou avec les projets Victoria.

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
- `Z`, puis clic sur une route : creer des parcelles residentielles
- `Echap` : annuler l'outil actif

Le village commence avec huit habitants, six foyers sans logement et un stock
de bois. Les habitants transportent physiquement le bois jusqu'aux chantiers.

## Frontiere d'integration

Le package embarque `Packages/com.victoria.citymode` expose
`ICityStateSource` et `ICityCommandSink`. La simulation locale n'est qu'un
adaptateur de prototype et devra etre remplacee par un adaptateur Victoria.

## Assets tiers

Tout pack externe reste intact sous `Assets/Vendor/<editeur>/<pack>`. Ajouter
sa fiche dans `Assets/Vendor/THIRD_PARTY_ASSETS.md` avant utilisation.
