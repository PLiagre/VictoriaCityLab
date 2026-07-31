using System;
using System.Collections.Generic;
using UnityEngine;

namespace Victoria.CityMode
{
    /// <summary>
    /// Simulation locale volontairement petite. Elle prouve la boucle de jeu,
    /// mais reste derriere les contrats afin de pouvoir etre remplacee par le
    /// moteur Victoria sans exposer de GameObject comme verite metier.
    /// </summary>
    public sealed class LocalCitySimulation : ICityStateSource, ICityCommandSink
    {
        public const int MaxHouseholds = 20;
        public const int MaxBuildings = 30;
        public const int WoodPerHouse = 6;
        public const float WorkPerHouse = 12f;
        public const float MapHalfExtent = 256f;
        public const int LumberCampCost = 8;
        public const int LumberCampMaxWorkers = 2;
        public const int LumberCampInitialTimber = 24;
        public const float LumberCampWorkPerWood = 5f;
        public const float LumberCampMinDistanceFromCentre = 35f;
        public const float LumberCampMaxDistanceFromCentre = 190f;
        public const float LumberCampMinSpacing = 28f;

        static readonly Vector3 StockPosition = new Vector3(0f, 0f, -12f);
        readonly CitySnapshot state;
        int nextRoadId = 1;
        int nextParcelId = 1;
        int nextBuildingId = 1;
        int nextProductionSiteId = 1;

        public LocalCitySimulation(CitySnapshot initial)
        {
            state = initial != null ? initial.DeepCopy() : throw new ArgumentNullException(nameof(initial));
            state.households ??= new List<HouseholdState>();
            state.roads ??= new List<RoadState>();
            state.parcels ??= new List<ParcelState>();
            state.buildings ??= new List<BuildingState>();
            state.villagers ??= new List<VillagerState>();
            state.productionSites ??= new List<ProductionSiteState>();
            nextRoadId = NextId(state.roads, item => item.id);
            nextParcelId = NextId(state.parcels, item => item.id);
            nextBuildingId = NextId(state.buildings, item => item.id);
            nextProductionSiteId = NextId(state.productionSites, item => item.id);
        }

        public static LocalCitySimulation FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("Fixture JSON vide.", nameof(json));
            var snapshot = JsonUtility.FromJson<CitySnapshot>(json);
            if (snapshot == null || snapshot.cityId <= 0)
                throw new ArgumentException("Fixture CitySnapshot invalide.", nameof(json));
            return new LocalCitySimulation(snapshot);
        }

        public CitySnapshot GetSnapshot(int cityId)
        {
            if (cityId != state.cityId)
                throw new KeyNotFoundException($"Ville inconnue: {cityId}");
            return state.DeepCopy();
        }

        public CityCommandResult Submit(CityCommand command)
        {
            if (command == null)
                return CityCommandResult.Reject("command-null");
            return command.kind switch
            {
                CityCommandKind.DrawRoad => DrawRoad(command),
                CityCommandKind.ZoneResidential => ZoneResidential(command.targetId),
                CityCommandKind.SetConstructionPriority => SetPriority(command.targetId, command.priority),
                CityCommandKind.PlaceLumberCamp => PlaceLumberCamp(command.position),
                _ => CityCommandResult.Reject("command-unknown")
            };
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
                return;
            var step = Mathf.Min(deltaTime, 0.1f);
            state.elapsedSeconds += step;

            TickProductionSites(step);

            state.villagers.Sort((a, b) => a.id.CompareTo(b.id));
            for (var i = 0; i < state.villagers.Count; i++)
            {
                var villager = state.villagers[i];
                switch (villager.activity)
                {
                    case VillagerActivity.Idle:
                        AssignWork(villager);
                        break;
                    case VillagerActivity.GoingToStock:
                        AdvanceToStock(villager, step);
                        break;
                    case VillagerActivity.GoingToSite:
                        AdvanceToSite(villager, step);
                        break;
                    case VillagerActivity.Building:
                        Build(villager, step);
                        break;
                }
            }
        }

        public bool SetRoadBlocked(int roadId, bool blocked)
        {
            var road = FindRoad(roadId);
            if (road == null)
                return false;
            road.blocked = blocked;
            foreach (var parcel in state.parcels)
                if (parcel.roadId == roadId)
                    parcel.accessible = !blocked;
            if (blocked)
                ReplanInaccessibleWorkers();
            return true;
        }

        public int TotalWoodInSystem()
        {
            var total = state.stockWood;
            foreach (var villager in state.villagers)
                total += villager.carryingWood;
            foreach (var building in state.buildings)
                total += building.deliveredWood;
            return total;
        }

