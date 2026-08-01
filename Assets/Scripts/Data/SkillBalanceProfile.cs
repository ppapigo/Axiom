using UnityEngine;
using Axiom.Skill;

namespace Axiom.Data
{
    [CreateAssetMenu(fileName = "SkillBalanceProfile", menuName = "Axiom/Skill/Balance Profile")]
    public sealed class SkillBalanceProfile : ScriptableObject
    {
        [Header("Skill Point Budget")]
        [SerializeField, Min(0)] private int loadoutPointBudget = 100;
        [SerializeField, Min(0)] private int damageCostPerTenPercent = 5;
        [SerializeField, Min(0)] private int radiusCostPerMeter = 6;
        [SerializeField, Min(0)] private int rangeCostPerMeter = 5;
        [SerializeField, Min(0)] private int cooldownCostPerSecond = 6;
        [SerializeField, Min(0)] private int elementCost = 10;
        [Header("Attack Type Costs")]
        [SerializeField, Min(0)] private int targetTypeCost = 8;
        [SerializeField, Min(0)] private int projectileTypeCost = 0;
        [SerializeField, Min(0)] private int selfAreaTypeCost = 12;
        [SerializeField, Min(0)] private int groundAreaTypeCost = 15;
        [SerializeField, Min(0)] private int globalTypeCost = 35;
        [SerializeField, Min(0)] private int coneTypeCost = 8;
        [Header("Effect Costs")]
        [SerializeField, Min(0)] private int slowCost = 10;
        [SerializeField, Min(0)] private int stunCost = 20;
        [SerializeField, Min(0)] private int knockUpCost = 18;
        [SerializeField, Min(0)] private int mobilityCost = 25;
        [SerializeField, Min(0)] private int shieldCost = 15;
        [SerializeField, Min(0)] private int healingCost = 15;

        [Header("Runtime Rules")]
        [SerializeField, Min(0f)] private float tankMaximumNonUltimateRange = 3f;
        [Header("Crowd Control")]
        [SerializeField, Range(0f, 1f)] private float slowMovementReduction = 0.3f;
        [SerializeField, Min(0f)] private float slowDuration = 2f;
        [SerializeField, Min(0f)] private float rootDuration = 1.5f;
        [SerializeField, Min(0f)] private float stunDuration = 1f;
        [SerializeField, Min(0f)] private float knockUpDuration = 0.7f;
        [Header("Elements")]
        [SerializeField, Min(0f)] private float elementTickInterval = 1f;
        [SerializeField, Min(0f)] private float burnDuration = 4f;
        [SerializeField, Min(0f)] private float burnAttackCoefficient = 0.08f;
        [SerializeField, Min(0f)] private float poisonDuration = 5f;
        [SerializeField, Min(0f)] private float poisonMaximumHealthCoefficient = 0.01f;
        [SerializeField, Min(0f)] private float lightningDamageMultiplier = 1.2f;
        [SerializeField, Range(0f, 1f)] private float waterHealingRatio = 0.1f;
        [SerializeField] private AnimationCurve castDelayBonus =
            AnimationCurve.Linear(0f, 1f, 1.5f, 1.5f);

        public int LoadoutPointBudget => loadoutPointBudget;
        public int DamageCostPerTenPercent => damageCostPerTenPercent;
        public int RadiusCostPerMeter => radiusCostPerMeter;
        public int RangeCostPerMeter => rangeCostPerMeter;
        public int CooldownCostPerSecond => cooldownCostPerSecond;
        public int ElementCost => elementCost;
        public float TankMaximumNonUltimateRange => tankMaximumNonUltimateRange;
        public float SlowMovementMultiplier => 1f - slowMovementReduction;
        public float ElementTickInterval => elementTickInterval;
        public float BurnDuration => burnDuration;
        public float BurnAttackCoefficient => burnAttackCoefficient;
        public float PoisonDuration => poisonDuration;
        public float PoisonMaximumHealthCoefficient => poisonMaximumHealthCoefficient;
        public float WaterHealingRatio => waterHealingRatio;

        public float GetElementDamageMultiplier(SkillElement element)
        {
            return element == SkillElement.Lightning
                ? lightningDamageMultiplier
                : 1f;
        }

        public float GetCrowdControlDuration(CrowdControlType type)
        {
            return type switch
            {
                CrowdControlType.Slow => slowDuration,
                CrowdControlType.Root => rootDuration,
                CrowdControlType.Stun => stunDuration,
                CrowdControlType.KnockUp => knockUpDuration,
                _ => 0f
            };
        }

