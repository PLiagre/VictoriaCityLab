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
        readonly HashSet<int> completedBuildingVisuals = new HashSet<int>();
        LocalCitySimulation simulation;
        CityVisualLibrary visualLibrary;
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
            visualLibrary = Resources.Load<CityVisualLibrary>("CityLabVisualLibrary");
            if (visualLibrary == null || !visualLibrary.HasDurableSlice)
                Debug.LogWarning("CityLab: catalogue visuel absent ou incomplet, utilisation des primitives de secours.");
            CreateMaterials();
            CreateWorld();
            CreateCamera();
            CreateLighting();
            CreateVillageCore();
            CreateEnvironmentDetails();
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
            else if (Array.Exists(Environment.GetCommandLineArgs(), item => item == "-citylabCapture"))
            {
                var road = Submit(CityCommand.DrawRoad(new Vector3(-42f, 0f, 12f), new Vector3(42f, 0f, 12f)));
                if (road.accepted)
                    Submit(CityCommand.ZoneResidential(road.createdId));
                gameObject.AddComponent<CityLabCaptureProbe>();
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
            var centre = InstantiateVisual(visualLibrary != null ? visualLibrary.townCentrePrefab : null,
                "Town Centre", Vector3.zero, Quaternion.identity);
            if (centre == null)
                centre = CreatePrimitive("Town Centre", PrimitiveType.Cube, new Vector3(0f, 1.8f, 0f), new Vector3(10f, 3.6f, 8f), houseMaterial);

            var stock = InstantiateVisual(visualLibrary != null ? visualLibrary.stockpilePrefab : null,
                "Wood Stock", new Vector3(0f, 0f, -12f), Quaternion.identity);
            if (stock == null)
                stock = CreatePrimitive("Wood Stock", PrimitiveType.Cube, new Vector3(0f, 0.8f, -12f), new Vector3(5f, 1.6f, 4f), woodMaterial);
            stock.AddComponent<StockpileMarker>();
        }

        void CreateEnvironmentDetails()
        {
            if (visualLibrary == null || visualLibrary.treePrefabs == null)
                return;
            var random = new System.Random(140001);
            for (var i = 0; i < 72; i++)
            {
                var radius = Mathf.Lerp(85f, 225f, (float)random.NextDouble());
                var angle = (float)random.NextDouble() * Mathf.PI * 2f;
                var position = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                var prefab = visualLibrary.treePrefabs[i % visualLibrary.treePrefabs.Length];
                var tree = InstantiateVisual(prefab, $"Tree {i + 1}", position,
                    Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f));
                if (tree != null)
                    tree.transform.localScale *= Mathf.Lerp(0.85f, 1.2f, (float)random.NextDouble());
            }
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
                    if (visualLibrary != null && visualLibrary.housePrefabs != null && visualLibrary.housePrefabs.Length > 0)
                    {
                        renderer.enabled = false;
                        if (completedBuildingVisuals.Add(building.id))
                        {
                            var prefab = visualLibrary.housePrefabs[Mathf.Abs(building.id) % visualLibrary.housePrefabs.Length];
                            var detailed = Instantiate(prefab, view.transform);
                            detailed.name = "Completed House Visual";
                            detailed.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                        }
                    }
                    else
                    {
                        view.transform.localScale = new Vector3(7f, 5.5f, 9f);
                        view.transform.position = building.position.ToVector3() + Vector3.up * 2.75f;
                        renderer.sharedMaterial = houseMaterial;
                    }
                    break;
            }
        }

        void SyncVillager(VillagerState villager)
        {
            if (!villagerViews.TryGetValue(villager.id, out var view))
            {
                var position = villager.position.ToVector3();
                view = InstantiateVisual(visualLibrary != null ? visualLibrary.villagerPrefab : null,
                    $"Villager {villager.id}", position, Quaternion.identity);
                if (view == null)
                {
                    view = CreatePrimitive($"Villager {villager.id}", PrimitiveType.Capsule,
                        position + Vector3.up, new Vector3(0.7f, 1f, 0.7f), villagerMaterial);
                    var collider = view.GetComponent<Collider>();
                    if (collider != null) collider.enabled = false;
                }
                else
                {
                    var visual = view.AddComponent<VillagerVisual>();
                    visual.Initialize(visualLibrary.villagerAnimatorController);
                }
                villagerViews.Add(villager.id, view);
            }
            var isDetailed = view.GetComponent<VillagerVisual>() != null;
            var target = villager.position.ToVector3() + (isDetailed ? Vector3.zero : Vector3.up);
            view.transform.position = Vector3.Lerp(view.transform.position, target, 1f - Mathf.Exp(-15f * Time.deltaTime));
            if (isDetailed)
                view.GetComponent<VillagerVisual>().Refresh(villager.activity, villager.carryingWood);
            else
                view.transform.localScale = villager.carryingWood > 0 ? new Vector3(0.8f, 1f, 0.8f) : new Vector3(0.7f, 1f, 0.7f);
        }

        static GameObject InstantiateVisual(GameObject prefab, string label, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
                return null;
            var result = Instantiate(prefab, position, rotation);
            result.name = label;
            return result;
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

    public sealed class CityLabCaptureProbe : MonoBehaviour
    {
        int frames;
        int captureFrame;

        void Awake()
        {
            Time.timeScale = 12f;
            var controller = FindFirstObjectByType<RtsCameraController>();
            if (controller != null)
            {
                controller.enabled = false;
                var rotation = Quaternion.Euler(48f, 35f, 0f);
                controller.transform.SetPositionAndRotation(
                    Vector3.zero - rotation * Vector3.forward * 85f, rotation);
            }
        }

        void Update()
        {
            frames++;
            if (frames == 480)
            {
                var path = System.IO.Path.GetFullPath("Logs/citylab-vendor.png");
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                ScreenCapture.CaptureScreenshot(path, 1);
                Debug.Log("CITYLAB_CAPTURE_WRITTEN path=" + path);
                captureFrame = frames;
            }
            if (captureFrame > 0 && frames > captureFrame + 45)
            {
                Time.timeScale = 1f;
                Application.Quit(0);
                enabled = false;
            }
        }
    }
}
