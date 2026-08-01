using Axiom.Role;

namespace Axiom.Combat
{
    public static class RoleAttackRules
    {
        public static bool CanUseBasicAttack(
            CharacterRoleDefinition role,
            BasicAttackDeliveryType deliveryType)
        {
            return deliveryType != BasicAttackDeliveryType.Ranged ||
                role == null ||
                role.AllowsRangedAttacks;
        }
    }
}

