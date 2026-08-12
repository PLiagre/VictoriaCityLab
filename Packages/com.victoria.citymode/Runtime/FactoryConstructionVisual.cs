using System;
using UnityEngine;

namespace Victoria.CityMode
{
    public enum FactoryConstructionStage : byte
    {
        Foundation = 1,
        Frame = 2,
        Roof = 3,
        Details = 4
    }

    /// <summary>
    /// Contrat visuel commun aux bâtiments générés par l'Asset Factory.
    /// Les couches sont cumulatives : une étape achevée reste visible aux suivantes.
    /// </summary>
    public sealed class FactoryConstructionVisual : MonoBehaviour
    {
        [SerializeField] GameObject[] stageRoots = Array.Empty<GameObject>();

        public int StageCount => stageRoots?.Length ?? 0;

        public void Configure(GameObject[] roots)
        {
            stageRoots = roots ?? Array.Empty<GameObject>();
            ShowStage(FactoryConstructionStage.Details);
        }

        public void ShowStage(FactoryConstructionStage completedStage)
        {
            var visibleCount = Mathf.Clamp((int)completedStage, 0, stageRoots?.Length ?? 0);
            for (var index = 0; index < (stageRoots?.Length ?? 0); index++)
                if (stageRoots[index] != null)
                    stageRoots[index].SetActive(index < visibleCount);
        }
    }

    /// <summary>
    /// Échafaudage runtime déterministe des chantiers civiques. Son état est
    /// entièrement dérivé de la phase et du terrassement persistés du bâtiment.
    /// </summary>
    public sealed class ConstructionScaffoldVisual : MonoBehaviour
    {
        [SerializeField] GameObject scaffoldRoot;
        [SerializeField] GameObject[] phaseRoots = Array.Empty<GameObject>();
        [SerializeField] GameObject selectionMarkers;
        bool isSelected;

        public BuildingPhase CurrentPhase { get; private set; } = BuildingPhase.Foundation;
        public bool TerrainPrepared { get; private set; }
        public bool IsSelected => isSelected;
        public bool IsVisible => scaffoldRoot != null && scaffoldRoot.activeInHierarchy;
        public int VisibleStageCount
        {
            get
            {
                if (!IsVisible || phaseRoots == null)
                    return 0;
                var count = 0;
                foreach (var phaseRoot in phaseRoots)
                    if (phaseRoot != null && phaseRoot.activeInHierarchy)
                        count++;
                return count;
            }
        }

        public void Initialize(float footprintWidth, float footprintDepth,
            Material timberMaterial, Material accentMaterial)
        {
            if (scaffoldRoot != null)
                return;

            scaffoldRoot = new GameObject("Construction scaffolding");
            scaffoldRoot.transform.SetParent(transform, false);
            phaseRoots = new GameObject[4];
            for (var index = 0; index < phaseRoots.Length; index++)
            {
                phaseRoots[index] = new GameObject($"Scaffold phase {index + 1}");
                phaseRoots[index].transform.SetParent(scaffoldRoot.transform, false);
            }

            var halfWidth = Mathf.Max(2.5f, footprintWidth * 0.5f + 0.75f);
            var halfDepth = Mathf.Max(2.5f, footprintDepth * 0.5f + 0.75f);
            BuildFoundationStage(phaseRoots[0].transform, halfWidth, halfDepth, timberMaterial);
            BuildFramingStage(phaseRoots[1].transform, halfWidth, halfDepth, timberMaterial);
            BuildRoofingStage(phaseRoots[2].transform, halfWidth, halfDepth, timberMaterial);
            BuildDetailingStage(phaseRoots[3].transform, halfWidth, halfDepth,
                timberMaterial, accentMaterial);
            selectionMarkers = BuildSelectionMarkers(scaffoldRoot.transform,
                halfWidth, halfDepth, accentMaterial);
            Refresh(BuildingPhase.Foundation, false);
        }

        public void Refresh(BuildingPhase phase, bool terrainPrepared)
        {
            CurrentPhase = phase;
            TerrainPrepared = terrainPrepared;
            var visibleStages = phase switch
            {
                BuildingPhase.Foundation => terrainPrepared ? 1 : 0,
                BuildingPhase.Framing => 2,
                BuildingPhase.Roofing => 3,
                BuildingPhase.Detailing => 4,
                _ => 0
            };
            if (scaffoldRoot != null)
                scaffoldRoot.SetActive(visibleStages > 0);
            for (var index = 0; index < (phaseRoots?.Length ?? 0); index++)
                if (phaseRoots[index] != null)
                    phaseRoots[index].SetActive(index < visibleStages);
            RefreshSelectionMarkers();
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            RefreshSelectionMarkers();
        }

        void RefreshSelectionMarkers()
        {
            if (selectionMarkers != null)
                selectionMarkers.SetActive(isSelected && IsVisible);
        }

        static void BuildFoundationStage(Transform parent, float halfWidth, float halfDepth,
            Material timberMaterial)
        {
            foreach (var corner in Corners(halfWidth, halfDepth))
                AddVerticalBeam(parent, corner + Vector3.up * 0.65f, 1.3f, 0.18f, timberMaterial);
            AddPerimeter(parent, halfWidth, halfDepth, 0.75f, 0.16f, timberMaterial);
        }

        static void BuildFramingStage(Transform parent, float halfWidth, float halfDepth,
            Material timberMaterial)
        {
            foreach (var corner in Corners(halfWidth, halfDepth))
                AddVerticalBeam(parent, corner + Vector3.up * 2.35f, 3.4f, 0.18f, timberMaterial);
            AddPerimeter(parent, halfWidth, halfDepth, 2.05f, 0.17f, timberMaterial);
            AddDeck(parent, halfWidth, halfDepth, 1.82f, timberMaterial);
        }

