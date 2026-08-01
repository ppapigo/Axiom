using System.Collections.Generic;
using Axiom.Role;

namespace Axiom.Skill
{
    public sealed class RoleElementPool
    {
        private const int MaximumDistinctElements = 2;
        private readonly Dictionary<CharacterRoleId, Dictionary<SkillSlot, SkillElement>>
            _assignments =
                new Dictionary<CharacterRoleId, Dictionary<SkillSlot, SkillElement>>();

        public bool CanAssign(
            CharacterRoleId role,
            SkillSlot slot,
            SkillElement element)
        {
            HashSet<SkillElement> elements = GetDistinctElements(role, slot);
            return elements.Contains(element) || elements.Count < MaximumDistinctElements;
        }

        public bool TryAssign(
            CharacterRoleId role,
            SkillSlot slot,
            SkillElement element)
        {
            if (!CanAssign(role, slot, element))
            {
                return false;
            }

            if (!_assignments.TryGetValue(role, out Dictionary<SkillSlot, SkillElement> slots))
            {
                slots = new Dictionary<SkillSlot, SkillElement>();
                _assignments.Add(role, slots);
            }

            slots[slot] = element;
            return true;
        }

        public void ClearAssignment(CharacterRoleId role, SkillSlot slot)
        {
            if (_assignments.TryGetValue(role, out Dictionary<SkillSlot, SkillElement> slots))
            {
                slots.Remove(slot);
            }
        }

        public int GetDistinctElementCount(CharacterRoleId role)
        {
            return GetDistinctElements(role, null).Count;
        }

        private HashSet<SkillElement> GetDistinctElements(
            CharacterRoleId role,
            SkillSlot? excludedSlot)
        {
            var result = new HashSet<SkillElement>();
            if (!_assignments.TryGetValue(role, out Dictionary<SkillSlot, SkillElement> slots))
            {
                return result;
            }

            foreach (KeyValuePair<SkillSlot, SkillElement> assignment in slots)
            {
                if (!excludedSlot.HasValue || assignment.Key != excludedSlot.Value)
                {
                    result.Add(assignment.Value);
                }
            }

            return result;
        }
    }
}
