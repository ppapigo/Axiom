using Axiom.Combat;
using UnityEngine;

namespace Axiom.Data
{
    [CreateAssetMenu(
        fileName = "BasicAttackProfile",
        menuName = "Axiom/Combat/Basic Attack Profile")]
    public sealed class BasicAttackProfile : ScriptableObject
    {
        [Header("Timing")]
        [SerializeField, Min(0f)] private float cooldown = 0.8f;

        [Header("Hit Shape")]
        [SerializeField] private BasicAttackDeliveryType deliveryType = BasicAttackDeliveryType.Melee;
        [SerializeField, Min(0f)] private float range = 2f;
        [SerializeField, Min(0f)] private float radius = 0.5f;
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("Damage Data")]
        [SerializeField, Min(0f)] private float damageCoefficient = 0.8f;
        [SerializeField, Min(0f)] private float castDelayBonus = 1f;
        [SerializeField, Min(0f)] private float distanceMultiplier = 1f;

        public BasicAttackParameters Parameters => new BasicAttackParameters(
            damageCoefficient,
            cooldown,
            range,
            radius,
            castDelayBonus,
            distanceMultiplier);

        public LayerMask TargetLayers => targetLayers;
        public QueryTriggerInteraction TriggerInteraction => triggerInteraction;
        public BasicAttackDeliveryType DeliveryType => deliveryType;

        public void Configure(
            BasicAttackDeliveryType configuredDeliveryType,
            float configuredRange,
            float configuredRadius,
            float configuredCooldown = 0.8f,
            float configuredDamageCoefficient = 0.8f)
        {
            deliveryType = configuredDeliveryType;
            range = Mathf.Max(0f, configuredRange);
            radius = Mathf.Max(0f, configuredRadius);
            cooldown = Mathf.Max(0f, configuredCooldown);
            damageCoefficient = Mathf.Max(0f, configuredDamageCoefficient);
        }

        private void OnValidate()
        {
            cooldown = Mathf.Max(0f, cooldown);
            range = Mathf.Max(0f, range);
            radius = Mathf.Max(0f, radius);
            damageCoefficient = Mathf.Max(0f, damageCoefficient);
            castDelayBonus = Mathf.Max(0f, castDelayBonus);
            distanceMultiplier = Mathf.Max(0f, distanceMultiplier);
        }
    }
}
