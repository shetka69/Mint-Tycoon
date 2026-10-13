using MintTycoon.Production;
using UnityEngine;

namespace MintTycoon.Presentation
{
    public sealed class ConveyorMotionView : MonoBehaviour
    {
        [SerializeField] private ResourceConveyor conveyor;
        [SerializeField] private Transform[] rollers;
        private void Update()
        {
            if (conveyor == null || !conveyor.IsUnlocked || rollers == null) return;
            foreach (Transform roller in rollers)
                if (roller != null) roller.Rotate(Vector3.right, 240f * Time.deltaTime, Space.Self);
        }
    }
}