        CityCommandResult DrawRoad(CityCommand command)
        {
            var start = command.start.ToVector3();
            var end = command.end.ToVector3();
            start.y = 0f;
            end.y = 0f;
            if (!InsideMap(start) || !InsideMap(end))
                return CityCommandResult.Reject("road-outside-map");
            var length = Vector3.Distance(start, end);
            if (length < 4f)
                return CityCommandResult.Reject("road-too-short");
            if (length > 150f)
                return CityCommandResult.Reject("road-too-long");

            var road = new RoadState
            {
                id = nextRoadId++,
                start = CityPoint.From(start),
                end = CityPoint.From(end),
                blocked = false
            };
            state.roads.Add(road);
            return CityCommandResult.Accept(road.id);
        }

        CityCommandResult ZoneResidential(int roadId)
        {
            var road = FindRoad(roadId);
            if (road == null)
                return CityCommandResult.Reject("road-unknown");
            if (road.blocked)
                return CityCommandResult.Reject("road-inaccessible");
            foreach (var existing in state.parcels)
                if (existing.roadId == roadId)
                    return CityCommandResult.Reject("road-already-zoned");

            var start = road.start.ToVector3();
            var end = road.end.ToVector3();
            var direction = (end - start).normalized;
            var normal = new Vector3(-direction.z, 0f, direction.x);
            var length = Vector3.Distance(start, end);
            var lotCountPerSide = Mathf.Clamp(Mathf.FloorToInt(length / 12f), 1, 8);
            var created = 0;

            for (var side = -1; side <= 1; side += 2)
            {
                for (var lot = 0; lot < lotCountPerSide && state.parcels.Count < MaxBuildings; lot++)
                {
                    var t = (lot + 0.5f) / lotCountPerSide;
                    var center = Vector3.Lerp(start, end, t) + normal * (side * 9f);
                    if (!InsideMap(center))
                        continue;
                    state.parcels.Add(new ParcelState
                    {
                        id = nextParcelId++,
                        roadId = roadId,
                        center = CityPoint.From(center),
                        width = 10f,
                        depth = 14f,
                        accessible = true
                    });
                    created++;
                }
            }

            if (created == 0)
                return CityCommandResult.Reject("no-valid-parcel");
            ClaimParcelsAndCreateSites();
            return CityCommandResult.Accept(roadId);
        }

        CityCommandResult SetPriority(int buildingId, int priority)
        {
            var building = FindBuilding(buildingId);
            if (building == null)
                return CityCommandResult.Reject("building-unknown");
            building.priority = Mathf.Clamp(priority, 0, 3);
            return CityCommandResult.Accept(buildingId);
        }

        CityCommandResult PlaceLumberCamp(CityPoint requestedPosition)
        {
            var position = requestedPosition.ToVector3();
            position.y = 0f;
            if (!InsideMap(position))
                return CityCommandResult.Reject("lumber-camp-outside-map");

            var distanceFromCentre = PlanarDistance(position, Vector3.zero);
            if (distanceFromCentre < LumberCampMinDistanceFromCentre)
                return CityCommandResult.Reject("lumber-camp-too-close-to-centre");
            if (distanceFromCentre > LumberCampMaxDistanceFromCentre)
                return CityCommandResult.Reject("lumber-camp-too-far-from-centre");

            foreach (var existing in state.productionSites)
            {
                if (existing.kind == ProductionSiteKind.LumberCamp &&
                    PlanarDistance(position, existing.position.ToVector3()) < LumberCampMinSpacing)
                    return CityCommandResult.Reject("lumber-camp-too-close-to-another-camp");
            }

            if (state.stockWood - state.reservedWood < LumberCampCost)
                return CityCommandResult.Reject("lumber-camp-insufficient-wood");

            var camp = new ProductionSiteState
            {
                id = nextProductionSiteId++,
                kind = ProductionSiteKind.LumberCamp,
                position = CityPoint.From(position),
                assignedWorkers = 0,
                maxWorkers = LumberCampMaxWorkers,
                productionProgress = 0f,
                remainingTimber = LumberCampInitialTimber
            };
            state.stockWood -= LumberCampCost;
            state.productionSites.Add(camp);
            RefreshProductionAssignments();
            return CityCommandResult.Accept(camp.id);
        }

