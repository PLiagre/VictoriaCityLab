using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using Victoria.CityMode.Assets;

namespace Victoria.CityMode.AssetHost
{
    /// <summary>Scene addresses and SceneManager stay exclusively in the host.</summary>
    public sealed class UnitySceneAssetPartitionHost : ICityModeAssetPartitionHost
    {
        readonly IReadOnlyDictionary<CityModeAssetPartitionKind, string> sceneNames;

        public UnitySceneAssetPartitionHost()
            : this(new Dictionary<CityModeAssetPartitionKind, string>
            {
                { CityModeAssetPartitionKind.Common, "AssetCommon" },
                { CityModeAssetPartitionKind.Biome, "AssetBiome" },
                { CityModeAssetPartitionKind.City, "AssetCity" }
            })
        {
        }

        public UnitySceneAssetPartitionHost(
            IReadOnlyDictionary<CityModeAssetPartitionKind, string> sceneNames)
        {
            this.sceneNames = sceneNames ?? throw new ArgumentNullException(nameof(sceneNames));
        }

        public async Task<CityModeAssetPartitionLoad> LoadAsync(
            CityModeAssetPartitionKind partition,
            CancellationToken cancellationToken)
        {
            var sceneName = NameFor(partition);
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
                throw new InvalidOperationException("Asset partition is already loaded: " + partition);
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (operation == null)
                throw new InvalidOperationException("Unity did not create a load operation: " + partition);
            try
            {
                while (!operation.isDone)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Yield();
                }
                cancellationToken.ThrowIfCancellationRequested();

                var scene = SceneManager.GetSceneByName(sceneName);
                var catalog = FindCatalog(scene);
                if (catalog.Partition != partition)
                    throw new InvalidOperationException("Partition scene contains the wrong catalogue: " + sceneName);
                catalog.Validate();
                var residentBytes = MeasureSceneResidentBytes(scene, catalog);
                return new CityModeAssetPartitionLoad(
                    partition,
                    catalog.Entries.Count,
                    residentBytes,
                    catalog.MaxResidentBytes);
            }
            catch
            {
                try
                {
                    await UnloadAsync(partition, CancellationToken.None);
                }
                catch
                {
                    // Preserve the original load/cancellation failure.
                }
                throw;
            }
        }

        public async Task UnloadAsync(
            CityModeAssetPartitionKind partition,
            CancellationToken cancellationToken)
        {
            var scene = SceneManager.GetSceneByName(NameFor(partition));
            if (scene.IsValid() && scene.isLoaded)
            {
                var operation = SceneManager.UnloadSceneAsync(scene);
                while (operation != null && !operation.isDone)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Yield();
                }
            }
            var release = Resources.UnloadUnusedAssets();
            while (release != null && !release.isDone)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
            }
        }

        string NameFor(CityModeAssetPartitionKind partition)
        {
            if (!sceneNames.TryGetValue(partition, out var value) || string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException("No scene address for asset partition: " + partition);
            return value;
        }

        static CityModeAssetPartitionCatalog FindCatalog(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
                throw new InvalidOperationException("Asset partition scene failed to load.");
            CityModeAssetPartitionCatalog found = null;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var catalog in root.GetComponentsInChildren<CityModeAssetPartitionCatalog>(true))
            {
                if (found != null)
                    throw new InvalidOperationException("Asset partition scene contains multiple catalogues.");
                found = catalog;
            }
            return found != null
                ? found
                : throw new InvalidOperationException("Asset partition scene contains no catalogue.");
        }

        static long MeasureSceneResidentBytes(
            Scene scene,
            CityModeAssetPartitionCatalog catalog)
        {
            long total = 0;
            var measured = new HashSet<int>();
            void Add(UnityEngine.Object value)
            {
                if (value == null || !measured.Add(value.GetInstanceID()))
                    return;
                total += Math.Max(0L, Profiler.GetRuntimeMemorySizeLong(value));
            }

            foreach (var entry in catalog.Entries)
                Add(entry.Asset);
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
                    Add(filter.sharedMesh);
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer is SkinnedMeshRenderer skinned)
                        Add(skinned.sharedMesh);
                    foreach (var material in renderer.sharedMaterials)
                    {
                        Add(material);
                        if (material == null)
                            continue;
                        foreach (var property in material.GetTexturePropertyNames())
                            Add(material.GetTexture(property));
                    }
                }
            }
            return total;
        }
    }
}
