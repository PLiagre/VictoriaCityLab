using UnityEngine;
using UnityEngine.UIElements;

namespace Victoria.CityMode
{
    public sealed class CityLabHud : MonoBehaviour
    {
        CityLabGame game;
        CityBuildController tools;
        UIDocument document;
        Label resources;
        Label population;
        Label construction;
        Label prompt;
        Label message;
        CitySnapshot pendingSnapshot;

        public void Initialize(CityLabGame owner, CityBuildController controller)
        {
            game = owner;
            tools = controller;
            document = gameObject.AddComponent<UIDocument>();
            var panel = ScriptableObject.CreateInstance<PanelSettings>();
            panel.name = "CityLab Runtime Panel";
            panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panel.referenceResolution = new Vector2Int(1920, 1080);
            panel.match = 0.5f;
            document.panelSettings = panel;
        }

        void Start()
        {
            BuildUi();
            if (pendingSnapshot != null)
                Refresh(pendingSnapshot);
        }

        void BuildUi()
        {
            var root = document.rootVisualElement;
            root.style.flexDirection = FlexDirection.Row;
            root.style.justifyContent = Justify.SpaceBetween;
            root.style.paddingLeft = 18;
            root.style.paddingRight = 18;
            root.style.paddingTop = 14;
            root.style.paddingBottom = 14;

            var panel = new VisualElement();
            panel.style.width = 360;
            panel.style.paddingLeft = 16;
            panel.style.paddingRight = 16;
            panel.style.paddingTop = 12;
            panel.style.paddingBottom = 12;
            panel.style.backgroundColor = new Color(0.055f, 0.042f, 0.035f, 0.93f);
            panel.style.borderTopLeftRadius = 5;
            panel.style.borderTopRightRadius = 5;
            panel.style.borderBottomLeftRadius = 5;
            panel.style.borderBottomRightRadius = 5;

            var title = MakeLabel("CITYLAB — BOURG 1400", 19, new Color(0.83f, 0.68f, 0.40f));
            resources = MakeLabel("", 15, Color.white);
            population = MakeLabel("", 15, Color.white);
            construction = MakeLabel("", 15, Color.white);
            prompt = MakeLabel("", 14, new Color(0.84f, 0.79f, 0.68f));
            prompt.style.whiteSpace = WhiteSpace.Normal;
            message = MakeLabel("", 13, new Color(0.72f, 0.82f, 0.62f));

            var routeButton = new Button(() => tools.SetMode(CityToolMode.DrawRoad)) { text = "Tracer une route [R]" };
            var zoneButton = new Button(() => tools.SetMode(CityToolMode.ZoneResidential)) { text = "Creer des parcelles [Z]" };
            StyleButton(routeButton);
            StyleButton(zoneButton);

            panel.Add(title);
            panel.Add(resources);
            panel.Add(population);
            panel.Add(construction);
            panel.Add(routeButton);
            panel.Add(zoneButton);
            panel.Add(prompt);
            panel.Add(message);
            root.Add(panel);

            var help = MakeLabel("WASD / bords: camera   Molette: zoom   Clic droit: rotation   F: recentrer", 13,
                new Color(0.9f, 0.86f, 0.75f));
            help.style.backgroundColor = new Color(0.055f, 0.042f, 0.035f, 0.88f);
            help.style.paddingLeft = 12;
            help.style.paddingRight = 12;
            help.style.paddingTop = 8;
            help.style.paddingBottom = 8;
            help.style.alignSelf = Align.FlexStart;
            root.Add(help);
        }

        public void Refresh(CitySnapshot snapshot)
        {
            pendingSnapshot = snapshot;
            if (resources == null)
                return;
            var complete = 0;
            var active = 0;
            foreach (var building in snapshot.buildings)
            {
                if (building.phase == BuildingPhase.Complete) complete++;
                else active++;
            }
            var housed = 0;
            foreach (var household in snapshot.households)
                if (household.homeBuildingId != 0) housed++;

            resources.text = $"Bois: {snapshot.stockWood}  (reserve: {snapshot.reservedWood})";
            population.text = $"Habitants: {snapshot.villagers.Count}   Foyers loges: {housed}/{snapshot.households.Count}";
            construction.text = $"Chantiers: {active}   Maisons terminees: {complete}";
            prompt.text = tools.Prompt;
        }

        public void ShowMessage(string text)
        {
            if (message != null)
                message.text = text;
        }

        static Label MakeLabel(string text, int size, Color color)
        {
            var label = new Label(text);
            label.style.unityFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.style.fontSize = size;
            label.style.color = color;
            label.style.marginBottom = 7;
            return label;
        }

        static void StyleButton(Button button)
        {
            button.style.unityFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            button.style.height = 34;
            button.style.marginTop = 5;
            button.style.marginBottom = 5;
            button.style.backgroundColor = new Color(0.28f, 0.18f, 0.10f);
            button.style.color = new Color(0.95f, 0.88f, 0.72f);
        }
    }
}
