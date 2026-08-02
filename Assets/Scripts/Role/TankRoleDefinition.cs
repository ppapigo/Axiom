using UnityEngine;

namespace Axiom.Role
{
    [CreateAssetMenu(
        fileName = "TankRoleDefinition",
        menuName = "Axiom/Role/Tank Definition")]
    public sealed class TankRoleDefinition : CharacterRoleDefinition
    {
        [Header("Combat")]
        [SerializeField, Min(0.01f)] private float maximumHealth = 1400f;
        [SerializeField, Min(0f)] private float attackPower = 80f;
        [SerializeField, Min(0.01f)] private float attackSpeedMultiplier = 0.9f;
        [SerializeField, Min(0f)] private float basicAttackRange = 2.2f;

        [Header("Mobility")]
        [SerializeField, Min(0f)] private float movementSpeedMultiplier = 0.95f;
        [SerializeField, Min(0f)] private float dashDistance = 4f;
        [SerializeField, Min(0f)] private float dashCooldown = 12f;

        public override CharacterRoleId RoleId => CharacterRoleId.Tank;
        public override float MaximumHealth => maximumHealth;
        public override float AttackPower => attackPower;
        public override float AttackSpeedMultiplier => attackSpeedMultiplier;
        public override float BasicAttackRange => basicAttackRange;
        public override float MovementSpeedMultiplier => movementSpeedMultiplier;
        public override float DashDistance => dashDistance;
        public override float DashCooldown => dashCooldown;
        public override bool AllowsRangedAttacks => false;
        public override bool AllowsAreaDamage => false;
        public override float MaximumAreaRadius => 0f;

        private void OnValidate()
        {
            maximumHealth = Mathf.Max(0.01f, maximumHealth);
            attackPower = Mathf.Max(0f, attackPower);
            attackSpeedMultiplier = Mathf.Max(0.01f, attackSpeedMultiplier);
            basicAttackRange = Mathf.Max(0f, basicAttackRange);
            movementSpeedMultiplier = Mathf.Max(0f, movementSpeedMultiplier);
            dashDistance = Mathf.Max(0f, dashDistance);
            dashCooldown = Mathf.Max(0f, dashCooldown);
        }
    }
}
