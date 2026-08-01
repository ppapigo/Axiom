using Axiom.Data;
using Axiom.Role;
using UnityEngine;

namespace Axiom.Skill
{
    public static class SkillDraftApplier
    {
        public static SkillDefinition Apply(
            in SkillDefinition baseDefinition,
            in SkillPointModifiers modifiers,
            CharacterRoleDefinition role,
            SkillBalanceProfile balance)
        {
            var draft = new SkillDraft(modifiers, null);
            return Apply(baseDefinition, draft, role, balance);
        }

        public static SkillDefinition Apply(
            in SkillDefinition baseDefinition,
            in SkillDraft draft,
            CharacterRoleDefinition role,
            SkillBalanceProfile balance)
        {
            SkillPointModifiers modifiers = draft.Modifiers;
            SkillType resolvedType = draft.Type ?? baseDefinition.Type;
            float range = baseDefinition.Range + modifiers.RangeIncrease;
            if (role != null && balance != null && !role.AllowsRangedAttacks &&
                baseDefinition.Slot != SkillSlot.Ultimate)
            {
                range = Mathf.Min(range, balance.TankMaximumNonUltimateRange);
            }

            return new SkillDefinition(
                baseDefinition.DisplayName,
                baseDefinition.Slot,
                resolvedType,
                baseDefinition.DamageCoefficient *
                (1f + (modifiers.DamageIncreasePercent / 100f)),
                Mathf.Max(0f, baseDefinition.Cooldown - modifiers.CooldownReduction),
                baseDefinition.CastDelay,
                range,
                baseDefinition.Radius + modifiers.RadiusIncrease,
                baseDefinition.ProjectileSpeed,
                ResolveCrowdControl(baseDefinition.CrowdControl, modifiers),
                draft.Element ?? baseDefinition.Element,
                balance == null
                    ? baseDefinition.PointCost
                    : balance.CalculatePointCost(
                        modifiers,
                        draft.SelectedElementCount,
                        resolvedType));
        }

        private static CrowdControlType ResolveCrowdControl(
            CrowdControlType fallback,
            in SkillPointModifiers modifiers)
        {
            if (modifiers.AppliesStun)
            {
                return CrowdControlType.Stun;
            }

            if (modifiers.AppliesKnockUp)
            {
                return CrowdControlType.KnockUp;
            }

            if (modifiers.AppliesSlow)
            {
                return CrowdControlType.Slow;
            }

            return fallback;
        }
    }
}
