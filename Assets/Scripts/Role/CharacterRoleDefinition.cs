using UnityEngine;

namespace Axiom.Role
{
    public abstract class CharacterRoleDefinition : ScriptableObject
    {
        public abstract CharacterRoleId RoleId { get; }
        public abstract float MaximumHealth { get; }
        public abstract float AttackPower { get; }
        public abstract float MovementSpeedMultiplier { get; }
        public abstract float DashDistance { get; }
        public abstract float DashCooldown { get; }
        public abstract bool AllowsRangedAttacks { get; }
    }
}

