using MintTycoon.Configs;
using MintTycoon.Economy;
using MintTycoon.Inventory;
using MintTycoon.Orders;
using MintTycoon.Production;
using UnityEngine;
using UnityEngine.UI;

namespace MintTycoon.Tutorial
{
    /// <summary>Brief hints and a world marker guide the player without covering the playfield.</summary>
    public sealed class ContextualTutorialView : MonoBehaviour
    {
        private const int FinalStep = 7;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private BufferedProductionStation furnace;
        [SerializeField] private BufferedProductionStation press;
        [SerializeField] private OrderProgress orderProgress;
        [SerializeField] private GoldWallet wallet;
        [SerializeField] private ResourceConveyor conveyor;
        [SerializeField] private UpgradeDefinition upgrade;
        [SerializeField] private Text stepLabel;
        [SerializeField] private Text instructionLabel;
        [SerializeField] private CanvasGroup hintGroup;
        [SerializeField] private Transform guideMarker;
        [SerializeField] private Transform[] stepTargets;
        [SerializeField] private Transform deliveryTarget;
        [SerializeField, Min(0.5f)] private float hintDuration = 3.5f;
        [SerializeField] private float markerHeight = 0.29f;
        [SerializeField] private float upgradeMarkerHeight = 0.29f;

        private int currentStep;
        private float hintRemaining;
        private Transform activeTarget;
        private string lastInstruction;

        private void OnEnable() { BindEvents(); Refresh(); }
        private void Start() { BindEvents(); Refresh(); }
        private void OnDisable() { UnbindEvents(); }

        private void BindEvents()
        {
            UnbindEvents();
            if (inventory != null) inventory.ContentsChanged += Refresh;
            if (furnace != null) furnace.BuffersChanged += Refresh;
            if (press != null) press.BuffersChanged += Refresh;
            if (orderProgress != null) orderProgress.Changed += Refresh;
            if (wallet != null) wallet.BalanceChanged += Refresh;
            if (conveyor != null) conveyor.UnlockChanged += Refresh;
        }

        private void UnbindEvents()
        {
            if (inventory != null) inventory.ContentsChanged -= Refresh;
            if (furnace != null) furnace.BuffersChanged -= Refresh;
            if (press != null) press.BuffersChanged -= Refresh;
            if (orderProgress != null) orderProgress.Changed -= Refresh;
            if (wallet != null) wallet.BalanceChanged -= Refresh;
            if (conveyor != null) conveyor.UnlockChanged -= Refresh;
        }

        private void Update()
        {
            hintRemaining = Mathf.Max(0f, hintRemaining - Time.deltaTime);
            if (hintGroup != null)
                hintGroup.alpha = Mathf.MoveTowards(hintGroup.alpha, hintRemaining > 0f ? 1f : 0f, Time.deltaTime * 4f);

            if (guideMarker == null || activeTarget == null || currentStep == FinalStep)
                return;
            // Pads have elevated trigger centers; the guide stays on the ground.
            Vector3 position = activeTarget.position;
            guideMarker.position = new Vector3(position.x, currentStep == 6 ? upgradeMarkerHeight : markerHeight, position.z);
            guideMarker.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 3.5f) * 0.07f);
        }

        private void Refresh()
        {
            if (conveyor != null && conveyor.IsUnlocked)
                currentStep = FinalStep;
            else
                while (currentStep < FinalStep && IsStepComplete(currentStep)) currentStep++;

            string instruction = GetInstruction();
            if (stepLabel != null) stepLabel.text = string.Empty;
            if (instructionLabel != null) instructionLabel.text = instruction;
            if (instruction != lastInstruction)
            {
                lastInstruction = instruction;
                hintRemaining = hintDuration;
            }
            activeTarget = GetTarget();
            if (guideMarker != null) guideMarker.gameObject.SetActive(activeTarget != null && currentStep < FinalStep);
        }

        private bool IsStepComplete(int step)
        {
            switch (step)
            {
                case 0: return inventory != null && inventory.HasResource && inventory.CurrentResource == ResourceType.CopperOre;
                case 1: return furnace != null && (furnace.InputCount > 0 || furnace.OutputCount > 0);
                case 2: return inventory != null && inventory.HasResource && inventory.CurrentResource == ResourceType.CopperIngot;
                case 3: return press != null && (press.InputCount > 0 || press.OutputCount > 0);
                case 4: return orderProgress != null && orderProgress.LoadedAmount > 0;
                case 5: return wallet != null && wallet.Balance >= (upgrade != null ? upgrade.GoldCost : 40);
                case 6: return conveyor != null && conveyor.IsUnlocked;
                default: return true;
            }
        }

        private Transform GetTarget()
        {
            if (currentStep == FinalStep) return null;
            if (currentStep == 4 && CarryingCoins()) return deliveryTarget;
            if (currentStep == 5)
            {
                if (CarryingCoins()) return deliveryTarget;
                if (inventory != null && inventory.HasResource)
                    return TargetAt(inventory.CurrentResource == ResourceType.CopperOre ? 1 : 3);
                if (press != null && press.OutputCount > 0) return TargetAt(4);
                if (furnace != null && furnace.OutputCount > 0) return TargetAt(2);
                return TargetAt(0);
            }
            return TargetAt(currentStep);
        }

        private Transform TargetAt(int index) => stepTargets != null && index >= 0 && index < stepTargets.Length ? stepTargets[index] : null;
        private bool CarryingCoins() => inventory != null && inventory.HasResource && inventory.CurrentResource == ResourceType.CopperCoin;

        private string GetInstruction()
        {
            switch (currentStep)
            {
                case 0: return "Stand here to mine";
                case 1: return "Ore → furnace";
                case 2: return "Pick up the ingots";
                case 3: return "Ingots → mint press";
                case 4: return CarryingCoins() ? "Coins → wagon" : "Pick up the coins";
                case 5: return "Finish the order";
                case 6: return "Unlock the conveyor";
                default: return "Conveyor unlocked!";
            }
        }
    }
}
