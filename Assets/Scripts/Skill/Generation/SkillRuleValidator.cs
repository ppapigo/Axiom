using System;
using System.Collections.Generic;
using Axiom.Data;
using Axiom.Role;

namespace Axiom.Skill.Generation
{
    public static class SkillRuleValidator
    {
        public static SkillRuleValidationResult Validate(
            in SkillDraft draft,
            in SkillDefinition baseDefinition,
            CharacterRoleDefinition role,
            SkillBalanceProfile balance,
            RoleElementPool elementPool)
        {
            var errors = new List<string>();
            if (balance == null)
            {
                errors.Add("Skill balance profile is required.");
            }

            if (role == null)
            {
                errors.Add("Character role definition is required.");
            }

            if (elementPool == null)
            {
                errors.Add("Role element pool is required.");
            }

            if (draft.Slot != baseDefinition.Slot)
            {
                errors.Add("Draft slot must match the base skill slot.");
            }

            bool hasSupportedType = draft.Type.HasValue &&
                Enum.IsDefined(typeof(SkillType), draft.Type.Value);
            if (!hasSupportedType)
            {
                errors.Add("Generated draft requires a supported skill type.");
            }

            if (draft.Element.HasValue &&
                !Enum.IsDefined(typeof(SkillElement), draft.Element.Value))
            {
                errors.Add("Generated draft contains an unsupported element.");
            }

            int crowdControlCount = CountCrowdControls(draft.Modifiers);
            if (crowdControlCount > 1)
            {
                errors.Add("A generated skill can select at most one crowd control effect.");
            }

            if (role != null && draft.Element.HasValue &&
                !RoleElementPool.IsElementAllowed(role.RoleId, draft.Element.Value))
            {
                errors.Add("This element is not available to this role.");
            }
            else if (role != null && elementPool != null && draft.Element.HasValue &&
                     !elementPool.CanAssign(role.RoleId, draft.Slot, draft.Element.Value))
            {
                errors.Add("Only two Q/E/R skills can have an element.");
            }

            if (role != null && balance != null && hasSupportedType)
            {
                ValidateRequestedRoleLimits(
                    draft,
                    baseDefinition,
                    role,
                    balance,
                    errors);
            }

            bool canResolve = balance != null && hasSupportedType;
            SkillDefinition definition = default;
            int pointCost = 0;
            if (canResolve)
            {
                definition = SkillDraftApplier.Apply(
                    baseDefinition,
                    draft,
                    role,
                    balance);
                pointCost = definition.PointCost;
                SkillValidationResult existingRules = SkillValidator.Validate(
                    definition,
                    role,
                    balance);
                foreach (string error in existingRules.Errors)
                {
                    errors.Add(error);
                }
            }

            return new SkillRuleValidationResult(
                definition,
                canResolve,
                pointCost,
                errors);
        }

        private static int CountCrowdControls(in SkillPointModifiers modifiers)
        {
            int count = modifiers.AppliesSlow ? 1 : 0;
            count += modifiers.AppliesStun ? 1 : 0;
            count += modifiers.AppliesKnockUp ? 1 : 0;
            return count;
        }

        private static void ValidateRequestedRoleLimits(
            in SkillDraft draft,
            in SkillDefinition baseDefinition,
            CharacterRoleDefinition role,
            SkillBalanceProfile balance,
            ICollection<string> errors)
        {
            bool isUltimate = draft.Slot == SkillSlot.Ultimate;
            float requestedRange = baseDefinition.Range + draft.Modifiers.RangeIncrease;
            if (!role.AllowsRangedAttacks && !isUltimate &&
                requestedRange > balance.TankMaximumNonUltimateRange)
            {
                errors.Add("Requested range exceeds this role's non-ultimate limit.");
            }

            SkillType type = draft.Type.Value;
            bool isArea = type == SkillType.GroundArea ||
                          type == SkillType.SelfArea ||
                          type == SkillType.Global;
            float requestedRadius = baseDefinition.Radius + draft.Modifiers.RadiusIncrease;
            if (isArea && role.MaximumAreaRadius > 0f &&
                !float.IsPositiveInfinity(role.MaximumAreaRadius) &&
                requestedRadius > role.MaximumAreaRadius)
            {
                errors.Add("Requested radius exceeds this role's area limit.");
            }
        }
    }
}
