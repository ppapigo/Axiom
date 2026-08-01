using System;
using Axiom.Data;
using Axiom.Role;

namespace Axiom.Skill
{
    public static class SkillValidator
    {
        public static SkillValidationResult Validate(
            in SkillDefinition definition,
            CharacterRoleDefinition role,
            SkillBalanceProfile balance)
        {
            if (balance == null)
            {
                throw new ArgumentNullException(nameof(balance));
            }

            var result = new SkillValidationResult();
            if (string.IsNullOrWhiteSpace(definition.DisplayName))
            {
                result.AddError("Skill name is required.");
            }

            ValidateNonNegative(definition, result);
            if (!SkillCastDelayRules.IsSupported(definition.CastDelay))
            {
                result.AddError("Cast delay must be Instant, 0.3, 0.6, 1.0, or 1.5 seconds.");
            }

            if (definition.PointCost > balance.LoadoutPointBudget)
            {
                result.AddError("Skill point cost exceeds the loadout budget.");
            }

            if (definition.Type == SkillType.Projectile && definition.ProjectileSpeed <= 0f)
            {
                result.AddError("Projectile skills require a positive projectile speed.");
            }

            if (RequiresRange(definition.Type) && definition.Range <= 0f)
            {
                result.AddError("This skill type requires a positive range.");
            }

            if (RequiresRadius(definition.Type) && definition.Radius <= 0f)
            {
                result.AddError("This skill type requires a positive radius.");
            }

            ValidateRoleRules(definition, role, balance, result);
            return result;
        }

        private static void ValidateNonNegative(
            in SkillDefinition definition,
            SkillValidationResult result)
        {
            if (definition.DamageCoefficient < 0f || definition.Cooldown < 0f ||
                definition.CastDelay < 0f || definition.Range < 0f ||
                definition.Radius < 0f || definition.ProjectileSpeed < 0f ||
                definition.PointCost < 0)
            {
                result.AddError("Numeric skill values cannot be negative.");
            }
        }

        private static void ValidateRoleRules(
            in SkillDefinition definition,
            CharacterRoleDefinition role,
            SkillBalanceProfile balance,
            SkillValidationResult result)
        {
            if (role == null)
            {
                return;
            }

            bool isUltimate = definition.Slot == SkillSlot.Ultimate;
            bool isRanged = definition.Type == SkillType.Target ||
                            definition.Type == SkillType.Projectile ||
                            definition.Type == SkillType.GroundArea ||
                            definition.Type == SkillType.Global ||
                            definition.Range > balance.TankMaximumNonUltimateRange;
            if (!role.AllowsRangedAttacks && !isUltimate && isRanged)
            {
                result.AddError("This role cannot use non-ultimate ranged skills.");
            }

            bool isArea = definition.Type == SkillType.GroundArea ||
                          definition.Type == SkillType.Cone ||
                          definition.Type == SkillType.SelfArea ||
                          definition.Type == SkillType.Global;
            if (isArea && definition.DamageCoefficient > 0f && !role.AllowsAreaDamage)
            {
                result.AddError("This role cannot use damaging area skills.");
            }
        }

        private static bool RequiresRange(SkillType type)
        {
            return type != SkillType.SelfArea && type != SkillType.Global;
        }

        private static bool RequiresRadius(SkillType type)
        {
            return type == SkillType.GroundArea || type == SkillType.Cone ||
                   type == SkillType.SelfArea;
        }
    }
}
