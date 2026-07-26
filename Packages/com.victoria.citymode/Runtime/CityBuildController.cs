using UnityEngine;
using UnityEngine.InputSystem;

namespace Victoria.CityMode
{
    public enum CityToolMode : byte
    {
        Inspect = 0,
        DrawRoad = 1,
        ZoneResidential = 2
    }

    public sealed class CityBuildController : MonoBehaviour
    {
        CityLabGame game;
        bool hasRoadStart;
        Vector3 roadStart;
        GameObject roadPreview;
        Material roadPreviewMaterial;

        public CityToolMode Mode { get; private set; }
        public int SelectedBuildingId { get; private set; }
        public string Prompt { get; private set; } = "Selectionnez un outil";

        public void Initialize(CityLabGame owner)
        {
            game = owner;
            SetMode(CityToolMode.Inspect);
        }

        public void SetMode(CityToolMode mode)
        {
            Mode = mode;
            hasRoadStart = false;
            if (roadPreview != null) roadPreview.SetActive(false);
            Prompt = mode switch
            {
                CityToolMode.DrawRoad => "Route: cliquez le point de depart",
                CityToolMode.ZoneResidential => "Parcelles: cliquez une route",
                _ => "R: route  |  Z: parcelles  |  Echap: annuler"
            };
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard == null || mouse == null || game == null)
                return;

            if (keyboard.rKey.wasPressedThisFrame) SetMode(CityToolMode.DrawRoad);
            if (keyboard.zKey.wasPressedThisFrame) SetMode(CityToolMode.ZoneResidential);
            if (keyboard.escapeKey.wasPressedThisFrame) SetMode(CityToolMode.Inspect);
            UpdateRoadPreview(mouse.position.ReadValue());
            if (!mouse.leftButton.wasPressedThisFrame)
                return;

            var ray = game.WorldCamera.ScreenPointToRay(mouse.position.ReadValue());
            if (Mode == CityToolMode.DrawRoad)
                HandleRoadClick(ray);
            else if (Mode == CityToolMode.ZoneResidential)
                HandleZoneClick(ray);
            else
                HandleInspectClick(ray);
        }

        void HandleInspectClick(Ray ray)
        {
            var selected = 0;
            if (Physics.Raycast(ray, out var hit, 1000f))
            {
                var building = hit.collider.GetComponentInParent<BuildingView>();
                if (building != null)
                    selected = building.BuildingId;
            }
            SelectBuilding(selected);
        }

        public void SelectBuilding(int buildingId)
        {
            SelectedBuildingId = buildingId;
            game.SetSelectedBuilding(buildingId);
            Prompt = buildingId == 0
                ? "R: route  |  Z: parcelles  |  Cliquez un chantier"
                : $"Chantier {buildingId} selectionne: choisissez sa priorite";
        }

        public void SetSelectedPriority(int priority)
        {
            if (SelectedBuildingId == 0)
            {
                Prompt = "Selectionnez d'abord un chantier";
                return;
            }
            var result = game.Submit(CityCommand.SetPriority(SelectedBuildingId, priority));
            Prompt = result.accepted
                ? $"Priorite du chantier {SelectedBuildingId}: {priority}"
                : $"Priorite refusee: {result.reason}";
        }

        void HandleRoadClick(Ray ray)
        {
            if (!Physics.Raycast(ray, out var hit, 1000f))
            {
                Prompt = "Route refusee: aucun terrain sous le curseur";
                return;
            }
            var point = hit.point;
            point.y = 0f;
            if (!hasRoadStart)
            {
                roadStart = point;
                hasRoadStart = true;
                EnsureRoadPreview();
                Prompt = "Route: cliquez le point d'arrivee";
                return;
            }

            var result = game.Submit(CityCommand.DrawRoad(roadStart, point));
            hasRoadStart = false;
            Prompt = result.accepted
                ? "Route creee. Z puis clic sur la route pour lotir."
                : $"Route refusee: {result.reason}";
            if (roadPreview != null) roadPreview.SetActive(false);
        }

        void EnsureRoadPreview()
        {
            if (roadPreview != null)
                return;
            roadPreview = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roadPreview.name = "Road placement preview";
            var collider = roadPreview.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
            var baseMaterial = Resources.Load<Material>("CityLabBaseMaterial");
            roadPreviewMaterial = new Material(baseMaterial) { name = "Runtime Road Preview" };
            roadPreview.GetComponent<Renderer>().sharedMaterial = roadPreviewMaterial;
        }

        void UpdateRoadPreview(Vector2 pointer)
        {
            if (Mode != CityToolMode.DrawRoad || !hasRoadStart)
            {
                if (roadPreview != null) roadPreview.SetActive(false);
                return;
            }
            EnsureRoadPreview();
            var ray = game.WorldCamera.ScreenPointToRay(pointer);
            if (!Physics.Raycast(ray, out var hit, 1000f))
            {
                roadPreview.SetActive(false);
                return;
            }
            var end = hit.point;
            end.y = 0f;
            var delta = end - roadStart;
            var length = delta.magnitude;
            if (length < 0.01f)
            {
                roadPreview.SetActive(false);
                return;
            }
            roadPreview.SetActive(true);
            roadPreview.transform.SetPositionAndRotation((roadStart + end) * 0.5f + Vector3.up * 0.18f,
                Quaternion.LookRotation(delta.normalized, Vector3.up));
            roadPreview.transform.localScale = new Vector3(4.6f, 0.18f, length);
            var valid = length >= 4f && length <= 150f && Mathf.Abs(end.x) <= 250f && Mathf.Abs(end.z) <= 250f;
            roadPreviewMaterial.color = valid
                ? new Color(0.32f, 0.72f, 0.30f, 0.82f)
                : new Color(0.88f, 0.20f, 0.14f, 0.88f);
        }

        void HandleZoneClick(Ray ray)
        {
            if (!Physics.Raycast(ray, out var hit, 1000f))
            {
                Prompt = "Aucune route sous le curseur";
                return;
            }
            var road = hit.collider.GetComponentInParent<RoadView>();
            if (road == null)
            {
                Prompt = "Selectionnez directement une route";
                return;
            }
            var result = game.Submit(CityCommand.ZoneResidential(road.RoadId));
            Prompt = result.accepted
                ? "Parcelles creees: les foyers lancent leurs chantiers"
                : $"Lotissement refuse: {result.reason}";
        }
    }
}
