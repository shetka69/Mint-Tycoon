using UnityEngine;
using UnityEngine.EventSystems;

namespace MintTycoon.Player
{
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField, Range(0.1f, 1f)] private float movementRange = 0.65f;

        public Vector2 Value { get; private set; }

        public void Configure(RectTransform track, RectTransform knob)
        {
            background = track;
            handle = knob;
        }

        public void OnPointerDown(PointerEventData eventData) => UpdateValue(eventData);
        public void OnDrag(PointerEventData eventData) => UpdateValue(eventData);

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetValue();
        }

        private void OnDisable() => ResetValue();

        private void ResetValue()
        {
            Value = Vector2.zero;
            if (handle != null)
                handle.anchoredPosition = Vector2.zero;
        }

        private void UpdateValue(PointerEventData eventData)
        {
            if (background == null || handle == null)
                return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    background, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
                return;

            float radius = Mathf.Min(background.rect.width, background.rect.height) * 0.5f;
            Vector2 normalized = radius > 0f ? localPoint / radius : Vector2.zero;
            Value = Vector2.ClampMagnitude(normalized, 1f);
            handle.anchoredPosition = Value * radius * movementRange;
        }
    }
}
