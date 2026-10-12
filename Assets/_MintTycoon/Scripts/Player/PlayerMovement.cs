using UnityEngine;
using UnityEngine.InputSystem;

namespace MintTycoon.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private VirtualJoystick joystick;
        [SerializeField] private Transform movementReference;
        [SerializeField, Min(0f)] private float moveSpeed = 5f;
        [SerializeField, Min(0f)] private float turnSpeed = 720f;

        private CharacterController characterController;
        private float verticalSpeed;

        public void SetJoystick(VirtualJoystick value)
        {
            joystick = value;
        }

        public void SetMovementReference(Transform value)
        {
            movementReference = value;
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Vector2 input = ReadInput();
            Vector3 forward = movementReference != null ? movementReference.forward : Vector3.forward;
            forward.y = 0f;
            forward = forward.sqrMagnitude > 0f ? forward.normalized : Vector3.forward;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 move = right * input.x + forward * input.y;

            if (move.sqrMagnitude > 1f)
                move.Normalize();

            if (characterController.isGrounded && verticalSpeed < 0f)
                verticalSpeed = -2f;
            verticalSpeed += Physics.gravity.y * Time.deltaTime;

            characterController.Move((move * moveSpeed + Vector3.up * verticalSpeed) * Time.deltaTime);

            if (move.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }

        private Vector2 ReadInput()
        {
            Vector2 value = joystick != null ? joystick.Value : Vector2.zero;
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return value;

            Vector2 keyboardValue = Vector2.zero;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) keyboardValue.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) keyboardValue.x += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) keyboardValue.y -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) keyboardValue.y += 1f;
            return keyboardValue.sqrMagnitude > 0f ? Vector2.ClampMagnitude(keyboardValue, 1f) : value;
        }
    }
}
