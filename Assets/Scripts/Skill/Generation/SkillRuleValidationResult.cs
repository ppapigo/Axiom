using System;
using System.Collections.Generic;

namespace Axiom.Skill.Generation
{
    public sealed class SkillRuleValidationResult
    {
        private readonly string[] _errors;

        internal SkillRuleValidationResult(
            in SkillDefinition definition,
            bool hasDefinition,
            int pointCost,
            IReadOnlyCollection<string> errors)
        {
            Definition = definition;
            HasDefinition = hasDefinition;
            PointCost = pointCost;
            _errors = new string[errors?.Count ?? 0];
            if (errors == null)
            {
                return;
            }

            int index = 0;
            foreach (string error in errors)
            {
                _errors[index++] = error;
            }
        }

        public bool IsValid => _errors.Length == 0;
        public bool HasDefinition { get; }
        public SkillDefinition Definition { get; }
        public int PointCost { get; }
        public IReadOnlyList<string> Errors => _errors ?? Array.Empty<string>();
    }
}
