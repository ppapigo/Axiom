using System;
using System.Collections.Generic;
using Axiom.Data;
using Axiom.Role;
using UnityEngine;

namespace Axiom.Skill.Generation
{
    public static class SkillAutoCorrector
    {
        public static SkillAutoCorrectionResult Correct(
            in SkillDraft source,
            in SkillDefinition baseDefinition,
            CharacterRoleDefinition role,
            SkillBalanceProfile balance,
            RoleElementPool elementPool)
        {
            var changes = new List<string>();
            SkillRuleValidationResult original = SkillRuleValidator.Validate(
                source,
                baseDefinition,
                role,
                balance,
                elementPool);
            if (original.IsValid)
            {
                return new SkillAutoCorrectionResult(
                    source,
                    original,
                    wasCorrected: false,
                    usedFallback: false,
                    changes);
            }

            SkillDraft corrected = Normalize(
                source,
                baseDefinition,
                role,
                elementPool,
                changes);
            if (balance != null)
            {
                corrected = TrimToBudget(corrected, balance, changes);
            }

            SkillRuleValidationResult correctedValidation = SkillRuleValidator.Validate(
                corrected,
                baseDefinition,
                role,
                balance,
                elementPool);
            if (correctedValidation.IsValid)
            {
                return new SkillAutoCorrectionResult(
                    corrected,
                    correctedValidation,
                    wasCorrected: true,
                    usedFallback: false,
                    changes);
            }

            SkillDraft fallback = CreateFallbackDraft(baseDefinition, role);
            changes.Add("Returned a role-safe fallback preset.");
            SkillRuleValidationResult fallbackValidation = SkillRuleValidator.Validate(
                fallback,
                baseDefinition,
                role,
                balance,
                elementPool);
            return new SkillAutoCorrectionResult(
                fallback,
                fallbackValidation,
                wasCorrected: true,
                usedFallback: true,
                changes);
        }

        public static SkillDraft CreateFallbackDraft(
            in SkillDefinition baseDefinition,
            CharacterRoleDefinition role)
        {
            SkillType type = role != null && role.RoleId == CharacterRoleId.Tank &&
                             baseDefinition.Slot != SkillSlot.Ultimate
                ? SkillType.Cone
                : role != null && role.RoleId == CharacterRoleId.Assassin
                    ? SkillType.Target
                    : SkillType.Projectile;
            return new SkillDraft(
                new SkillPointModifiers(),
                element: null,
                type,
                baseDefinition.Slot);
        }

        private static SkillDraft Normalize(
            in SkillDraft source,
            in SkillDefinition baseDefinition,
            CharacterRoleDefinition role,
            RoleElementPool elementPool,
            ICollection<string> changes)
        {
            SkillType type = source.Type.HasValue &&
                             Enum.IsDefined(typeof(SkillType), source.Type.Value)
                ? source.Type.Value
                : baseDefinition.Type;
            if (!source.Type.HasValue || type != source.Type.Value)
            {
                changes.Add("Replaced an unsupported skill type.");
            }

            if (role != null && role.RoleId == CharacterRoleId.Tank &&
                baseDefinition.Slot != SkillSlot.Ultimate && type != SkillType.Cone)
            {
                type = SkillType.Cone;
                changes.Add("Changed Tank non-ultimate skill to a melee cone.");
            }

            SkillElement? element = source.Element;
            if (element.HasValue &&
                (!Enum.IsDefined(typeof(SkillElement), element.Value) ||
                 role != null && elementPool != null &&
                 !elementPool.CanAssign(role.RoleId, baseDefinition.Slot, element.Value)))
            {
                element = null;
                changes.Add("Removed an unavailable element or a third elemental skill.");
            }

            SkillPointModifiers sourceModifiers = source.Modifiers;
            bool stun = sourceModifiers.AppliesStun;
            bool knockUp = !stun && sourceModifiers.AppliesKnockUp;
            bool slow = !stun && !knockUp && sourceModifiers.AppliesSlow;
            int crowdControlCount = (sourceModifiers.AppliesSlow ? 1 : 0) +
                                    (sourceModifiers.AppliesStun ? 1 : 0) +
                                    (sourceModifiers.AppliesKnockUp ? 1 : 0);
            if (crowdControlCount > 1)
            {
                changes.Add("Reduced crowd control selection to one effect.");
            }

            float damage = FloorToStep(sourceModifiers.DamageIncreasePercent, 10f);
            float radius = FloorToStep(sourceModifiers.RadiusIncrease, 1f);
            float range = FloorToStep(sourceModifiers.RangeIncrease, 1f);
            float cooldown = FloorToStep(sourceModifiers.CooldownReduction, 1f);
            if (role != null && role.RoleId == CharacterRoleId.Tank &&
                baseDefinition.Slot != SkillSlot.Ultimate && range > 0f)
            {
                range = 0f;
                changes.Add("Removed Tank non-ultimate range increase.");
            }

            bool isArea = type == SkillType.GroundArea ||
                          type == SkillType.SelfArea ||
                          type == SkillType.Global;
            if (role != null && isArea && role.MaximumAreaRadius > 0f &&
                !float.IsPositiveInfinity(role.MaximumAreaRadius))
            {
                float maximumIncrease = Mathf.Max(
                    0f,
                    role.MaximumAreaRadius - baseDefinition.Radius);
                if (radius > maximumIncrease)
                {
                    radius = maximumIncrease;
                    changes.Add("Clamped area radius to the role limit.");
                }
            }

            var modifiers = new SkillPointModifiers(
                damage,
                radius,
                range,
                cooldown,
                slow,
                stun,
                knockUp,
                sourceModifiers.AddsMobility,
                sourceModifiers.CreatesShield,
                sourceModifiers.Heals);
            return new SkillDraft(modifiers, element, type, baseDefinition.Slot);
        }

