using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using Victoria.CityMode.Integration;
using Victoria.CityMode.Presentation;

namespace Victoria.CityMode.TransitionHost
{
    /// <summary>Opt-in player proof; inert unless --city-transition-probe is passed.</summary>
    public sealed class TransitionPlayerProbe : MonoBehaviour
    {
        const string ProbeFlag = "--city-transition-probe";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void BootIfRequested()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), ProbeFlag) < 0)
                return;
            var root = new GameObject(nameof(TransitionPlayerProbe));
            DontDestroyOnLoad(root);
            root.AddComponent<TransitionPlayerProbe>();
        }

        async void Start()
        {
            var exitCode = 1;
            CityModeTransitionShell shell = null;
            try
            {
                var map = UnityEngine.Object.FindFirstObjectByType<MapMirrorController>();
                if (map == null)
                    throw new InvalidOperationException("Map mirror controller is missing.");
                var gateway = new FakeGateway();
                shell = new CityModeTransitionShell(
                    new UnitySceneTransitionHost("MapMirror", "CityModeView"));
                for (var index = 0; index < 50; index++)
                {
                    var context = map.SelectCity(
                        "session:player:" + index,
                        "city:paris",
                        "cell:paris",
                        "{\"cycle\":" + index + "}");
                    var entered = await shell.EnterAsync(context, gateway, gateway);
                    if (entered == null || !entered.Succeeded)
                        throw new InvalidOperationException(
                            "enter " + index + " failed: " + entered?.Message);

                    var exited = await shell.ExitAsync();
                    if (exited == null || !exited.Succeeded)
                        throw new InvalidOperationException(
                            "exit " + index + " failed: " + exited?.Message);
                }

                var metrics = shell.Metrics;
                if (SceneManager.GetActiveScene().name != "MapMirror" ||
                    CityModePresentationHost.HasActiveInstance ||
                    metrics.ColdEntryMilliseconds >= 10000d ||
                    metrics.MaximumWarmEntryMilliseconds >= 3000d ||
                    metrics.MaximumReturnMilliseconds >= 5000d)
                    throw new InvalidOperationException("Player transition gates failed.");

                Debug.Log(
                    "CITY_MODE_TRANSITION_PLAYER_OK cycles=50 cold_ms=" +
                    Format(metrics.ColdEntryMilliseconds) +
                    " warm_max_ms=" + Format(metrics.MaximumWarmEntryMilliseconds) +
                    " return_max_ms=" + Format(metrics.MaximumReturnMilliseconds) +
                    " restored_cell=" + map.SelectedMapCellId);
                exitCode = 0;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                shell?.Dispose();
                Application.Quit(exitCode);
            }
        }

        static string Format(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        sealed class FakeGateway : ICityModeSnapshotSource, ICityModeIntentSink
        {
            public CitySnapshotEnvelope ReadSnapshot(CityLaunchContext context)
            {
                return new CitySnapshotEnvelope
                {
                    cityId = context.cityId,
                    worldTick = context.worldTick,
                    stateRevision = context.stateRevision,
                    isFullSnapshot = true,
                    payloadJson = "{}",
                    payloadSha256 = new string('f', 64)
                };
            }

            public CityIntentReceipt SubmitIntent(CityIntentEnvelope intent)
            {
                return new CityIntentReceipt
                {
                    sessionId = intent.sessionId,
                    intentId = intent.intentId,
                    cityId = intent.cityId,
                    status = CityIntentStatus.Accepted,
                    errorCode = CityModeErrorCode.None,
                    resultingWorldTick = intent.issuedAtWorldTick,
                    resultingStateRevision = intent.expectedStateRevision
                };
            }
        }
    }
}
