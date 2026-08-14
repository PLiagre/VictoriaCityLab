using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Victoria.CityMode.TransitionHost.Editor
{
    public static class TransitionHostSceneSetup
    {
        const string ScenesDirectory = "Assets/Scenes";
        const string MapScenePath = ScenesDirectory + "/MapMirror.unity";
        const string CityScenePath = ScenesDirectory + "/CityModeView.unity";
        const string PlayerPath = "Builds/CityModeTransitionHost/CityModeTransitionHost.exe";

        public static void Run()
        {
            var exitCode = 1;
            try
            {
                Directory.CreateDirectory(ScenesDirectory);

                var mapScene = EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var mapRoot = new GameObject("ForgeHistory Map Mirror");
                mapRoot.AddComponent<MapMirrorController>();
                EditorSceneManager.SaveScene(mapScene, MapScenePath);

                var cityScene = EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var cityRoot = new GameObject("City Mode View Mirror");
                cityRoot.AddComponent<CityModeMirrorView>();
                EditorSceneManager.SaveScene(cityScene, CityScenePath);

                EditorBuildSettings.scenes = new[]
                {
                    new EditorBuildSettingsScene(MapScenePath, true),
                    new EditorBuildSettingsScene(CityScenePath, true)
                };
                AssetDatabase.SaveAssets();
                Debug.Log("CITY_MODE_TRANSITION_HOST_SETUP_OK scenes=2");
                exitCode = 0;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                EditorApplication.Exit(exitCode);
            }
        }

        public static void BuildPlayer()
        {
            var exitCode = 1;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(PlayerPath) ?? "Builds");
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { MapScenePath, CityScenePath },
                    locationPathName = PlayerPath,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development
                });
                if (report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException(
                        "Transition host build result: " + report.summary.result);
                Debug.Log(
                    "CITY_MODE_TRANSITION_BUILD_OK bytes=" + report.summary.totalSize +
                    " duration=" + report.summary.totalTime);
                exitCode = 0;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                EditorApplication.Exit(exitCode);
            }
        }
    }
}
