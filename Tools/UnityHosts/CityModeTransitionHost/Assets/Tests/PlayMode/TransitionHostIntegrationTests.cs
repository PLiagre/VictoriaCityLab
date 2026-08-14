using System;
using System.Collections;
using System.Globalization;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Victoria.CityMode.Integration;
using Victoria.CityMode.Presentation;

namespace Victoria.CityMode.TransitionHost.Tests
{
    public sealed class TransitionHostIntegrationTests
    {
        const string MapScene = "MapMirror";
        const string CityScene = "CityModeView";
        CityModeTransitionShell shell;
        MapMirrorController map;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            var load = SceneManager.LoadSceneAsync(MapScene, LoadSceneMode.Single);
            while (load != null && !load.isDone)
                yield return null;
            map = UnityEngine.Object.FindFirstObjectByType<MapMirrorController>();
            Assert.IsNotNull(map);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            shell?.Dispose();
            shell = null;
            var city = SceneManager.GetSceneByName(CityScene);
            if (city.IsValid() && city.isLoaded)
            {
                var unload = SceneManager.UnloadSceneAsync(city);
                while (unload != null && !unload.isDone)
                    yield return null;
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator SelectedCityLoadsAdditivelyAndRestoresViewport()
        {
            var gateway = new FakeGateway();
            var host = new UnitySceneTransitionHost(MapScene, CityScene);
            shell = new CityModeTransitionShell(host);
            var context = map.SelectCity(
                "session:mirror:one",
                "city:paris",
                "cell:paris",
                "{\"x\":0.42,\"y\":0.51,\"zoom\":3}");

            CityModeTransitionResult entered = null;
            yield return Await(shell.EnterAsync(context, gateway, gateway), value => entered = value);
            Assert.IsTrue(entered.Succeeded, entered.Message);
            Assert.IsTrue(SceneManager.GetSceneByName(CityScene).isLoaded);
            Assert.AreEqual(CityScene, SceneManager.GetActiveScene().name);
            var view = UnityEngine.Object.FindFirstObjectByType<CityModeMirrorView>();
            Assert.IsNotNull(view);
            Assert.AreEqual("city:paris", view.OpenedCityId);
            Assert.AreEqual(17, view.PresentedRevision);

            CityModeTransitionResult exited = null;
            yield return Await(shell.ExitAsync(), value => exited = value);
            Assert.IsTrue(exited.Succeeded, exited.Message);
            Assert.IsFalse(SceneManager.GetSceneByName(CityScene).isLoaded);
            Assert.AreEqual(MapScene, SceneManager.GetActiveScene().name);
            Assert.AreEqual("cell:paris", map.SelectedMapCellId);
            Assert.AreEqual("map:mirror", map.RestoredViewId);
            Assert.AreEqual(
                "{\"x\":0.42,\"y\":0.51,\"zoom\":3}",
                map.RestoredViewStateJson);
        }

        [UnityTest]
        public IEnumerator FiftyRealSceneTransitionsMeetBudgetsAndStayBounded()
        {
            var gateway = new FakeGateway();
            var host = new UnitySceneTransitionHost(MapScene, CityScene);
            shell = new CityModeTransitionShell(host);
            GC.Collect();
            yield return null;
            var memoryBefore = Profiler.GetTotalAllocatedMemoryLong();

            for (var index = 0; index < 50; index++)
            {
                var context = map.SelectCity(
                    "session:mirror:" + index,
                    "city:paris",
                    "cell:paris",
                    "{\"cycle\":" + index + "}");
                CityModeTransitionResult entered = null;
                yield return Await(
                    shell.EnterAsync(context, gateway, gateway),
                    value => entered = value);
                Assert.IsTrue(entered.Succeeded, "enter " + index + ": " + entered.Message);

                CityModeTransitionResult exited = null;
                yield return Await(shell.ExitAsync(), value => exited = value);
                Assert.IsTrue(exited.Succeeded, "exit " + index + ": " + exited.Message);
                yield return null;
            }

            GC.Collect();
            yield return null;
            var memoryAfter = Profiler.GetTotalAllocatedMemoryLong();
            var memoryDelta = Math.Max(0L, memoryAfter - memoryBefore);
            var metrics = shell.Metrics;
            Debug.Log(
                "CITY_MODE_TRANSITION_METRICS_OK cycles=50 cold_ms=" +
                Format(metrics.ColdEntryMilliseconds) +
                " warm_max_ms=" + Format(metrics.MaximumWarmEntryMilliseconds) +
                " return_max_ms=" + Format(metrics.MaximumReturnMilliseconds) +
                " allocated_delta_bytes=" + memoryDelta);

            Assert.AreEqual(50, metrics.CompletedEntries);
            Assert.AreEqual(50, metrics.CompletedReturns);
            Assert.Less(metrics.ColdEntryMilliseconds, 10000d);
            Assert.Less(metrics.MaximumWarmEntryMilliseconds, 3000d);
            Assert.Less(metrics.MaximumReturnMilliseconds, 5000d);
            Assert.Less(memoryDelta, 64L * 1024L * 1024L);
            Assert.IsFalse(CityModePresentationHost.HasActiveInstance);
            Assert.AreEqual(MapScene, SceneManager.GetActiveScene().name);
        }

        static IEnumerator Await<T>(Task<T> task, Action<T> complete)
        {
            while (!task.IsCompleted)
                yield return null;
            if (task.IsFaulted)
                throw task.Exception ?? new Exception("Transition task faulted.");
            if (task.IsCanceled)
                throw new OperationCanceledException();
            complete(task.Result);
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
                    payloadSha256 = new string('e', 64)
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
