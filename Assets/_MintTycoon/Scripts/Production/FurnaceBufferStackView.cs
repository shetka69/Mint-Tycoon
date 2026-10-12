using System.Collections.Generic;
using MintTycoon.Configs;
using UnityEngine;

namespace MintTycoon.Production
{
    /// <summary>Shows the furnace's stored ore and ingots in the two marked buffer bays.</summary>
    public sealed class FurnaceBufferStackView : MonoBehaviour
    {
        private const int MaxVisibleItems = 6;

        [SerializeField] private FurnaceStation station;
        [SerializeField] private ResourceDefinition inputResource;
        [SerializeField] private ResourceDefinition outputResource;
        [SerializeField] private Transform inputAnchor;
        [SerializeField] private Transform outputAnchor;
        [SerializeField] private TextMesh inputCountLabel;
        [SerializeField] private TextMesh outputCountLabel;

        private readonly List<GameObject> spawnedItems = new List<GameObject>(MaxVisibleItems * 2);

        private void OnEnable()
        {
            if (station != null)
                station.BuffersChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (station != null)
                station.BuffersChanged -= Refresh;
            ClearVisuals();
        }

        private void Refresh()
        {
            ClearVisuals();
            if (station == null)
                return;

            if (inputCountLabel != null)
                inputCountLabel.text = $"ORE  {station.InputCount}/{station.InputCapacity}";
            if (outputCountLabel != null)
                outputCountLabel.text = $"INGOTS  {station.OutputCount}/{station.OutputCapacity}";

            SpawnStack(inputResource, inputAnchor, station.InputCount, "Stored ore");
            SpawnStack(outputResource, outputAnchor, station.OutputCount, "Stored ingot");
        }

        private void SpawnStack(ResourceDefinition definition, Transform anchor, int count, string namePrefix)
        {
            if (definition == null || definition.StackVisualPrefab == null || anchor == null)
                return;

            int visibleCount = Mathf.Min(count, MaxVisibleItems);
            for (int index = 0; index < visibleCount; index++)
            {
                int column = index % 3;
                int row = index / 3;
                GameObject item = Instantiate(definition.StackVisualPrefab, anchor);
                item.name = $"{namePrefix} {index + 1}";
                item.transform.localPosition = new Vector3((column - 1) * 0.32f, row * 0.2f, 0f);
                item.transform.localRotation = Quaternion.identity;
                spawnedItems.Add(item);
            }
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
