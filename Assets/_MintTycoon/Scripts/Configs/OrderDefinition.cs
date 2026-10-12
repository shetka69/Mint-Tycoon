using UnityEngine;

namespace MintTycoon.Configs
{
    [CreateAssetMenu(menuName = "Mint Tycoon/Orders/Order", fileName = "Order")]
    public sealed class OrderDefinition : ScriptableObject
    {
        [SerializeField] private ResourceDefinition requestedResource;
        [SerializeField, Min(1)] private int requestedAmount = 1;
        [SerializeField, Min(0)] private int goldReward;

        public ResourceDefinition RequestedResource => requestedResource;
        public int RequestedAmount => requestedAmount;
        public int GoldReward => goldReward;
    }
}
