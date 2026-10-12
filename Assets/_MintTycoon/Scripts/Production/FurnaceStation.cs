using System;
using System.Collections.Generic;
using MintTycoon.Configs;
using MintTycoon.Interaction;
using MintTycoon.Inventory;
using UnityEngine;

namespace MintTycoon.Production
{
    /// <summary>Consumes ore from a bounded input buffer and produces ingots into a bounded output buffer.</summary>
    public sealed class FurnaceStation : MonoBehaviour
    {
        [SerializeField] private ProductionRecipeDefinition recipe;
        [SerializeField] private InteractionZone inputZone;
        [SerializeField] private InteractionZone outputZone;
        [SerializeField, Min(1)] private int inputCapacity = 12;
        [SerializeField, Min(1)] private int outputCapacity = 12;
        [SerializeField, Min(0.01f)] private float transferIntervalSeconds = 0.12f;

        [SerializeField, HideInInspector] private int inputCount;
        [SerializeField, HideInInspector] private int outputCount;

        private readonly Dictionary<PlayerInventory, float> inputElapsed = new Dictionary<PlayerInventory, float>();
        private readonly Dictionary<PlayerInventory, float> outputElapsed = new Dictionary<PlayerInventory, float>();
        private readonly List<PlayerInventory> playersToProcess = new List<PlayerInventory>();
        private float productionElapsed;

        public event Action BuffersChanged;

        public int InputCount => inputCount;
        public int OutputCount => outputCount;
        public int InputCapacity => Mathf.Max(1, inputCapacity);
        public int OutputCapacity => Mathf.Max(1, outputCapacity);
        public ProductionRecipeDefinition Recipe => recipe;

        private void OnEnable()
        {
            if (inputZone != null)
            {
                inputZone.PlayerEntered += HandleInputEntered;
                inputZone.PlayerExited += HandleInputExited;
            }

            if (outputZone != null)
            {
                outputZone.PlayerEntered += HandleOutputEntered;
                outputZone.PlayerExited += HandleOutputExited;
            }
        }

        private void OnDisable()
        {
            if (inputZone != null)
            {
                inputZone.PlayerEntered -= HandleInputEntered;
                inputZone.PlayerExited -= HandleInputExited;
            }

            if (outputZone != null)
            {
                outputZone.PlayerEntered -= HandleOutputEntered;
                outputZone.PlayerExited -= HandleOutputExited;
            }

            inputElapsed.Clear();
            outputElapsed.Clear();
            playersToProcess.Clear();
        }

        private void Update()
        {
            TransferForPlayers(inputElapsed, true);
            TransferForPlayers(outputElapsed, false);
            ProcessProduction();
        }

        private void TransferForPlayers(Dictionary<PlayerInventory, float> timers, bool toInput)
        {
            if (recipe == null || recipe.Input == null || recipe.Output == null || timers.Count == 0)
                return;

            playersToProcess.Clear();
            foreach (PlayerInventory inventory in timers.Keys)
                playersToProcess.Add(inventory);

            foreach (PlayerInventory inventory in playersToProcess)
            {
                if (inventory == null)
                {
                    timers.Remove(inventory);
                    continue;
                }

                float elapsed = timers[inventory] + Time.deltaTime;
                while (elapsed >= transferIntervalSeconds && CanTransfer(inventory, toInput))
                {
                    if (toInput)
                    {
                        // Remove the carried unit before committing it to the buffer. Capacity was
                        // checked immediately above, and the main-thread buffer cannot reject it.
                        if (!inventory.TryRemove(recipe.Input.Type, 1))
                        {
                            elapsed = 0f;
                            break;
                        }

                        inputCount++;
                    }
                    else
                    {
                        // Reserve the output before notifying inventory listeners; restore it if
                        // the inventory changed unexpectedly during the notification.
                        outputCount--;
                        if (!inventory.TryAdd(recipe.Output.Type, 1))
                        {
                            outputCount++;
                            elapsed = 0f;
                            break;
                        }
                    }

                    elapsed -= transferIntervalSeconds;
                    BuffersChanged?.Invoke();
                }

                timers[inventory] = CanTransfer(inventory, toInput) ? elapsed : 0f;
            }
        }

        private bool CanTransfer(PlayerInventory inventory, bool toInput)
        {
            if (inventory == null || recipe == null || recipe.Input == null || recipe.Output == null)
                return false;

            if (toInput)
                return inputCount < InputCapacity
                    && inventory.HasResource
                    && inventory.CurrentResource == recipe.Input.Type;

            return outputCount > 0
                && inventory.FreeCapacity > 0
                && (!inventory.HasResource || inventory.CurrentResource == recipe.Output.Type);
        }

        private void ProcessProduction()
        {
            if (recipe == null || recipe.Input == null || recipe.Output == null
                || recipe.InputAmount <= 0 || recipe.OutputAmount <= 0 || recipe.CycleDurationSeconds <= 0f)
                return;

            if (inputCount >= recipe.InputAmount && outputCapacity - outputCount >= recipe.OutputAmount)
                productionElapsed += Time.deltaTime;

            while (productionElapsed >= recipe.CycleDurationSeconds
                   && inputCount >= recipe.InputAmount
                   && outputCapacity - outputCount >= recipe.OutputAmount)
            {
                productionElapsed -= recipe.CycleDurationSeconds;
                inputCount -= recipe.InputAmount;
                outputCount += recipe.OutputAmount;
                BuffersChanged?.Invoke();
            }
        }

        private void HandleInputEntered(PlayerInventory inventory) => AddPlayer(inputElapsed, inventory);
        private void HandleOutputEntered(PlayerInventory inventory) => AddPlayer(outputElapsed, inventory);
        private void HandleInputExited(PlayerInventory inventory) => RemovePlayer(inputElapsed, inventory);
        private void HandleOutputExited(PlayerInventory inventory) => RemovePlayer(outputElapsed, inventory);

        private static void AddPlayer(Dictionary<PlayerInventory, float> timers, PlayerInventory inventory)
        {
            if (inventory != null && !timers.ContainsKey(inventory))
                timers.Add(inventory, 0f);
        }

        private static void RemovePlayer(Dictionary<PlayerInventory, float> timers, PlayerInventory inventory)
        {
            if (inventory != null)
                timers.Remove(inventory);
        }

        private void OnValidate()
        {
            inputCapacity = Mathf.Max(1, inputCapacity);
            outputCapacity = Mathf.Max(1, outputCapacity);
            transferIntervalSeconds = Mathf.Max(0.01f, transferIntervalSeconds);
            inputCount = Mathf.Clamp(inputCount, 0, inputCapacity);
            outputCount = Mathf.Clamp(outputCount, 0, outputCapacity);
        }
    }
}
