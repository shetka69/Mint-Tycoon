using MintTycoon.Player;
using MintTycoon.UI;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MintTycoon.Editor
{
    /// <summary>Builds one invisible full-screen input surface with touch-only visuals.</summary>
    public static class FloatingJoystickSetup
    {
        public static void Build(Canvas canvas, PlayerMovement movement)
        {
            foreach (VirtualJoystick previous in canvas.GetComponentsInChildren<VirtualJoystick>(true))
                Object.DestroyImmediate(previous.gameObject);

            GameObject root = new GameObject("Floating Joystick", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VirtualJoystick));
            RectTransform surface = root.GetComponent<RectTransform>();
            surface.SetParent(canvas.transform, false);
            surface.SetAsFirstSibling();
            surface.anchorMin = Vector2.zero; surface.anchorMax = Vector2.one;
            surface.offsetMin = surface.offsetMax = Vector2.zero;
            surface.pivot = new Vector2(0.5f, 0.5f);
            Image input = root.GetComponent<Image>();
            input.color = Color.clear; input.raycastTarget = true;
            root.GetComponent<CanvasRenderer>().cullTransparentMesh = false;

            RectTransform track = Circle("Joystick Background", surface, 190f, new Color(1f, 1f, 1f, 0.12f));
            RectTransform knob = Circle("Joystick Handle", track, 72f, new Color(1f, 1f, 1f, 0.30f));
            VirtualJoystick joystick = root.GetComponent<VirtualJoystick>();
            joystick.Configure(track, knob);
            movement.SetJoystick(joystick);
        }

        private static RectTransform Circle(string name, Transform parent, float size, Color colour)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(CircleGraphic));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            CircleGraphic graphic = go.GetComponent<CircleGraphic>();
            graphic.color = colour; graphic.raycastTarget = false;
            return rect;
        }
    }
}
