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
        private Quaternion leftArmRest, rightArmRest, leftLegRest, rightLegRest;
        private void OnEnable() => CachePose();
        private void Start() => CachePose();
        private void CachePose()
        {
            lastPosition = transform.position;
            if (visualRoot != null) restPosition = visualRoot.localPosition;
            if (leftArm != null) leftArmRest = leftArm.localRotation;
            if (rightArm != null) rightArmRest = rightArm.localRotation;
            if (leftLeg != null) leftLegRest = leftLeg.localRotation;
            if (rightLeg != null) rightLegRest = rightLeg.localRotation;
        }
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
            SetSwing(leftArm, leftArmRest, -swing); SetSwing(rightArm, rightArmRest, swing);
            SetSwing(leftLeg, leftLegRest, swing); SetSwing(rightLeg, rightLegRest, -swing);
        }
        private void SetSwing(Transform limb, Quaternion rest, float angle)
        {
            if (limb == null || limb.parent == null) return;
            Vector3 axis = limb.parent.InverseTransformDirection(transform.right);
            limb.localRotation = Quaternion.AngleAxis(angle, axis) * rest;
        }
    }
}
