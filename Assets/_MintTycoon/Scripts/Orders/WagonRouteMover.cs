using MintTycoon.Orders;
using UnityEngine;

namespace MintTycoon.Orders
{
    /// <summary>Moves the wagon out along configured waypoints, then reverses back to the station.</summary>
    public sealed class WagonRouteMover : MonoBehaviour
    {
        [SerializeField] private OrderProgress orderProgress;
        [SerializeField] private Vector3[] waypoints;
        [SerializeField, Min(0.1f)] private float speed = 3.5f;
        [SerializeField, Min(1f)] private float turnSpeedDegrees = 180f;

        private bool travelling;
        private bool returning;
        private int targetIndex;
        private Quaternion homeRotation;
        private Vector3 homePosition;

        private void OnEnable()
        {
            homePosition = transform.position;
            homeRotation = transform.rotation;
            if (orderProgress != null)
                orderProgress.OrderCompleted += HandleOrderCompleted;
        }

        private void OnDisable()
        {
            if (orderProgress != null)
                orderProgress.OrderCompleted -= HandleOrderCompleted;
        }

        private void Update()
        {
            if (!travelling || waypoints == null || waypoints.Length < 2)
                return;

            Vector3 target = homePosition + waypoints[targetIndex];
            Vector3 toTarget = target - transform.position;
            Vector3 direction = toTarget.normalized;
            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion facing = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, facing, turnSpeedDegrees * Time.deltaTime);
            }

            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            if ((transform.position - target).sqrMagnitude > 0.0001f)
                return;

            if (!returning && targetIndex == waypoints.Length - 1)
            {
                returning = true;
                targetIndex--;
            }
            else if (returning && targetIndex == 0)
            {
                travelling = false;
                returning = false;
                transform.rotation = homeRotation;
                if (orderProgress != null)
                    orderProgress.AdvanceToNextOrder();
            }
            else
            {
                targetIndex += returning ? -1 : 1;
            }
        }

        private void HandleOrderCompleted()
        {
            if (waypoints == null || waypoints.Length < 2)
            {
                if (orderProgress != null)
                    orderProgress.AdvanceToNextOrder();
                return;
            }

            travelling = true;
            returning = false;
            targetIndex = 1;
        }

        private void OnValidate()
        {
            speed = Mathf.Max(0.1f, speed);
            turnSpeedDegrees = Mathf.Max(1f, turnSpeedDegrees);
        }
    }
}
