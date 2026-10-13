using MintTycoon.Production;
using UnityEngine;

namespace MintTycoon.Presentation
{
    /// <summary>Shows the artist's locked and unlocked pad variants from real upgrade state.</summary>
    public sealed class UpgradeVisualView : MonoBehaviour
    {
        [SerializeField] private ResourceConveyor conveyor;
        [SerializeField] private GameObject lockedVisual;
        [SerializeField] private GameObject unlockedVisual;
        private void OnEnable() { Bind(); Refresh(); }
        private void Start() { Bind(); Refresh(); }
        private void Bind() { if (conveyor != null) { conveyor.UnlockChanged -= Refresh; conveyor.UnlockChanged += Refresh; } }
        private void OnDisable() { if (conveyor != null) conveyor.UnlockChanged -= Refresh; }
        private void Refresh()
        {
            bool unlocked = conveyor != null && conveyor.IsUnlocked;
            if (lockedVisual != null) lockedVisual.SetActive(!unlocked);
            if (unlockedVisual != null) unlockedVisual.SetActive(unlocked);
        }
    }
}
