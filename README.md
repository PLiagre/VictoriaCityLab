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
- l'apercu de route devient vert si le trace est valide et rouge sinon
- `Z`, puis clic sur une route : creer des parcelles residentielles
- `Echap` : annuler l'outil actif
- en mode inspection, cliquer un chantier puis choisir sa priorite dans le HUD

Le village commence avec huit habitants, six foyers sans logement et un stock
de bois. Les habitants transportent physiquement le bois jusqu'aux chantiers.
Le bois porte est materialise par un petit faisceau de buches. Les chantiers
passent des fondations a l'ossature en bois, puis au prefab final.

## Validation locale

Les tests et builds automatises passent toujours par `Tools/run_unity_locked.py`.
Les resultats XML, journaux de build et captures du player Windows sont conserves
sous `Logs/`; le build de travail est produit sous `Builds/Windows`.
Le detail des portes, commandes et derniers resultats est conserve dans
`Docs/VALIDATION.md`.

## Frontiere d'integration

Le package embarque `Packages/com.victoria.citymode` expose
`ICityStateSource` et `ICityCommandSink`. La simulation locale n'est qu'un
adaptateur de prototype et devra etre remplacee par un adaptateur Victoria.

## Assets tiers

Les imports Unity Store restent intacts dans leur dossier racine d'origine afin
de préserver les mises à jour. CityLab ne consomme que les copies normalisées
de `Assets/CityLabHost/Adapted`; les sources et leur licence sont répertoriées
dans `Assets/Vendor/THIRD_PARTY_ASSETS.md` et auditées dans
`Docs/VENDOR_AUDIT.md`.

L'AssetFactory reste la porte d'entrée pour les assets générés, les remplacements
et les opérations de normalisation autorisées. Elle ne revendique jamais la
paternité des packs Unity Store et son worktree séparé n'est pas modifié par
CityLab.