        private static SkillDraft TrimToBudget(
            in SkillDraft source,
            SkillBalanceProfile balance,
            ICollection<string> changes)
        {
            SkillPointModifiers modifiers = source.Modifiers;
            float damage = modifiers.DamageIncreasePercent;
            float radius = modifiers.RadiusIncrease;
            float range = modifiers.RangeIncrease;
            float cooldown = modifiers.CooldownReduction;
            bool slow = modifiers.AppliesSlow;
            bool stun = modifiers.AppliesStun;
            bool knockUp = modifiers.AppliesKnockUp;
            bool mobility = modifiers.AddsMobility;
            bool shield = modifiers.CreatesShield;
            bool heals = modifiers.Heals;
            SkillElement? element = source.Element;
            SkillType? sourceType = source.Type;

            if (GetCost() > balance.LoadoutPointBudget && cooldown > 0f)
            {
                cooldown = 0f;
                changes.Add("Removed cooldown reduction to fit the point budget.");
            }
            if (GetCost() > balance.LoadoutPointBudget && range > 0f)
            {
                range = 0f;
                changes.Add("Removed range increase to fit the point budget.");
            }
            if (GetCost() > balance.LoadoutPointBudget && radius > 0f)
            {
                radius = 0f;
                changes.Add("Removed radius increase to fit the point budget.");
            }
            if (GetCost() > balance.LoadoutPointBudget && damage > 0f)
            {
                damage = 0f;
                changes.Add("Removed bonus damage to fit the point budget.");
            }
            if (GetCost() > balance.LoadoutPointBudget && heals)
            {
                heals = false;
                changes.Add("Removed healing to fit the point budget.");
            }
            if (GetCost() > balance.LoadoutPointBudget && shield)
            {
                shield = false;
                changes.Add("Removed shield to fit the point budget.");
            }
            if (GetCost() > balance.LoadoutPointBudget && mobility)
            {
                mobility = false;
                changes.Add("Removed mobility to fit the point budget.");
            }
            if (GetCost() > balance.LoadoutPointBudget && (slow || stun || knockUp))
            {
                slow = false;
                stun = false;
                knockUp = false;
                changes.Add("Removed crowd control to fit the point budget.");
            }
            if (GetCost() > balance.LoadoutPointBudget && element.HasValue)
            {
                element = null;
                changes.Add("Removed element to fit the point budget.");
            }

            var correctedModifiers = new SkillPointModifiers(
                damage,
                radius,
                range,
                cooldown,
                slow,
                stun,
                knockUp,
                mobility,
                shield,
                heals);
            return new SkillDraft(correctedModifiers, element, source.Type, source.Slot);

            int GetCost()
            {
                var current = new SkillPointModifiers(
                    damage,
                    radius,
                    range,
                    cooldown,
                    slow,
                    stun,
                    knockUp,
                    mobility,
                    shield,
                    heals);
                return balance.CalculatePointCost(
                    current,
                    element.HasValue ? 1 : 0,
                    sourceType);
            }
        }

        private static float FloorToStep(float value, float step)
        {
            return Mathf.Floor(Mathf.Max(0f, value) / step) * step;
        }
    }
}