        void TickProductionSites(float deltaTime)
        {
            if (state.productionSites.Count == 0)
                return;

            RefreshProductionAssignments();
            foreach (var site in state.productionSites)
            {
                if (site.kind != ProductionSiteKind.LumberCamp || site.assignedWorkers <= 0 ||
                    site.remainingTimber <= 0)
                    continue;

                site.productionProgress += deltaTime * site.assignedWorkers;
                while (site.productionProgress + 0.000001f >= LumberCampWorkPerWood &&
                       site.remainingTimber > 0)
                {
                    site.productionProgress = Mathf.Max(0f, site.productionProgress - LumberCampWorkPerWood);
                    site.remainingTimber -= 1;
                    state.stockWood += 1;
                }

                if (site.remainingTimber == 0)
                {
                    site.productionProgress = 0f;
                    site.assignedWorkers = 0;
                }
            }
            RefreshProductionAssignments();
        }

        void RefreshProductionAssignments()
        {
            state.productionSites.Sort((left, right) => left.id.CompareTo(right.id));
            var availableWorkers = state.villagers.Count;
            foreach (var site in state.productionSites)
            {
                if (site.kind != ProductionSiteKind.LumberCamp || site.remainingTimber <= 0)
                {
                    site.assignedWorkers = 0;
                    continue;
                }

                var capacity = Mathf.Clamp(site.maxWorkers, 0, LumberCampMaxWorkers);
                site.assignedWorkers = Mathf.Min(capacity, availableWorkers);
                availableWorkers -= site.assignedWorkers;
            }
        }

        void ClaimParcelsAndCreateSites()
        {
            state.households.Sort((a, b) => a.id.CompareTo(b.id));
            state.parcels.Sort((a, b) => a.id.CompareTo(b.id));
            foreach (var household in state.households)
            {
                if (household.homeBuildingId != 0 || household.claimedParcelId != 0)
                    continue;
                ParcelState parcel = null;
                foreach (var candidate in state.parcels)
                {
                    if (candidate.accessible && candidate.householdId == 0 && candidate.buildingId == 0)
                    {
                        parcel = candidate;
                        break;
                    }
                }
                if (parcel == null || state.buildings.Count >= MaxBuildings)
                    break;

                var building = new BuildingState
                {
                    id = nextBuildingId++,
                    parcelId = parcel.id,
                    householdId = household.id,
                    position = parcel.center,
                    yaw = ParcelFacingYaw(parcel),
                    priority = 1,
                    requiredWood = WoodPerHouse,
                    deliveredWood = 0,
                    workRemaining = WorkPerHouse,
                    phase = BuildingPhase.Foundation
                };
                household.claimedParcelId = parcel.id;
                parcel.householdId = household.id;
                parcel.buildingId = building.id;
                state.buildings.Add(building);
            }
        }

        void AssignWork(VillagerState villager)
        {
            BuildingState chosen = null;
            var chosenInTransit = 0;
            foreach (var building in state.buildings)
            {
                if (building.phase == BuildingPhase.Complete || !IsBuildingAccessible(building))
                    continue;
                var inTransit = WoodInTransit(building.id);
                if (building.deliveredWood + inTransit >= building.requiredWood)
                {
                    if (building.deliveredWood >= building.requiredWood &&
                        (chosen == null || building.priority > chosen.priority ||
                         (building.priority == chosen.priority && building.id < chosen.id)))
                    {
                        chosen = building;
                        chosenInTransit = inTransit;
                    }
                    continue;
                }
                if (state.stockWood - state.reservedWood <= 0)
                    continue;
                if (chosen == null || building.priority > chosen.priority ||
                    (building.priority == chosen.priority && building.id < chosen.id))
                {
                    chosen = building;
                    chosenInTransit = inTransit;
                }
            }
            if (chosen == null)
                return;

            villager.targetBuildingId = chosen.id;
            if (chosen.deliveredWood + chosenInTransit < chosen.requiredWood)
            {
                villager.reservedWood = 1;
                state.reservedWood += 1;
                villager.destination = CityPoint.From(StockPosition);
                villager.activity = VillagerActivity.GoingToStock;
            }
            else
            {
                villager.destination = chosen.position;
                villager.activity = VillagerActivity.GoingToSite;
            }
        }

        void AdvanceToStock(VillagerState villager, float deltaTime)
        {
            if (state.stockWood <= 0 || villager.reservedWood <= 0)
            {
                ReleaseReservation(villager);
                SetIdle(villager);
                return;
            }
            if (!MoveTowards(villager, StockPosition, deltaTime))
                return;
            state.stockWood -= 1;
            state.reservedWood -= 1;
            villager.reservedWood = 0;
            villager.carryingWood = 1;
            var building = FindBuilding(villager.targetBuildingId);
            if (building == null || !IsBuildingAccessible(building))
            {
                state.stockWood += villager.carryingWood;
                villager.carryingWood = 0;
                SetIdle(villager);
                return;
            }
            villager.destination = building.position;
            villager.activity = VillagerActivity.GoingToSite;
        }

