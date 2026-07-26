using System;
using System.Collections.Generic;
using UnityEngine;

namespace Victoria.CityMode
{
    [Serializable]
    public struct CityPoint : IEquatable<CityPoint>
    {
        public float x;
        public float y;
        public float z;

        public CityPoint(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3 ToVector3() => new Vector3(x, y, z);
        public static CityPoint From(Vector3 value) => new CityPoint(value.x, value.y, value.z);
        public bool Equals(CityPoint other) => x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
    }

    public enum CityCommandKind : byte
    {
        DrawRoad = 1,
        ZoneResidential = 2,
        SetConstructionPriority = 3
    }

    [Serializable]
    public sealed class CityCommand
    {
        public CityCommandKind kind;
        public int targetId;
        public int priority;
        public CityPoint start;
        public CityPoint end;

        public static CityCommand DrawRoad(Vector3 start, Vector3 end) => new CityCommand
        {
            kind = CityCommandKind.DrawRoad,
            start = CityPoint.From(start),
            end = CityPoint.From(end)
        };

        public static CityCommand ZoneResidential(int roadId) => new CityCommand
        {
            kind = CityCommandKind.ZoneResidential,
            targetId = roadId
        };

        public static CityCommand SetPriority(int buildingId, int priority) => new CityCommand
        {
            kind = CityCommandKind.SetConstructionPriority,
            targetId = buildingId,
            priority = priority
        };
    }

    [Serializable]
    public sealed class CityCommandResult
    {
        public bool accepted;
        public string reason;
        public int createdId;

        public static CityCommandResult Accept(int createdId = 0) => new CityCommandResult
        {
            accepted = true,
            reason = "accepted",
            createdId = createdId
        };

        public static CityCommandResult Reject(string reason) => new CityCommandResult
        {
            accepted = false,
            reason = reason,
            createdId = 0
        };
    }

    public interface ICityStateSource
    {
        CitySnapshot GetSnapshot(int cityId);
    }

    public interface ICityCommandSink
    {
        CityCommandResult Submit(CityCommand command);
    }

    [Serializable]
    public sealed class CitySnapshot
    {
        public int schemaVersion = 1;
        public int cityId;
        public int seed;
        public float elapsedSeconds;
        public int stockWood;
        public int reservedWood;
        public List<HouseholdState> households = new List<HouseholdState>();
        public List<RoadState> roads = new List<RoadState>();
        public List<ParcelState> parcels = new List<ParcelState>();
        public List<BuildingState> buildings = new List<BuildingState>();
        public List<VillagerState> villagers = new List<VillagerState>();

        public CitySnapshot DeepCopy() => JsonUtility.FromJson<CitySnapshot>(JsonUtility.ToJson(this));
    }

    [Serializable]
    public sealed class HouseholdState
    {
        public int id;
        public int memberCount;
        public int homeBuildingId;
        public int claimedParcelId;
    }

    [Serializable]
    public sealed class RoadState
    {
        public int id;
        public CityPoint start;
        public CityPoint end;
        public bool blocked;
    }

    [Serializable]
    public sealed class ParcelState
    {
        public int id;
        public int roadId;
        public CityPoint center;
        public float width;
        public float depth;
        public bool accessible;
        public int householdId;
        public int buildingId;
    }

    public enum BuildingPhase : byte
    {
        Foundation = 0,
        Framing = 1,
        Complete = 2
    }

    [Serializable]
    public sealed class BuildingState
    {
        public int id;
        public int parcelId;
        public int householdId;
        public CityPoint position;
        public float yaw;
        public int priority;
        public int requiredWood;
        public int deliveredWood;
        public float workRemaining;
        public BuildingPhase phase;
    }

    public enum VillagerActivity : byte
    {
        Idle = 0,
        GoingToStock = 1,
        GoingToSite = 2,
        Building = 3
    }

    [Serializable]
    public sealed class VillagerState
    {
        public int id;
        public int householdId;
        public CityPoint position;
        public CityPoint destination;
        public VillagerActivity activity;
        public int targetBuildingId;
        public int carryingWood;
        public int reservedWood;
    }
}

