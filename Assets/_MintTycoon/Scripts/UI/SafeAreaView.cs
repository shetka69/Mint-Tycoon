using UnityEngine;

namespace MintTycoon.UI
{
    /// <summary>Keeps the compact HUD and joystick clear of mobile display cutouts.</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaView : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void Awake() => rectTransform = GetComponent<RectTransform>();
        private void OnEnable() => Apply();
        private void Update()
        {
            if (lastSafeArea != Screen.safeArea || lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
                Apply();
        }

        private void Apply()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (Screen.width <= 0 || Screen.height <= 0) return;
            lastSafeArea = Screen.safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            rectTransform.anchorMin = new Vector2(lastSafeArea.xMin / Screen.width, lastSafeArea.yMin / Screen.height);
            rectTransform.anchorMax = new Vector2(lastSafeArea.xMax / Screen.width, lastSafeArea.yMax / Screen.height);
            rectTransform.offsetMin = rectTransform.offsetMax = Vector2.zero;
        }
    }
}
