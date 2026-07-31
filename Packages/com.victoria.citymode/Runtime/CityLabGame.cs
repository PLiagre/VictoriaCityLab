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
        readonly Dictionary<int, GameObject> productionSiteViews = new Dictionary<int, GameObject>();
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
        Material smokeMaterial;
        Material baseMaterial;
        Terrain worldTerrain;
        CityLabPerformanceProbe performanceProbe;
        float viewRefreshTimer;
        float lastRunningSpeed = 1f;
        bool automatedRun;

        public ICityStateSource StateSource => simulation;
        public ICityCommandSink CommandSink => simulation;
        public Camera WorldCamera => worldCamera;
        public float SimulationSpeed { get; private set; } = 1f;
        public bool IsPaused => SimulationSpeed <= 0f;
        public bool IsPointerOverHud(Vector2 screenPoint) => hud != null && hud.ContainsPointer(screenPoint);

        void Awake()
        {
            var arguments = Environment.GetCommandLineArgs();
            var isSmoke = Array.Exists(arguments, item => item == "-citylabSmoke");
            var isCapture = Array.Exists(arguments, item => item == "-citylabCapture");
            automatedRun = isSmoke || isCapture;
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
            StylizedEnvironment.CreateGroundCover(worldTerrain, baseMaterial);
            StylizedEnvironment.CreateVillageDressing(worldTerrain, baseMaterial, roadMaterial, woodMaterial);
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
                Submit(CityCommand.PlaceLumberCamp(new Vector3(38f, 0f, -10f)));
                performanceProbe = gameObject.AddComponent<CityLabPerformanceProbe>();
                performanceProbe.Initialize(this);
            }
            else if (isCapture)
            {
                var road = Submit(CityCommand.DrawRoad(new Vector3(-42f, 0f, 12f), new Vector3(42f, 0f, 12f)));
                if (road.accepted)
                    Submit(CityCommand.ZoneResidential(road.createdId));
                Submit(CityCommand.PlaceLumberCamp(new Vector3(38f, 0f, -10f)));
                gameObject.AddComponent<CityLabCaptureProbe>();
            }
        }

        void Update()
        {
            if (simulation == null)
                return;
            UpdateSimulationClockInput();
            simulation.Tick(Time.deltaTime * SimulationSpeed);
            viewRefreshTimer -= Time.deltaTime;
            if (viewRefreshTimer <= 0f)
                RefreshSnapshotViews();
        }

        public void SetSimulationSpeed(float speed)
        {
            if (speed <= 0f)
            {
                SimulationSpeed = 0f;
                return;
            }
            SimulationSpeed = speed <= 1f ? 1f : speed <= 2f ? 2f : 4f;
            lastRunningSpeed = SimulationSpeed;
        }

        public void TogglePause() => SetSimulationSpeed(IsPaused ? lastRunningSpeed : 0f);

        void UpdateSimulationClockInput()
        {
            if (automatedRun)
                return;
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;
            if (keyboard.spaceKey.wasPressedThisFrame) TogglePause();
            if (keyboard.digit1Key.wasPressedThisFrame) SetSimulationSpeed(1f);
            if (keyboard.digit2Key.wasPressedThisFrame) SetSimulationSpeed(2f);
            if (keyboard.digit3Key.wasPressedThisFrame) SetSimulationSpeed(4f);
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
            hud?.ShowMessage(result.accepted ? "Ordre accepte" : $"Refus : {DescribeReason(result.reason)}", result.accepted);
            return result;
        }

        public static string DescribeReason(string reason) => reason switch
        {
            "road-too-short" => "route trop courte",
            "road-too-long" => "route trop longue",
            "road-outside-map" => "hors des limites du domaine",
            "road-unknown" => "route introuvable",
            "road-inaccessible" => "route inaccessible",
            "road-already-zoned" => "route deja lotie",
            "no-valid-parcel" => "aucune parcelle valide",
            "building-unknown" => "chantier introuvable",
            "lumber-camp-outside-map" => "camp hors des limites du domaine",
            "lumber-camp-too-close-to-centre" => "camp trop proche du bourg",
            "lumber-camp-too-far-from-centre" => "camp trop eloigne du bourg",
            "lumber-camp-too-close-to-another-camp" => "un camp forestier est deja trop proche",
            "lumber-camp-insufficient-wood" => "8 unites de bois sont requises",
            "command-null" => "ordre vide",
            "command-unknown" => "ordre inconnu",
            _ => reason
        };

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
            var roadTexture = Resources.Load<Texture2D>("Textures/StylizedRoad_Albedo");
            if (roadTexture != null)
            {
                roadTexture.wrapMode = TextureWrapMode.Repeat;
                roadTexture.filterMode = FilterMode.Trilinear;
                roadTexture.anisoLevel = 8;
                roadMaterial.SetTexture("_BaseMap", roadTexture);
                roadMaterial.SetTexture("_MainTex", roadTexture);
                roadMaterial.color = new Color(0.78f, 0.69f, 0.56f);
                roadMaterial.SetFloat("_Smoothness", 0.03f);
            }
            parcelMaterial = MakeMaterial("Parcel", new Color(0.18f, 0.36f, 0.13f, 0.14f), true);
            foundationMaterial = MakeMaterial("Foundation", new Color(0.39f, 0.37f, 0.32f));
            framingMaterial = MakeMaterial("Framing", new Color(0.48f, 0.29f, 0.12f));
            houseMaterial = MakeMaterial("House", new Color(0.46f, 0.22f, 0.14f));
            villagerMaterial = MakeMaterial("Villager", new Color(0.20f, 0.29f, 0.43f));
            woodMaterial = MakeMaterial("Wood", new Color(0.43f, 0.24f, 0.09f));
            smokeMaterial = MakeMaterial("Hearth Smoke", new Color(0.14f, 0.15f, 0.14f, 0.20f), true);
            var smokeTexture = CreateSoftParticleTexture();
            smokeMaterial.SetTexture("_BaseMap", smokeTexture);
            smokeMaterial.SetTexture("_MainTex", smokeTexture);
            smokeMaterial.SetFloat("_Smoothness", 0f);
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

        static Texture2D CreateSoftParticleTexture()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                name = "Runtime soft smoke particle",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var pixels = new Color[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var uv = new Vector2((x + 0.5f) / size, (y + 0.5f) / size) * 2f - Vector2.one;
                var distance = uv.magnitude;
                var noise = Mathf.PerlinNoise(x * 0.17f + 4.1f, y * 0.17f + 8.3f);
                var alpha = Mathf.Clamp01(1f - distance);
                alpha = alpha * alpha * Mathf.Lerp(0.68f, 1f, noise);
                pixels[y * size + x] = new Color(0.74f, 0.78f, 0.75f, alpha);
            }
            texture.SetPixels(pixels);
            texture.Apply(true, true);
            return texture;
        }

        void CreateWorld()
        {
            var terrainData = new TerrainData
            {
                heightmapResolution = 257,
                size = new Vector3(512f, 18f, 512f)
            };
            var heights = new float[257, 257];
            for (var z = 0; z < 257; z++)
            for (var x = 0; x < 257; x++)
            {
                var nx = (x - 128f) / 128f;
                var nz = (z - 128f) / 128f;
                var rim = Mathf.Max(0f, (nx * nx + nz * nz - 0.18f) * 0.018f);
                var broad = Mathf.PerlinNoise(x * 0.018f + 13.1f, z * 0.018f + 7.9f) * 0.011f;
                var detail = Mathf.PerlinNoise(x * 0.061f, z * 0.061f) * 0.0035f;
                var villageFlattening = Mathf.SmoothStep(0.16f, 0.48f, nx * nx + nz * nz);
                heights[z, x] = 0.0055f + rim + (broad + detail) * villageFlattening;
            }
            terrainData.SetHeights(0, 0, heights);
            var terrainObject = Terrain.CreateTerrainGameObject(terrainData);
            terrainObject.name = "European Terrain 512m";
            terrainObject.transform.position = new Vector3(-256f, -terrainData.GetInterpolatedHeight(0.5f, 0.5f), -256f);
            worldTerrain = terrainObject.GetComponent<Terrain>();
            worldTerrain.drawInstanced = true;
            worldTerrain.heightmapPixelError = 3f;
            worldTerrain.basemapDistance = 650f;
            // The terrain still receives prop/building shadows, but does not self-shadow its
            // low-amplitude height field (which otherwise muddies the painterly albedo).
            worldTerrain.shadowCastingMode = ShadowCastingMode.Off;
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
            var paintedMeadow = Resources.Load<Texture2D>("Textures/StylizedMeadow_Albedo");
            if (paintedMeadow != null)
            {
                paintedMeadow.wrapMode = TextureWrapMode.Repeat;
                paintedMeadow.filterMode = FilterMode.Trilinear;
                paintedMeadow.anisoLevel = 8;
            }
            var meadow = CreateTerrainLayer("Painted Highland Meadow", new Color(0.20f, 0.43f, 0.11f), 13.7f, paintedMeadow);
            var grass = CreateTerrainLayer("Dry Grass", new Color(0.48f, 0.44f, 0.17f), 27.4f);
            var earth = CreateTerrainLayer("Earth", new Color(0.36f, 0.20f, 0.08f), 41.8f);
            data.terrainLayers = new[] { meadow, grass, earth };
            data.alphamapResolution = 256;
            var blend = new float[256, 256, 3];
            for (var z = 0; z < 256; z++)
            for (var x = 0; x < 256; x++)
            {
                var broad = Mathf.PerlinNoise(x * 0.026f + 8.1f, z * 0.026f + 3.7f);
                var fine = Mathf.PerlinNoise(x * 0.082f + 1.9f, z * 0.082f + 6.2f);
                var earthWeight = Mathf.Clamp01((fine - 0.62f) * 3.5f) * 0.38f;
                var dryWeight = Mathf.Lerp(0.04f, 0.32f, broad) * (1f - earthWeight);
                blend[z, x, 0] = 1f - dryWeight - earthWeight;
                blend[z, x, 1] = dryWeight;
                blend[z, x, 2] = earthWeight;
            }
            data.SetAlphamaps(0, 0, blend);
        }

        static TerrainLayer CreateTerrainLayer(string label, Color baseColor, float seed, Texture2D authoredTexture = null)
        {
            const int size = 64;
            var texture = authoredTexture != null ? authoredTexture : new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                name = label + " Texture",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            if (authoredTexture == null)
            {
                var pixels = new Color[size * size];
                for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var noise = Mathf.PerlinNoise(x * 0.16f + seed, y * 0.16f + seed * 0.37f);
                    pixels[y * size + x] = baseColor * Mathf.Lerp(0.78f, 1.16f, noise);
                }
                texture.SetPixels(pixels);
                texture.Apply(true, true);
            }
            var layer = new TerrainLayer
            {
                name = label,
                diffuseTexture = texture,
                tileSize = authoredTexture != null ? new Vector2(26f, 26f) : new Vector2(18f, 18f),
                metallic = 0f,
                smoothness = authoredTexture != null ? 0.025f : 0.08f,
                normalScale = 0f
            };
            if (authoredTexture != null)
            {
                layer.diffuseRemapMin = new Vector4(0.065f, 0.125f, 0.028f, 0f);
                layer.diffuseRemapMax = new Vector4(1.46f, 1.72f, 1.30f, 1f);
            }
            else
            {
                layer.diffuseRemapMin = new Vector4(baseColor.r, baseColor.g, baseColor.b, 0f) * 0.06f;
                layer.diffuseRemapMax = new Vector4(1.14f, 1.14f, 1.10f, 1f);
            }
            return layer;
        }

        static Texture2D CreateTerrainNormal(float seed)
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, true, true)
            {
                name = "Runtime painterly terrain normal",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Trilinear
            };
            var heights = new float[size, size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
                heights[x, y] = Mathf.PerlinNoise(x * 0.15f + seed, y * 0.15f + seed * 0.41f);
            var pixels = new Color[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var left = heights[(x - 1 + size) % size, y];
                var right = heights[(x + 1) % size, y];
                var down = heights[x, (y - 1 + size) % size];
                var up = heights[x, (y + 1) % size];
                var normal = new Vector3((left - right) * 1.9f, (down - up) * 1.9f, 1f).normalized;
                pixels[y * size + x] = new Color(normal.x * 0.5f + 0.5f, normal.y * 0.5f + 0.5f,
                    normal.z * 0.5f + 0.5f, 1f);
            }
            texture.SetPixels(pixels);
            texture.Apply(true, true);
            return texture;
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
            cameraObject.AddComponent<AudioSource>();
            cameraObject.AddComponent<ProceduralAmbience>();
            cameraObject.AddComponent<RtsCameraController>();
            StylizedEnvironment.ConfigurePostProcessing(worldCamera);
        }

        void CreateLighting()
        {
            var lightObject = new GameObject("Sun");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 2.05f;
            light.color = new Color(1f, 0.95f, 0.84f);
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.82f;
            light.shadowBias = 0.035f;
            light.shadowNormalBias = 0.30f;
            RenderSettings.sun = light;

            var fillObject = new GameObject("Sky Fill");
            fillObject.transform.rotation = Quaternion.Euler(52f, 132f, 0f);
            var fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.62f;
            fill.color = new Color(0.52f, 0.66f, 0.82f);
            fill.shadows = LightShadows.None;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.48f, 0.57f, 0.67f);
            RenderSettings.ambientEquatorColor = new Color(0.31f, 0.34f, 0.30f);
            RenderSettings.ambientGroundColor = new Color(0.21f, 0.19f, 0.15f);
            RenderSettings.ambientIntensity = 1.05f;
            RenderSettings.reflectionIntensity = 0.82f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.00125f;
            RenderSettings.fogColor = new Color(0.52f, 0.59f, 0.61f);
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
            if (visualLibrary == null)
                return;
            var random = new System.Random(140001);
            ScatterVisuals(visualLibrary.treePrefabs, "Tree", 145, 48f, 218f, 0.85f, 1.2f, random);
            ScatterVisuals(visualLibrary.bushPrefabs, "Forest bush", 48, 30f, 185f, 0.78f, 1.25f, random);
            ScatterVisuals(visualLibrary.rockPrefabs, "Highland rock", 28, 38f, 212f, 0.75f, 1.35f, random);
            ScatterVisuals(visualLibrary.grassPrefabs, "Authored grass clump", 72, 24f, 95f, 0.75f, 1.18f, random);
            PlaceVillageProps(random);
        }

        void ScatterVisuals(GameObject[] prefabs, string label, int count, float innerRadius, float outerRadius,
            float minimumScale, float maximumScale, System.Random random)
        {
            if (prefabs == null || prefabs.Length == 0)
                return;
            for (var i = 0; i < count; i++)
            {
                var radius = Mathf.Lerp(innerRadius, outerRadius, Mathf.Sqrt((float)random.NextDouble()));
                var angle = (float)random.NextDouble() * Mathf.PI * 2f;
                var position = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                if (worldTerrain != null)
                    position.y = worldTerrain.SampleHeight(position) + worldTerrain.transform.position.y;
                var prefab = prefabs[i % prefabs.Length];
                var visual = InstantiateVisual(prefab, $"{label} {i + 1}", position,
                    Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f));
                if (visual != null)
                    visual.transform.localScale *= Mathf.Lerp(minimumScale, maximumScale, (float)random.NextDouble());
            }
        }

        void PlaceVillageProps(System.Random random)
        {
            if (visualLibrary.propPrefabs == null || visualLibrary.propPrefabs.Length == 0)
                return;
            var positions = new[]
            {
                new Vector3(-19f, 0f, -14.5f), new Vector3(10f, 0f, -14.5f),
                new Vector3(-4f, 0f, 8.5f), new Vector3(3f, 0f, 8f),
                new Vector3(7f, 0f, -9f), new Vector3(-8f, 0f, -10f),
                new Vector3(11f, 0f, 4f), new Vector3(-15f, 0f, 5.5f),
                new Vector3(17f, 0f, -7f)
            };
            for (var i = 0; i < positions.Length; i++)
            {
                var position = positions[i];
                if (worldTerrain != null)
                    position.y = worldTerrain.SampleHeight(position) + worldTerrain.transform.position.y;
                InstantiateVisual(visualLibrary.propPrefabs[i % visualLibrary.propPrefabs.Length],
                    $"Village prop {i + 1}", position,
                    Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f));
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
            foreach (var site in snapshot.productionSites)
                SyncProductionSite(site);
        }

        void SyncProductionSite(ProductionSiteState site)
        {
            if (site.kind != ProductionSiteKind.LumberCamp)
                return;
            if (!productionSiteViews.TryGetValue(site.id, out var view))
            {
                view = StylizedEnvironment.CreateLumberCamp($"Lumber camp {site.id}", site.position.ToVector3(),
                    worldTerrain, baseMaterial, roadMaterial, woodMaterial);
                productionSiteViews.Add(site.id, view);
            }
            var camp = view.GetComponent<LumberCampVisual>();
            if (camp != null)
                camp.Refresh(site.remainingTimber, site.assignedWorkers);
        }

        void SyncRoad(RoadState road)
        {
            if (roadViews.ContainsKey(road.id))
                return;
            var start = road.start.ToVector3();
            var end = road.end.ToVector3();
            var view = StylizedEnvironment.CreateTerrainRoad($"Road {road.id}", start, end, 4.8f, worldTerrain, roadMaterial);
            view.AddComponent<RoadView>().RoadId = road.id;
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
            var border = MakeMaterial("Parcel Border", new Color(0.70f, 0.53f, 0.20f));
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
                    renderer.enabled = false;
                    view.transform.localScale = Vector3.one;
                    view.transform.position = building.position.ToVector3();
                    EnsureFoundationVisual(view, building.id).SetActive(true);
                    ConfigureBuildingCollider(view, new Vector3(0f, 0.45f, 0f), new Vector3(7.4f, 0.9f, 9.4f));
                    break;
                case BuildingPhase.Framing:
                    view.transform.localScale = Vector3.one;
                    view.transform.position = building.position.ToVector3();
                    renderer.enabled = false;
                    EnsureFoundationVisual(view, building.id).SetActive(true);
                    ConfigureBuildingCollider(view, new Vector3(0f, 2f, 0f), new Vector3(7f, 4.5f, 9f));
                    EnsureFramingVisual(view).SetActive(true);
                    break;
                case BuildingPhase.Complete:
                    SetFramingVisible(view, false);
                    SetFoundationVisible(view, false);
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
                            AddOccupiedHomeDetails(view.transform);
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

        void AddOccupiedHomeDetails(Transform house)
        {
            var details = new GameObject("Occupied household");
            details.transform.SetParent(house, false);

            var hearth = new GameObject("Warm hearth light");
            hearth.transform.SetParent(details.transform, false);
            hearth.transform.localPosition = new Vector3(0f, 1.8f, -3.6f);
            var light = hearth.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.55f, 0.20f);
            light.intensity = 1.25f;
            light.range = 7f;
            light.shadows = LightShadows.None;

            var smokeRoot = new GameObject("Chimney smoke");
            smokeRoot.transform.SetParent(details.transform, false);
            smokeRoot.transform.localPosition = new Vector3(1.35f, 4.75f, 1.15f);
            var particles = smokeRoot.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(3.8f, 6.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.22f, 0.48f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.28f, 0.62f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.34f, 0.36f, 0.34f, 0.12f),
                new Color(0.56f, 0.58f, 0.54f, 0.24f));
            main.maxParticles = 32;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            var emission = particles.emission;
            emission.rateOverTime = 3.2f;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.22f;
            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.x = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.38f, 0.72f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            var noise = particles.noise;
            noise.enabled = true;
            noise.strength = 0.16f;
            noise.frequency = 0.24f;
            var particleRenderer = smokeRoot.GetComponent<ParticleSystemRenderer>();
            particleRenderer.sharedMaterial = smokeMaterial;
            particleRenderer.shadowCastingMode = ShadowCastingMode.Off;
            particleRenderer.receiveShadows = false;
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

        GameObject EnsureFoundationVisual(GameObject view, int seed)
        {
            var existing = view.transform.Find("Stone foundations");
            if (existing != null)
                return existing.gameObject;

            var root = new GameObject("Stone foundations");
            root.transform.SetParent(view.transform, false);
            var random = new System.Random(seed * 7919 + 140001);
            AddFoundationSide(root.transform, random, new Vector3(-3.35f, 0.25f, 0f), Vector3.forward, 8, 1.12f);
            AddFoundationSide(root.transform, random, new Vector3(3.35f, 0.25f, 0f), Vector3.forward, 8, 1.12f);
            AddFoundationSide(root.transform, random, new Vector3(0f, 0.25f, -4.25f), Vector3.right, 6, 1.10f);
            AddFoundationSide(root.transform, random, new Vector3(0f, 0.25f, 4.25f), Vector3.right, 6, 1.10f);

            for (var i = 0; i < 5; i++)
            {
                var plank = CreatePrimitive("Stacked building plank", PrimitiveType.Cube, Vector3.zero,
                    new Vector3(2.7f, 0.13f, 0.25f), framingMaterial);
                plank.transform.SetParent(root.transform, false);
                plank.transform.localPosition = new Vector3(1.55f, 0.19f + i * 0.14f, 2.8f + i * 0.05f);
                plank.transform.localRotation = Quaternion.Euler(0f, -12f + i * 2f, 0f);
                var collider = plank.GetComponent<Collider>();
                if (collider != null) collider.enabled = false;
            }
            return root;
        }

        void AddFoundationSide(Transform parent, System.Random random, Vector3 center, Vector3 axis, int count, float spacing)
        {
            var midpoint = (count - 1) * 0.5f;
            for (var i = 0; i < count; i++)
            {
                var offset = axis * ((i - midpoint) * spacing);
                var stone = CreatePrimitive("Irregular foundation stone", PrimitiveType.Cube, Vector3.zero,
                    new Vector3(
                        Mathf.Lerp(0.72f, 1.08f, (float)random.NextDouble()),
                        Mathf.Lerp(0.38f, 0.62f, (float)random.NextDouble()),
                        Mathf.Lerp(0.62f, 0.96f, (float)random.NextDouble())),
                    foundationMaterial);
                stone.transform.SetParent(parent, false);
                stone.transform.localPosition = center + offset + new Vector3(0f, Mathf.Lerp(-0.06f, 0.08f, (float)random.NextDouble()), 0f);
                stone.transform.localRotation = Quaternion.Euler(
                    Mathf.Lerp(-4f, 4f, (float)random.NextDouble()),
                    Mathf.Lerp(-10f, 10f, (float)random.NextDouble()),
                    Mathf.Lerp(-3f, 3f, (float)random.NextDouble()));
                var collider = stone.GetComponent<Collider>();
                if (collider != null) collider.enabled = false;
            }
        }

        static void ConfigureBuildingCollider(GameObject view, Vector3 center, Vector3 size)
        {
            var collider = view.GetComponent<BoxCollider>();
            if (collider == null)
                return;
            collider.center = center;
            collider.size = size;
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

        static void SetFoundationVisible(GameObject view, bool visible)
        {
            var foundation = view.transform.Find("Stone foundations");
            if (foundation != null) foundation.gameObject.SetActive(visible);
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

    public sealed class ChimneySmokeVisual : MonoBehaviour
    {
        Transform[] puffs;

        public void Initialize(Transform[] smokePuffs) => puffs = smokePuffs;

        void Update()
        {
            if (puffs == null)
                return;
            for (var i = 0; i < puffs.Length; i++)
            {
                var phase = Mathf.Repeat(Time.unscaledTime * 0.16f + i / (float)puffs.Length, 1f);
                puffs[i].localPosition = new Vector3(
                    1.35f + Mathf.Sin(phase * Mathf.PI * 2f) * 0.22f,
                    4.7f + phase * 2.4f,
                    1.15f + Mathf.Cos(phase * Mathf.PI * 2f) * 0.16f);
                puffs[i].localScale = Vector3.one * Mathf.Lerp(0.16f, 0.46f, phase);
            }
        }
    }

    public sealed class DirectionalLightCycle : MonoBehaviour
    {
        [Range(0f, 1f)] public float normalizedTime = 0.40f;
        public float cycleSeconds = 720f;

        void Update()
        {
            normalizedTime = Mathf.Repeat(normalizedTime + Time.unscaledDeltaTime / cycleSeconds, 1f);
            var solarArc = Mathf.Sin(normalizedTime * Mathf.PI * 2f - Mathf.PI * 0.5f);
            var daylight = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-0.14f, 0.24f, solarArc));
            var height = Mathf.Clamp01((solarArc + 0.08f) / 1.08f);
            transform.rotation = Quaternion.Euler(Mathf.Lerp(-8f, 64f, height), -52f + normalizedTime * 360f, 0f);
            var light = GetComponent<Light>();
            light.intensity = Mathf.Lerp(0.08f, 2.15f, daylight);
            light.color = Color.Lerp(new Color(1f, 0.47f, 0.20f), new Color(1f, 0.985f, 0.90f), height);
            RenderSettings.ambientIntensity = Mathf.Lerp(0.28f, 1.12f, daylight);
            RenderSettings.fogColor = Color.Lerp(new Color(0.075f, 0.09f, 0.13f), new Color(0.52f, 0.59f, 0.61f), daylight);
            if (RenderSettings.skybox != null && RenderSettings.skybox.HasProperty("_Exposure"))
                RenderSettings.skybox.SetFloat("_Exposure", Mathf.Lerp(0.28f, 1.12f, daylight));
        }
    }

    public sealed class CityLabPerformanceProbe : MonoBehaviour
    {
        const int WarmupFrames = 120;
        const int SampleFrames = 600;
        const float MaximumAverageMilliseconds = 20f;
        const float MaximumP95Milliseconds = 25f;
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
            if (average > MaximumAverageMilliseconds || p95 > MaximumP95Milliseconds)
            {
                Debug.LogError($"CITYLAB_PERF_FAIL timing avg_ms={average:F3} p95_ms={p95:F3} " +
                               $"limits={MaximumAverageMilliseconds:F1}/{MaximumP95Milliseconds:F1}");
                Application.Quit(3);
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
                var rotation = Quaternion.Euler(48f, 31f, 0f);
                controller.transform.SetPositionAndRotation(
                    new Vector3(0f, 0f, 3f) - rotation * Vector3.forward * 72f, rotation);
            }
            var game = FindFirstObjectByType<CityLabGame>();
            var tools = FindFirstObjectByType<CityBuildController>();
            if (game != null && tools != null)
            {
                game.SetSimulationSpeed(1f);
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
                var path = System.IO.Path.GetFullPath("Logs/Captures/milestone-forest-revival-20260731.png");
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
            // The automated capture also exercises a harmless invalid command so
            // the player-build screenshot proves the localized refusal treatment.
            game.Submit(CityCommand.DrawRoad(Vector3.zero, Vector3.one));
        }
    }
}
