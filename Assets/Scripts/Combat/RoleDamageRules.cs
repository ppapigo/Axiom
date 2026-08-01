using Axiom.Role;

namespace Axiom.Combat
{
    public static class RoleDamageRules
    {
        public static bool CanApply(
            CharacterRoleDefinition role,
            DamageApplicationType applicationType)
        {
            return applicationType != DamageApplicationType.Area ||
                role == null ||
                role.AllowsAreaDamage;
        }

        public static float GetDamageLimit(
            CharacterRoleDefinition role,
            DamageApplicationType applicationType)
        {
            if (applicationType == DamageApplicationType.Area &&
                role is MageRoleDefinition mage)
            {
                return mage.MaximumAreaDamage;
            }

            return float.PositiveInfinity;
        }
    }
}

