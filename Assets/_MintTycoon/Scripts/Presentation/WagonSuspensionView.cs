using UnityEngine;

namespace MintTycoon.Presentation
{
    /// <summary>Small distance-driven chassis movement; wheel pivots remain on the ground.</summary>
    public sealed class WagonSuspensionView : MonoBehaviour
    {
        [SerializeField] private Transform chassis;
        [SerializeField, Min(0f)] private float travel = 0.012f;
        private Vector3 restPosition;
        private Quaternion restRotation;
        private Vector3 lastPosition;
        private float phase, blend;

        private void OnEnable()
        {
            lastPosition = transform.position;
            if (chassis == null) return;
            restPosition = chassis.localPosition;
            restRotation = chassis.localRotation;
        }

        private void LateUpdate()
        {
            Vector3 delta = transform.position - lastPosition;
            lastPosition = transform.position; delta.y = 0f;
            if (chassis == null || Time.deltaTime <= 0f) return;
            phase += delta.magnitude * 7f;
            blend = Mathf.MoveTowards(blend, Mathf.Clamp01(delta.magnitude / Time.deltaTime), Time.deltaTime * 5f);
            chassis.localPosition = restPosition + Vector3.up * (Mathf.Sin(phase) * travel * blend);
            chassis.localRotation = restRotation * Quaternion.Euler(Mathf.Sin(phase * 0.6f) * 0.6f * blend, 0f, Mathf.Sin(phase) * 0.4f * blend);
        }

        private void OnDisable()
        {
            if (chassis == null) return;
            chassis.localPosition = restPosition; chassis.localRotation = restRotation;
        }
    }
}
