using MintTycoon.Production;
using UnityEngine;

namespace MintTycoon.Presentation
{
    /// <summary>Animates and sounds a station from its real recipe progress.</summary>
    public sealed class StationFeedback : MonoBehaviour
    {
        [SerializeField] private BufferedProductionStation station;
        [SerializeField] private Transform ram;
        [SerializeField] private Transform flywheel;
        [SerializeField] private ParticleSystem smoke;
        [SerializeField] private ParticleSystem sparks;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip cycleSound;
        [SerializeField, Min(0f)] private float ramTravel = 0.38f;
        private Vector3 restPosition;
        private float nextSoundTime;

        private void Awake() { if (ram != null) restPosition = ram.localPosition; }
        private void OnEnable() { Bind(); }
        private void Start() { if (ram != null) restPosition = ram.localPosition; Bind(); }
        private void Bind()
        {
            if (station == null) return;
            station.ProductionCompleted -= HandleCycle;
            station.ProductionCompleted += HandleCycle;
        }
        private void OnDisable() { if (station != null) station.ProductionCompleted -= HandleCycle; }
        private void Update()
        {
            bool active = station != null && station.IsProducing;
            if (smoke != null) { var emission = smoke.emission; emission.enabled = active; }
            if (ram != null)
            {
                float stroke = active ? Mathf.Pow(Mathf.Sin(station.CycleProgress * Mathf.PI), 2f) : 0f;
                Vector3 down = ram.parent != null ? ram.parent.InverseTransformVector(-transform.up) : Vector3.down;
                ram.localPosition = restPosition + down * (stroke * ramTravel);
            }
            if (flywheel != null && active) flywheel.Rotate(transform.up, 300f * Time.deltaTime, Space.World);
        }
        private void HandleCycle()
        {
            if (sparks != null) sparks.Emit(8);
            if (audioSource != null && cycleSound != null && Time.time >= nextSoundTime)
            {
                audioSource.PlayOneShot(cycleSound);
                nextSoundTime = Time.time + 0.15f;
            }
        }
    }
}
