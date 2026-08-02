using System;
using System.Collections.Generic;

namespace Axiom.Skill.Generation
{
    public readonly struct SkillDraftMappingResult
    {
        private readonly string[] _errors;

        private SkillDraftMappingResult(
            bool isSuccess,
            in SkillDraft draft,
            string[] errors)
        {
            IsSuccess = isSuccess;
            Draft = draft;
            _errors = errors ?? Array.Empty<string>();
        }

        public bool IsSuccess { get; }
        public SkillDraft Draft { get; }
        public IReadOnlyList<string> Errors => _errors ?? Array.Empty<string>();

        public static SkillDraftMappingResult Succeeded(in SkillDraft draft)
        {
            return new SkillDraftMappingResult(true, draft, Array.Empty<string>());
        }

        public static SkillDraftMappingResult Failed(IReadOnlyCollection<string> errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            var copy = new string[errors.Count];
            int index = 0;
            foreach (string error in errors)
            {
                copy[index++] = error;
            }
            return new SkillDraftMappingResult(false, default, copy);
        }
    }
}
