using System;
using System.Collections.Generic;

namespace Axiom.Skill.Generation
{
    public sealed class SkillGenerationPipelineResult
    {
        private readonly string[] _errors;
        private readonly string[] _changes;

        internal SkillGenerationPipelineResult(
            SkillGenerationResponseDto response,
            in SkillDraft draft,
            SkillRuleValidationResult validation,
            SkillPointCostBreakdown pointCost,
            bool usedFallback,
            IReadOnlyCollection<string> errors,
            IReadOnlyCollection<string> changes)
        {
            Response = response;
            Draft = draft;
            Validation = validation ?? throw new ArgumentNullException(nameof(validation));
            PointCost = pointCost ?? throw new ArgumentNullException(nameof(pointCost));
            UsedFallback = usedFallback;
            _errors = Copy(errors);
            _changes = Copy(changes);
        }

        public bool IsSuccess => _errors.Length == 0 && Validation.IsValid;
        public SkillGenerationResponseDto Response { get; }
        public SkillDraft Draft { get; }
        public SkillRuleValidationResult Validation { get; }
        public SkillPointCostBreakdown PointCost { get; }
        public bool WasAutoCorrected => _changes.Length > 0;
        public bool UsedFallback { get; }
        public IReadOnlyList<string> Errors => _errors;
        public IReadOnlyList<string> Changes => _changes;

        private static string[] Copy(IReadOnlyCollection<string> source)
        {
            var copy = new string[source?.Count ?? 0];
            if (source == null)
            {
                return copy;
            }

            int index = 0;
            foreach (string value in source)
            {
                copy[index++] = value;
            }

            return copy;
        }
    }
}
