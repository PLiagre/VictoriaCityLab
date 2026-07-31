# Validation du vertical slice

Derniere validation complete : 31 juillet 2026, Unity `6000.0.43f1`, Windows 11,
player URP en 1920 x 1080. Toutes les commandes Unity passent par le verrou propre
a CityLab : `py Tools/run_unity_locked.py -- <Unity.exe> ...`.

Cette validation porte sur un vertical slice jouable. Elle ne constitue pas une
certification qu'un jeu AAA complet est termine.

## Resultats

| Porte | Resultat | Preuve locale |
|---|---|---|
| EditMode | 12/12 reussis | `Logs/editmode-delivery.xml` |
| PlayMode | 1/1 reussi | `Logs/playmode-delivery.xml` |
| Build Windows x64 | reussi, 128 004 595 octets | `Logs/build-final-lighting.log` |
| Smoke final | 20 foyers, 30 batiments, 30 habitants, porte performance reussie | `Logs/player-smoke-delivery-2.log` |
| Performance | 60,0 FPS moyens, p95 16,683 ms sur 600 frames | `Logs/player-smoke-aaa-final.log` |
| Capture player | inspectee, sans shader rose ni asset casse visible | `Logs/Captures/milestone-forest-revival-20260731.png` |

## Couverture fonctionnelle

- route en deux clics avec apercu vert/rouge et refus localise ;
- parcelles deterministes des deux cotes, maisons orientees vers la route ;
- placement d'un camp forestier avec cout, limites, espacement et refus stables ;
- affectation abstraite de deux bucherons au maximum, production periodique et
  epuisement deterministe de la reserve locale ;
- revendication par les foyers, reservation et conservation du bois ;
- transport visible, fondations, ossature, maison achevee et foyer visible ;
- selection et priorite de chantier reliees a `ICityCommandSink` ;
- idle, marche, port de bois et activite de chantier differencies ;
- commandes `R`, `Z`, `B`, `Echap`, pause `Espace` et vitesses x1/x2/x4 avec
  `1`/`2`/`3` ;
- camera RTS, HUD 1080p et retours de succes, refus et manque de bois ;
- textures originales de prairie et route, dressing medieval dark-fantasy,
  post-traitement, particules et ambiance procedurale ;
- fallbacks visuels conserves si le catalogue hote est absent.

## Commandes de reproduction

```powershell
py Tools/run_unity_locked.py -- `
  'C:\Program Files\Unity\Hub\Editor\6000.0.43f1\Editor\Unity.exe' `
  -batchmode -nographics -projectPath 'C:\Users\liagr\VictoriaCityLab' `
  -runTests -testPlatform EditMode -testResults 'Logs/editmode-delivery.xml' `
  -logFile 'Logs/editmode-delivery.log'

py Tools/run_unity_locked.py -- `
  'C:\Program Files\Unity\Hub\Editor\6000.0.43f1\Editor\Unity.exe' `
  -batchmode -nographics -projectPath 'C:\Users\liagr\VictoriaCityLab' `
  -runTests -testPlatform PlayMode -testResults 'Logs/playmode-delivery.xml' `
  -logFile 'Logs/playmode-delivery.log'

py Tools/run_unity_locked.py -- `
  'C:\Program Files\Unity\Hub\Editor\6000.0.43f1\Editor\Unity.exe' `
  -batchmode -quit -projectPath 'C:\Users\liagr\VictoriaCityLab' `
  -executeMethod Victoria.CityLab.Editor.CityLabProjectSetup.BuildWindows `
  -logFile 'Logs/build-final-lighting.log'

& 'Builds\Windows\VictoriaCityLab.exe' -screen-width 1920 -screen-height 1080 `
  -screen-fullscreen 0 -citylabSmoke -logFile 'Logs/player-smoke-delivery-2.log'
```

## Isolation

Les seules ecritures de cette mission sont dans `C:\Users\liagr\VictoriaCityLab`.
Les sources Vendor ne sont jamais modifiees par l'admission ; les variantes sont
generees sous `Assets/CityLabHost/Adapted`. Aucun push distant n'est effectue.
