# City Mode transition mirror host

This Unity `6000.0.43f1` project is a disposable integration fixture for
`M3-FH-04`. It mirrors the topology of ForgeHistory `Main.unity` with two tiny
scenes: a persistent map scene and an additive City Mode scene.

It imports only the City Mode contracts and presentation packages, owns all
`SceneManager` calls, restores the selected map cell and viewport JSON, and
contains no CityLab simulation, fixture, save service, production map source or
ForgeHistory source file.

Generate/refresh the scenes with:

```powershell
Unity.exe -batchmode -nographics -projectPath . `
  -executeMethod Victoria.CityMode.TransitionHost.Editor.TransitionHostSceneSetup.Run
```
