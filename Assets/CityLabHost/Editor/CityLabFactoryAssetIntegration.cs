using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Victoria.CityMode;

namespace Victoria.CityLab.Editor
{
    [InitializeOnLoad]
    public static class CityLabFactoryAssetIntegration
    {
        const string FactoryRoot = "Assets/CityLabHost/Adapted/Factory";
        const string PrefabRoot = FactoryRoot + "/Prefabs";
        const string MaterialRoot = FactoryRoot + "/Materials";
        const string LibraryPath = "Assets/CityLabHost/Resources/CityLabVisualLibrary.asset";
        internal static readonly string[] ModelPaths =
        {
            FactoryRoot + "/Models/building_sawmill_frontier_01_a.fbx",
            FactoryRoot + "/Models/building_sawmill_frontier_01_b.fbx",
            FactoryRoot + "/Models/building_sawmill_frontier_01_c.fbx"
        };

        static readonly string[] PrefabPaths =
        {
            PrefabRoot + "/CityLab_Lumber_Camp_Factory_A.prefab",
            PrefabRoot + "/CityLab_Lumber_Camp_Factory_B.prefab",
            PrefabRoot + "/CityLab_Lumber_Camp_Factory_C.prefab"
        };

        static CityLabFactoryAssetIntegration()
        {
            EditorApplication.delayCall += () => Integrate(false);
        }

        [MenuItem("Victoria/CityLab/Integrate Factory Assets")]
        public static void IntegrateFromMenu() => Integrate(true);

