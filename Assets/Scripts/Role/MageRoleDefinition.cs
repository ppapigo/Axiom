using UnityEngine;

namespace Axiom.Role
{
    [CreateAssetMenu(
        fileName = "MageRoleDefinition",
        menuName = "Axiom/Role/Mage Definition")]
    public sealed class MageRoleDefinition : CharacterRoleDefinition
    {
        [Header("Combat")]
        [SerializeField, Min(0.01f)] private float maximumHealth = 900f;
        [SerializeField, Min(0f)] private float attackPower = 115f;
        [SerializeField, Min(0.01f)] private float attackSpeedMultiplier = 1f;
        [SerializeField, Min(0f)] private float basicAttackRange = 7f;
        [SerializeField, Min(0f)] private float maximumAreaDamage = 300f;

        [Header("Mobility")]
        [SerializeField, Min(0f)] private float movementSpeedMultiplier = 1f;
        [SerializeField, Min(0f)] private float dashDistance = 4f;
        [SerializeField, Min(0f)] private float dashCooldown = 12f;

        public override CharacterRoleId RoleId => CharacterRoleId.Mage;
        public override float MaximumHealth => maximumHealth;
        public override float AttackPower => attackPower;
        public override float AttackSpeedMultiplier => attackSpeedMultiplier;
        public override float BasicAttackRange => basicAttackRange;
        public override float MovementSpeedMultiplier => movementSpeedMultiplier;
        public override float DashDistance => dashDistance;
        public override float DashCooldown => dashCooldown;
        public override bool AllowsRangedAttacks => true;
        public override bool AllowsAreaDamage => true;
        public override float MaximumAreaRadius => float.PositiveInfinity;
        public float MaximumAreaDamage => maximumAreaDamage;

        private void OnValidate()
        {
            maximumHealth = Mathf.Max(0.01f, maximumHealth);
            attackPower = Mathf.Max(0f, attackPower);
            attackSpeedMultiplier = Mathf.Max(0.01f, attackSpeedMultiplier);
            basicAttackRange = Mathf.Max(0f, basicAttackRange);
            maximumAreaDamage = Mathf.Max(0f, maximumAreaDamage);
            movementSpeedMultiplier = Mathf.Max(0f, movementSpeedMultiplier);
            dashDistance = Mathf.Max(0f, dashDistance);
            dashCooldown = Mathf.Max(0f, dashCooldown);
        }
    }
}
