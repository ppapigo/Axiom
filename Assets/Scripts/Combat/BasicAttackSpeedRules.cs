using UnityEngine;

namespace Axiom.Combat
{
    public static class BasicAttackSpeedRules
    {
        private const float MinimumMultiplier = 0.01f;

        public static float GetCooldown(
            float baseCooldown,
            float attackSpeedMultiplier)
        {
            return Mathf.Max(0f, baseCooldown) /
                   Mathf.Max(MinimumMultiplier, attackSpeedMultiplier);
        }
    }
}
