using UnityEngine;

namespace Axiom.Role
{
    [CreateAssetMenu(
        fileName = "AssassinRoleDefinition",
        menuName = "Axiom/Role/Assassin Definition")]
    public sealed class AssassinRoleDefinition : CharacterRoleDefinition
    {
        [Header("Combat")]
        [SerializeField, Min(0.01f)] private float maximumHealth = 900f;
        [SerializeField, Min(0f)] private float attackPower = 115f;
        [SerializeField, Min(0.01f)] private float attackSpeedMultiplier = 1.2f;
        [SerializeField, Min(0f)] private float basicAttackRange = 2.5f;
        [SerializeField, Min(0f)] private float maximumAreaRadius = 3f;

        [Header("Mobility")]
        [SerializeField, Min(0f)] private float movementSpeedMultiplier = 1.10f;
        [SerializeField, Min(0f)] private float dashDistance = 8f;
        [SerializeField, Min(0f)] private float dashCooldown = 5f;

        public override CharacterRoleId RoleId => CharacterRoleId.Assassin;
        public override float MaximumHealth => maximumHealth;
        public override float AttackPower => attackPower;
        public override float AttackSpeedMultiplier => attackSpeedMultiplier;
        public override float BasicAttackRange => basicAttackRange;
        public override float MovementSpeedMultiplier => movementSpeedMultiplier;
        public override float DashDistance => dashDistance;
        public override float DashCooldown => dashCooldown;
        public override bool AllowsRangedAttacks => true;
        public override bool AllowsAreaDamage => true;
        public override float MaximumAreaRadius => maximumAreaRadius;

        private void OnValidate()
        {
            maximumHealth = Mathf.Max(0.01f, maximumHealth);
            attackPower = Mathf.Max(0f, attackPower);
            attackSpeedMultiplier = Mathf.Max(0.01f, attackSpeedMultiplier);
            basicAttackRange = Mathf.Max(0f, basicAttackRange);
            maximumAreaRadius = Mathf.Max(0f, maximumAreaRadius);
            movementSpeedMultiplier = Mathf.Max(0f, movementSpeedMultiplier);
            dashDistance = Mathf.Max(0f, dashDistance);
            dashCooldown = Mathf.Max(0f, dashCooldown);
        }
    }
}
