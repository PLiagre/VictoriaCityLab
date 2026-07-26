# Validation du vertical slice

Derniere validation complete : 26 juillet 2026, Unity `6000.0.43f1`, Windows 11,
player URP en 1920 x 1080. Toutes les commandes Unity passent par le verrou propre
a CityLab : `py Tools/run_unity_locked.py -- <Unity.exe> ...`.

## Resultats

| Porte | Resultat | Preuve locale |
|---|---|---|
| EditMode | 8/8 reussis | `Logs/editmode-house-facing.xml` |
| PlayMode | 1/1 reussi sur le commit final | `Logs/playmode-final.xml` |
| Build Windows x64 | reussi, 150 520 278 octets | `Logs/build-house-facing.log` |
| Smoke 1080p | 20 foyers, 30 batiments, 30 habitants | `Logs/player-smoke-final.log` |
| Performance | 60,0 FPS moyens, p95 16,683 ms sur 600 frames | `Logs/player-smoke-final.log` |
| Capture player | inspectee, sans shader rose ni asset casse visible | `Logs/Captures/milestone-house-facing-20260726.png` |

## Couverture fonctionnelle

- route en deux clics avec apercu vert/rouge et refus localise ;
- parcelles deterministes des deux cotes, maisons orientees vers la route ;
- revendication par les foyers, reservation et conservation du bois ;
- transport visible, fondations, ossature, maison achevee et foyer visible ;
- selection et priorite de chantier reliees a `ICityCommandSink` ;
- idle, marche, port de bois et activite de chantier differencies ;
- camera RTS, HUD 1080p et retours de succes, refus et manque de bois ;
- fallbacks visuels conserves si le catalogue hote est absent.

## Commandes de reproduction

```powershell
py Tools/run_unity_locked.py -- `
  'C:\Program Files\Unity\Hub\Editor\6000.0.43f1\Editor\Unity.exe' `
  -batchmode -nographics -projectPath 'C:\Users\liagr\VictoriaCityLab' `
  -runTests -testPlatform EditMode -testResults 'Logs/editmode-results.xml' `
  -logFile 'Logs/editmode.log'

py Tools/run_unity_locked.py -- `
  'C:\Program Files\Unity\Hub\Editor\6000.0.43f1\Editor\Unity.exe' `
  -batchmode -nographics -projectPath 'C:\Users\liagr\VictoriaCityLab' `
  -runTests -testPlatform PlayMode -testResults 'Logs/playmode-results.xml' `
  -logFile 'Logs/playmode.log'

py Tools/run_unity_locked.py -- `
  'C:\Program Files\Unity\Hub\Editor\6000.0.43f1\Editor\Unity.exe' `
  -batchmode -quit -projectPath 'C:\Users\liagr\VictoriaCityLab' `
  -executeMethod Victoria.CityLab.Editor.CityLabProjectSetup.BuildWindows `
  -logFile 'Logs/build.log'

& 'Builds\Windows\VictoriaCityLab.exe' -screen-width 1920 -screen-height 1080 `
  -screen-fullscreen 0 -citylabSmoke -logFile 'Logs/player-smoke.log'
```

## Isolation

Les seules ecritures de cette mission sont dans `C:\Users\liagr\VictoriaCityLab`.
Les sources Vendor ne sont jamais modifiees par l'admission ; les variantes sont
generees sous `Assets/CityLabHost/Adapted`. Aucun push distant n'est effectue.
