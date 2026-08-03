using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Victoria.CityMode.Tests
{
    public sealed class PhysicalConstructionTests
    {
        [Test]
        public void NewSite_PreparesSampledTerrainBeforeRequestingMaterials()
        {
            var simulation = CreateSimulation();
            simulation.SetParcelTerrainSampler(new SlopedTerrainSampler());
            var result = simulation.Submit(CityCommand.PlaceBuilding(
                BuildingArchetype.Granary, new Vector3(20f, 0f, 0f)));
            Assert.IsTrue(result.accepted, result.reason);

            var initial = simulation.GetSnapshot(1001).buildings.Single();
            Assert.IsFalse(initial.terrainPrepared);
            Assert.Greater(initial.terrainCutFillMillimeters, 0);
            Assert.Greater(initial.terrainWorkRemaining, 0f);
            Assert.AreEqual(0, initial.constructionMaterials.Sum(item => item.delivered));

            var observedTerracing = false;
            for (var tick = 0; tick < 700 && !observedTerracing; tick++)
            {
                simulation.Tick(0.1f);
                var building = simulation.GetSnapshot(1001).buildings.Single();
                if (!building.terrainPrepared)
                    Assert.AreEqual(0, building.constructionMaterials.Sum(item => item.delivered));
                else
                    observedTerracing = true;
            }

            Assert.IsTrue(observedTerracing, "Le terrassement ne s'est pas achevé.");
        }

        [Test]
        public void CivicSite_ConsumesPhaseSpecificMaterialsInOrder()
        {
            var simulation = CreateSimulation();
            var result = simulation.Submit(CityCommand.PlaceBuilding(
                BuildingArchetype.Granary, new Vector3(20f, 0f, 0f)));
            Assert.IsTrue(result.accepted, result.reason);

            var expectedPhases = new[]
            {
                BuildingPhase.Foundation,
                BuildingPhase.Framing,
                BuildingPhase.Roofing,
                BuildingPhase.Detailing,
                BuildingPhase.Complete
            };
            var observedPhases = new List<BuildingPhase> { BuildingPhase.Foundation };
            for (var tick = 0; tick < 20000; tick++)
            {
                simulation.Tick(0.1f);
                var phase = simulation.GetSnapshot(1001).buildings.Single().phase;
                if (observedPhases[observedPhases.Count - 1] != phase)
                    observedPhases.Add(phase);
                if (phase == BuildingPhase.Complete)
                    break;
            }

            var completed = simulation.GetSnapshot(1001).buildings.Single();
            CollectionAssert.AreEqual(expectedPhases, observedPhases);
            Assert.IsTrue(completed.terrainPrepared);
            Assert.AreEqual(4, completed.constructionMaterials.Count);
            Assert.IsTrue(completed.constructionMaterials.All(item => item.delivered == item.required));
            CollectionAssert.AreEqual(
                new[] { CityResourceKind.Stone, CityResourceKind.Wood,
                    CityResourceKind.Planks, CityResourceKind.Tools },
                completed.constructionMaterials.Select(item => item.resource));
        }

        [Test]
        public void MidConstruction_SaveReloadRemainsBitExact()
        {
            var original = CreateSimulation();
            Assert.IsTrue(original.Submit(CityCommand.PlaceBuilding(
                BuildingArchetype.Granary, new Vector3(20f, 0f, 0f))).accepted);
            for (var tick = 0; tick < 520; tick++)
                original.Tick(0.1f);

            var beforeReload = original.GetSnapshot(1001);
            var document = CitySaveService.Serialize(beforeReload);
            Assert.IsTrue(CitySaveService.TryDeserialize(document, out var reloaded, out var reason), reason);
            Assert.AreEqual(JsonUtility.ToJson(beforeReload.buildings.Single()),
                JsonUtility.ToJson(reloaded.buildings.Single()));
            var left = new LocalCitySimulation(reloaded);
            var right = new LocalCitySimulation(reloaded);
            for (var tick = 0; tick < 800; tick++)
            {
                left.Tick(0.1f);
                right.Tick(0.1f);
            }

            Assert.AreEqual(JsonUtility.ToJson(left.GetSnapshot(1001)),
                JsonUtility.ToJson(right.GetSnapshot(1001)));
        }

        static LocalCitySimulation CreateSimulation()
        {
            var snapshot = new CitySnapshot
            {
                cityId = 1001,
                seed = 140001,
                stockWood = 200,
                villagers = new List<VillagerState>
                {
                    new VillagerState
                    {
                        id = 1,
                        householdId = 1,
                        position = CityPoint.From(Vector3.zero)
                    }
                }
            };
            var simulation = new LocalCitySimulation(snapshot);
            simulation.AddResource(CityResourceKind.Stone, 100);
            simulation.AddResource(CityResourceKind.Planks, 100);
            simulation.AddResource(CityResourceKind.Tools, 30);
            return simulation;
        }

        sealed class SlopedTerrainSampler : IParcelTerrainSampler
        {
            public float SampleHeight(Vector3 worldPosition) =>
                worldPosition.x * 0.04f + worldPosition.z * 0.015f;
        }
    }
}
