using System;
using UnityEngine;

namespace Axiom.Skill
{
    public static class SkillPointCostCalculator
    {
        public static int Calculate(
            in SkillPointModifiers modifiers,
            int damageCostPerTenPercent,
            int radiusCostPerMeter,
            int rangeCostPerMeter,
            int cooldownCostPerSecond,
            int burnOrPoisonCost,
            int slowCost,
            int stunCost,
            int knockUpCost,
            int mobilityCost,
            int shieldCost,
            int healingCost)
        {
            int cost = 0;
            cost += IncrementCost(modifiers.DamageIncreasePercent, 10f, damageCostPerTenPercent);
            cost += IncrementCost(modifiers.RadiusIncrease, 1f, radiusCostPerMeter);
            cost += IncrementCost(modifiers.RangeIncrease, 1f, rangeCostPerMeter);
            cost += IncrementCost(modifiers.CooldownReduction, 1f, cooldownCostPerSecond);
            cost += modifiers.AppliesBurnOrPoison ? burnOrPoisonCost : 0;
            cost += modifiers.AppliesSlow ? slowCost : 0;
            cost += modifiers.AppliesStun ? stunCost : 0;
            cost += modifiers.AppliesKnockUp ? knockUpCost : 0;
            cost += modifiers.AddsMobility ? mobilityCost : 0;
            cost += modifiers.CreatesShield ? shieldCost : 0;
            cost += modifiers.Heals ? healingCost : 0;
            return Mathf.Max(0, cost);
        }

        private static int IncrementCost(float amount, float increment, int costPerIncrement)
        {
            if (amount <= 0f || increment <= 0f || costPerIncrement <= 0)
            {
                return 0;
            }

            return (int)Math.Ceiling(amount / increment) * costPerIncrement;
        }
    }
}
