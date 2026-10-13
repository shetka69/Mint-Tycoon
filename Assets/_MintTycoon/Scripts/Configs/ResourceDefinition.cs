using UnityEngine;

namespace MintTycoon.Configs
{
    [CreateAssetMenu(menuName = "Mint Tycoon/Resources/Resource Definition", fileName = "Resource")]
    public sealed class ResourceDefinition : ScriptableObject
    {
        [SerializeField] private ResourceType type;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject stackVisualPrefab;
        [SerializeField] private GameObject[] stackVisualVariants;
        [SerializeField, Min(0f)] private float stackRowSpacing;

        public ResourceType Type => type;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public GameObject StackVisualPrefab => stackVisualPrefab;
        public float StackRowSpacing => Mathf.Max(0f, stackRowSpacing);

        public GameObject GetStackVisual(int index)
        {
            if (stackVisualVariants != null && stackVisualVariants.Length > 0)
            {
                GameObject variant = stackVisualVariants[Mathf.Abs(index % stackVisualVariants.Length)];
                if (variant != null) return variant;
            }
            return stackVisualPrefab;
        }
    }
}
