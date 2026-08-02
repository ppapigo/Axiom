using Axiom.Combat;
using Axiom.Role;
using NUnit.Framework;
using UnityEngine;

namespace Axiom.Tests.EditMode
{
    public sealed class MageRoleTests
    {
        [Test]
        public void MageDefinition_HasRequiredDefaultStats()
        {
            MageRoleDefinition definition = ScriptableObject.CreateInstance<MageRoleDefinition>();

            Assert.That(definition.RoleId, Is.EqualTo(CharacterRoleId.Mage));
            Assert.That(definition.MaximumHealth, Is.EqualTo(900f));
            Assert.That(definition.AttackPower, Is.EqualTo(115f));
            Assert.That(definition.AttackSpeedMultiplier, Is.EqualTo(1f));
            Assert.That(definition.BasicAttackRange, Is.EqualTo(7f));
            Assert.That(definition.MovementSpeedMultiplier, Is.EqualTo(1f));
            Assert.That(definition.DashDistance, Is.EqualTo(4f));
            Assert.That(definition.DashCooldown, Is.EqualTo(12f));
            Assert.That(definition.AllowsRangedAttacks, Is.True);
            Assert.That(definition.AllowsAreaDamage, Is.True);
            Assert.That(definition.MaximumAreaRadius, Is.EqualTo(float.PositiveInfinity));

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void Mage_AreaDamageUsesConfiguredMaximum()
        {
            MageRoleDefinition definition = ScriptableObject.CreateInstance<MageRoleDefinition>();
            float limit = RoleDamageRules.GetDamageLimit(
                definition,
                DamageApplicationType.Area);
            var request = new DamageRequest(null, 500f, 1f, 1f, 1f, limit);

            Assert.That(DamageCalculator.Calculate(request), Is.EqualTo(300f));

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void Mage_DirectDamageIsNotCappedByAreaLimit()
        {
            MageRoleDefinition definition = ScriptableObject.CreateInstance<MageRoleDefinition>();
            float limit = RoleDamageRules.GetDamageLimit(
                definition,
                DamageApplicationType.Direct);
            var request = new DamageRequest(null, 500f, 1f, 1f, 1f, limit);

            Assert.That(DamageCalculator.Calculate(request), Is.EqualTo(500f));

            Object.DestroyImmediate(definition);
        }
    }
}
