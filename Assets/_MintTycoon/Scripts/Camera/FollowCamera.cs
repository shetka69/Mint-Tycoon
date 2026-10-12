using UnityEngine;

namespace MintTycoon.CameraSystem
{
    [RequireComponent(typeof(Camera))]
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(0f)] private float distance = 22f;
        [SerializeField, Range(10f, 80f)] private float pitch = 50f;
        [SerializeField] private float yaw = 45f;
        [SerializeField, Min(0f)] private float followSharpness = 8f;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredPosition = target.position + targetOffset - rotation * Vector3.forward * distance;
            float blend = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, blend);
            transform.rotation = rotation;
        }
    }
}
