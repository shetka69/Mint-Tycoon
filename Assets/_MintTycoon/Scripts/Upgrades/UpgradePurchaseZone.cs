using MintTycoon.Configs;
using MintTycoon.Economy;
using MintTycoon.Interaction;
using MintTycoon.Inventory;
using MintTycoon.Production;
using UnityEngine;

namespace MintTycoon.Upgrades
{
    /// <summary>Purchases an upgrade once when the player enters its marked zone.</summary>
    public sealed class UpgradePurchaseZone : MonoBehaviour
    {
        [SerializeField] private InteractionZone interactionZone;
        [SerializeField] private UpgradeDefinition upgrade;
        [SerializeField] private ResourceConveyor conveyor;
        [SerializeField] private TextMesh sign;
        [SerializeField, HideInInspector] private bool purchased;

        private void OnEnable()
        {
            if (interactionZone != null)
                interactionZone.PlayerEntered += HandlePlayerEntered;
            RefreshSign();
        }

        private void OnDisable()
        {
            if (interactionZone != null)
                interactionZone.PlayerEntered -= HandlePlayerEntered;
        }

        private void HandlePlayerEntered(PlayerInventory inventory)
        {
            if (purchased || inventory == null || upgrade == null || conveyor == null)
                return;

            GoldWallet wallet = inventory.GetComponent<GoldWallet>();
            if (wallet == null || !wallet.TrySpend(upgrade.GoldCost))
                return;

            purchased = true;
            conveyor.Unlock();
            RefreshSign();
        }

        private void RefreshSign()
        {
            if (sign == null)
                return;

            sign.text = purchased || (conveyor != null && conveyor.IsUnlocked)
                ? "AUTO INGOT BELT\nACTIVE"
                : $"AUTO INGOT BELT\n{(upgrade != null ? upgrade.GoldCost : 40)} GOLD";
        }
    }
}
