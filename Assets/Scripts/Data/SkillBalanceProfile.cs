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
        [SerializeField, Min(0)] private int burnOrPoisonCost = 10;
        [SerializeField, Min(0)] private int slowCost = 10;
        [SerializeField, Min(0)] private int stunCost = 20;
        [SerializeField, Min(0)] private int knockUpCost = 18;
        [SerializeField, Min(0)] private int mobilityCost = 25;
        [SerializeField, Min(0)] private int shieldCost = 15;
        [SerializeField, Min(0)] private int healingCost = 15;

        [Header("Runtime Rules")]
        [SerializeField, Min(0f)] private float tankMaximumNonUltimateRange = 3f;
        [SerializeField] private AnimationCurve castDelayBonus =
            AnimationCurve.Linear(0f, 1f, 1.5f, 1.5f);

        public int LoadoutPointBudget => loadoutPointBudget;
        public int DamageCostPerTenPercent => damageCostPerTenPercent;
        public int RadiusCostPerMeter => radiusCostPerMeter;
        public int RangeCostPerMeter => rangeCostPerMeter;
        public int CooldownCostPerSecond => cooldownCostPerSecond;
        public float TankMaximumNonUltimateRange => tankMaximumNonUltimateRange;

        public int GetEffectCost(SkillPointEffect effect)
        {
            return effect switch
            {
                SkillPointEffect.BurnOrPoison => burnOrPoisonCost,
                SkillPointEffect.Slow => slowCost,
                SkillPointEffect.Stun => stunCost,
                SkillPointEffect.KnockUp => knockUpCost,
                SkillPointEffect.Mobility => mobilityCost,
                SkillPointEffect.Shield => shieldCost,
                SkillPointEffect.Healing => healingCost,
                _ => throw new System.ArgumentOutOfRangeException(nameof(effect), effect, null)
            };
        }

        public int CalculatePointCost(in SkillPointModifiers modifiers)
        {
            return SkillPointCostCalculator.Calculate(
                modifiers,
                damageCostPerTenPercent,
                radiusCostPerMeter,
                rangeCostPerMeter,
                cooldownCostPerSecond,
                burnOrPoisonCost,
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
            burnOrPoisonCost = Mathf.Max(0, burnOrPoisonCost);
            slowCost = Mathf.Max(0, slowCost);
            stunCost = Mathf.Max(0, stunCost);
            knockUpCost = Mathf.Max(0, knockUpCost);
            mobilityCost = Mathf.Max(0, mobilityCost);
            shieldCost = Mathf.Max(0, shieldCost);
            healingCost = Mathf.Max(0, healingCost);
            tankMaximumNonUltimateRange = Mathf.Max(0f, tankMaximumNonUltimateRange);
            castDelayBonus ??= AnimationCurve.Linear(0f, 1f, 1.5f, 1.5f);
        }
    }
}
