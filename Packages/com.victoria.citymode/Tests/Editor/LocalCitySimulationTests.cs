using NUnit.Framework;
using UnityEngine;

namespace Victoria.CityMode.Tests
{
    public sealed class LocalCitySimulationTests
    {
        const string Fixture = @"{
          ""schemaVersion"":1,""cityId"":1001,""seed"":140001,""stockWood"":72,
          ""households"":[
            {""id"":1,""memberCount"":3},{""id"":2,""memberCount"":4},
            {""id"":3,""memberCount"":2},{""id"":4,""memberCount"":5},
            {""id"":5,""memberCount"":3},{""id"":6,""memberCount"":4}],
          ""roads"":[],""parcels"":[],""buildings"":[],
          ""villagers"":[
            {""id"":1,""householdId"":1,""position"":{""x"":-5,""z"":-5}},
            {""id"":2,""householdId"":1,""position"":{""x"":-3,""z"":-6}},
            {""id"":3,""householdId"":2,""position"":{""x"":-1,""z"":-5}},
            {""id"":4,""householdId"":3,""position"":{""x"":1,""z"":-6}},
            {""id"":5,""householdId"":4,""position"":{""x"":3,""z"":-5}},
            {""id"":6,""householdId"":5,""position"":{""x"":5,""z"":-6}},
            {""id"":7,""householdId"":6,""position"":{""x"":7,""z"":-5}},
            {""id"":8,""householdId"":6,""position"":{""x"":9,""z"":-6}}] }";

        [Test]
        public void SameFixtureAndCommands_ProduceSameSnapshot()
        {
            var left = LocalCitySimulation.FromJson(Fixture);
            var right = LocalCitySimulation.FromJson(Fixture);
            var command = CityCommand.DrawRoad(new Vector3(-42, 0, 12), new Vector3(42, 0, 12));
            var leftRoad = left.Submit(command);
            var rightRoad = right.Submit(command);
            left.Submit(CityCommand.ZoneResidential(leftRoad.createdId));
            right.Submit(CityCommand.ZoneResidential(rightRoad.createdId));
            for (var i = 0; i < 500; i++)
            {
                left.Tick(0.1f);
                right.Tick(0.1f);
            }

            Assert.AreEqual(JsonUtility.ToJson(left.GetSnapshot(1001)), JsonUtility.ToJson(right.GetSnapshot(1001)));
        }

        [Test]
        public void InvalidRoad_IsRejectedWithReason()
        {
            var simulation = LocalCitySimulation.FromJson(Fixture);
            var result = simulation.Submit(CityCommand.DrawRoad(Vector3.zero, Vector3.one));
            Assert.IsFalse(result.accepted);
            Assert.AreEqual("road-too-short", result.reason);
            Assert.AreEqual(0, simulation.GetSnapshot(1001).roads.Count);
        }

        [Test]
        public void Construction_ConservesWoodAndCompletesAHome()
        {
            var simulation = LocalCitySimulation.FromJson(Fixture);
            var initialWood = simulation.TotalWoodInSystem();
            var road = simulation.Submit(CityCommand.DrawRoad(new Vector3(-42, 0, 12), new Vector3(42, 0, 12)));
            Assert.IsTrue(road.accepted);
            Assert.IsTrue(simulation.Submit(CityCommand.ZoneResidential(road.createdId)).accepted);

            for (var i = 0; i < 5000; i++)
                simulation.Tick(0.1f);

            var snapshot = simulation.GetSnapshot(1001);
            Assert.AreEqual(initialWood, simulation.TotalWoodInSystem(), "Le bois ne doit etre ni cree ni perdu.");
            Assert.That(snapshot.buildings.FindAll(item => item.phase == BuildingPhase.Complete).Count, Is.GreaterThan(0));
            Assert.That(snapshot.households.FindAll(item => item.homeBuildingId != 0).Count, Is.GreaterThan(0));
            Assert.GreaterOrEqual(snapshot.stockWood, 0);
        }

        [Test]
        public void BlockedRoad_ReleasesReservationsAndReplansWorkers()
        {
            var simulation = LocalCitySimulation.FromJson(Fixture);
            var initialWood = simulation.TotalWoodInSystem();
            var road = simulation.Submit(CityCommand.DrawRoad(new Vector3(-42, 0, 12), new Vector3(42, 0, 12)));
            simulation.Submit(CityCommand.ZoneResidential(road.createdId));
            for (var i = 0; i < 15; i++)
                simulation.Tick(0.1f);

            Assert.IsTrue(simulation.SetRoadBlocked(road.createdId, true));
            var snapshot = simulation.GetSnapshot(1001);
            Assert.AreEqual(0, snapshot.reservedWood);
            Assert.AreEqual(initialWood, simulation.TotalWoodInSystem());
            Assert.That(snapshot.villagers.TrueForAll(item => item.activity == VillagerActivity.Idle), Is.True);
        }

        [Test]
        public void ZoningUnknownOrBlockedRoad_IsRejected()
        {
            var simulation = LocalCitySimulation.FromJson(Fixture);
            Assert.AreEqual("road-unknown", simulation.Submit(CityCommand.ZoneResidential(999)).reason);
            var road = simulation.Submit(CityCommand.DrawRoad(new Vector3(-20, 0, 0), new Vector3(20, 0, 0)));
            simulation.SetRoadBlocked(road.createdId, true);
            Assert.AreEqual("road-inaccessible", simulation.Submit(CityCommand.ZoneResidential(road.createdId)).reason);
        }
    }
}

