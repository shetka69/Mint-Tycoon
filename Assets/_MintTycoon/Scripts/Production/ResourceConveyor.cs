using System;
using UnityEngine;

namespace MintTycoon.Production
{
    /// <summary>Moves compatible finished output into another station after being unlocked.</summary>
    public sealed class ResourceConveyor : MonoBehaviour
    {
        [SerializeField] private BufferedProductionStation source;
        [SerializeField] private BufferedProductionStation destination;
        [SerializeField, Min(0.01f)] private float transferIntervalSeconds = 0.12f;
        [SerializeField, HideInInspector] private bool isUnlocked;

        private float elapsed;

        public event Action UnlockChanged;
        public bool IsUnlocked => isUnlocked;

        public void Unlock()
        {
            if (isUnlocked)
                return;

            isUnlocked = true;
            elapsed = 0f;
            UnlockChanged?.Invoke();
        }

        private void Update()
        {
            if (!isUnlocked || source == null || destination == null)
                return;

            elapsed += Time.deltaTime;
            while (elapsed >= Mathf.Max(0.01f, transferIntervalSeconds))
            {
                if (!source.TryTransferOutputTo(destination, 1))
                {
                    elapsed = 0f;
                    return;
                }

                elapsed -= Mathf.Max(0.01f, transferIntervalSeconds);
            }
        }
    }
}
