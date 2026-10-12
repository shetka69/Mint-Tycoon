using System.Collections.Generic;
using MintTycoon.Configs;
using MintTycoon.Interaction;
using MintTycoon.Inventory;
using UnityEngine;

namespace MintTycoon.Resources
{
    /// <summary>Continuously adds ore to players while they remain inside the mine zone.</summary>
    public sealed class MiningStation : MonoBehaviour
    {
        [SerializeField] private ResourceDefinition outputResource;
        [SerializeField] private InteractionZone collectionZone;
        [SerializeField, Min(0.01f)] private float secondsPerUnit = 0.4f;

        private readonly Dictionary<PlayerInventory, float> elapsedByPlayer = new Dictionary<PlayerInventory, float>();
        private readonly List<PlayerInventory> playersToProcess = new List<PlayerInventory>();

        public bool IsActive => elapsedByPlayer.Count > 0;

        private void OnEnable()
        {
            if (collectionZone == null)
                return;

            collectionZone.PlayerEntered += HandlePlayerEntered;
            collectionZone.PlayerExited += HandlePlayerExited;
        }

        private void OnDisable()
        {
            if (collectionZone != null)
            {
                collectionZone.PlayerEntered -= HandlePlayerEntered;
                collectionZone.PlayerExited -= HandlePlayerExited;
            }

            elapsedByPlayer.Clear();
            playersToProcess.Clear();
        }

        private void Update()
        {
            if (outputResource == null || secondsPerUnit <= 0f || elapsedByPlayer.Count == 0)
                return;

            playersToProcess.Clear();
            foreach (PlayerInventory inventory in elapsedByPlayer.Keys)
                playersToProcess.Add(inventory);

            foreach (PlayerInventory inventory in playersToProcess)
            {
                if (inventory == null)
                {
                    elapsedByPlayer.Remove(inventory);
                    continue;
                }

                if (!CanCollect(inventory))
                {
                    elapsedByPlayer[inventory] = 0f;
                    continue;
                }

                float elapsed = elapsedByPlayer[inventory] + Time.deltaTime;
                while (elapsed >= secondsPerUnit && CanCollect(inventory))
                {
                    if (!inventory.TryAdd(outputResource.Type, 1))
                    {
                        elapsed = 0f;
                        break;
                    }

                    elapsed -= secondsPerUnit;
                }

                elapsedByPlayer[inventory] = CanCollect(inventory) ? elapsed : 0f;
            }
        }

        private bool CanCollect(PlayerInventory inventory)
        {
            return inventory != null
                && inventory.FreeCapacity > 0
                && (!inventory.HasResource || inventory.CurrentResource == outputResource.Type);
        }

        private void HandlePlayerEntered(PlayerInventory inventory)
        {
            if (inventory != null && !elapsedByPlayer.ContainsKey(inventory))
                elapsedByPlayer.Add(inventory, 0f);
        }

        private void HandlePlayerExited(PlayerInventory inventory)
        {
            if (inventory != null)
                elapsedByPlayer.Remove(inventory);
        }

        private void OnValidate()
        {
            secondsPerUnit = Mathf.Max(0.01f, secondsPerUnit);
        }
    }
}
