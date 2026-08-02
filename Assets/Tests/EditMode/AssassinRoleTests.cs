using System;
using Axiom.Combat;
using Axiom.Role;
using NUnit.Framework;
using UnityEngine;

namespace Axiom.Tests.EditMode
{
    public sealed class AssassinRoleTests
    {
        [Test]
        public void AssassinDefinition_HasRequiredDefaultStats()
        {
            AssassinRoleDefinition definition = ScriptableObject.CreateInstance<AssassinRoleDefinition>();

            Assert.That(definition.RoleId, Is.EqualTo(CharacterRoleId.Assassin));
            Assert.That(definition.MaximumHealth, Is.EqualTo(900f));
            Assert.That(definition.AttackPower, Is.EqualTo(115f));
            Assert.That(definition.AttackSpeedMultiplier, Is.EqualTo(1.2f));
            Assert.That(definition.BasicAttackRange, Is.EqualTo(2.5f));
            Assert.That(definition.MovementSpeedMultiplier, Is.EqualTo(1.10f));
            Assert.That(definition.DashDistance, Is.EqualTo(8f));
            Assert.That(definition.DashCooldown, Is.EqualTo(5f));
            Assert.That(definition.AllowsAreaDamage, Is.True);

            UnityEngine.Object.DestroyImmediate(definition);
        }

        [Test]
        public void Assassin_AreaRadiusIsClampedToConfiguredMaximum()
        {
            AssassinRoleDefinition definition = ScriptableObject.CreateInstance<AssassinRoleDefinition>();

            float radius = RoleDamageRules.ClampAreaRadius(definition, 8f);

            Assert.That(radius, Is.EqualTo(3f));
            UnityEngine.Object.DestroyImmediate(definition);
        }

        [Test]
        public void AreaRadius_RejectsNegativeRequest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                RoleDamageRules.ClampAreaRadius(null, -1f));
        }
    }
}
