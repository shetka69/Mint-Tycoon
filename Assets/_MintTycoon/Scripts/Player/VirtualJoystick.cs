using UnityEngine;
using UnityEngine.EventSystems;

namespace MintTycoon.Player
{
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IInitializePotentialDragHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField, Range(0.1f, 1f)] private float movementRange = 0.65f;
        [SerializeField, Range(0f, 0.3f)] private float deadZone = 0.08f;

        private RectTransform inputSurface;
        private int activePointerId;
        private bool pointerHeld;
        private Vector2Int screenSize;

        public Vector2 Value { get; private set; }

        private void Awake() => inputSurface = GetComponent<RectTransform>();
        private void OnEnable()
        {
            screenSize = new Vector2Int(Screen.width, Screen.height);
            ResetValue();
        }

        public void Configure(RectTransform track, RectTransform knob)
        {
            background = track;
            handle = knob;
            ResetValue();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (pointerHeld || eventData.button != PointerEventData.InputButton.Left || background == null || handle == null)
                return;
            if (inputSurface == null) inputSurface = GetComponent<RectTransform>();
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    inputSurface, eventData.position, eventData.pressEventCamera, out Vector2 origin))
                return;

            activePointerId = eventData.pointerId;
            pointerHeld = true;
            background.anchoredPosition = origin;
            background.gameObject.SetActive(true);
            handle.anchoredPosition = Vector2.zero;
            Value = Vector2.zero;
            eventData.useDragThreshold = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (pointerHeld && eventData.pointerId == activePointerId) UpdateValue(eventData);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;

        public void OnPointerUp(PointerEventData eventData)
        {
            if (pointerHeld && eventData.pointerId == activePointerId) ResetValue();
        }

        private void OnDisable() => ResetValue();
        private void OnApplicationFocus(bool hasFocus) { if (!hasFocus) ResetValue(); }
        private void OnApplicationPause(bool paused) { if (paused) ResetValue(); }

        private void Update()
        {
            Vector2Int currentSize = new Vector2Int(Screen.width, Screen.height);
            if (currentSize == screenSize) return;
            screenSize = currentSize;
            ResetValue();
        }

        private void ResetValue()
        {
            Value = Vector2.zero;
            pointerHeld = false;
            if (handle != null)
                handle.anchoredPosition = Vector2.zero;
            if (background != null && background.gameObject != gameObject)
                background.gameObject.SetActive(false);
        }

        private void UpdateValue(PointerEventData eventData)
        {
            if (background == null || handle == null)
                return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    background, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
                return;

            float radius = Mathf.Min(background.rect.width, background.rect.height) * 0.5f * movementRange;
            Vector2 normalized = radius > 0f ? localPoint / radius : Vector2.zero;
            Vector2 displacement = Vector2.ClampMagnitude(normalized, 1f);
            float magnitude = displacement.magnitude;
            Value = magnitude > deadZone
                ? displacement.normalized * Mathf.InverseLerp(deadZone, 1f, magnitude)
                : Vector2.zero;
            handle.anchoredPosition = displacement * radius;
        }
    }
}
