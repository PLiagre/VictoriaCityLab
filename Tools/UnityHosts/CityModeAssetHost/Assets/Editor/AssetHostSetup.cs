using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Victoria.CityMode.Assets;

namespace Victoria.CityMode.AssetHost.Editor
{
    public static class AssetHostSetup
    {
        const string Revision = "city-mode-asset-port-v1";
        const string PackageRoot = "Packages/com.victoria.citymode.assets/Runtime/Content";
        const string ScenesDirectory = "Assets/Scenes";
        const string SettingsDirectory = "Assets/Settings";
        const string MapScenePath = ScenesDirectory + "/AssetMap.unity";
        const string CommonScenePath = ScenesDirectory + "/AssetCommon.unity";
        const string BiomeScenePath = ScenesDirectory + "/AssetBiome.unity";
        const string CityScenePath = ScenesDirectory + "/AssetCity.unity";
        const string PlayerPath = "Builds/CityModeAssetHost/CityModeAssetHost.exe";
        const string OriginalLicense = "LicenseRef-Victoria-Original";
        const string VendorLicense = "LicenseRef-Unity-Asset-Store-EULA";

        static readonly string[] CommonPaths =
        {
            PackageRoot + "/Common/CityLabTrim_AO.png",
            PackageRoot + "/Common/CityLabTrim_BaseColor.png",
            PackageRoot + "/Common/CityLabTrim_Metallic.png",
            PackageRoot + "/Common/CityLabTrim_Normal.png",
            PackageRoot + "/Common/CityLabTrim_Roughness.png",
            PackageRoot + "/Common/CityLabTrim_VariationMask.png"
        };

        static readonly string[] BiomePaths =
        {
            PackageRoot + "/Biome/StylizedMeadow_Albedo.png",
            PackageRoot + "/Biome/StylizedRoad_Albedo.png"
        };

        static readonly string[] CityPaths =
        {
            PackageRoot + "/City/building_sawmill_frontier_01_a.fbx",
            PackageRoot + "/City/building_sawmill_frontier_01_b.fbx",
            PackageRoot + "/City/building_sawmill_frontier_01_c.fbx"
        };

        public static void Run()
        {
            var exitCode = 1;
            try
            {
                Directory.CreateDirectory(ScenesDirectory);
                Directory.CreateDirectory(SettingsDirectory);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                ConfigureImporters();
                ConfigureUrp();
                CreateMapScene();
                CreateCommonScene();
                CreateBiomeScene();
                CreateCityScene();
                EditorBuildSettings.scenes = new[]
                {
                    new EditorBuildSettingsScene(MapScenePath, true),
                    new EditorBuildSettingsScene(CommonScenePath, true),
                    new EditorBuildSettingsScene(BiomeScenePath, true),
                    new EditorBuildSettingsScene(CityScenePath, true)
                };
                AssetDatabase.SaveAssets();
                Debug.Log("CITY_MODE_ASSET_HOST_SETUP_OK scenes=4 assets=11 partitions=3 pipeline=URP17");
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
                    scenes = new[] { MapScenePath, CommonScenePath, BiomeScenePath, CityScenePath },
                    locationPathName = PlayerPath,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development
                });
                if (report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException("Asset host build result: " + report.summary.result);
                Debug.Log(
                    "CITY_MODE_ASSET_BUILD_OK bytes=" + report.summary.totalSize +
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

        static void ConfigureImporters()
        {
            foreach (var path in CommonPaths.Concat(BiomePaths))
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                    throw new InvalidOperationException("Texture importer missing: " + path);
                importer.mipmapEnabled = true;
                importer.streamingMipmaps = true;
                importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.SaveAndReimport();
            }
            foreach (var path in CityPaths)
            {
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null)
                    throw new InvalidOperationException("Model importer missing: " + path);
                importer.importAnimation = false;
                importer.importCameras = false;
                importer.importLights = false;
                importer.isReadable = false;
                importer.SaveAndReimport();
            }
        }

