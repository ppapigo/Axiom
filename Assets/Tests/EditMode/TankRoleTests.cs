using Axiom.Character;
using Axiom.Combat;
using Axiom.Role;
using NUnit.Framework;
using UnityEngine;

namespace Axiom.Tests.EditMode
{
    public sealed class TankRoleTests
    {
        [Test]
        public void TankDefinition_HasRequiredDefaultStats()
        {
            TankRoleDefinition definition = ScriptableObject.CreateInstance<TankRoleDefinition>();

            Assert.That(definition.RoleId, Is.EqualTo(CharacterRoleId.Tank));
            Assert.That(definition.MaximumHealth, Is.EqualTo(1400f));
            Assert.That(definition.AttackPower, Is.EqualTo(80f));
            Assert.That(definition.AttackSpeedMultiplier, Is.EqualTo(0.9f));
            Assert.That(definition.BasicAttackRange, Is.EqualTo(2.2f));
            Assert.That(definition.MovementSpeedMultiplier, Is.EqualTo(0.95f));
            Assert.That(definition.DashDistance, Is.EqualTo(4f));
            Assert.That(definition.DashCooldown, Is.EqualTo(12f));
            Assert.That(definition.AllowsRangedAttacks, Is.False);
            Assert.That(definition.AllowsAreaDamage, Is.False);
            Assert.That(definition.MaximumAreaRadius, Is.EqualTo(0f));

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void MovementParameters_AppliesTankSpeedMultiplier()
        {
            var parameters = new MovementParameters(5f, 10f, 10f, -25f, -2f);

            MovementParameters tankParameters = parameters.WithSpeedMultiplier(0.95f);

            Assert.That(tankParameters.MaximumSpeed, Is.EqualTo(4.75f).Within(0.001f));
        }

        [Test]
        public void DashCooldown_BlocksForTwelveSeconds()
        {
            var cooldown = new DashCooldown();
            cooldown.TryStart(5f, 12f);

            Assert.That(cooldown.TryStart(16.99f, 12f), Is.False);
            Assert.That(cooldown.TryStart(17f, 12f), Is.True);
        }

        [Test]
        public void Tank_CannotUseRangedBasicAttack()
        {
            TankRoleDefinition definition = ScriptableObject.CreateInstance<TankRoleDefinition>();

            Assert.That(
                RoleAttackRules.CanUseBasicAttack(
                    definition,
                    BasicAttackDeliveryType.Ranged),
                Is.False);
            Assert.That(
                RoleAttackRules.CanUseBasicAttack(
                    definition,
                    BasicAttackDeliveryType.Melee),
                Is.True);

            Object.DestroyImmediate(definition);
        }
    }
}
