using System.Collections.Generic;
using System;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Victoria.CityMode
{
    public sealed class CityLabGame : MonoBehaviour
    {
        const int CityId = 1001;
        readonly Dictionary<int, GameObject> roadViews = new Dictionary<int, GameObject>();
        readonly Dictionary<int, GameObject> parcelViews = new Dictionary<int, GameObject>();
        readonly Dictionary<int, GameObject> buildingViews = new Dictionary<int, GameObject>();
        readonly Dictionary<int, GameObject> villagerViews = new Dictionary<int, GameObject>();
        LocalCitySimulation simulation;
        Camera worldCamera;
        DirectionalLightCycle lightCycle;
        CityBuildController buildController;
        CityLabHud hud;
        Material roadMaterial;
        Material parcelMaterial;
        Material foundationMaterial;
        Material framingMaterial;
        Material houseMaterial;
        Material villagerMaterial;
        Material woodMaterial;
        Material baseMaterial;
        CityLabPerformanceProbe performanceProbe;

        public ICityStateSource StateSource => simulation;
        public ICityCommandSink CommandSink => simulation;
        public Camera WorldCamera => worldCamera;

        void Awake()
        {
            var fixture = Resources.Load<TextAsset>("city_fixture_1001");
            if (fixture == null)
            {
                Debug.LogError("CityLab: Resources/city_fixture_1001.json introuvable.");
                enabled = false;
                return;
            }
            simulation = LocalCitySimulation.FromJson(fixture.text);
            CreateMaterials();
            CreateWorld();
            CreateCamera();
            CreateLighting();
            CreateVillageCore();
            buildController = gameObject.AddComponent<CityBuildController>();
            buildController.Initialize(this);
            hud = gameObject.AddComponent<CityLabHud>();
            hud.Initialize(this, buildController);
            SyncViews(simulation.GetSnapshot(CityId));
            if (Array.Exists(Environment.GetCommandLineArgs(), item => item == "-citylabSmoke"))
            {
                var road = Submit(CityCommand.DrawRoad(new Vector3(-42f, 0f, 12f), new Vector3(42f, 0f, 12f)));
                if (road.accepted)
                    Submit(CityCommand.ZoneResidential(road.createdId));
                performanceProbe = gameObject.AddComponent<CityLabPerformanceProbe>();
            }
        }

        void Update()
        {
            if (simulation == null)
                return;
            simulation.Tick(Time.deltaTime);
            var snapshot = simulation.GetSnapshot(CityId);
            SyncViews(snapshot);
            hud?.Refresh(snapshot);
        }

        public CityCommandResult Submit(CityCommand command)
        {
            var result = simulation.Submit(command);
            if (result.accepted)
                SyncViews(simulation.GetSnapshot(CityId));
            hud?.ShowMessage(result.accepted ? "Ordre accepte" : $"Refus: {result.reason}");
            return result;
        }

        void CreateMaterials()
        {
            baseMaterial = Resources.Load<Material>("CityLabBaseMaterial");
            if (baseMaterial == null)
                throw new System.InvalidOperationException("CityLabBaseMaterial is missing from Resources.");
            roadMaterial = MakeMaterial("Road", new Color(0.16f, 0.12f, 0.09f));
            parcelMaterial = MakeMaterial("Parcel", new Color(0.16f, 0.36f, 0.18f, 0.42f), true);
            foundationMaterial = MakeMaterial("Foundation", new Color(0.28f, 0.25f, 0.22f));
            framingMaterial = MakeMaterial("Framing", new Color(0.36f, 0.20f, 0.09f));
            houseMaterial = MakeMaterial("House", new Color(0.24f, 0.09f, 0.075f));
            villagerMaterial = MakeMaterial("Villager", new Color(0.18f, 0.23f, 0.34f));
            woodMaterial = MakeMaterial("Wood", new Color(0.30f, 0.16f, 0.06f));
        }

        Material MakeMaterial(string label, Color color, bool transparent = false)
        {
            var material = new Material(baseMaterial) { name = $"Runtime {label}", color = color };
            if (transparent)
            {
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 0f);
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0f);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = 3000;
            }
            return material;
        }

        void CreateWorld()
        {
            var terrainData = new TerrainData
            {
                heightmapResolution = 129,
                size = new Vector3(512f, 18f, 512f)
            };
            var heights = new float[129, 129];
            for (var z = 0; z < 129; z++)
            for (var x = 0; x < 129; x++)
            {
                var nx = (x - 64f) / 64f;
                var nz = (z - 64f) / 64f;
                var rim = Mathf.Max(0f, (nx * nx + nz * nz - 0.18f) * 0.018f);
                heights[z, x] = 0.006f + rim + Mathf.PerlinNoise(x * 0.035f, z * 0.035f) * 0.012f;
            }
            terrainData.SetHeights(0, 0, heights);
            var terrainObject = Terrain.CreateTerrainGameObject(terrainData);
            terrainObject.name = "European Terrain 512m";
            terrainObject.transform.position = new Vector3(-256f, -terrainData.GetInterpolatedHeight(0.5f, 0.5f), -256f);
            var terrain = terrainObject.GetComponent<Terrain>();
            terrain.drawInstanced = true;
            terrain.materialTemplate = MakeMaterial("Terrain", new Color(0.18f, 0.27f, 0.12f));

            var surface = terrainObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.BuildNavMesh();
        }

        void CreateCamera()
        {
            var cameraObject = new GameObject("RTS Camera");
            worldCamera = cameraObject.AddComponent<Camera>();
            worldCamera.fieldOfView = 50f;
            worldCamera.nearClipPlane = 0.2f;
            worldCamera.farClipPlane = 1000f;
            worldCamera.clearFlags = CameraClearFlags.Skybox;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<RtsCameraController>();
        }

        void CreateLighting()
        {
            var lightObject = new GameObject("Sun");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.shadows = LightShadows.Soft;
            lightCycle = lightObject.AddComponent<DirectionalLightCycle>();
        }

        void CreateVillageCore()
        {
            CreatePrimitive("Town Centre", PrimitiveType.Cube, new Vector3(0f, 1.8f, 0f), new Vector3(10f, 3.6f, 8f), houseMaterial);
            var stock = CreatePrimitive("Wood Stock", PrimitiveType.Cube, new Vector3(0f, 0.8f, -12f), new Vector3(5f, 1.6f, 4f), woodMaterial);
            stock.AddComponent<StockpileMarker>();
        }

        void SyncViews(CitySnapshot snapshot)
        {
            foreach (var road in snapshot.roads)
                SyncRoad(road);
            foreach (var parcel in snapshot.parcels)
                SyncParcel(parcel);
            foreach (var building in snapshot.buildings)
                SyncBuilding(building);
            foreach (var villager in snapshot.villagers)
                SyncVillager(villager);
        }

        void SyncRoad(RoadState road)
        {
            if (roadViews.ContainsKey(road.id))
                return;
            var start = road.start.ToVector3();
            var end = road.end.ToVector3();
            var center = (start + end) * 0.5f + Vector3.up * 0.15f;
            var length = Vector3.Distance(start, end);
            var view = CreatePrimitive($"Road {road.id}", PrimitiveType.Cube, center, new Vector3(4f, 0.3f, length), roadMaterial);
            view.transform.rotation = Quaternion.LookRotation((end - start).normalized, Vector3.up);
            view.AddComponent<RoadView>().RoadId = road.id;
            roadViews.Add(road.id, view);
        }

        void SyncParcel(ParcelState parcel)
        {
            if (!parcelViews.TryGetValue(parcel.id, out var view))
            {
                view = CreatePrimitive($"Parcel {parcel.id}", PrimitiveType.Cube,
                    parcel.center.ToVector3() + Vector3.up * 0.06f,
                    new Vector3(parcel.width, 0.12f, parcel.depth), parcelMaterial);
                var collider = view.GetComponent<Collider>();
                if (collider != null) collider.enabled = false;
                parcelViews.Add(parcel.id, view);
            }
            view.SetActive(parcel.buildingId == 0);
        }

        void SyncBuilding(BuildingState building)
        {
            if (!buildingViews.TryGetValue(building.id, out var view))
            {
                view = CreatePrimitive($"House {building.id}", PrimitiveType.Cube,
                    building.position.ToVector3() + Vector3.up * 0.35f,
                    new Vector3(7f, 0.7f, 9f), foundationMaterial);
                view.transform.rotation = Quaternion.Euler(0f, building.yaw, 0f);
                buildingViews.Add(building.id, view);
            }

            var renderer = view.GetComponent<Renderer>();
            switch (building.phase)
            {
                case BuildingPhase.Foundation:
                    view.transform.localScale = new Vector3(7f, 0.7f, 9f);
                    renderer.sharedMaterial = foundationMaterial;
                    break;
                case BuildingPhase.Framing:
                    view.transform.localScale = new Vector3(7f, 3.5f, 9f);
                    view.transform.position = building.position.ToVector3() + Vector3.up * 1.75f;
                    renderer.sharedMaterial = framingMaterial;
                    break;
                case BuildingPhase.Complete:
                    view.transform.localScale = new Vector3(7f, 5.5f, 9f);
                    view.transform.position = building.position.ToVector3() + Vector3.up * 2.75f;
                    renderer.sharedMaterial = houseMaterial;
                    break;
            }
        }

        void SyncVillager(VillagerState villager)
        {
            if (!villagerViews.TryGetValue(villager.id, out var view))
            {
                view = CreatePrimitive($"Villager {villager.id}", PrimitiveType.Capsule,
                    villager.position.ToVector3() + Vector3.up, new Vector3(0.7f, 1f, 0.7f), villagerMaterial);
                var collider = view.GetComponent<Collider>();
                if (collider != null) collider.enabled = false;
                villagerViews.Add(villager.id, view);
            }
            var target = villager.position.ToVector3() + Vector3.up;
            view.transform.position = Vector3.Lerp(view.transform.position, target, 1f - Mathf.Exp(-15f * Time.deltaTime));
            view.transform.localScale = villager.carryingWood > 0 ? new Vector3(0.8f, 1f, 0.8f) : new Vector3(0.7f, 1f, 0.7f);
        }

        static GameObject CreatePrimitive(string label, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
        {
            var result = GameObject.CreatePrimitive(type);
            result.name = label;
            result.transform.position = position;
            result.transform.localScale = scale;
            result.GetComponent<Renderer>().sharedMaterial = material;
            return result;
        }
    }

    public sealed class RoadView : MonoBehaviour
    {
        public int RoadId { get; set; }
    }

    public sealed class StockpileMarker : MonoBehaviour { }

    public sealed class DirectionalLightCycle : MonoBehaviour
    {
        [Range(0f, 1f)] public float normalizedTime = 0.32f;
        public float cycleSeconds = 240f;

        void Update()
        {
            normalizedTime = Mathf.Repeat(normalizedTime + Time.deltaTime / cycleSeconds, 1f);
            transform.rotation = Quaternion.Euler(normalizedTime * 360f - 90f, 25f, 0f);
            var light = GetComponent<Light>();
            light.intensity = Mathf.Lerp(0.08f, 1.3f, Mathf.Clamp01(Mathf.Sin(normalizedTime * Mathf.PI)));
        }
    }

    public sealed class CityLabPerformanceProbe : MonoBehaviour
    {
        const int WarmupFrames = 120;
        const int SampleFrames = 600;
        readonly List<float> samples = new List<float>(SampleFrames);
        int frames;

        void Update()
        {
            frames++;
            if (frames <= WarmupFrames)
                return;
            samples.Add(Time.unscaledDeltaTime * 1000f);
            if (samples.Count < SampleFrames)
                return;

            samples.Sort();
            var sum = 0f;
            foreach (var sample in samples) sum += sample;
            var average = sum / samples.Count;
            var p95 = samples[Mathf.Clamp(Mathf.CeilToInt(samples.Count * 0.95f) - 1, 0, samples.Count - 1)];
            Debug.Log($"CITYLAB_PERF_OK frames={samples.Count} avg_ms={average:F3} p95_ms={p95:F3} avg_fps={1000f / average:F1}");
            Application.Quit(0);
            enabled = false;
        }
    }
}
