using System;
using UnityEngine;

namespace MintTycoon.Inventory
{
    /// <summary>Small single-resource inventory carried by the player.</summary>
    public sealed class PlayerInventory : MonoBehaviour
    {
        [SerializeField, Min(1)] private int capacity = 12;
        [SerializeField] private ResourceType resourceType;
        [SerializeField, Min(0)] private int count;

        public event Action ContentsChanged;

        public int Capacity => Mathf.Max(1, capacity);
        public int Count => Mathf.Clamp(count, 0, Capacity);
        public int FreeCapacity => Capacity - Count;
        public bool HasResource => Count > 0;
        public ResourceType CurrentResource => resourceType;

        /// <summary>Adds the whole amount, or changes nothing when it will not fit.</summary>
        public bool TryAdd(ResourceType type, int amount)
        {
            if (amount <= 0 || amount > FreeCapacity || (HasResource && resourceType != type))
                return false;

            resourceType = type;
            count += amount;
            ContentsChanged?.Invoke();
            return true;
        }

        /// <summary>Removes the whole amount, or changes nothing when unavailable.</summary>
        public bool TryRemove(ResourceType type, int amount)
        {
            if (amount <= 0 || !HasResource || resourceType != type || amount > Count)
                return false;

            count -= amount;
            if (count == 0)
                resourceType = default;

            ContentsChanged?.Invoke();
            return true;
        }

        public void Clear()
        {
            if (!HasResource)
                return;

            count = 0;
            resourceType = default;
            ContentsChanged?.Invoke();
        }

        private void OnValidate()
        {
            capacity = Mathf.Max(1, capacity);
            count = Mathf.Clamp(count, 0, capacity);
            if (count == 0)
                resourceType = default;
        }
    }
}
