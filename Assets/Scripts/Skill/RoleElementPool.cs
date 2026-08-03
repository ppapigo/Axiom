using System.Collections.Generic;
using Axiom.Role;

namespace Axiom.Skill
{
    public sealed class RoleElementPool
    {
        public const int MaximumElementSkillCount = 2;
        private readonly Dictionary<CharacterRoleId, Dictionary<SkillSlot, SkillElement>>
            _assignments =
                new Dictionary<CharacterRoleId, Dictionary<SkillSlot, SkillElement>>();

        public static bool IsElementAllowed(
            CharacterRoleId role,
            SkillElement element)
        {
            if (element == SkillElement.None)
            {
                return true;
            }

            return role switch
            {
                CharacterRoleId.Tank => element == SkillElement.Wind ||
                                        element == SkillElement.Earth,
                CharacterRoleId.Assassin => element == SkillElement.Poison ||
                                            element == SkillElement.Lightning,
                CharacterRoleId.Mage => true,
                _ => false
            };
        }

        public bool CanAssign(
            CharacterRoleId role,
            SkillSlot slot,
            SkillElement element)
        {
            if (slot == SkillSlot.BasicAttack && element != SkillElement.None)
            {
                return false;
            }

            if (!IsElementAllowed(role, element))
            {
                return false;
            }

            if (element == SkillElement.None)
            {
                return true;
            }

            return HasAssignment(role, slot) ||
                   GetAssignedSkillCount(role) < MaximumElementSkillCount;
        }

        public bool TryAssign(
            CharacterRoleId role,
            SkillSlot slot,
            SkillElement element)
        {
            if (element == SkillElement.None)
            {
                ClearAssignment(role, slot);
                return true;
            }

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

        public int GetAssignedSkillCount(CharacterRoleId role)
        {
            return _assignments.TryGetValue(
                role,
                out Dictionary<SkillSlot, SkillElement> slots)
                ? slots.Count
                : 0;
        }

        private bool HasAssignment(CharacterRoleId role, SkillSlot slot)
        {
            return _assignments.TryGetValue(
                       role,
                       out Dictionary<SkillSlot, SkillElement> slots) &&
                   slots.ContainsKey(slot);
        }
    }
}
