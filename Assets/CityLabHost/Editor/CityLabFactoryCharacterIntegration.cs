using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Victoria.CityMode;

namespace Victoria.CityLab.Editor
{
    public sealed class CityLabFactoryCharacterImporter : AssetPostprocessor
    {
        const string CharacterRoot = "Assets/CityLabHost/Adapted/Factory/Characters/";

        void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(CharacterRoot, StringComparison.OrdinalIgnoreCase))
                return;
            var importer = (ModelImporter)assetImporter;
            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.optimizeGameObjects = false;
            importer.importBlendShapes = false;
            importer.isReadable = false;
        }
    }

    [InitializeOnLoad]
    public static class CityLabFactoryCharacterIntegration
    {
        const string FactoryRoot = "Assets/CityLabHost/Adapted/Factory";
        const string CharacterRoot = FactoryRoot + "/Characters";
        const string RoleRoot = CharacterRoot + "/Roles";
        const string PrefabRoot = FactoryRoot + "/Prefabs/Characters";
        const string MaterialRoot = FactoryRoot + "/Materials/Characters";
        const string LibraryPath = "Assets/CityLabHost/Resources/CityLabVisualLibrary.asset";
        static readonly string[] RoleOrder =
        {
            "worker", "wealthy", "peasant", "religious",
            "soldier", "noble", "bourgeois", "beggar"
        };

        static CityLabFactoryCharacterIntegration()
        {
            EditorApplication.delayCall += () => Integrate(false);
        }

        [MenuItem("Victoria/CityLab/Integrate Factory Characters")]
        public static void IntegrateFromMenu() => Integrate(true);

        static void Integrate(bool force)
        {
            var library = AssetDatabase.LoadAssetAtPath<CityVisualLibrary>(LibraryPath);
            if (library == null || !AssetDatabase.IsValidFolder(RoleRoot))
                return;
            var models = FindRoleModels();
            if (models.Count != RoleOrder.Length)
            {
                if (force)
                    throw new InvalidOperationException(
                        $"Factory character capsule count is {models.Count}; expected {RoleOrder.Length}.");
                return;
            }

            var expectedPrefabs = RoleOrder.Select(role =>
                AssetDatabase.LoadAssetAtPath<GameObject>(PrefabRoot + "/CityLab_" + role + ".prefab")).ToArray();
            if (!force && expectedPrefabs.All(prefab => prefab != null) &&
                library.villagerPrefabs != null && library.villagerPrefabs.SequenceEqual(expectedPrefabs))
                return;

            EnsureFolder(PrefabRoot);
            EnsureFolder(MaterialRoot);
            var prefabs = new GameObject[models.Count];
            for (var index = 0; index < models.Count; index++)
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(models[index]);
                if (model == null)
                    throw new InvalidOperationException("Factory character model is not imported: " + models[index]);
                var animator = model.GetComponentInChildren<Animator>(true);
                if (animator == null || animator.avatar == null ||
                    !animator.avatar.isValid || !animator.avatar.isHuman)
                    throw new InvalidOperationException("Humanoid avatar validation failed: " + models[index]);
                var role = RoleOrder[index];
                prefabs[index] = BuildPrefab(model, role, PrefabRoot + "/CityLab_" + role + ".prefab");
            }

            library.villagerPrefabs = prefabs;
            library.villagerPrefab = prefabs[0];
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            Debug.Log("CITYLAB_FACTORY_CHARACTERS_INTEGRATED roles=8 lods=3 humanoid=true");
        }

        static List<string> FindRoleModels()
        {
            var paths = AssetDatabase.FindAssets(string.Empty, new[] { RoleRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)).ToArray();
            var result = new List<string>();
            foreach (var role in RoleOrder)
            {
                var token = "/role_" + role + "_";
                var matches = paths.Where(path => path.IndexOf(token,
                    StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
                if (matches.Length != 1)
                    throw new InvalidOperationException(
                        $"Factory role '{role}' resolves to {matches.Length} models.");
                result.Add(matches[0]);
            }
            return result;
        }

        static GameObject BuildPrefab(GameObject model, string role, string prefabPath)
        {
            var root = new GameObject("CityLab " + role);
            try
            {
                var visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
                visual.name = "Rigged Character";
                visual.transform.SetParent(root.transform, false);
                visual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                visual.transform.localScale = Vector3.one;
                PrefabUtility.UnpackPrefabInstance(visual,
                    PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                foreach (var sourceLod in visual.GetComponentsInChildren<LODGroup>(true))
                    UnityEngine.Object.DestroyImmediate(sourceLod);
                foreach (var collider in visual.GetComponentsInChildren<Collider>(true))
                    UnityEngine.Object.DestroyImmediate(collider);
                ReplaceMaterials(visual, role);
                ConfigureLods(root, visual);
                return PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        static void ConfigureLods(GameObject root, GameObject visual)
        {
            Renderer[] ForLevel(int level) => visual.GetComponentsInChildren<Renderer>(true)
                .Where(renderer => renderer.gameObject.name.IndexOf("_LOD" + level,
                    StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
            var lod0 = ForLevel(0);
            var lod1 = ForLevel(1);
            var lod2 = ForLevel(2);
            if (lod0.Length == 0 || lod0.Length != lod1.Length || lod0.Length != lod2.Length)
                throw new InvalidOperationException("Factory character requires matching renderers in all three LODs.");
            var group = root.AddComponent<LODGroup>();
            group.SetLODs(new[]
            {
                new LOD(0.48f, lod0),
                new LOD(0.20f, lod1),
                new LOD(0.035f, lod2)
            });
            group.RecalculateBounds();
        }

        static void ReplaceMaterials(GameObject visual, string role)
        {
            foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (var index = 0; index < materials.Length; index++)
                    if (materials[index] != null)
                        materials[index] = GetOrCreateMaterial(materials[index].name, role);
                renderer.sharedMaterials = materials;
            }
        }

        static Material GetOrCreateMaterial(string sourceName, string role)
        {
            var normalized = sourceName.Replace(" (Instance)", string.Empty).ToLowerInvariant();
            var safeName = string.Concat(normalized.Select(character =>
                char.IsLetterOrDigit(character) || character == '_' ? character : '_'));
            var path = MaterialRoot + "/" + role + "_" + safeName + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                throw new InvalidOperationException("URP Lit shader missing for Factory characters.");
            if (material == null)
            {
                material = new Material(shader) { name = "CityLab Character " + role + " " + normalized };
                AssetDatabase.CreateAsset(material, path);
            }
            var color = new Color(0.24f, 0.09f, 0.035f, 1f);
            var metallic = 0f;
            var smoothness = 0.18f;
            if (normalized.Contains("skin")) color = new Color(0.58f, 0.30f, 0.18f, 1f);
            else if (normalized.Contains("eyes")) color = new Color(0.07f, 0.22f, 0.17f, 1f);
            else if (normalized.Contains("hair")) color = new Color(0.055f, 0.025f, 0.012f, 1f);
            else if (normalized.Contains("cloth_primary")) color = RoleColor(role, false);
            else if (normalized.Contains("cloth_accent")) color = RoleColor(role, true);
            else if (normalized.Contains("leather")) color = new Color(0.10f, 0.035f, 0.014f, 1f);
            else if (normalized.Contains("wood")) color = new Color(0.25f, 0.08f, 0.02f, 1f);
            else if (normalized.Contains("iron"))
            {
                color = new Color(0.10f, 0.12f, 0.13f, 1f);
                metallic = 0.82f;
                smoothness = 0.38f;
            }
            material.shader = shader;
            material.enableInstancing = true;
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Color RoleColor(string role, bool accent)
        {
            if (role == "wealthy") return accent ? new Color(0.72f, 0.42f, 0.06f) : new Color(0.34f, 0.025f, 0.12f);
            if (role == "peasant") return accent ? new Color(0.34f, 0.24f, 0.08f) : new Color(0.15f, 0.12f, 0.045f);
            if (role == "religious") return accent ? new Color(0.17f, 0.12f, 0.07f) : new Color(0.025f, 0.030f, 0.040f);
            if (role == "soldier") return accent ? new Color(0.42f, 0.045f, 0.025f) : new Color(0.10f, 0.13f, 0.15f);
            if (role == "noble") return accent ? new Color(0.68f, 0.40f, 0.06f) : new Color(0.045f, 0.07f, 0.30f);
            if (role == "bourgeois") return accent ? new Color(0.46f, 0.23f, 0.05f) : new Color(0.06f, 0.21f, 0.18f);
            if (role == "beggar") return accent ? new Color(0.19f, 0.13f, 0.065f) : new Color(0.085f, 0.060f, 0.035f);
            return accent ? new Color(0.44f, 0.25f, 0.06f) : new Color(0.20f, 0.07f, 0.02f);
        }

        static void EnsureFolder(string path)
        {
            var normalized = path.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(normalized))
                return;
            var parent = Path.GetDirectoryName(normalized)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(parent))
                return;
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(normalized));
        }
    }
}