        static void ConfigureUrp()
        {
            const string rendererPath = SettingsDirectory + "/AssetHostRenderer.asset";
            const string pipelinePath = SettingsDirectory + "/AssetHostPipeline.asset";
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, rendererPath);
            }
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, pipelinePath);
            }
            pipeline.shadowDistance = 120f;
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            EditorUtility.SetDirty(pipeline);
        }

        static void CreateMapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "AssetMap";
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.38f, 0.46f, 0.56f);
            RenderSettings.ambientEquatorColor = new Color(0.22f, 0.20f, 0.17f);
            RenderSettings.ambientGroundColor = new Color(0.08f, 0.07f, 0.055f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.36f, 0.40f, 0.42f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 70f;
            RenderSettings.fogEndDistance = 150f;

            var cameraObject = new GameObject("Asset Host Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.31f, 0.37f, 0.42f);
            camera.nearClipPlane = 0.2f;
            camera.farClipPlane = 250f;
            camera.transform.position = new Vector3(0f, 62f, -68f);
            camera.transform.LookAt(new Vector3(0f, 5f, 2f));
            EditorSceneManager.SaveScene(scene, MapScenePath);
        }

        static void CreateCommonScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("City Mode Common Partition");
            ConfigureCatalog(
                root,
                CityModeAssetPartitionKind.Common,
                128L * 1024L * 1024L,
                CommonPaths,
                "common",
                OriginalLicense);

            var sun = new GameObject("Common Sun");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.88f, 0.70f);
            light.intensity = 1.35f;
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(47f, -32f, 0f);
            EditorSceneManager.SaveScene(scene, CommonScenePath);
        }

        static void CreateBiomeScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("City Mode Biome Partition");
            ConfigureCatalog(
                root,
                CityModeAssetPartitionKind.Biome,
                64L * 1024L * 1024L,
                BiomePaths,
                "biome",
                OriginalLicense);

            var meadowTexture = Load<Texture2D>(BiomePaths[0]);
            var roadTexture = Load<Texture2D>(BiomePaths[1]);
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ported Meadow";
            ground.transform.position = new Vector3(0f, -0.1f, 5f);
            ground.transform.localScale = new Vector3(12f, 1f, 10f);
            UnityEngine.Object.DestroyImmediate(ground.GetComponent<Collider>());

            var roads = new List<Renderer>();
            for (var index = -1; index <= 1; index++)
            {
                var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
                road.name = "Ported Road " + index;
                road.transform.position = new Vector3(index * 24f, 0f, 4f);
                road.transform.localScale = new Vector3(5f, 0.12f, 110f);
                UnityEngine.Object.DestroyImmediate(road.GetComponent<Collider>());
                roads.Add(road.GetComponent<Renderer>());
            }

            var binder = root.AddComponent<AssetPartitionVisualBinder>();
            binder.ConfigureBiome(
                meadowTexture,
                roadTexture,
                new[] { ground.GetComponent<Renderer>() },
                roads.ToArray());
            EditorSceneManager.SaveScene(scene, BiomeScenePath);
        }

        static void CreateCityScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("City Mode City Partition");
            ConfigureCatalog(
                root,
                CityModeAssetPartitionKind.City,
                96L * 1024L * 1024L,
                CityPaths,
                "city.sawmill",
                VendorLicense);

            var renderers = new List<Renderer>();
            for (var index = 0; index < CityPaths.Length; index++)
            {
                var model = Load<GameObject>(CityPaths[index]);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
                instance.name = "Ported Sawmill " + (char)('A' + index);
                instance.transform.position = new Vector3((index - 1) * 25f, 0f, index == 1 ? 3f : 9f);
                instance.transform.rotation = Quaternion.Euler(0f, index == 0 ? 12f : index == 2 ? -14f : 0f, 0f);
                PrefabUtility.UnpackPrefabInstance(
                    instance,
                    PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);
                foreach (var sourceLod in instance.GetComponentsInChildren<LODGroup>(true))
                    UnityEngine.Object.DestroyImmediate(sourceLod);
                foreach (var collider in instance.GetComponentsInChildren<Collider>(true))
                    UnityEngine.Object.DestroyImmediate(collider);

                var allRenderers = instance.GetComponentsInChildren<Renderer>(true);
                var completed = allRenderers.Where(renderer =>
                    renderer.gameObject.name.IndexOf("__P04_DETAILS_", StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
                if (completed.Length != 3)
                    throw new InvalidOperationException("Ported city model requires exactly three completed-stage LODs: " + model.name);
                foreach (var renderer in allRenderers)
                    renderer.gameObject.SetActive(completed.Contains(renderer));
                var group = instance.AddComponent<LODGroup>();
                group.SetLODs(new[]
                {
                    new LOD(0.42f, Level(completed, 0)),
                    new LOD(0.16f, Level(completed, 1)),
                    new LOD(0.025f, Level(completed, 2))
                });
                group.RecalculateBounds();
                renderers.AddRange(completed);
            }

            var binder = root.AddComponent<AssetPartitionVisualBinder>();
            binder.ConfigureCity(renderers.ToArray());
            EditorSceneManager.SaveScene(scene, CityScenePath);
        }

        static Renderer[] Level(Renderer[] renderers, int level)
        {
            var result = renderers.Where(renderer =>
                renderer.gameObject.name.IndexOf("_LOD" + level, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
            if (result.Length != 1)
                throw new InvalidOperationException("Completed city phase requires one renderer for LOD" + level + ".");
            return result;
        }

        static void ConfigureCatalog(
            GameObject root,
            CityModeAssetPartitionKind partition,
            long budget,
            string[] paths,
            string idPrefix,
            string license)
        {
            var entries = paths.Select((path, index) => new CityModeAssetCatalogEntry(
                idPrefix + "." + (index + 1),
                AssetDatabase.AssetPathToGUID(path),
                Hash(path),
                license,
                AssetDatabase.LoadMainAssetAtPath(path))).ToArray();
            root.AddComponent<CityModeAssetPartitionCatalog>().Configure(
                Revision, partition, budget, entries);
        }

        static T Load<T>(string path) where T : UnityEngine.Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            return asset != null
                ? asset
                : throw new InvalidOperationException("Ported asset is missing: " + path);
        }

        static string Hash(string assetPath)
        {
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(assetPath);
            if (package == null)
                throw new InvalidOperationException("Package metadata missing: " + assetPath);
            var relative = assetPath.Substring(package.assetPath.Length).TrimStart('/', '\\');
            var physicalPath = Path.Combine(package.resolvedPath, relative.Replace('/', Path.DirectorySeparatorChar));
            using (var stream = File.OpenRead(physicalPath))
            using (var algorithm = SHA256.Create())
                return string.Concat(algorithm.ComputeHash(stream).Select(value => value.ToString("x2")));
        }
    }
}
