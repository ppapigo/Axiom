using UnityEngine;

namespace Axiom.AI
{
    public readonly struct AITargetCandidate
    {
        public AITargetCandidate(Transform transform, float healthRatio)
        {
            Transform = transform;
            HealthRatio = Mathf.Clamp01(healthRatio);
        }

        public Transform Transform { get; }
        public float HealthRatio { get; }
    }
}
