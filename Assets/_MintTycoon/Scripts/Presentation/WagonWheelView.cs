using UnityEngine;

namespace MintTycoon.Presentation
{
    public sealed class WagonWheelView : MonoBehaviour
    {
        [SerializeField] private Transform[] wheels;
        [SerializeField, Min(0.01f)] private float radius = 0.38f;
        [SerializeField] private Vector3 localAxis = Vector3.up;
        private Vector3 lastPosition;
        private void OnEnable() => lastPosition = transform.position;
        private void LateUpdate()
        {
            Vector3 delta = transform.position - lastPosition;
            lastPosition = transform.position;
            float angle = Vector3.Dot(delta, transform.forward) / Mathf.Max(0.01f, radius) * Mathf.Rad2Deg;
            if (wheels == null) return;
            foreach (Transform wheel in wheels) if (wheel != null) wheel.Rotate(localAxis, -angle, Space.Self);
        }
    }
}
