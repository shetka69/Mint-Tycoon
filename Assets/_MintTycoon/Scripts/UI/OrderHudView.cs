using MintTycoon.Configs;
using MintTycoon.Economy;
using MintTycoon.Orders;
using UnityEngine;
using UnityEngine.UI;

namespace MintTycoon.UI
{
    /// <summary>Shows the active wagon order and the player's separate gold balance.</summary>
    public sealed class OrderHudView : MonoBehaviour
    {
        [SerializeField] private OrderProgress orderProgress;
        [SerializeField] private GoldWallet wallet;
        [SerializeField] private Text orderTitle;
        [SerializeField] private Text orderDetails;
        [SerializeField] private Text goldBalance;
        [SerializeField] private TextMesh wagonOrderLabel;
        [SerializeField] private bool compactLayout;

        private void OnEnable()
        {
            BindEvents();
            Refresh();
        }

        private void Start()
        {
            BindEvents();
            Refresh();
        }

        private void BindEvents()
        {
            UnbindEvents();
            if (orderProgress != null)
                orderProgress.Changed += Refresh;
            if (wallet != null)
                wallet.BalanceChanged += Refresh;
        }

        private void OnDisable()
        {
            UnbindEvents();
        }

        private void UnbindEvents()
        {
            if (orderProgress != null)
                orderProgress.Changed -= Refresh;
            if (wallet != null)
                wallet.BalanceChanged -= Refresh;
        }

        private void Refresh()
        {
            SetText(goldBalance, compactLayout ? $"{wallet?.Balance ?? 0}" : $"GOLD  {wallet?.Balance ?? 0}");
            OrderDefinition order = orderProgress != null ? orderProgress.CurrentOrder : null;
            if (order == null || order.RequestedResource == null)
            {
                SetText(orderTitle, compactLayout ? string.Empty : "NO ROYAL ORDER");
                SetText(orderDetails, compactLayout ? "—" : string.Empty);
                if (wagonOrderLabel != null)
                    wagonOrderLabel.text = "AWAITING ORDER";
                return;
            }

            if (compactLayout)
            {
                SetText(orderTitle, string.Empty);
                SetText(orderDetails, orderProgress.IsCompleted ? "✓" : $"{orderProgress.LoadedAmount}/{order.RequestedAmount}");
                if (wagonOrderLabel != null)
                    wagonOrderLabel.text = orderProgress.IsCompleted ? "+" + order.GoldReward : $"{orderProgress.LoadedAmount}/{order.RequestedAmount}";
                return;
            }

            string resourceName = order.RequestedResource.DisplayName.ToUpperInvariant();
            string progress = $"{orderProgress.LoadedAmount}/{order.RequestedAmount}  {resourceName}";
            SetText(orderTitle, orderProgress.IsCompleted
                ? $"ORDER {orderProgress.OrderNumber}/{orderProgress.OrderCount} COMPLETE"
                : $"ROYAL ORDER {orderProgress.OrderNumber}/{orderProgress.OrderCount}");
            SetText(orderDetails, orderProgress.IsCompleted
                ? "WAGON DEPARTING"
                : $"{progress}     REWARD  {order.GoldReward} GOLD");
            SetText(goldBalance, $"GOLD  {wallet?.Balance ?? 0}");
            if (wagonOrderLabel != null)
            {
                wagonOrderLabel.text = orderProgress.IsCompleted
                    ? "ORDER COMPLETE\nWAGON DEPARTING"
                    : $"COINS  {orderProgress.LoadedAmount}/{order.RequestedAmount}\nREWARD  {order.GoldReward} GOLD";
            }
        }

        private static void SetText(Text label, string value)
        {
            if (label != null)
                label.text = value;
        }
    }
}
