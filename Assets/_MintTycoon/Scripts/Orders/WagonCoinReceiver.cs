using System.Collections.Generic;
using MintTycoon.Interaction;
using MintTycoon.Inventory;
using UnityEngine;

namespace MintTycoon.Orders
{
    /// <summary>Transfers only the active order's requested resource from players inside the wagon zone.</summary>
    public sealed class WagonCoinReceiver : MonoBehaviour
    {
        [SerializeField] private InteractionZone deliveryZone;
        [SerializeField] private OrderProgress orderProgress;
        [SerializeField, Min(0.01f)] private float transferIntervalSeconds = 0.12f;

        private readonly Dictionary<PlayerInventory, float> elapsedByPlayer = new Dictionary<PlayerInventory, float>();
        private readonly List<PlayerInventory> playersToProcess = new List<PlayerInventory>();

        private void OnEnable()
        {
            if (deliveryZone == null)
                return;

            deliveryZone.PlayerEntered += HandlePlayerEntered;
            deliveryZone.PlayerExited += HandlePlayerExited;
        }

        private void OnDisable()
        {
            if (deliveryZone != null)
            {
                deliveryZone.PlayerEntered -= HandlePlayerEntered;
                deliveryZone.PlayerExited -= HandlePlayerExited;
            }

            elapsedByPlayer.Clear();
            playersToProcess.Clear();
        }

        private void Update()
        {
            if (orderProgress == null || !orderProgress.CanAccept || elapsedByPlayer.Count == 0)
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

                float elapsed = elapsedByPlayer[inventory] + Time.deltaTime;
                while (elapsed >= transferIntervalSeconds && orderProgress.TryDeliverOne(inventory))
                    elapsed -= transferIntervalSeconds;

                elapsedByPlayer[inventory] = orderProgress.CanAccept ? elapsed : 0f;
            }
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
            transferIntervalSeconds = Mathf.Max(0.01f, transferIntervalSeconds);
        }
    }
}
