using System;
using Axiom.Role;
using Axiom.Skill;

namespace Axiom.Demo
{
    public static class DemoSkillDefinitionFactory
    {
        public static SkillDefinition Create(CharacterRoleId role, SkillSlot slot)
        {
            return slot switch
            {
                SkillSlot.Q => CreateDefinition(
                    "Q Skill",
                    slot,
                    role == CharacterRoleId.Tank ? SkillType.Cone : SkillType.Projectile,
                    1.2f,
                    4f,
                    role == CharacterRoleId.Tank ? 3f : 7f,
                    1.5f,
                    GetDefaultElement(role, slot)),
                SkillSlot.E => CreateDefinition(
                    "E Skill",
                    slot,
                    role == CharacterRoleId.Tank ? SkillType.Cone : SkillType.GroundArea,
                    1.8f,
                    7f,
                    role == CharacterRoleId.Tank ? 3f : 6f,
                    3f,
                    GetDefaultElement(role, slot)),
                SkillSlot.Ultimate => CreateDefinition(
                    "Ultimate",
                    slot,
                    role == CharacterRoleId.Mage
                        ? SkillType.GroundArea
                        : SkillType.Projectile,
                    3f,
                    15f,
                    8f,
                    3f,
                    GetDefaultElement(role, slot)),
                _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
            };
        }

        public static SkillElement GetDefaultElement(
            CharacterRoleId role,
            SkillSlot slot)
        {
            return slot switch
            {
                SkillSlot.Q => role switch
                {
                    CharacterRoleId.Mage => SkillElement.Fire,
                    CharacterRoleId.Assassin => SkillElement.Poison,
                    _ => SkillElement.Earth
                },
                SkillSlot.E => role == CharacterRoleId.Mage
                    ? SkillElement.Ice
                    : SkillElement.Wind,
                SkillSlot.Ultimate => role switch
                {
                    CharacterRoleId.Mage => SkillElement.Fire,
                    CharacterRoleId.Assassin => SkillElement.Poison,
                    _ => SkillElement.Earth
                },
                _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
            };
        }

        private static SkillDefinition CreateDefinition(
            string name,
            SkillSlot slot,
            SkillType type,
            float coefficient,
            float cooldown,
            float range,
            float radius,
            SkillElement element)
        {
            return new SkillDefinition(
                name,
                slot,
                type,
                coefficient,
                cooldown,
                0.3f,
                range,
                radius,
                12f,
                CrowdControlType.None,
                element,
                1);
        }
    }
}
