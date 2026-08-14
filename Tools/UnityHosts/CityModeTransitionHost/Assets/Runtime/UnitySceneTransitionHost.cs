using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Victoria.CityMode.Integration;
using Victoria.CityMode.Presentation;

namespace Victoria.CityMode.TransitionHost
{
    /// <summary>
    /// Real SceneManager adapter owned by the host mirror. City Mode receives
    /// only the ICityModeTransitionHost port and never names either scene.
    /// </summary>
    public sealed class UnitySceneTransitionHost : ICityModeTransitionHost
    {
        readonly string mapSceneName;
        readonly string citySceneName;
        Task<ICityModePresentationView> currentLoad;

        public UnitySceneTransitionHost(string mapSceneName, string citySceneName)
        {
            this.mapSceneName = mapSceneName;
            this.citySceneName = citySceneName;
        }

        public Task<ICityModePresentationView> LoadCityAsync(
            CityLaunchContext context,
            IProgress<float> progress,
            CancellationToken cancellationToken)
        {
            currentLoad = LoadCoreAsync(progress, cancellationToken);
            return currentLoad;
        }

        async Task<ICityModePresentationView> LoadCoreAsync(
            IProgress<float> progress,
            CancellationToken cancellationToken)
        {
            if (SceneManager.GetSceneByName(citySceneName).isLoaded)
                throw new InvalidOperationException("City mirror scene is already loaded.");
            var operation = SceneManager.LoadSceneAsync(
                citySceneName, LoadSceneMode.Additive);
            if (operation == null)
                throw new InvalidOperationException("Unity did not create a city load operation.");

            try
            {
                while (!operation.isDone)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    progress?.Report(operation.progress);
                    await Task.Yield();
                }
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                // Unity scene loads cannot be aborted. Finish activation so the
                // shell's idempotent unload rollback can release the scene.
                while (!operation.isDone)
                    await Task.Yield();
                throw;
            }

            var cityScene = SceneManager.GetSceneByName(citySceneName);
            if (!cityScene.IsValid() || !cityScene.isLoaded)
                throw new InvalidOperationException("City mirror scene did not load.");
            SceneManager.SetActiveScene(cityScene);
            progress?.Report(1f);

            foreach (var root in cityScene.GetRootGameObjects())
            foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour is ICityModePresentationView view)
                    return view;
            }
            throw new InvalidOperationException(
                "City mirror scene has no ICityModePresentationView.");
        }

        public async Task UnloadCityAsync(
            CityLaunchContext context,
            CancellationToken cancellationToken)
        {
            var load = currentLoad;
            currentLoad = null;
            if (load != null && !load.IsCompleted)
            {
                try
                {
                    await load;
                }
                catch
                {
                    // The rollback below owns the final scene state.
                }
            }

            var cityScene = SceneManager.GetSceneByName(citySceneName);
            if (!cityScene.IsValid() || !cityScene.isLoaded)
                return;
            var operation = SceneManager.UnloadSceneAsync(cityScene);
            if (operation == null)
                return;
            while (!operation.isDone)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
            }
        }

        public Task RestoreMapAsync(
            CityLaunchContext context,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var mapScene = SceneManager.GetSceneByName(mapSceneName);
            if (!mapScene.IsValid() || !mapScene.isLoaded)
                throw new InvalidOperationException("Map mirror scene is unavailable.");
            SceneManager.SetActiveScene(mapScene);
            foreach (var root in mapScene.GetRootGameObjects())
            {
                var controller = root.GetComponentInChildren<MapMirrorController>(true);
                if (controller == null)
                    continue;
                controller.Restore(context);
                return Task.CompletedTask;
            }
            throw new InvalidOperationException("Map mirror controller is unavailable.");
        }
    }
}
