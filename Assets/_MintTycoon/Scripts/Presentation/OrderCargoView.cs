using MintTycoon.Orders;
using UnityEngine;

namespace MintTycoon.Presentation
{
    /// <summary>Fills the wagon with authored coin stacks as the actual order is loaded.</summary>
    public sealed class OrderCargoView : MonoBehaviour
    {
        [SerializeField] private OrderProgress order;
        [SerializeField] private GameObject[] cargoStages;
        [SerializeField] private int[] loadedThresholds;
        private void OnEnable() { Bind(); Refresh(); }
        private void Start() { Bind(); Refresh(); }
        private void Bind() { if (order != null) { order.Changed -= Refresh; order.Changed += Refresh; } }
        private void OnDisable() { if (order != null) order.Changed -= Refresh; }
        private void Refresh()
        {
            if (cargoStages == null || loadedThresholds == null) return;
            int selected = -1;
            int count = order != null ? order.LoadedAmount : 0;
            int length = Mathf.Min(cargoStages.Length, loadedThresholds.Length);
            for (int index = 0; index < length; index++)
                if (count >= loadedThresholds[index]) selected = index;
            for (int index = 0; index < cargoStages.Length; index++)
                if (cargoStages[index] != null) cargoStages[index].SetActive(index == selected);
        }
    }
}
