using MintTycoon.Player;
using UnityEngine;

namespace MintTycoon.Presentation
{
    /// <summary>Drives the supplied walk clip from actual controller displacement.</summary>
    [RequireComponent(typeof(PlayerMovement))]
    public sealed class PlayerAnimatorView : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField, Min(0.1f)] private float walkMetersPerSecond = 1.5f;
        private PlayerMovement movement;
        private static readonly int MoveAmount = Animator.StringToHash("MoveAmount");
        private static readonly int PlaybackRate = Animator.StringToHash("PlaybackRate");

        private void Awake() => movement = GetComponent<PlayerMovement>();

        private void LateUpdate()
        {
            if (animator == null || movement == null || Time.deltaTime <= 0f) return;
            float speed = movement.PlanarSpeed;
            // Blend into walking quickly; a blocked character returns to its standing pose.
            animator.SetFloat(MoveAmount, Mathf.Clamp01(speed / 0.45f), 0.08f, Time.deltaTime);
            animator.SetFloat(PlaybackRate, speed < 0.05f ? 1f : Mathf.Clamp(speed / walkMetersPerSecond, 0.35f, 5f));
        }
    }
}
