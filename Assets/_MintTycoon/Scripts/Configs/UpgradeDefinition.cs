using UnityEngine;

namespace MintTycoon.Configs
{
    [CreateAssetMenu(menuName = "Mint Tycoon/Upgrades/Upgrade", fileName = "Upgrade")]
    public sealed class UpgradeDefinition : ScriptableObject
    {
        [SerializeField] private string upgradeId;
        [SerializeField] private string displayName;
        [SerializeField, Min(0)] private int goldCost;
        [SerializeField, TextArea] private string description;

        public string UpgradeId => upgradeId;
        public string DisplayName => displayName;
        public int GoldCost => goldCost;
        public string Description => description;
    }
}
