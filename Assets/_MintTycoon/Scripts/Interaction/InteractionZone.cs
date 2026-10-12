using System;
using System.Collections.Generic;
using MintTycoon.Inventory;
using UnityEngine;

namespace MintTycoon.Interaction
{
    /// <summary>Reports player inventory components entering and leaving a trigger zone.</summary>
    [RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
    public sealed class InteractionZone : MonoBehaviour
    {
        public event Action<PlayerInventory> PlayerEntered;
        public event Action<PlayerInventory> PlayerExited;

        private readonly Dictionary<Collider, PlayerInventory> colliderOwners = new Dictionary<Collider, PlayerInventory>();
        private readonly Dictionary<PlayerInventory, int> overlapCounts = new Dictionary<PlayerInventory, int>();
        private readonly List<PlayerInventory> exitingPlayers = new List<PlayerInventory>();
        private BoxCollider triggerCollider;
        private Rigidbody triggerBody;

        private void Awake()
        {
            ConfigurePhysics();
        }

        private void OnValidate()
        {
            ConfigurePhysics();
        }

        private void Start()
        {
            // Trigger callbacks can be missed for objects that are already overlapping when
            // play begins. Seed the overlap state once so a player spawning in a station zone
            // starts the same interaction as a player who walks into it.
            if (triggerCollider == null)
                return;

            Physics.SyncTransforms();
            Vector3 halfExtents = Vector3.Scale(triggerCollider.size * 0.5f, transform.lossyScale);
            Collider[] overlaps = Physics.OverlapBox(
                triggerCollider.transform.TransformPoint(triggerCollider.center),
                halfExtents,
                triggerCollider.transform.rotation,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore);

            foreach (Collider overlap in overlaps)
                RegisterPlayerCollider(overlap);
        }

        private void OnTriggerEnter(Collider other)
        {
            RegisterPlayerCollider(other);
        }

        private void RegisterPlayerCollider(Collider other)
        {
            if (other == null || colliderOwners.ContainsKey(other))
                return;

            PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
            if (inventory == null)
                return;

            colliderOwners.Add(other, inventory);
            overlapCounts.TryGetValue(inventory, out int overlaps);
            overlapCounts[inventory] = overlaps + 1;
            if (overlaps == 0)
                PlayerEntered?.Invoke(inventory);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other == null || !colliderOwners.TryGetValue(other, out PlayerInventory inventory))
                return;

            colliderOwners.Remove(other);
            if (inventory == null || !overlapCounts.TryGetValue(inventory, out int overlaps))
                return;

            if (overlaps <= 1)
            {
                overlapCounts.Remove(inventory);
                PlayerExited?.Invoke(inventory);
            }
            else
            {
                overlapCounts[inventory] = overlaps - 1;
            }
        }

        private void OnDisable()
        {
            exitingPlayers.Clear();
            foreach (PlayerInventory inventory in overlapCounts.Keys)
                exitingPlayers.Add(inventory);

            colliderOwners.Clear();
            overlapCounts.Clear();
            foreach (PlayerInventory inventory in exitingPlayers)
            {
                if (inventory != null)
                    PlayerExited?.Invoke(inventory);
            }

            exitingPlayers.Clear();
        }

        private void ConfigurePhysics()
        {
            if (triggerCollider == null)
                triggerCollider = GetComponent<BoxCollider>();
            if (triggerCollider != null)
                triggerCollider.isTrigger = true;

            if (triggerBody == null)
                triggerBody = GetComponent<Rigidbody>();
            if (triggerBody != null)
            {
                triggerBody.isKinematic = true;
                triggerBody.useGravity = false;
            }
        }
    }
}
