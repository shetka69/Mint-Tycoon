using MintTycoon.Inventory;
using MintTycoon.Orders;
using MintTycoon.Production;
using UnityEngine;

namespace MintTycoon.Presentation
{
    /// <summary>Plays short, rate-limited sounds only for successful resource and reward events.</summary>
    public sealed class PlayerActionFeedback : MonoBehaviour
    {
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private OrderProgress order;
        [SerializeField] private ResourceConveyor conveyor;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip oreSound;
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private AudioClip rewardSound;
        [SerializeField] private AudioClip unlockSound;
        [SerializeField] private ParticleSystem celebration;
        private int previousCount;
        private float nextPickupTime;
        private void OnEnable() { Bind(); }
        private void Start() { Bind(); }
        private void Bind()
        {
            Unbind();
            previousCount = inventory != null ? inventory.Count : 0;
            if (inventory != null) inventory.ContentsChanged += HandleInventory;
            if (order != null) order.OrderCompleted += HandleReward;
            if (conveyor != null) conveyor.UnlockChanged += HandleUnlock;
        }
        private void OnDisable() => Unbind();
        private void Unbind()
        {
            if (inventory != null) inventory.ContentsChanged -= HandleInventory;
            if (order != null) order.OrderCompleted -= HandleReward;
            if (conveyor != null) conveyor.UnlockChanged -= HandleUnlock;
        }
        private void HandleInventory()
        {
            if (inventory.Count > previousCount && Time.time >= nextPickupTime)
            {
                Play(inventory.CurrentResource == ResourceType.CopperOre ? oreSound : pickupSound);
                nextPickupTime = Time.time + 0.12f;
            }
            previousCount = inventory.Count;
        }
        private void HandleReward() { Play(rewardSound); if (celebration != null) celebration.Emit(22); }
        private void HandleUnlock() { if (conveyor.IsUnlocked) { Play(unlockSound); if (celebration != null) celebration.Emit(16); } }
        private void Play(AudioClip clip) { if (audioSource != null && clip != null) audioSource.PlayOneShot(clip); }
    }
}
