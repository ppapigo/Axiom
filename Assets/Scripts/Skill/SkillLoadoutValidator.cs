using System.Collections.Generic;
using Axiom.Data;
using Axiom.Role;

namespace Axiom.Skill
{
    public static class SkillLoadoutValidator
    {
        public static SkillValidationResult Validate(
            IEnumerable<SkillDefinition> skills,
            CharacterRoleDefinition role,
            SkillBalanceProfile balance)
        {
            var result = new SkillValidationResult();
            var occupiedSlots = new HashSet<SkillSlot>();
            int totalPointCost = 0;
            int elementalSkillCount = 0;

            foreach (SkillDefinition skill in skills)
            {
                SkillValidationResult skillResult = SkillValidator.Validate(skill, role, balance);
                foreach (string error in skillResult.Errors)
                {
                    result.AddError($"{skill.DisplayName}: {error}");
                }

                if (!occupiedSlots.Add(skill.Slot))
                {
                    result.AddError($"Slot {skill.Slot} is assigned more than once.");
                }

                totalPointCost += skill.PointCost;
                if (skill.Slot != SkillSlot.BasicAttack &&
                    skill.Element != SkillElement.None)
                {
                    elementalSkillCount++;
                }
            }

            if (elementalSkillCount > RoleElementPool.MaximumElementSkillCount)
            {
                result.AddError("Only two Q/E/R skills can have an element.");
            }

            if (totalPointCost > balance.LoadoutPointBudget)
            {
                result.AddError("Total skill point cost exceeds the loadout budget.");
            }

            return result;
        }
    }
}