        public int GetEffectCost(SkillPointEffect effect)
        {
            return effect switch
            {
                SkillPointEffect.Slow => slowCost,
                SkillPointEffect.Stun => stunCost,
                SkillPointEffect.KnockUp => knockUpCost,
                SkillPointEffect.Mobility => mobilityCost,
                SkillPointEffect.Shield => shieldCost,
                SkillPointEffect.Healing => healingCost,
                _ => throw new System.ArgumentOutOfRangeException(nameof(effect), effect, null)
            };
        }

        public int GetSkillTypeCost(SkillType type)
        {
            return type switch
            {
                SkillType.Target => targetTypeCost,
                SkillType.Projectile => projectileTypeCost,
                SkillType.SelfArea => selfAreaTypeCost,
                SkillType.GroundArea => groundAreaTypeCost,
                SkillType.Global => globalTypeCost,
                SkillType.Cone => coneTypeCost,
                _ => throw new System.ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public int CalculatePointCost(
            in SkillPointModifiers modifiers,
            int selectedElementCount = 0,
            SkillType? selectedType = null)
        {
            return SkillPointCostCalculator.Calculate(
                modifiers,
                selectedElementCount,
                selectedType.HasValue ? GetSkillTypeCost(selectedType.Value) : 0,
                damageCostPerTenPercent,
                radiusCostPerMeter,
                rangeCostPerMeter,
                cooldownCostPerSecond,
                elementCost,
                slowCost,
                stunCost,
                knockUpCost,
                mobilityCost,
                shieldCost,
                healingCost);
        }

        public float EvaluateCastDelayBonus(float castDelay)
        {
            return Mathf.Max(0f, castDelayBonus.Evaluate(Mathf.Max(0f, castDelay)));
        }

        private void OnValidate()
        {
            loadoutPointBudget = Mathf.Max(0, loadoutPointBudget);
            damageCostPerTenPercent = Mathf.Max(0, damageCostPerTenPercent);
            radiusCostPerMeter = Mathf.Max(0, radiusCostPerMeter);
            rangeCostPerMeter = Mathf.Max(0, rangeCostPerMeter);
            cooldownCostPerSecond = Mathf.Max(0, cooldownCostPerSecond);
            elementCost = Mathf.Max(0, elementCost);
            targetTypeCost = Mathf.Max(0, targetTypeCost);
            projectileTypeCost = Mathf.Max(0, projectileTypeCost);
            selfAreaTypeCost = Mathf.Max(0, selfAreaTypeCost);
            groundAreaTypeCost = Mathf.Max(0, groundAreaTypeCost);
            globalTypeCost = Mathf.Max(0, globalTypeCost);
            coneTypeCost = Mathf.Max(0, coneTypeCost);
            slowCost = Mathf.Max(0, slowCost);
            stunCost = Mathf.Max(0, stunCost);
            knockUpCost = Mathf.Max(0, knockUpCost);
            mobilityCost = Mathf.Max(0, mobilityCost);
            shieldCost = Mathf.Max(0, shieldCost);
            healingCost = Mathf.Max(0, healingCost);
            tankMaximumNonUltimateRange = Mathf.Max(0f, tankMaximumNonUltimateRange);
            slowMovementReduction = Mathf.Clamp01(slowMovementReduction);
            slowDuration = Mathf.Max(0f, slowDuration);
            rootDuration = Mathf.Max(0f, rootDuration);
            stunDuration = Mathf.Max(0f, stunDuration);
            knockUpDuration = Mathf.Max(0f, knockUpDuration);
            elementTickInterval = Mathf.Max(0.01f, elementTickInterval);
            burnDuration = Mathf.Max(0f, burnDuration);
            burnAttackCoefficient = Mathf.Max(0f, burnAttackCoefficient);
            poisonDuration = Mathf.Max(0f, poisonDuration);
            poisonMaximumHealthCoefficient = Mathf.Max(0f, poisonMaximumHealthCoefficient);
            lightningDamageMultiplier = Mathf.Max(0f, lightningDamageMultiplier);
            waterHealingRatio = Mathf.Clamp01(waterHealingRatio);
            castDelayBonus ??= AnimationCurve.Linear(0f, 1f, 1.5f, 1.5f);
        }
    }
}
