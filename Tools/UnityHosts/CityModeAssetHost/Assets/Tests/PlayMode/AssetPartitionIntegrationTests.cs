using System;
using System.Collections;
using System.Globalization;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Victoria.CityMode.Assets;

namespace Victoria.CityMode.AssetHost.Tests
{
    public sealed class AssetPartitionIntegrationTests
    {
        static readonly string[] PartitionScenes = { "AssetCommon", "AssetBiome", "AssetCity" };
        CityModeAssetPartitionLoader loader;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            var load = SceneManager.LoadSceneAsync("AssetMap", LoadSceneMode.Single);
            while (load != null && !load.isDone)
                yield return null;
            loader = new CityModeAssetPartitionLoader(new UnitySceneAssetPartitionHost());
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (loader != null && loader.IsLoaded)
            {
                CityModeAssetPartitionResult result = null;
                yield return Await(loader.UnloadCityAsync(), value => result = value);
                Assert.IsTrue(result.Succeeded, result.Message);
            }
            loader?.Dispose();
            loader = null;
        }

        [UnityTest]
        public IEnumerator RealPartitionsLoadWithoutMissingReferencesAndUnloadInReverse()
        {
            CityModeAssetPartitionResult result = null;
            yield return Await(loader.LoadCityAsync(), value => result = value);
            Assert.IsTrue(result.Succeeded, result.Message);
            Assert.IsTrue(loader.IsLoaded);
            Assert.AreEqual(3, loader.Metrics.CompletedLoads);

            var catalogCount = 0;
            var assetCount = 0;
            foreach (var sceneName in PartitionScenes)
            {
                var scene = SceneManager.GetSceneByName(sceneName);
                Assert.IsTrue(scene.IsValid() && scene.isLoaded, sceneName);
                foreach (var root in scene.GetRootGameObjects())
                foreach (var catalog in root.GetComponentsInChildren<CityModeAssetPartitionCatalog>(true))
                {
                    catalog.Validate();
                    catalogCount++;
                    foreach (var entry in catalog.Entries)
                    {
                        Assert.IsNotNull(entry.Asset, entry.Id);
                        assetCount++;
                    }
                }
            }
            Assert.AreEqual(3, catalogCount);
            Assert.AreEqual(11, assetCount);
            Assert.Greater(UnityEngine.Object.FindObjectsByType<Renderer>(
                FindObjectsInactive.Include, FindObjectsSortMode.None).Length, 3);

            yield return Await(loader.UnloadCityAsync(), value => result = value);
            Assert.IsTrue(result.Succeeded, result.Message);
            Assert.AreEqual(3, loader.Metrics.CompletedUnloads);
            foreach (var sceneName in PartitionScenes)
                Assert.IsFalse(SceneManager.GetSceneByName(sceneName).isLoaded, sceneName);
        }

        [UnityTest]
        public IEnumerator TenRealPartitionCyclesMeetMemoryAndTimingBudgets()
        {
            GC.Collect();
            yield return null;
            var memoryBefore = Profiler.GetTotalAllocatedMemoryLong();
            for (var cycle = 0; cycle < 10; cycle++)
            {
                CityModeAssetPartitionResult result = null;
                yield return Await(loader.LoadCityAsync(), value => result = value);
                Assert.IsTrue(result.Succeeded, "load " + cycle + ": " + result.Message);
                yield return null;
                yield return Await(loader.UnloadCityAsync(), value => result = value);
                Assert.IsTrue(result.Succeeded, "unload " + cycle + ": " + result.Message);
                yield return null;
            }
            GC.Collect();
            yield return null;
            var memoryAfter = Profiler.GetTotalAllocatedMemoryLong();
            var delta = Math.Max(0L, memoryAfter - memoryBefore);
            var metrics = loader.Metrics;
            Debug.Log(
                "CITY_MODE_ASSET_METRICS_OK cycles=10 loads=" + metrics.CompletedLoads +
                " unloads=" + metrics.CompletedUnloads +
                " peak_partition_bytes=" + metrics.PeakPartitionResidentBytes +
                " allocated_delta_bytes=" + delta +
                " load_max_ms=" + Format(metrics.MaximumLoadMilliseconds) +
                " unload_max_ms=" + Format(metrics.MaximumUnloadMilliseconds));
            Assert.AreEqual(30, metrics.CompletedLoads);
            Assert.AreEqual(30, metrics.CompletedUnloads);
            Assert.Less(metrics.MaximumLoadMilliseconds, 10000d);
            Assert.Less(metrics.MaximumUnloadMilliseconds, 5000d);
            Assert.Less(delta, 64L * 1024L * 1024L);
        }

        static IEnumerator Await<T>(Task<T> task, Action<T> complete)
        {
            while (!task.IsCompleted)
                yield return null;
            if (task.IsFaulted)
                throw task.Exception ?? new Exception("Asset partition task faulted.");
            if (task.IsCanceled)
                throw new OperationCanceledException();
            complete(task.Result);
        }

        static string Format(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
