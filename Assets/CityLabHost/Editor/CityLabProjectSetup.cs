using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
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
        const string TerrainMaterialPath = "Assets/CityLabHost/Resources/CityLabTerrainMaterial.mat";
        const string PanelSettingsPath = "Assets/CityLabHost/Resources/CityLabPanelSettings.asset";
        const string RuntimeThemePath = "Assets/CityLabHost/Resources/CityLabRuntimeTheme.asset";
        const string ScenePath = SceneFolder + "/CityLab.unity";
        const string DefaultPostProcessDataPath =
            "Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset";

        [MenuItem("Victoria/CityLab/Configure Project")]
        public static void Configure()
        {
            EnsureFolder("Assets", "CityLabHost");
            EnsureFolder("Assets/CityLabHost", "Settings");
            EnsureFolder("Assets/CityLabHost", "Scenes");
            ConfigureRenderPipeline();
            ConfigurePlayer();
            ConfigureRuntimeUi();
            CreateBootstrapScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("CITYLAB_SETUP_OK scene=" + ScenePath + " pipeline=" + PipelinePath);
        }

        [MenuItem("Victoria/CityLab/Build Windows")]
        public static void BuildWindows()
        {
            BuildWindowsPlayer(BuildOptions.None, "CITYLAB_BUILD_RELEASE_OK");
        }

        [MenuItem("Victoria/CityLab/Build Windows Development")]
        public static void BuildWindowsDevelopment()
        {
            BuildWindowsPlayer(BuildOptions.Development, "CITYLAB_BUILD_DEVELOPMENT_OK");
        }

        static void BuildWindowsPlayer(BuildOptions buildOptions, string successMarker)
        {
            Configure();
            Directory.CreateDirectory("Builds/Windows");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Windows/VictoriaCityLab.exe",
                target = BuildTarget.StandaloneWindows64,
                options = buildOptions
            };
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"CityLab build failed: {report.summary.result}");
            Debug.Log($"{successMarker} bytes={report.summary.totalSize} duration={report.summary.totalTime}");
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
            if (renderer.postProcessData == null)
            {
                renderer.postProcessData = AssetDatabase.LoadAssetAtPath<PostProcessData>(DefaultPostProcessDataPath);
                EditorUtility.SetDirty(renderer);
            }
            ScreenSpaceAmbientOcclusion ambientOcclusion = null;
            foreach (var feature in renderer.rendererFeatures)
            {
                if (feature is ScreenSpaceAmbientOcclusion existingAmbientOcclusion)
                {
                    ambientOcclusion = existingAmbientOcclusion;
                    break;
                }
            }
            if (ambientOcclusion == null)
            {
                ambientOcclusion = ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();
                ambientOcclusion.name = "CityLab Contact Shadows";
                ambientOcclusion.Create();
                AssetDatabase.AddObjectToAsset(ambientOcclusion, renderer);
                renderer.rendererFeatures.Add(ambientOcclusion);
                EditorUtility.SetDirty(renderer);
            }

            // Keep creases readable without crushing the painterly palette in Linear color space.
            var ambientOcclusionSettings = new SerializedObject(ambientOcclusion);
            ambientOcclusionSettings.FindProperty("m_Settings.Intensity").floatValue = 0.75f;
            ambientOcclusionSettings.FindProperty("m_Settings.DirectLightingStrength").floatValue = 0.10f;
            ambientOcclusionSettings.FindProperty("m_Settings.Radius").floatValue = 0.018f;
            ambientOcclusionSettings.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(ambientOcclusion);

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
            pipeline.renderScale = 1f;
            pipeline.msaaSampleCount = 4;
            pipeline.supportsCameraDepthTexture = true;
            pipeline.mainLightShadowmapResolution = 4096;
            pipeline.shadowDistance = 190f;
            pipeline.shadowCascadeCount = 4;
            EditorUtility.SetDirty(pipeline);

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

            if (AssetDatabase.LoadAssetAtPath<Material>(TerrainMaterialPath) == null)
            {
                var terrainShader = Shader.Find("Universal Render Pipeline/Terrain/Lit");
                if (terrainShader == null)
                    throw new BuildFailedException("Shader URP Terrain Lit introuvable.");
                var terrainMaterial = new Material(terrainShader)
                {
                    name = "CityLab Terrain Material",
                    enableInstancing = true
                };
                AssetDatabase.CreateAsset(terrainMaterial, TerrainMaterialPath);
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
            PlayerSettings.colorSpace = ColorSpace.Linear;

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

        static void ConfigureRuntimeUi()
        {
            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(RuntimeThemePath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<ThemeStyleSheet>();
                theme.name = "CityLab Runtime Theme";
                AssetDatabase.CreateAsset(theme, RuntimeThemePath);
            }

            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (panel == null)
            {
                panel = ScriptableObject.CreateInstance<PanelSettings>();
                panel.name = "CityLab Runtime Panel";
                AssetDatabase.CreateAsset(panel, PanelSettingsPath);
            }
            panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panel.referenceResolution = new Vector2Int(1920, 1080);
            panel.match = 0.5f;
            panel.themeStyleSheet = theme;
            EditorUtility.SetDirty(panel);
        }

        static void CreateBootstrapScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
                return;
            }
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
