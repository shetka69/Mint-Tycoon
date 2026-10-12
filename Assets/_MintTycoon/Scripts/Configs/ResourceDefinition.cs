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

        public ResourceType Type => type;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public GameObject StackVisualPrefab => stackVisualPrefab;
    }
}
