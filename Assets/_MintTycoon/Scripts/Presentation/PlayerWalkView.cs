using UnityEngine;

namespace MintTycoon.Presentation
{
    /// <summary>Gives the low-poly mint worker a walk cycle without an animation dependency.</summary>
    public sealed class PlayerWalkView : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;
        [SerializeField] private Transform leftLeg;
        [SerializeField] private Transform rightLeg;
        private Vector3 lastPosition;
        private Vector3 restPosition;
        private float phase;
        private float blend;
        private void OnEnable() { lastPosition = transform.position; if (visualRoot != null) restPosition = visualRoot.localPosition; }
        private void LateUpdate()
        {
            Vector3 delta = transform.position - lastPosition;
            lastPosition = transform.position;
            delta.y = 0f;
            float speed = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;
            blend = Mathf.MoveTowards(blend, Mathf.Clamp01(speed / 3f), Time.deltaTime * 8f);
            phase += delta.magnitude * 5.5f;
            float swing = Mathf.Sin(phase) * 24f * blend;
            if (visualRoot != null) visualRoot.localPosition = restPosition + Vector3.up * (Mathf.Abs(Mathf.Sin(phase)) * 0.045f * blend);
            SetSwing(leftArm, -swing); SetSwing(rightArm, swing);
            SetSwing(leftLeg, swing); SetSwing(rightLeg, -swing);
        }
        private static void SetSwing(Transform limb, float angle) { if (limb != null) limb.localRotation = Quaternion.Euler(angle, 0f, 0f); }
    }
}
