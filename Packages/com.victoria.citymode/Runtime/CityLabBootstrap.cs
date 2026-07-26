using UnityEngine;

namespace Victoria.CityMode
{
    public static class CityLabBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsureRuntime()
        {
            if (Object.FindFirstObjectByType<CityLabGame>() != null)
                return;
            var root = new GameObject("CityLab Runtime");
            root.AddComponent<CityLabGame>();
        }
    }
}

