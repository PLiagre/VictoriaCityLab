using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Victoria.CityMode;

namespace Victoria.CityLab.Editor
{
    public static class CityLabProjectSetup
    {
        const string SettingsFolder = "Assets/CityLabHost/Settings";
        const string SceneFolder = "Assets/CityLabHost/Scenes";
        const string RendererPath = SettingsFolder + "/CityLabRenderer.asset";
        const string PipelinePath = SettingsFolder + "/CityLabURP.asset";
        const string RuntimeMaterialPath = "Assets/CityLabHost/Resources/CityLabBaseMaterial.mat";
        const string ScenePath = SceneFolder + "/CityLab.unity";

        [MenuItem("Victoria/CityLab/Configure Project")]
        public static void Configure()
        {
            EnsureFolder("Assets", "CityLabHost");
            EnsureFolder("Assets/CityLabHost", "Settings");
            EnsureFolder("Assets/CityLabHost", "Scenes");
            ConfigureRenderPipeline();
            ConfigurePlayer();
            CreateBootstrapScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("CITYLAB_SETUP_OK scene=" + ScenePath + " pipeline=" + PipelinePath);
        }

        [MenuItem("Victoria/CityLab/Build Windows")]
        public static void BuildWindows()
        {
            Configure();
            Directory.CreateDirectory("Builds/Windows");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Windows/VictoriaCityLab.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"CityLab build failed: {report.summary.result}");
            Debug.Log($"CITYLAB_BUILD_OK bytes={report.summary.totalSize} duration={report.summary.totalTime}");
        }

        static void ConfigureRenderPipeline()
        {
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                renderer.name = "CityLab Renderer";
                AssetDatabase.CreateAsset(renderer, RendererPath);
            }

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                pipeline.name = "CityLab URP";
                pipeline.renderScale = 1f;
                pipeline.msaaSampleCount = 4;
                pipeline.shadowDistance = 140f;
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            RemoveAlwaysIncludedShader("Universal Render Pipeline/Lit");
            if (AssetDatabase.LoadAssetAtPath<Material>(RuntimeMaterialPath) == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                    throw new BuildFailedException("Shader URP Lit introuvable.");
                var material = new Material(shader) { name = "CityLab Base Material", enableInstancing = true };
                AssetDatabase.CreateAsset(material, RuntimeMaterialPath);
            }
        }

        static void RemoveAlwaysIncludedShader(string shaderName)
        {
            var shader = Shader.Find(shaderName);
            if (shader == null) return;
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
            if (assets.Length == 0)
                return;
            var serialized = new SerializedObject(assets[0]);
            var shaders = serialized.FindProperty("m_AlwaysIncludedShaders");
            if (shaders == null)
                return;
            for (var i = shaders.arraySize - 1; i >= 0; i--)
                if (shaders.GetArrayElementAtIndex(i).objectReferenceValue == shader)
                    shaders.DeleteArrayElementAtIndex(i);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "Victoria Project";
            PlayerSettings.productName = "Victoria CityLab";
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.runInBackground = true;

            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets.Length > 0)
            {
                var serialized = new SerializedObject(assets[0]);
                var input = serialized.FindProperty("activeInputHandler");
                if (input != null)
                {
                    input.intValue = 1;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        static void CreateBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "CityLab";
            var marker = new GameObject("CityLab Scene — runtime generated");
            marker.transform.position = Vector3.zero;
            marker.AddComponent<CityLabGame>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Object.DestroyImmediate(marker);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
