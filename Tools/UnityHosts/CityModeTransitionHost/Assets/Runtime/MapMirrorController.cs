using UnityEngine;
using Victoria.CityMode.Integration;

namespace Victoria.CityMode.TransitionHost
{
    /// <summary>
    /// Minimal stand-in for ForgeHistory map selection and viewport ownership.
    /// It carries identifiers only; no world simulation lives in this fixture.
    /// </summary>
    public sealed class MapMirrorController : MonoBehaviour
    {
        public string SelectedMapCellId { get; private set; } = string.Empty;
        public string RestoredViewId { get; private set; } = string.Empty;
        public string RestoredViewStateJson { get; private set; } = string.Empty;

        public CityLaunchContext SelectCity(
            string sessionId,
            string cityId,
            string mapCellId,
            string viewportJson)
        {
            SelectedMapCellId = mapCellId;
            return new CityLaunchContext
            {
                sessionId = sessionId,
                cityId = cityId,
                mapCellId = mapCellId,
                worldSeed = 42195,
                worldTick = 1400,
                stateRevision = 17,
                timePolicy = CityWorldTimePolicy.PauseWorld,
                worldTimeScalePermille = 0,
                returnViewId = "map:mirror",
                returnViewStateJson = viewportJson
            };
        }

        public void Restore(CityLaunchContext context)
        {
            SelectedMapCellId = context.mapCellId;
            RestoredViewId = context.returnViewId;
            RestoredViewStateJson = context.returnViewStateJson;
        }
    }
}