        static void BuildRoofingStage(Transform parent, float halfWidth, float halfDepth,
            Material timberMaterial)
        {
            foreach (var corner in Corners(halfWidth, halfDepth))
                AddVerticalBeam(parent, corner + Vector3.up * 4.45f, 2.6f, 0.18f, timberMaterial);
            AddPerimeter(parent, halfWidth, halfDepth, 4.65f, 0.17f, timberMaterial);
            AddDeck(parent, halfWidth, halfDepth, 4.35f, timberMaterial);
            AddBeamBetween(parent,
                new Vector3(-halfWidth, 0.25f, -halfDepth),
                new Vector3(-halfWidth, 4.35f, -halfDepth), 0.16f, timberMaterial);
        }

        static void BuildDetailingStage(Transform parent, float halfWidth, float halfDepth,
            Material timberMaterial, Material accentMaterial)
        {
            AddBeamBetween(parent,
                new Vector3(-halfWidth, 0.85f, -halfDepth),
                new Vector3(halfWidth, 4.45f, -halfDepth), 0.13f, timberMaterial);
            AddBeamBetween(parent,
                new Vector3(halfWidth, 0.85f, halfDepth),
                new Vector3(-halfWidth, 4.45f, halfDepth), 0.13f, timberMaterial);
            AddVerticalBeam(parent, new Vector3(halfWidth + 0.7f, 3.1f, 0f),
                6.2f, 0.20f, timberMaterial);
            AddBeamBetween(parent,
                new Vector3(halfWidth + 0.7f, 5.9f, -0.1f),
                new Vector3(halfWidth + 0.7f, 5.9f, -2.1f), 0.16f, timberMaterial);
            AddVerticalBeam(parent, new Vector3(halfWidth + 0.7f, 4.35f, -2.1f),
                3.1f, 0.06f, accentMaterial);
        }

        static GameObject BuildSelectionMarkers(Transform parent, float halfWidth, float halfDepth,
            Material accentMaterial)
        {
            var root = new GameObject("Selected scaffold markers");
            root.transform.SetParent(parent, false);
            foreach (var corner in Corners(halfWidth, halfDepth))
            {
                var marker = CreatePart("Selection pennant", root.transform,
                    corner + Vector3.up * 5.95f, new Vector3(0.42f, 0.24f, 0.06f),
                    accentMaterial);
                marker.transform.localRotation = Quaternion.Euler(0f, 35f, 18f);
            }
            root.SetActive(false);
            return root;
        }

        static Vector3[] Corners(float halfWidth, float halfDepth) => new[]
        {
            new Vector3(-halfWidth, 0f, -halfDepth),
            new Vector3(-halfWidth, 0f, halfDepth),
            new Vector3(halfWidth, 0f, halfDepth),
            new Vector3(halfWidth, 0f, -halfDepth)
        };

        static void AddPerimeter(Transform parent, float halfWidth, float halfDepth,
            float height, float thickness, Material material)
        {
            AddBeamBetween(parent, new Vector3(-halfWidth, height, -halfDepth),
                new Vector3(halfWidth, height, -halfDepth), thickness, material);
            AddBeamBetween(parent, new Vector3(-halfWidth, height, halfDepth),
                new Vector3(halfWidth, height, halfDepth), thickness, material);
            AddBeamBetween(parent, new Vector3(-halfWidth, height, -halfDepth),
                new Vector3(-halfWidth, height, halfDepth), thickness, material);
            AddBeamBetween(parent, new Vector3(halfWidth, height, -halfDepth),
                new Vector3(halfWidth, height, halfDepth), thickness, material);
        }

        static void AddDeck(Transform parent, float halfWidth, float halfDepth,
            float height, Material material)
        {
            CreatePart("Scaffold deck north", parent,
                new Vector3(0f, height, halfDepth),
                new Vector3(halfWidth * 2f + 0.35f, 0.11f, 0.72f), material);
            CreatePart("Scaffold deck south", parent,
                new Vector3(0f, height, -halfDepth),
                new Vector3(halfWidth * 2f + 0.35f, 0.11f, 0.72f), material);
            CreatePart("Scaffold deck west", parent,
                new Vector3(-halfWidth, height, 0f),
                new Vector3(0.72f, 0.11f, halfDepth * 2f + 0.35f), material);
            CreatePart("Scaffold deck east", parent,
                new Vector3(halfWidth, height, 0f),
                new Vector3(0.72f, 0.11f, halfDepth * 2f + 0.35f), material);
        }

        static void AddVerticalBeam(Transform parent, Vector3 center, float height,
            float thickness, Material material) =>
            CreatePart("Scaffold post", parent, center,
                new Vector3(thickness, height, thickness), material);

        static void AddBeamBetween(Transform parent, Vector3 start, Vector3 end,
            float thickness, Material material)
        {
            var delta = end - start;
            if (delta.sqrMagnitude <= 0.0001f)
                return;
            var beam = CreatePart("Scaffold rail", parent, (start + end) * 0.5f,
                new Vector3(thickness, delta.magnitude, thickness), material);
            beam.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
        }

        static GameObject CreatePart(string label, Transform parent, Vector3 localPosition,
            Vector3 localScale, Material material)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = label;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            var renderer = part.GetComponent<Renderer>();
            if (renderer != null && material != null)
                renderer.sharedMaterial = material;
            var collider = part.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;
            return part;
        }
    }
}
