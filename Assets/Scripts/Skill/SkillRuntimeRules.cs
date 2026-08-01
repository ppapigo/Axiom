using Axiom.Combat;
using Axiom.Data;
using Axiom.Role;
using UnityEngine;

namespace Axiom.Skill
{
    public static class SkillRuntimeRules
    {
        public static float GetEffectiveRadius(
            in SkillDefinition definition,
            CharacterRoleDefinition role)
        {
            return IsArea(definition.Type)
                ? RoleDamageRules.ClampAreaRadius(role, definition.Radius)
                : definition.Radius;
        }

        public static DamageRequest CreateDamageRequest(
            GameObject attacker,
            float attackPower,
            in SkillDefinition definition,
            CharacterRoleDefinition role,
            SkillBalanceProfile balance,
            float distanceFromAreaCenter)
        {
            DamageApplicationType applicationType = IsArea(definition.Type)
                ? DamageApplicationType.Area
                : DamageApplicationType.Direct;
            float distanceMultiplier = SkillAreaFalloff.GetMultiplier(
                definition.Type,
                distanceFromAreaCenter);

            return new DamageRequest(
                attacker,
                attackPower,
                definition.DamageCoefficient,
                balance.EvaluateCastDelayBonus(definition.CastDelay),
                distanceMultiplier,
                RoleDamageRules.GetDamageLimit(role, applicationType));
        }

        public static bool IsArea(SkillType type)
        {
            return type == SkillType.GroundArea || type == SkillType.Cone ||
                   type == SkillType.SelfArea;
        }
    }
}
