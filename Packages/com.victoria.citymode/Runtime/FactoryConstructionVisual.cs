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
}
