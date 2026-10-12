using UnityEngine;

namespace MintTycoon.Configs
{
    [CreateAssetMenu(menuName = "Mint Tycoon/Production/Recipe", fileName = "Recipe")]
    public sealed class ProductionRecipeDefinition : ScriptableObject
    {
        [SerializeField] private ResourceDefinition input;
        [SerializeField, Min(1)] private int inputAmount = 1;
        [SerializeField] private ResourceDefinition output;
        [SerializeField, Min(1)] private int outputAmount = 1;
        [SerializeField, Min(0.01f)] private float cycleDurationSeconds = 1f;

        public ResourceDefinition Input => input;
        public int InputAmount => inputAmount;
        public ResourceDefinition Output => output;
        public int OutputAmount => outputAmount;
        public float CycleDurationSeconds => cycleDurationSeconds;
    }
}
