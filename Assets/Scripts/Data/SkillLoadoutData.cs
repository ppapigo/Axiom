using System.Collections.Generic;
using Axiom.Role;
using Axiom.Skill;
using UnityEngine;

namespace Axiom.Data
{
    [CreateAssetMenu(fileName = "SkillLoadout", menuName = "Axiom/Skill/Loadout")]
    public sealed class SkillLoadoutData : ScriptableObject
    {
        [SerializeField] private CharacterRoleDefinition role;
        [SerializeField] private SkillBalanceProfile balance;
        [SerializeField] private SkillData[] skills = new SkillData[4];

        public CharacterRoleDefinition Role => role;
        public SkillBalanceProfile Balance => balance;
        public IReadOnlyList<SkillData> Skills => skills;

        public SkillValidationResult ValidateLoadout()
        {
            if (balance == null)
            {
                var missingBalance = new SkillValidationResult();
                missingBalance.AddError("A skill balance profile is required.");
                return missingBalance;
            }

            var definitions = new List<SkillDefinition>();
            foreach (SkillData skill in skills)
            {
                if (skill != null)
                {
                    definitions.Add(skill.Definition);
                }
            }

            return SkillLoadoutValidator.Validate(definitions, role, balance);
        }
    }
}
