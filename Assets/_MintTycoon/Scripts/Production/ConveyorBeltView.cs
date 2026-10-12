using UnityEngine;

namespace MintTycoon.Production
{
    /// <summary>Changes the conveyor's appearance when its upgrade is purchased.</summary>
    public sealed class ConveyorBeltView : MonoBehaviour
    {
        [SerializeField] private ResourceConveyor conveyor;
        [SerializeField] private Renderer[] beltRenderers;
        [SerializeField] private Material lockedMaterial;
        [SerializeField] private Material unlockedMaterial;

        private void OnEnable()
        {
            if (conveyor != null)
                conveyor.UnlockChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (conveyor != null)
                conveyor.UnlockChanged -= Refresh;
        }

        private void Refresh()
        {
            Material material = conveyor != null && conveyor.IsUnlocked ? unlockedMaterial : lockedMaterial;
            if (material == null || beltRenderers == null)
                return;

            foreach (Renderer beltRenderer in beltRenderers)
            {
                if (beltRenderer != null)
                    beltRenderer.sharedMaterial = material;
            }
        }
    }
}
