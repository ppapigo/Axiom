using System;
using System.Collections.Generic;

namespace Axiom.Skill.Generation
{
    public static class SkillDraftMapper
    {
        public static SkillDraftMappingResult Map(
            SkillGenerationResponseDto response,
            SkillSlot slot)
        {
            var errors = new List<string>();
            if (response == null)
            {
                errors.Add("Generation response is required.");
                return SkillDraftMappingResult.Failed(errors);
            }

            if (slot == SkillSlot.BasicAttack)
            {
                errors.Add("Generated skills can only use Q, E, or Ultimate slots.");
            }

            bool hasType = TryParseEnum(response.skillType, out SkillType skillType);
            if (!hasType)
            {
                errors.Add($"Unsupported skill type: '{response.skillType ?? string.Empty}'.");
            }

            bool hasElement = TryParseOptionalEnum(
                response.element,
                out SkillElement? element);
            if (!hasElement)
            {
                errors.Add($"Unsupported element: '{response.element ?? string.Empty}'.");
            }

            bool hasCrowdControl = TryParseOptionalEnum(
                response.crowdControl,
                out CrowdControlType? crowdControl);
            if (!hasCrowdControl)
            {
                errors.Add(
                    $"Unsupported crowd control: '{response.crowdControl ?? string.Empty}'.");
            }
            else if (crowdControl == CrowdControlType.Root ||
                     crowdControl == CrowdControlType.Taunt)
            {
                errors.Add(
                    $"Crowd control '{crowdControl}' is not available in the skill forge yet.");
            }

            ValidateNonNegative(
                response.damageIncreasePercent,
                nameof(response.damageIncreasePercent),
                errors);
            ValidateNonNegative(response.radiusIncrease, nameof(response.radiusIncrease), errors);
            ValidateNonNegative(response.rangeIncrease, nameof(response.rangeIncrease), errors);
            ValidateNonNegative(
                response.cooldownReduction,
                nameof(response.cooldownReduction),
                errors);

            if (errors.Count > 0)
            {
                return SkillDraftMappingResult.Failed(errors);
            }

            var modifiers = new SkillPointModifiers(
                response.damageIncreasePercent,
                response.radiusIncrease,
                response.rangeIncrease,
                response.cooldownReduction,
                appliesSlow: crowdControl == CrowdControlType.Slow,
                appliesStun: crowdControl == CrowdControlType.Stun,
                appliesKnockUp: crowdControl == CrowdControlType.KnockUp,
                addsMobility: response.addsMobility,
                createsShield: response.createsShield,
                heals: response.heals);
            var draft = new SkillDraft(modifiers, element, skillType, slot);
            return SkillDraftMappingResult.Succeeded(draft);
        }

        private static void ValidateNonNegative(
            float value,
            string fieldName,
            ICollection<string> errors)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                errors.Add($"{fieldName} must be a finite non-negative value.");
            }
        }

        private static bool TryParseOptionalEnum<T>(string raw, out T? value)
            where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(raw) ||
                string.Equals(raw.Trim(), "None", StringComparison.OrdinalIgnoreCase))
            {
                value = null;
                return true;
            }

            if (TryParseEnum(raw, out T parsed))
            {
                value = parsed;
                return true;
            }

            value = null;
            return false;
        }

        private static bool TryParseEnum<T>(string raw, out T value)
            where T : struct, Enum
        {
            string normalized = Normalize(raw);
            foreach (T candidate in Enum.GetValues(typeof(T)))
            {
                if (Normalize(candidate.ToString()) == normalized)
                {
                    value = candidate;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty)
                .Replace(" ", string.Empty)
                .Replace("_", string.Empty)
                .Replace("-", string.Empty)
                .Trim()
                .ToUpperInvariant();
        }
    }
}
