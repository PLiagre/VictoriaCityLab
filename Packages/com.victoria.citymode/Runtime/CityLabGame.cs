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
        const float ViewRefreshInterval = 0.1f;
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
        Terrain worldTerrain;
        CityLabPerformanceProbe performanceProbe;
        float viewRefreshTimer;

        public ICityStateSource StateSource => simulation;
        public ICityCommandSink CommandSink => simulation;
        public Camera WorldCamera => worldCamera;

        void Awake()
        {
            var arguments = Environment.GetCommandLineArgs();
            var isSmoke = Array.Exists(arguments, item => item == "-citylabSmoke");
            var isCapture = Array.Exists(arguments, item => item == "-citylabCapture");
            var fixture = Resources.Load<TextAsset>(isSmoke ? "city_fixture_performance_1001" : "city_fixture_1001");
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
            RefreshSnapshotViews();
            if (isSmoke)
            {
                var road = Submit(CityCommand.DrawRoad(new Vector3(-42f, 0f, 12f), new Vector3(42f, 0f, 12f)));
                if (road.accepted)
                    Submit(CityCommand.ZoneResidential(road.createdId));
                var secondRoad = Submit(CityCommand.DrawRoad(new Vector3(-42f, 0f, 40f), new Vector3(42f, 0f, 40f)));
                if (secondRoad.accepted)
                    Submit(CityCommand.ZoneResidential(secondRoad.createdId));
                performanceProbe = gameObject.AddComponent<CityLabPerformanceProbe>();
                performanceProbe.Initialize(this);
            }
            else if (isCapture)
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
            viewRefreshTimer -= Time.deltaTime;
            if (viewRefreshTimer <= 0f)
                RefreshSnapshotViews();
        }

        void RefreshSnapshotViews()
        {
            viewRefreshTimer = ViewRefreshInterval;
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

        public void SetSelectedBuilding(int buildingId)
        {
            foreach (var pair in buildingViews)
            {
                var selectable = pair.Value.GetComponent<BuildingView>();
                if (selectable != null)
                    selectable.SetSelected(pair.Key == buildingId);
            }
        }

        void CreateMaterials()
        {
            baseMaterial = Resources.Load<Material>("CityLabBaseMaterial");
            if (baseMaterial == null)
                throw new System.InvalidOperationException("CityLabBaseMaterial is missing from Resources.");
            roadMaterial = MakeMaterial("Road", new Color(0.30f, 0.22f, 0.14f));
            parcelMaterial = MakeMaterial("Parcel", new Color(0.34f, 0.57f, 0.25f, 0.28f), true);
            foundationMaterial = MakeMaterial("Foundation", new Color(0.39f, 0.37f, 0.32f));
            framingMaterial = MakeMaterial("Framing", new Color(0.48f, 0.29f, 0.12f));
            houseMaterial = MakeMaterial("House", new Color(0.46f, 0.22f, 0.14f));
            villagerMaterial = MakeMaterial("Villager", new Color(0.20f, 0.29f, 0.43f));
            woodMaterial = MakeMaterial("Wood", new Color(0.43f, 0.24f, 0.09f));
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
            worldTerrain = terrainObject.GetComponent<Terrain>();
            worldTerrain.drawInstanced = true;
            var terrainMaterial = Resources.Load<Material>("CityLabTerrainMaterial");
            if (terrainMaterial != null)
                worldTerrain.materialTemplate = terrainMaterial;
            ConfigureTerrainLayers(terrainData);

            var surface = terrainObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.BuildNavMesh();
        }

        static void ConfigureTerrainLayers(TerrainData data)
        {
            var meadow = CreateTerrainLayer("Meadow", new Color(0.20f, 0.43f, 0.11f), 13.7f);
            var grass = CreateTerrainLayer("Dry Grass", new Color(0.48f, 0.44f, 0.17f), 27.4f);
            var earth = CreateTerrainLayer("Earth", new Color(0.36f, 0.20f, 0.08f), 41.8f);
            data.terrainLayers = new[] { meadow, grass, earth };
            data.alphamapResolution = 128;
            var blend = new float[128, 128, 3];
            for (var z = 0; z < 128; z++)
            for (var x = 0; x < 128; x++)
            {
                var broad = Mathf.PerlinNoise(x * 0.035f + 8.1f, z * 0.035f + 3.7f);
                var fine = Mathf.PerlinNoise(x * 0.09f + 1.9f, z * 0.09f + 6.2f);
                var earthWeight = Mathf.Clamp01((fine - 0.57f) * 3.2f) * 0.60f;
                var dryWeight = Mathf.Lerp(0.08f, 0.60f, broad) * (1f - earthWeight);
                blend[z, x, 0] = 1f - dryWeight - earthWeight;
                blend[z, x, 1] = dryWeight;
                blend[z, x, 2] = earthWeight;
            }
            data.SetAlphamaps(0, 0, blend);
        }

        static TerrainLayer CreateTerrainLayer(string label, Color baseColor, float seed)
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                name = label + " Texture",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            var pixels = new Color[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var noise = Mathf.PerlinNoise(x * 0.16f + seed, y * 0.16f + seed * 0.37f);
                pixels[y * size + x] = baseColor * Mathf.Lerp(0.78f, 1.16f, noise);
            }
            texture.SetPixels(pixels);
            texture.Apply(true, true);
            return new TerrainLayer
            {
                name = label,
                diffuseTexture = texture,
                tileSize = new Vector2(18f, 18f),
                metallic = 0f,
                smoothness = 0.08f
            };
        }

        void CreateCamera()
        {
            var cameraObject = new GameObject("RTS Camera");
            worldCamera = cameraObject.AddComponent<Camera>();
            worldCamera.fieldOfView = 50f;
            worldCamera.nearClipPlane = 0.2f;
            worldCamera.farClipPlane = 1000f;
            worldCamera.clearFlags = CameraClearFlags.Skybox;
            worldCamera.allowHDR = true;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<RtsCameraController>();
        }

        void CreateLighting()
        {
            var lightObject = new GameObject("Sun");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.35f;
            light.color = new Color(1f, 0.91f, 0.75f);
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.82f;
            RenderSettings.sun = light;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.48f, 0.57f, 0.67f);
            RenderSettings.ambientEquatorColor = new Color(0.31f, 0.34f, 0.30f);
            RenderSettings.ambientGroundColor = new Color(0.15f, 0.13f, 0.10f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0018f;
            RenderSettings.fogColor = new Color(0.60f, 0.66f, 0.69f);
            var skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader != null)
            {
                var sky = new Material(skyShader) { name = "CityLab Day Sky" };
                sky.SetColor("_SkyTint", new Color(0.38f, 0.52f, 0.68f));
                sky.SetColor("_GroundColor", new Color(0.38f, 0.35f, 0.29f));
                sky.SetFloat("_AtmosphereThickness", 0.85f);
                sky.SetFloat("_Exposure", 1.15f);
                sky.SetFloat("_SunSize", 0.035f);
                RenderSettings.skybox = sky;
            }
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
            for (var i = 0; i < 80; i++)
            {
                var radius = Mathf.Lerp(52f, 155f, (float)random.NextDouble());
                var angle = (float)random.NextDouble() * Mathf.PI * 2f;
                var position = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                if (worldTerrain != null)
                    position.y = worldTerrain.SampleHeight(position) + worldTerrain.transform.position.y;
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
            var center = (start + end) * 0.5f + Vector3.up * 0.10f;
            var length = Vector3.Distance(start, end);
            var view = CreatePrimitive($"Road {road.id}", PrimitiveType.Cube, center, new Vector3(4.6f, 0.18f, length), roadMaterial);
            var rotation = Quaternion.LookRotation((end - start).normalized, Vector3.up);
            view.transform.rotation = rotation;
            view.AddComponent<RoadView>().RoadId = road.id;
            var verge = MakeMaterial("Road Verge", new Color(0.43f, 0.34f, 0.20f));
            var side = rotation * Vector3.right * 2.05f;
            var left = CreatePrimitive("Left worn edge", PrimitiveType.Cube, center - side + Vector3.up * 0.04f,
                new Vector3(0.24f, 0.06f, length), verge);
            var right = CreatePrimitive("Right worn edge", PrimitiveType.Cube, center + side + Vector3.up * 0.04f,
                new Vector3(0.24f, 0.06f, length), verge);
            left.transform.rotation = rotation;
            right.transform.rotation = rotation;
            foreach (var edge in new[] { left, right })
            {
                var edgeCollider = edge.GetComponent<Collider>();
                if (edgeCollider != null) edgeCollider.enabled = false;
            }
            roadViews.Add(road.id, view);
        }

        void SyncParcel(ParcelState parcel)
        {
            if (!parcelViews.TryGetValue(parcel.id, out var view))
            {
                view = new GameObject($"Parcel {parcel.id}");
                view.transform.position = parcel.center.ToVector3() + Vector3.up * 0.06f;
                var fill = CreatePrimitive("Zoning fill", PrimitiveType.Cube, Vector3.zero,
                    new Vector3(parcel.width, 0.06f, parcel.depth), parcelMaterial);
                fill.transform.SetParent(view.transform, false);
                var collider = fill.GetComponent<Collider>();
                if (collider != null) collider.enabled = false;
                AddParcelBorder(view.transform, parcel.width, parcel.depth);
                parcelViews.Add(parcel.id, view);
            }
            view.SetActive(parcel.buildingId == 0);
        }

        void AddParcelBorder(Transform root, float width, float depth)
        {
            var border = MakeMaterial("Parcel Border", new Color(0.67f, 0.72f, 0.38f));
            var north = CreatePrimitive("Boundary North", PrimitiveType.Cube, Vector3.zero, new Vector3(width, 0.10f, 0.16f), border);
            var south = CreatePrimitive("Boundary South", PrimitiveType.Cube, Vector3.zero, new Vector3(width, 0.10f, 0.16f), border);
            var west = CreatePrimitive("Boundary West", PrimitiveType.Cube, Vector3.zero, new Vector3(0.16f, 0.10f, depth), border);
            var east = CreatePrimitive("Boundary East", PrimitiveType.Cube, Vector3.zero, new Vector3(0.16f, 0.10f, depth), border);
            north.transform.SetParent(root, false);
            south.transform.SetParent(root, false);
            west.transform.SetParent(root, false);
            east.transform.SetParent(root, false);
            north.transform.localPosition = new Vector3(0f, 0.06f, depth * 0.5f);
            south.transform.localPosition = new Vector3(0f, 0.06f, -depth * 0.5f);
            west.transform.localPosition = new Vector3(-width * 0.5f, 0.06f, 0f);
            east.transform.localPosition = new Vector3(width * 0.5f, 0.06f, 0f);
            foreach (Transform edge in root)
            {
                var edgeCollider = edge.GetComponent<Collider>();
                if (edgeCollider != null) edgeCollider.enabled = false;
            }
        }

        void SyncBuilding(BuildingState building)
        {
            if (!buildingViews.TryGetValue(building.id, out var view))
            {
                view = CreatePrimitive($"House {building.id}", PrimitiveType.Cube,
                    building.position.ToVector3() + Vector3.up * 0.35f,
                    new Vector3(7f, 0.7f, 9f), foundationMaterial);
                view.transform.rotation = Quaternion.Euler(0f, building.yaw, 0f);
                view.AddComponent<BuildingView>().Initialize(building.id, baseMaterial);
                buildingViews.Add(building.id, view);
            }

            var renderer = view.GetComponent<Renderer>();
            switch (building.phase)
            {
                case BuildingPhase.Foundation:
                    SetFramingVisible(view, false);
                    renderer.enabled = true;
                    view.transform.localScale = new Vector3(7f, 0.7f, 9f);
                    view.transform.position = building.position.ToVector3() + Vector3.up * 0.35f;
                    renderer.sharedMaterial = foundationMaterial;
                    break;
                case BuildingPhase.Framing:
                    view.transform.localScale = Vector3.one;
                    view.transform.position = building.position.ToVector3();
                    renderer.enabled = false;
                    var framingCollider = view.GetComponent<BoxCollider>();
                    if (framingCollider != null)
                    {
                        framingCollider.center = new Vector3(0f, 2f, 0f);
                        framingCollider.size = new Vector3(7f, 4.5f, 9f);
                    }
                    EnsureFramingVisual(view).SetActive(true);
                    break;
                case BuildingPhase.Complete:
                    SetFramingVisible(view, false);
                    view.transform.localScale = Vector3.one;
                    view.transform.position = building.position.ToVector3();
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

        GameObject EnsureFramingVisual(GameObject view)
        {
            var existing = view.transform.Find("Timber frame");
            if (existing != null)
                return existing.gameObject;
            var frame = new GameObject("Timber frame");
            frame.transform.SetParent(view.transform, false);
            var postPositions = new[]
            {
                new Vector3(-3f, 1.7f, -4f), new Vector3(3f, 1.7f, -4f),
                new Vector3(-3f, 1.7f, 4f), new Vector3(3f, 1.7f, 4f)
            };
            foreach (var position in postPositions)
                AddFrameBeam(frame.transform, position, new Vector3(0.32f, 3.4f, 0.32f), Quaternion.identity);
            AddFrameBeam(frame.transform, new Vector3(0f, 3.35f, -4f), new Vector3(6.5f, 0.32f, 0.32f), Quaternion.identity);
            AddFrameBeam(frame.transform, new Vector3(0f, 3.35f, 4f), new Vector3(6.5f, 0.32f, 0.32f), Quaternion.identity);
            AddFrameBeam(frame.transform, new Vector3(-3f, 3.35f, 0f), new Vector3(0.32f, 0.32f, 8.5f), Quaternion.identity);
            AddFrameBeam(frame.transform, new Vector3(3f, 3.35f, 0f), new Vector3(0.32f, 0.32f, 8.5f), Quaternion.identity);
            return frame;
        }

        void AddFrameBeam(Transform parent, Vector3 localPosition, Vector3 scale, Quaternion rotation)
        {
            var beam = CreatePrimitive("Timber beam", PrimitiveType.Cube, Vector3.zero, scale, framingMaterial);
            beam.transform.SetParent(parent, false);
            beam.transform.SetLocalPositionAndRotation(localPosition, rotation);
            var collider = beam.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
        }

        static void SetFramingVisible(GameObject view, bool visible)
        {
            var framing = view.transform.Find("Timber frame");
            if (framing != null) framing.gameObject.SetActive(visible);
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
            if (isDetailed)
                view.GetComponent<VillagerVisual>().Refresh(target, villager.activity, villager.carryingWood);
            else
            {
                view.transform.position = Vector3.Lerp(view.transform.position, target,
                    1f - Mathf.Exp(-15f * ViewRefreshInterval));
                view.transform.localScale = villager.carryingWood > 0 ? new Vector3(0.8f, 1f, 0.8f) : new Vector3(0.7f, 1f, 0.7f);
            }
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

    public sealed class BuildingView : MonoBehaviour
    {
        static readonly Vector3[] SelectionCorners =
        {
            new Vector3(-3.8f, 0f, -4.8f), new Vector3(-3.8f, 0f, 4.8f),
            new Vector3(3.8f, 0f, 4.8f), new Vector3(3.8f, 0f, -4.8f)
        };
        LineRenderer selection;

        public int BuildingId { get; private set; }

        public void Initialize(int buildingId, Material baseMaterial)
        {
            BuildingId = buildingId;
            var marker = new GameObject($"Building {buildingId} selection");
            selection = marker.AddComponent<LineRenderer>();
            selection.loop = true;
            selection.useWorldSpace = true;
            selection.positionCount = 4;
            selection.startWidth = 0.18f;
            selection.endWidth = 0.18f;
            selection.numCornerVertices = 2;
            selection.sharedMaterial = new Material(baseMaterial)
            {
                name = "Runtime Selection Gold",
                color = new Color(1f, 0.72f, 0.16f)
            };
            selection.enabled = false;
        }

        public void SetSelected(bool selected)
        {
            if (selection != null)
                selection.enabled = selected;
        }

        void LateUpdate()
        {
            if (selection == null || !selection.enabled)
                return;
            var rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            var center = transform.position + Vector3.up * 0.16f;
            for (var i = 0; i < SelectionCorners.Length; i++)
                selection.SetPosition(i, center + rotation * SelectionCorners[i]);
        }

        void OnDestroy()
        {
            if (selection != null)
                Destroy(selection.gameObject);
        }
    }

    public sealed class StockpileMarker : MonoBehaviour { }

    public sealed class DirectionalLightCycle : MonoBehaviour
    {
        [Range(0f, 1f)] public float normalizedTime = 0.32f;
        public float cycleSeconds = 240f;

        void Update()
        {
            normalizedTime = Mathf.Repeat(normalizedTime + Time.unscaledDeltaTime / cycleSeconds, 1f);
            var daylight = Mathf.Sin(normalizedTime * Mathf.PI);
            transform.rotation = Quaternion.Euler(Mathf.Lerp(28f, 58f, daylight), -38f + normalizedTime * 22f, 0f);
            var light = GetComponent<Light>();
            light.intensity = Mathf.Lerp(1.05f, 1.45f, daylight);
        }
    }

    public sealed class CityLabPerformanceProbe : MonoBehaviour
    {
        const int WarmupFrames = 120;
        const int SampleFrames = 600;
        readonly List<float> samples = new List<float>(SampleFrames);
        CityLabGame game;
        int frames;

        public void Initialize(CityLabGame owner)
        {
            game = owner;
            var snapshot = game.StateSource.GetSnapshot(1001);
            Debug.Log($"CITYLAB_SMOKE_SCENARIO households={snapshot.households.Count} buildings={snapshot.buildings.Count} villagers={snapshot.villagers.Count}");
        }

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
            var snapshot = game.StateSource.GetSnapshot(1001);
            if (snapshot.households.Count < 20 || snapshot.buildings.Count < 30 || snapshot.villagers.Count < 30)
            {
                Debug.LogError($"CITYLAB_PERF_FAIL scenario households={snapshot.households.Count} buildings={snapshot.buildings.Count} villagers={snapshot.villagers.Count}");
                Application.Quit(2);
                enabled = false;
                return;
            }
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
                var rotation = Quaternion.Euler(39f, 27f, 0f);
                controller.transform.SetPositionAndRotation(
                    new Vector3(0f, 0f, 6f) - rotation * Vector3.forward * 82f, rotation);
            }
            var game = FindFirstObjectByType<CityLabGame>();
            var tools = FindFirstObjectByType<CityBuildController>();
            if (game != null && tools != null)
            {
                var snapshot = game.StateSource.GetSnapshot(1001);
                var site = snapshot.buildings.Find(item => item.phase != BuildingPhase.Complete);
                if (site != null)
                    tools.SelectBuilding(site.id);
            }
        }

        void Update()
        {
            frames++;
            if (frames == 450)
                SelectActiveSite();
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

        static void SelectActiveSite()
        {
            var game = FindFirstObjectByType<CityLabGame>();
            var tools = FindFirstObjectByType<CityBuildController>();
            if (game == null || tools == null)
                return;
            var snapshot = game.StateSource.GetSnapshot(1001);
            var site = snapshot.buildings.Find(item => item.phase != BuildingPhase.Complete);
            if (site != null)
                tools.SelectBuilding(site.id);
        }
    }
}
