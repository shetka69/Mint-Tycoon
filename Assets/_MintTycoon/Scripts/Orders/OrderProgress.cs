using System;
using MintTycoon.Configs;
using MintTycoon.Economy;
using MintTycoon.Inventory;
using UnityEngine;

namespace MintTycoon.Orders
{
    /// <summary>Tracks the active royal wagon order and awards its gold exactly once.</summary>
    public sealed class OrderProgress : MonoBehaviour
    {
        [SerializeField] private OrderDefinition[] orders;
        [SerializeField] private GoldWallet wallet;
        [SerializeField, HideInInspector] private int orderIndex;
        [SerializeField, HideInInspector] private int loadedAmount;
        [SerializeField, HideInInspector] private bool completed;

        public event Action Changed;
        public event Action OrderCompleted;

        public OrderDefinition CurrentOrder => orders != null && orders.Length > 0
            ? orders[Mathf.Clamp(orderIndex, 0, orders.Length - 1)]
            : null;
        public int LoadedAmount => loadedAmount;
        public int OrderNumber => orders == null || orders.Length == 0 ? 0 : Mathf.Clamp(orderIndex, 0, orders.Length - 1) + 1;
        public int OrderCount => orders?.Length ?? 0;
        public bool IsCompleted => completed;
        public bool CanAccept => !completed && CurrentOrder != null && CurrentOrder.RequestedResource != null;

        public bool TryDeliverOne(PlayerInventory inventory)
        {
            OrderDefinition order = CurrentOrder;
            if (!CanAccept || inventory == null || order == null || order.RequestedResource == null)
                return false;

            if (!inventory.TryRemove(order.RequestedResource.Type, 1))
                return false;

            loadedAmount++;
            if (loadedAmount >= order.RequestedAmount)
            {
                loadedAmount = order.RequestedAmount;
                completed = true;
                if (wallet != null)
                    wallet.Add(order.GoldReward);
                Changed?.Invoke();
                OrderCompleted?.Invoke();
                return true;
            }

            Changed?.Invoke();
            return true;
        }

        public bool AdvanceToNextOrder()
        {
            if (!completed || orders == null || orders.Length == 0)
                return false;

            orderIndex = (orderIndex + 1) % orders.Length;
            loadedAmount = 0;
            completed = false;
            Changed?.Invoke();
            return true;
        }

        private void OnValidate()
        {
            if (orders == null || orders.Length == 0)
            {
                orderIndex = 0;
                loadedAmount = 0;
                completed = false;
                return;
            }

            orderIndex = Mathf.Clamp(orderIndex, 0, orders.Length - 1);
            OrderDefinition order = CurrentOrder;
            loadedAmount = Mathf.Clamp(loadedAmount, 0, order == null ? 0 : order.RequestedAmount);
        }
    }
}
