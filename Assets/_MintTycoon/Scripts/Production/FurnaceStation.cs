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
                        // Remove the carried unit before committing it to the buffer. The buffer
                        // cannot reject it after the capacity check because all changes run on
                        // Unity's main thread.
                        if (!inventory.TryRemove(recipe.Input.Type, 1))
                        {
                            elapsed = 0f;
                            break;
                        }

                        inp