        void AdvanceToSite(VillagerState villager, float deltaTime)
        {
            var building = FindBuilding(villager.targetBuildingId);
            if (building == null || !IsBuildingAccessible(building))
            {
                if (villager.carryingWood > 0)
                {
                    state.stockWood += villager.carryingWood;
                    villager.carryingWood = 0;
                }
                SetIdle(villager);
                return;
            }
            if (!MoveTowards(villager, building.position.ToVector3(), deltaTime))
                return;
            if (villager.carryingWood > 0)
            {
                building.deliveredWood += villager.carryingWood;
                villager.carryingWood = 0;
                building.phase = building.deliveredWood >= building.requiredWood / 2
                    ? BuildingPhase.Framing
                    : BuildingPhase.Foundation;
            }
            villager.activity = VillagerActivity.Building;
        }

        void Build(VillagerState villager, float deltaTime)
        {
            var building = FindBuilding(villager.targetBuildingId);
            if (building == null || !IsBuildingAccessible(building))
            {
                SetIdle(villager);
                return;
            }
            if (building.deliveredWood < building.requiredWood)
            {
                SetIdle(villager);
                return;
            }
            building.workRemaining = Mathf.Max(0f, building.workRemaining - deltaTime);
            if (building.workRemaining > 0f)
                return;
            building.phase = BuildingPhase.Complete;
            var household = FindHousehold(building.householdId);
            if (household != null)
                household.homeBuildingId = building.id;
            SetIdle(villager);
        }

        void ReplanInaccessibleWorkers()
        {
            foreach (var villager in state.villagers)
            {
                var building = FindBuilding(villager.targetBuildingId);
                if (building == null || IsBuildingAccessible(building))
                    continue;
                ReleaseReservation(villager);
                if (villager.carryingWood > 0)
                {
                    state.stockWood += villager.carryingWood;
                    villager.carryingWood = 0;
                }
                SetIdle(villager);
            }
        }

        bool MoveTowards(VillagerState villager, Vector3 target, float deltaTime)
        {
            var position = villager.position.ToVector3();
            var next = Vector3.MoveTowards(position, target, 3.5f * deltaTime);
            villager.position = CityPoint.From(next);
            return Vector3.SqrMagnitude(next - target) <= 0.01f;
        }

        void ReleaseReservation(VillagerState villager)
        {
            if (villager.reservedWood <= 0)
                return;
            state.reservedWood = Mathf.Max(0, state.reservedWood - villager.reservedWood);
            villager.reservedWood = 0;
        }

        static void SetIdle(VillagerState villager)
        {
            villager.activity = VillagerActivity.Idle;
            villager.targetBuildingId = 0;
            villager.destination = villager.position;
        }

        int WoodInTransit(int buildingId)
        {
            var count = 0;
            foreach (var villager in state.villagers)
                if (villager.targetBuildingId == buildingId)
                    count += villager.carryingWood + villager.reservedWood;
            return count;
        }

        bool IsBuildingAccessible(BuildingState building)
        {
            var parcel = FindParcel(building.parcelId);
            return parcel != null && parcel.accessible;
        }

        float ParcelFacingYaw(ParcelState parcel)
        {
            var road = FindRoad(parcel.roadId);
            if (road == null)
                return 0f;
            var start = road.start.ToVector3();
            var segment = road.end.ToVector3() - start;
            var lengthSquared = segment.sqrMagnitude;
            if (lengthSquared <= 0.0001f)
                return 0f;
            var center = parcel.center.ToVector3();
            var t = Mathf.Clamp01(Vector3.Dot(center - start, segment) / lengthSquared);
            var facing = (start + segment * t - center).normalized;
            return Mathf.Atan2(facing.x, facing.z) * Mathf.Rad2Deg;
        }

        RoadState FindRoad(int id) => state.roads.Find(item => item.id == id);
        ParcelState FindParcel(int id) => state.parcels.Find(item => item.id == id);
        BuildingState FindBuilding(int id) => state.buildings.Find(item => item.id == id);
        HouseholdState FindHousehold(int id) => state.households.Find(item => item.id == id);

        static float PlanarDistance(Vector3 left, Vector3 right)
        {
            var deltaX = left.x - right.x;
            var deltaZ = left.z - right.z;
            return Mathf.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
        }

        static bool InsideMap(Vector3 point) =>
            Mathf.Abs(point.x) <= MapHalfExtent - 4f && Mathf.Abs(point.z) <= MapHalfExtent - 4f;

        static int NextId<T>(List<T> items, Func<T, int> selector)
        {
            var max = 0;
            foreach (var item in items)
                max = Mathf.Max(max, selector(item));
            return max + 1;
        }
    }
}
