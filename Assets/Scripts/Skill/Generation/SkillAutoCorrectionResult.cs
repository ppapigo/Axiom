using System;
using System.Collections.Generic;

namespace Axiom.Skill.Generation
{
    public sealed class SkillAutoCorrectionResult
    {
        private readonly string[] _changes;

        internal SkillAutoCorrectionResult(
            in SkillDraft draft,
            SkillRuleValidationResult validation,
            bool wasCorrected,
            bool usedFallback,
            IReadOnlyCollection<string> changes)
        {
            Draft = draft;
            Validation = validation ?? throw new ArgumentNullException(nameof(validation));
            WasCorrected = wasCorrected;
            UsedFallback = usedFallback;
            _changes = new string[changes?.Count ?? 0];
            if (changes == null)
            {
                return;
            }

            int index = 0;
            foreach (string change in changes)
            {
                _changes[index++] = change;
            }
        }

        public SkillDraft Draft { get; }
        public SkillRuleValidationResult Validation { get; }
        public bool WasCorrected { get; }
        public bool UsedFallback { get; }
        public IReadOnlyList<string> Changes => _changes ?? Array.Empty<string>();
    }
}
