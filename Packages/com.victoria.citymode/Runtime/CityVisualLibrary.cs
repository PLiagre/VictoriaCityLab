using UnityEngine;

namespace Victoria.CityMode
{
    [CreateAssetMenu(menuName = "Victoria/City Visual Library", fileName = "CityLabVisualLibrary")]
    public sealed class CityVisualLibrary : ScriptableObject
    {
        public GameObject townCentrePrefab;
        public GameObject stockpilePrefab;
        public GameObject[] housePrefabs;
        public GameObject villagerPrefab;
        public RuntimeAnimatorController villagerAnimatorController;
        public GameObject[] treePrefabs;
        public GameObject[] rockPrefabs;

        public bool HasDurableSlice => townCentrePrefab != null && stockpilePrefab != null &&
            housePrefabs != null && housePrefabs.Length > 0 && villagerPrefab != null;
    }

    public sealed class VillagerVisual : MonoBehaviour
    {
        Animator animator;
        Vector3 lastPosition;

        public void Initialize(RuntimeAnimatorController controller)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator != null && controller != null)
            {
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
            }
            lastPosition = transform.position;
        }

        public void Refresh(VillagerActivity activity, int carryingWood)
        {
            var delta = transform.position - lastPosition;
            var moving = delta.sqrMagnitude > 0.00001f;
            if (moving)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(delta.normalized, Vector3.up), 0.35f);

            if (animator != null)
            {
                animator.SetFloat("Speed", moving ? 1f : 0f);
                animator.SetBool("Working", activity == VillagerActivity.Building);
            }
            lastPosition = transform.position;
        }
    }
}
