# API d'hébergement City Mode

État : API de contrats et lifecycle v1 disponible ; présentation hébergée en
cours de découpage (`M3-FH-02`).

## Import minimal

Un hôte qui veut seulement compiler le contrat importe le package sans
dépendance Unity additionnelle :

```text
Packages/com.victoria.citymode.contracts
```

Pour un import UPM Git, épingler un commit VictoriaCityLab et le sous-chemin :

```text
https://github.com/PLiagre/VictoriaCityLab.git?path=/Packages/com.victoria.citymode.contracts#<commit>
```

Le package expose l'assembly `Victoria.CityMode.Contracts`, marqué
`noEngineReferences`. Il ne dépend ni d'URP, ni de l'Input System, ni d'AI
Navigation. Le package historique `com.victoria.citymode` conserve encore ces
dépendances pour le laboratoire et la présentation.

## Ouverture explicite

L'hôte construit un contexte valide, implémente les deux ports autoritaires et
ouvre une session :

```csharp
using Victoria.CityMode.Integration;

CityModeSession session;
CityModeErrorCode error;
if (!CityModeSession.TryOpen(
        launchContext,
        forgeHistorySnapshotSource,
        forgeHistoryIntentSink,
        out session,
        out error))
{
    // Rester ou revenir à la carte ; ne jamais démarrer une simulation locale.
}
```

`TryOpen` :

- valide version, identité, tick, révision et politique de temps ;
- refuse un port absent et un snapshot incompatible ;
- exige une égalité tick/révision sous `PauseWorld` ;
- refuse une deuxième session active avec `SessionAlreadyActive` ;
- ne crée aucun `GameObject`, aucune scène, aucun tick et aucune sauvegarde.

## Boucle de session

`CurrentSnapshot` est le dernier snapshot complet validé. Une vue émet une
intention à la révision courante via `TrySubmitIntent`. Une intention périmée est
refusée localement avant d'appeler l'hôte et produit un reçu
`RevisionConflict`. Après une acceptation, la présentation appelle
`TryRefreshSnapshot` avant l'intention suivante.

La fermeture appartient à l'hôte :

```csharp
session.Dispose();
```

`Dispose` est idempotent et libère le verrou de session. Le chargement de scène,
la progression, l'annulation et la restauration du viewport seront ajoutés dans
`M3-FH-04` ; ils ne sont pas simulés par cette API.

## Laboratoire autonome

`CityLabBootstrap` n'a plus de `RuntimeInitializeOnLoadMethod`. La scène
`Assets/CityLabHost/Scenes/CityLab.unity` contient déjà explicitement
`CityLabGame`, ce qui conserve son comportement autonome. Les outils qui créent
une scène de laboratoire à la volée peuvent appeler :

```csharp
var game = CityLabBootstrap.StartLaboratory();
// ...
CityLabBootstrap.StopLaboratory(game);
```

`StartLaboratory` retourne l'instance existante et n'en crée donc pas une
seconde. Cette API est réservée aux fixtures, smokes et revues du laboratoire ;
elle n'accepte pas de `CityLaunchContext` et ne doit pas être appelée par
ForgeHistory.

## Limite de l'incrément actuel

Les contrats forment maintenant un package réellement autonome et le bootstrap
global est retiré. La présentation, `LocalCitySimulation`, `CitySaveService` et
les fixtures restent toutefois dans le même package historique. `M3-FH-02` ne
sera `DONE` qu'après séparation de cet adaptateur de laboratoire, import dans un
hôte Unity minimal et exécution des suites Unity/player.
