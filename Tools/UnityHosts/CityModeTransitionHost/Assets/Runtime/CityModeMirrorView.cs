using UnityEngine;
using Victoria.CityMode.Integration;
using Victoria.CityMode.Presentation;

namespace Victoria.CityMode.TransitionHost
{
    public sealed class CityModeMirrorView : MonoBehaviour, ICityModePresentationView
    {
        public string OpenedCityId { get; private set; } = string.Empty;
        public long PresentedRevision { get; private set; } = -1;
        public int CloseCount { get; private set; }

        public void Open(CityLaunchContext context)
        {
            OpenedCityId = context.cityId;
        }

        public void Present(CitySnapshotEnvelope snapshot)
        {
            PresentedRevision = snapshot.stateRevision;
        }

        public void CompleteIntent(CityIntentReceipt receipt)
        {
        }

        public void Close()
        {
            CloseCount++;
        }
    }
}
