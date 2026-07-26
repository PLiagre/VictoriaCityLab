using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Victoria.CityMode.Tests
{
    public sealed class CityLabPlayModeTests
    {
        [UnityTest]
        public IEnumerator Bootstrap_CreatesPlayableWorld()
        {
            SceneManager.LoadScene("CityLab", LoadSceneMode.Single);
            yield return null;
            yield return null;

            var game = Object.FindFirstObjectByType<CityLabGame>();
            Assert.IsNotNull(game, "Le bootstrap doit creer CityLabGame.");
            Assert.IsNotNull(game.WorldCamera, "La camera RTS doit exister.");
            Assert.IsNotNull(Object.FindFirstObjectByType<Terrain>(), "Le terrain 512 m doit exister.");
            Assert.IsNotNull(Object.FindFirstObjectByType<CityLabHud>(), "Le HUD UI Toolkit doit exister.");
            var visuals = Resources.Load<CityVisualLibrary>("CityLabVisualLibrary");
            Assert.IsNotNull(visuals, "Le catalogue d'assets admis doit être chargé depuis l'hôte.");
            Assert.IsTrue(visuals.HasDurableSlice, "Le catalogue visuel doit contenir le slice durable.");

            var road = game.Submit(CityCommand.DrawRoad(new Vector3(-30f, 0f, 8f), new Vector3(30f, 0f, 8f)));
            Assert.IsTrue(road.accepted);
            Assert.IsTrue(game.Submit(CityCommand.ZoneResidential(road.createdId)).accepted);
            yield return null;

            var snapshot = game.StateSource.GetSnapshot(1001);
            Assert.Greater(snapshot.parcels.Count, 0);
            Assert.Greater(snapshot.buildings.Count, 0);
            Assert.IsNotNull(Object.FindFirstObjectByType<RoadView>());
        }
    }
}
