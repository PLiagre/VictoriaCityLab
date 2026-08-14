using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using Victoria.CityMode.Assets;

namespace Victoria.CityMode.AssetHost
{
    /// <summary>Opt-in player proof; inert unless --city-asset-probe is passed.</summary>
    public sealed class AssetPartitionPlayerProbe : MonoBehaviour
    {
        const string ProbeFlag = "--city-asset-probe";
        const string CaptureArgument = "--city-asset-capture-dir";
        const int Cycles = 10;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void BootIfRequested()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), ProbeFlag) < 0)
                return;
            var root = new GameObject(nameof(AssetPartitionPlayerProbe));
            DontDestroyOnLoad(root);
            root.AddComponent<AssetPartitionPlayerProbe>();
        }

        async void Start()
        {
            var exitCode = 1;
            CityModeAssetPartitionLoader loader = null;
            try
            {
                var captureDirectory = ReadArgument(CaptureArgument);
                if (string.IsNullOrWhiteSpace(captureDirectory))
                    throw new InvalidOperationException(CaptureArgument + " is required.");
                captureDirectory = Path.GetFullPath(captureDirectory);
                Directory.CreateDirectory(captureDirectory);

                GC.Collect();
                await Frames(2);
                var memoryBefore = Profiler.GetTotalAllocatedMemoryLong();
                loader = new CityModeAssetPartitionLoader(new UnitySceneAssetPartitionHost());

                var firstLoad = await loader.LoadCityAsync();
                if (!firstLoad.Succeeded)
                    throw new InvalidOperationException(firstLoad.Message);
                await Frames(4);
                await CaptureZooms(captureDirectory);
                var firstUnload = await loader.UnloadCityAsync();
                if (!firstUnload.Succeeded)
                    throw new InvalidOperationException(firstUnload.Message);

                for (var cycle = 1; cycle < Cycles; cycle++)
                {
                    var loaded = await loader.LoadCityAsync();
                    if (!loaded.Succeeded)
                        throw new InvalidOperationException("load cycle " + cycle + ": " + loaded.Message);
                    await Frames(1);
                    var unloaded = await loader.UnloadCityAsync();
                    if (!unloaded.Succeeded)
                        throw new InvalidOperationException("unload cycle " + cycle + ": " + unloaded.Message);
                }

                GC.Collect();
                await Frames(3);
                var memoryAfter = Profiler.GetTotalAllocatedMemoryLong();
                var memoryDelta = Math.Max(0L, memoryAfter - memoryBefore);
                var metrics = loader.Metrics;
                if (metrics.CompletedLoads != Cycles * 3 ||
                    metrics.CompletedUnloads != Cycles * 3 ||
                    metrics.MaximumLoadMilliseconds >= 10000d ||
                    metrics.MaximumUnloadMilliseconds >= 5000d ||
                    memoryDelta >= 64L * 1024L * 1024L)
                    throw new InvalidOperationException("Asset partition player budgets failed.");

                Debug.Log(
                    "CITY_MODE_ASSET_PLAYER_OK cycles=" + Cycles +
                    " assets=11 common_bytes=" + Resident(metrics, CityModeAssetPartitionKind.Common) +
                    " biome_bytes=" + Resident(metrics, CityModeAssetPartitionKind.Biome) +
                    " city_bytes=" + Resident(metrics, CityModeAssetPartitionKind.City) +
                    " peak_partition_bytes=" + metrics.PeakPartitionResidentBytes +
                    " allocated_delta_bytes=" + memoryDelta +
                    " load_max_ms=" + Format(metrics.MaximumLoadMilliseconds) +
                    " unload_max_ms=" + Format(metrics.MaximumUnloadMilliseconds) +
                    " captures=3");
                exitCode = 0;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                loader?.Dispose();
                Application.Quit(exitCode);
            }
        }

        static async Task CaptureZooms(string directory)
        {
            var camera = Camera.main;
            if (camera == null)
                throw new InvalidOperationException("Asset host main camera is missing.");
            var target = new Vector3(0f, 5f, 2f);
            Capture(camera, target, new Vector3(0f, 62f, -68f), 52f,
                Path.Combine(directory, "01_strategic.png"));
            Capture(camera, target, new Vector3(0f, 31f, -38f), 48f,
                Path.Combine(directory, "02_district.png"));
            Capture(camera, new Vector3(0f, 5f, 0f), new Vector3(0f, 13f, -18f), 42f,
                Path.Combine(directory, "03_detail.png"));
            await Task.CompletedTask;
        }

        static void Capture(
            Camera camera,
            Vector3 target,
            Vector3 position,
            float fieldOfView,
            string path)
        {
            camera.transform.position = position;
            camera.transform.LookAt(target);
            camera.fieldOfView = fieldOfView;
            if (File.Exists(path))
                File.Delete(path);

            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var renderTarget = RenderTexture.GetTemporary(1280, 720, 24, RenderTextureFormat.ARGB32);
            var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = renderTarget;
                RenderTexture.active = renderTarget;
                camera.Render();
                pixels.ReadPixels(new Rect(0f, 0f, 1280f, 720f), 0, 0, false);
                pixels.Apply(false, false);
                File.WriteAllBytes(path, pixels.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(renderTarget);
                Destroy(pixels);
            }
            if (!File.Exists(path) || new FileInfo(path).Length <= 10_000)
                throw new IOException("Capture was not written: " + path);
        }

        static async Task Frames(int count)
        {
            for (var index = 0; index < count; index++)
                await Task.Yield();
        }

        static long Resident(
            CityModeAssetPartitionMetrics metrics,
            CityModeAssetPartitionKind partition)
        {
            return metrics.ResidentBytes.TryGetValue(partition, out var value) ? value : -1L;
        }

        static string ReadArgument(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (var index = 0; index + 1 < args.Length; index++)
                if (string.Equals(args[index], name, StringComparison.Ordinal))
                    return args[index + 1];
            return null;
        }

        static string Format(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
