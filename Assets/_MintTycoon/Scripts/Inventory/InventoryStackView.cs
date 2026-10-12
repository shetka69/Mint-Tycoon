using System.Collections.Generic;
using MintTycoon.Configs;
using UnityEngine;

namespace MintTycoon.Inventory
{
    /// <summary>Renders a compact stack of the carried resource above the player.</summary>
    public sealed class InventoryStackView : MonoBehaviour
    {
        private const int MaxVisibleItems = 6;

        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private Transform stackAnchor;
        [SerializeField] private ResourceDefinition[] resourceDefinitions;
        [SerializeField, Min(0.05f)] private float horizontalSpacing = 0.28f;
        [SerializeField, Min(0.05f)] private float verticalSpacing = 0.24f;

        private readonly List<GameObject> spawnedItems = new List<GameObject>(MaxVisibleItems);

        private void OnEnable()
        {
            if (inventory == null)
                inventory = GetComponent<PlayerInventory>();

            if (inventory != null)
                inventory.ContentsChanged += Refresh;

            Refresh();
        }

        private void OnDisable()
        {
            if (inventory != null)
                inventory.ContentsChanged -= Refresh;

            ClearVisuals();
        }

        private void Refresh()
        {
            ClearVisuals();
            if (inventory == null || stackAnchor == null || !inventory.HasResource)
                return;

            GameObject prefab = FindPrefab(inventory.CurrentResource);
            if (prefab == null)
                return;

            int visibleCount = Mathf.Min(inventory.Count, MaxVisibleItems);
            for (int index = 0; index < visibleCount; index++)
            {
                int column = index % 2;
                int row = index / 2;
                Vector3 localPosition = new Vector3((column - 0.5f) * horizontalSpacing, row * verticalSpacing, 0f);
                GameObject item = Instantiate(prefab, stackAnchor);
                item.name = $"Carried {inventory.CurrentResource} {index + 1}";
                item.transform.localPosition = localPosition;
                item.transform.localRotation = Quaternion.identity;
                spawnedItems.Add(item);
            }
        }

        private GameObject FindPrefab(ResourceType type)
        {
            if (resourceDefinitions == null)
                return null;

            foreach (ResourceDefinition definition in resourceDefinitions)
            {
                if (definition != null && definition.Type == type)
                    return definition.StackVisualPrefab;
            }

            return null;
        }

        private void ClearVisuals()
        {
            for (int i = 0; i < spawnedItems.Count; i++)
            {
                if (spawnedItems[i] != null)
                    Destroy(spawnedItems[i]);
            }

            spawnedItems.Clear();
        }
    }
}
