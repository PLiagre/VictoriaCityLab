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

        public CityToolMode Mode { get; private set; }
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
            if (!mouse.leftButton.wasPressedThisFrame)
                return;

            var ray = game.WorldCamera.ScreenPointToRay(mouse.position.ReadValue());
            if (Mode == CityToolMode.DrawRoad)
                HandleRoadClick(ray);
            else if (Mode == CityToolMode.ZoneResidential)
                HandleZoneClick(ray);
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
                Prompt = "Route: cliquez le point d'arrivee";
                return;
            }

            var result = game.Submit(CityCommand.DrawRoad(roadStart, point));
            hasRoadStart = false;
            Prompt = result.accepted
                ? "Route creee. Z puis clic sur la route pour lotir."
                : $"Route refusee: {result.reason}";
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