        internal static void Integrate(bool force)
        {
            var library = AssetDatabase.LoadAssetAtPath<CityVisualLibrary>(LibraryPath);
            if (library == null)
                return;

            var existing = PrefabPaths.Select(path =>
                AssetDatabase.LoadAssetAtPath<GameObject>(path)).ToArray();
            if (!force && existing.All(prefab => prefab != null) &&
                library.lumberCampPrefabs != null &&
                library.lumberCampPrefabs.SequenceEqual(existing))
                return;

            EnsureFolder(FactoryRoot);
            EnsureFolder(PrefabRoot);
            EnsureFolder(MaterialRoot);

            var admitted = new GameObject[ModelPaths.Length];
            for (var index = 0; index < ModelPaths.Length; index++)
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPaths[index]);
                if (model == null)
                    return;
                admitted[index] = BuildPrefab(model, PrefabPaths[index], index);
            }
            library.lumberCampPrefabs = admitted;
            library.lumberCampPrefab = admitted[0];
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            Debug.Log("CITYLAB_FACTORY_INTEGRATED family=building_sawmill_frontier_01 " +
                "variants=" + admitted.Length + " stages=4");
        }

        internal static GameObject BuildPrefab(GameObject model, string prefabPath, int variantIndex,
            bool addLumberCampVisual = true)
        {
            var root = new GameObject("CityLab Factory " + model.name + " " + (char)('A' + variantIndex));
            try
            {
                var visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
                visual.name = "Factory Model Source";
                visual.transform.SetParent(root.transform, false);
                visual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                visual.transform.localScale = Vector3.one;

                // FBX import can synthesize LODGroups from the _LOD suffixes. The
                // admitted prefab owns its phase-specific groups, so keep a single
                // authority for every renderer.
                PrefabUtility.UnpackPrefabInstance(visual,
                    PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                foreach (var sourceLod in visual.GetComponentsInChildren<LODGroup>(true))
                    UnityEngine.Object.DestroyImmediate(sourceLod);

                foreach (var collider in visual.GetComponentsInChildren<Collider>(true))
                    UnityEngine.Object.DestroyImmediate(collider);
                ReplaceMaterials(visual);
                var stageRoots = ConfigureConstructionStages(root, visual);
                root.AddComponent<FactoryConstructionVisual>().Configure(stageRoots);
                return PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        static GameObject[] ConfigureConstructionStages(GameObject root, GameObject visual)
        {
            var phaseTokens = new[] { "__P01_FOUNDATION_", "__P02_FRAME_", "__P03_ROOF_", "__P04_DETAILS_" };
            var phaseLabels = new[] { "01 Foundation", "02 Frame", "03 Roof", "04 Details" };
            var allRenderers = visual.GetComponentsInChildren<Renderer>(true);
            var stageRoots = new GameObject[phaseTokens.Length];
            for (var stageIndex = 0; stageIndex < phaseTokens.Length; stageIndex++)
            {
                var stageRoot = new GameObject(phaseLabels[stageIndex]);
                stageRoot.transform.SetParent(root.transform, false);
                stageRoots[stageIndex] = stageRoot;
                var stageRenderers = allRenderers.Where(renderer =>
                    renderer.gameObject.name.IndexOf(phaseTokens[stageIndex],
                        StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
                if (stageRenderers.Length != 3)
                    throw new InvalidOperationException("Factory construction phase must contain exactly three LOD renderers: " + phaseLabels[stageIndex]);
                foreach (var renderer in stageRenderers)
                    renderer.transform.SetParent(stageRoot.transform, true);
                ConfigureLods(stageRoot, stageRenderers);
            }
            return stageRoots;
        }

        static void ConfigureLods(GameObject stageRoot, Renderer[] renderers)
        {
            Renderer[] ForLevel(int level) => renderers.Where(renderer =>
                renderer.gameObject.name.IndexOf("_LOD" + level,
                    StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
            var lod0 = ForLevel(0);
            var lod1 = ForLevel(1);
            var lod2 = ForLevel(2);
            if (lod0.Length != 1 || lod1.Length != 1 || lod2.Length != 1)
                throw new InvalidOperationException("Each Factory construction phase requires one renderer per LOD.");

            var group = stageRoot.AddComponent<LODGroup>();
            group.SetLODs(new[]
            {
                new LOD(0.52f, lod0),
                new LOD(0.22f, lod1),
                new LOD(0.045f, lod2)
            });
            group.RecalculateBounds();
        }

        static void ReplaceMaterials(GameObject visual)
        {
            foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
            {
                var replacements = renderer.sharedMaterials;
                for (var index = 0; index < replacements.Length; index++)
                {
                    if (replacements[index] != null)
                        replacements[index] = GetOrCreateMaterial(replacements[index].name);
                }
                renderer.sharedMaterials = replacements;
            }
        }

        static Material GetOrCreateMaterial(string sourceName)
        {
            var safeName = string.Concat(sourceName.Select(character =>
                char.IsLetterOrDigit(character) || character == '_' ? character : '_'));
            var path = MaterialRoot + "/" + safeName + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                throw new InvalidOperationException("URP Lit shader missing for Factory material admission.");
            if (material == null)
            {
                material = new Material(shader) { name = "CityLab Factory " + sourceName };
                AssetDatabase.CreateAsset(material, path);
            }

            var lower = sourceName.ToLowerInvariant();
            var color = new Color(0.16f, 0.055f, 0.015f, 1f);
            var metallic = 0f;
            var smoothness = 0.16f;
            if (lower.Contains("timber_a")) color = HtmlColor("#0E0602");
            else if (lower.Contains("timber_b")) color = HtmlColor("#160B05");
            else if (lower.Contains("timber_c")) color = HtmlColor("#291306");
            else if (lower.Contains("roof_accent_a")) color = HtmlColor("#2F0D05");
            else if (lower.Contains("roof_accent_b")) color = HtmlColor("#343D45");
            else if (lower.Contains("roof_accent_c")) color = HtmlColor("#55603A");
            else if (lower.Contains("roof_a")) color = HtmlColor("#120502");
            else if (lower.Contains("roof_b")) color = HtmlColor("#171B20");
            else if (lower.Contains("roof_c")) color = HtmlColor("#252B16");
            else if (lower.Contains("stone")) color = new Color(0.18f, 0.20f, 0.19f, 1f);
            else if (lower.Contains("plaster")) color = new Color(0.28f, 0.18f, 0.07f, 1f);
            else if (lower.Contains("fresh") || lower.Contains("cut_wood")) color = new Color(0.55f, 0.25f, 0.05f, 1f);
            else if (lower.Contains("bark")) color = new Color(0.075f, 0.022f, 0.008f, 1f);
            else if (lower.Contains("roof") || lower.Contains("shingle")) color = new Color(0.15f, 0.04f, 0.015f, 1f);
            else if (lower.Contains("iron"))
            {
                color = new Color(0.025f, 0.028f, 0.03f, 1f);
                metallic = 0.82f;
                smoothness = 0.34f;
            }
            else if (lower.Contains("steel"))
            {
                color = new Color(0.30f, 0.34f, 0.34f, 1f);
                metallic = 0.92f;
                smoothness = 0.58f;
            }
            else if (lower.Contains("bronze"))
            {
                color = new Color(0.36f, 0.12f, 0.025f, 1f);
                metallic = 0.76f;
                smoothness = 0.48f;
            }
            else if (lower.Contains("ember"))
            {
                color = new Color(0.95f, 0.045f, 0.003f, 1f);
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 3.2f);
            }

            material.shader = shader;
            material.enableInstancing = true;
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Color HtmlColor(string value)
        {
            if (ColorUtility.TryParseHtmlString(value, out var color))
                return color;
            throw new InvalidOperationException("Invalid Factory palette color: " + value);
        }

        static void EnsureFolder(string path)
        {
            var parts = path.Split('/');
            var current = parts[0];
            for (var index = 1; index < parts.Length; index++)
            {
                var next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[index]);
                current = next;
            }
        }
    }

    public sealed class CityLabFactoryAssetPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (importedAssets.Any(path => CityLabFactoryAssetIntegration.ModelPaths.Contains(path)))
                EditorApplication.delayCall += () => CityLabFactoryAssetIntegration.Integrate(true);
        }
    }
}
