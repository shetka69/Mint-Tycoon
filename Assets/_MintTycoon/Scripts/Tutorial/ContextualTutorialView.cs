using MintTycoon.Economy;
using MintTycoon.Inventory;
using MintTycoon.Orders;
using MintTycoon.Production;
using UnityEngine;
using UnityEngine.UI;

namespace MintTycoon.Tutorial
{
    /// <summary>Shows the next useful gameplay hint based on actual player progress.</summary>
    public sealed class ContextualTutorialView : MonoBehaviour
    {
        private const int FinalStep = 7;

        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private BufferedProductionStation furnace;
        [SerializeField] private BufferedProductionStation press;
        [SerializeField] private OrderProgress orderProgress;
        [SerializeField] private GoldWallet wallet;
        [SerializeField] private ResourceConveyor conveyor;
        [SerializeField] private Text stepLabel;
        [SerializeField] private Text instructionLabel;

        private int currentStep;

        private void OnEnable()
        {
            BindEvents();
            Refresh();
        }

        private void Start()
        {
            // Enter Play Mode can keep the scene and domain loaded, so references assigned
            // by an editor setup tool may not have been present during the earlier OnEnable.
            BindEvents();
            Refresh();
        }

        private void BindEvents()
        {
            UnbindEvents();
            if (inventory != null)
                inventory.ContentsChanged += Refresh;
            if (furnace != null)
                furnace.BuffersChanged += Refresh;
            if (press != null)
                press.BuffersChanged += Refresh;
            if (orderProgress != null)
                orderProgress.Changed += Refresh;
            if (wallet != null)
                wallet.BalanceChanged += Refresh;
            if (conveyor != null)
                conveyor.UnlockChanged += Refresh;

        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        private void UnbindEvents()
        {
            if (inventory != null)
                inventory.ContentsChanged -= Refresh;
            if (furnace != null)
                furnace.BuffersChanged -= Refresh;
            if (press != null)
                press.BuffersChanged -= Refresh;
            if (orderProgress != null)
                orderProgress.Changed -= Refresh;
            if (wallet != null)
                wallet.BalanceChanged -= Refresh;
            if (conveyor != null)
                conveyor.UnlockChanged -= Refresh;
        }

        private void Refresh()
        {
            while (currentStep < FinalStep && IsStepComplete(currentStep))
                currentStep++;

            if (stepLabel != null)
                stepLabel.text = currentStep == FinalStep
                    ? "FIELD GUIDE  ·  COMPLETE"
                    : $"FIELD GUIDE  ·  {currentStep + 1}/7";

            if (instructionLabel != null)
                instructionLabel.text = GetInstruction(currentStep);
        }

        private bool IsStepComplete(int step)
        {
            switch (step)
            {
                case 0:
                    return inventory != null && inventory.HasResource && inventory.CurrentResource == ResourceType.CopperOre;
                case 1:
                    return furnace != null && furnace.InputCount > 0;
                case 2:
                    return inventory != null && inventory.HasResource && inventory.CurrentResource == ResourceType.CopperIngot;
                case 3:
                    return press != null && press.InputCount > 0;
                case 4:
                    return orderProgress != null && orderProgress.LoadedAmount > 0;
                case 5:
                    return wallet != null && wallet.Balance >= 40;
                case 6:
                    return conveyor != null && conveyor.IsUnlocked;
                default:
                    return true;
            }
        }

        private static string GetInstruction(int step)
        {
            switch (step)
            {
                case 0: return "Stand on the amber mine pad. Copper ore is collected automatically.";
                case 1: return "Carry copper ore to the furnace's amber input pad.";
                case 2: return "The furnace smelts on its own. Pick up ingots at its teal output pad.";
                case 3: return "Carry ingots to the mint press's amber input pad.";
                case 4: return "The press makes two coins per ingot. Take coins to the wagon's delivery pad.";
                case 5: return "Finish the royal order to earn gold. The conveyor upgrade costs 40.";
                case 6: return "Step on the gold upgrade pad to buy the automatic ingot conveyor.";
                default: return "The conveyor is active. It feeds ingots to the press when its buffer has room.";
            }
        }
    }
}
