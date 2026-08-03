using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Victoria.CityMode;

namespace Victoria.CityLab.Editor
{
    [InitializeOnLoad]
    public static class CityLabFactoryBuildingPilotIntegration
    {
        const string ModelRoot = "Assets/CityLabHost/Adapted/Factory/Models";
        const string PrefabRoot = "Assets/CityLabHost/Adapted/Factory/Prefabs";
        const string LibraryPath = "Assets/CityLabHost/Resources/CityLabVisualLibrary.asset";
        static readonly string[] Families =
        {
            "building_residence_frontier_01",
            "building_granary_frontier_01",
            "building_warehouse_frontier_01",
            "building_market_frontier_01",
            "building_blacksmith_frontier_01",
            "building_barn_frontier_01",
            "building_chapel_frontier_01"
        };

        internal static readonly string[] ModelPaths = Families.SelectMany(family =>
            new[] { "a", "b", "c" }.Select(variant =>
                $"{ModelRoot}/{family}_{variant}.fbx")).ToArray();

        static CityLabFactoryBuildingPilotIntegration()
        {
            EditorApplication.delayCall += () => Integrate(false);
        }

        [MenuItem("Victoria/CityLab/Integrate Factory Building Pilot")]
        public static void IntegrateFromMenu() => Integrate(true);

        internal static void Integrate(bool force)
        {
            var library = AssetDatabase.LoadAssetAtPath<CityVisualLibrary>(LibraryPath);
            if (library == null || ModelPaths.Any(path =>
                    AssetDatabase.LoadAssetAtPath<GameObject>(path) == null))
                return;
            EnsureFolder(PrefabRoot);
            var admitted = new GameObject[Families.Length][];
            for (var familyIndex = 0; familyIndex < Families.Length; familyIndex++)
            {
                admitted[familyIndex] = new GameObject[3];
                for (var variantIndex = 0; variantIndex < 3; variantIndex++)
                {
                    var modelPath = ModelPaths[familyIndex * 3 + variantIndex];
                    var prefabPath = $"{PrefabRoot}/CityLab_{Families[familyIndex]}_{(char)('A' + variantIndex)}.prefab";
                    var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    admitted[familyIndex][variantIndex] = !force && existing != null
                        ? existing
                        : CityLabFactoryAssetIntegration.BuildPrefab(
                            AssetDatabase.LoadAssetAtPath<GameObject>(modelPath),
                            prefabPath, variantIndex, false);
                }
            }
            library.housePrefabs = admitted[0];
            library.granaryPrefabs = admitted[1];
            library.warehousePrefabs = admitted[2];
            library.marketPrefabs = admitted[3];
            library.blacksmithPrefabs = admitted[4];
            library.barnPrefabs = admitted[5];
            library.chapelPrefabs = admitted[6];
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            Debug.Log("CITYLAB_FACTORY_BUILDING_PILOT_INTEGRATED families=7 variants=21 stages=4");
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

    public sealed class CityLabFactoryBuildingPilotPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (importedAssets.Any(path => CityLabFactoryBuildingPilotIntegration.ModelPaths.Contains(path)))
                EditorApplication.delayCall += () => CityLabFactoryBuildingPilotIntegration.Integrate(true);
        }
    }
}
