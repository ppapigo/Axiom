using System.Collections.Generic;

namespace Axiom.Skill
{
    public sealed class SkillValidationResult
    {
        private readonly List<string> _errors = new List<string>();

        public bool IsValid => _errors.Count == 0;
        public IReadOnlyList<string> Errors => _errors;

        internal void AddError(string error)
        {
            _errors.Add(error);
        }
    }
}
