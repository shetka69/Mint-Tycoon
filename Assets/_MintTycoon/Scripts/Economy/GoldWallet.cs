using System;
using UnityEngine;

namespace MintTycoon.Economy
{
    /// <summary>Stores gold separately from the player's carried-resource inventory.</summary>
    public sealed class GoldWallet : MonoBehaviour
    {
        [SerializeField, Min(0)] private int balance;

        public event Action BalanceChanged;

        public int Balance => Mathf.Max(0, balance);

        public void Add(int amount)
        {
            if (amount <= 0)
                return;

            balance = amount > int.MaxValue - balance ? int.MaxValue : balance + amount;
            BalanceChanged?.Invoke();
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || amount > balance)
                return false;

            balance -= amount;
            BalanceChanged?.Invoke();
            return true;
        }

        private void OnValidate()
        {
            balance = Mathf.Max(0, balance);
        }
    }
}
