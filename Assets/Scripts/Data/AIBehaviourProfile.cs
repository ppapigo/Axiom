using UnityEngine;

namespace Axiom.Data
{
    [CreateAssetMenu(fileName = "AIBehaviourProfile", menuName = "Axiom/AI/Behaviour Profile")]
    public sealed class AIBehaviourProfile : ScriptableObject
    {
        [Header("Sensing")]
        [SerializeField, Min(0f)] private float detectionRange = 15f;
        [SerializeField, Min(0.02f)] private float thinkInterval = 0.15f;
        [SerializeField] private LayerMask targetLayers = ~0;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float baseMoveSpeed = 5f;
        [SerializeField, Range(0f, 1f)] private float retreatHealthRatio = 0.2f;
        [SerializeField, Min(0f)] private float preferredMinimumRange = 4f;
        [SerializeField, Min(0f)] private float preferredMaximumRange = 7f;
        [SerializeField, Min(0f)] private float arrivalDistance = 0.2f;

        [Header("Role Tactics")]
        [SerializeField, Min(0f)] private float tankTauntRange = 3f;
        [SerializeField, Min(0f)] private float mageClusterRadius = 3f;
        [SerializeField, Min(2)] private int mageClusterCount = 2;
        [SerializeField, Min(0f)] private float assassinRearOffset = 2f;

        public float DetectionRange => detectionRange;
        public float ThinkInterval => thinkInterval;
        public LayerMask TargetLayers => targetLayers;
        public float BaseMoveSpeed => baseMoveSpeed;
        public float RetreatHealthRatio => retreatHealthRatio;
        public float PreferredMinimumRange => preferredMinimumRange;
        public float PreferredMaximumRange => preferredMaximumRange;
        public float ArrivalDistance => arrivalDistance;
        public float TankTauntRange => tankTauntRange;
        public float MageClusterRadius => mageClusterRadius;
        public int MageClusterCount => mageClusterCount;
        public float AssassinRearOffset => assassinRearOffset;

        private void OnValidate()
        {
            detectionRange = Mathf.Max(0f, detectionRange);
            thinkInterval = Mathf.Max(0.02f, thinkInterval);
            baseMoveSpeed = Mathf.Max(0f, baseMoveSpeed);
            retreatHealthRatio = Mathf.Clamp01(retreatHealthRatio);
            preferredMinimumRange = Mathf.Max(0f, preferredMinimumRange);
            preferredMaximumRange = Mathf.Max(preferredMinimumRange, preferredMaximumRange);
            arrivalDistance = Mathf.Max(0f, arrivalDistance);
            tankTauntRange = Mathf.Max(0f, tankTauntRange);
            mageClusterRadius = Mathf.Max(0f, mageClusterRadius);
            mageClusterCount = Mathf.Max(2, mageClusterCount);
            assassinRearOffset = Mathf.Max(0f, assassinRearOffset);
        }
    }
}
