using System.Linq;
using Axiom.Combat;
using Axiom.Data;
using Axiom.Role;
using Axiom.Skill;
using NUnit.Framework;
using UnityEngine;

namespace Axiom.Tests.EditMode
{
    public sealed class SkillCreationTests
    {
        [Test]
        public void SkillData_ProvidesExpectedEditableDefaults()
        {
            SkillData data = ScriptableObject.CreateInstance<SkillData>();

            SkillDefinition definition = data.Definition;

            Assert.That(definition.DamageCoefficient, Is.EqualTo(1.2f));
            Assert.That(definition.Cooldown, Is.EqualTo(5f));
            Assert.That(definition.CastDelay, Is.EqualTo(0.3f));
            Assert.That(definition.Type, Is.EqualTo(SkillType.Projectile));
            Object.DestroyImmediate(data);
        }

        [Test]
        public void SkillBalance_DefaultBudget_IsOneHundredPoints()
        {
            SkillBalanceProfile balance = CreateBalance();

            Assert.That(balance.LoadoutPointBudget, Is.EqualTo(100));
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillPointCost_UsesConfiguredBaselineCosts()
        {
            SkillBalanceProfile balance = CreateBalance();
            var modifiers = new SkillPointModifiers(
                damageIncreasePercent: 20f,
                radiusIncrease: 2f,
                rangeIncrease: 1f,
                cooldownReduction: 1f,
                appliesBurnOrPoison: true,
                appliesSlow: true,
                appliesStun: true,
                appliesKnockUp: true,
                addsMobility: true,
                createsShield: true,
                heals: true);

            int cost = balance.CalculatePointCost(modifiers);

            Assert.That(cost, Is.EqualTo(146));
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void SkillPointCost_RoundsPartialIncrementsUp()
        {
            SkillBalanceProfile balance = CreateBalance();
            var modifiers = new SkillPointModifiers(
                damageIncreasePercent: 1f,
                radiusIncrease: 0.1f,
                rangeIncrease: 0.1f,
                cooldownReduction: 0.1f);

            int cost = balance.CalculatePointCost(modifiers);

            Assert.That(cost, Is.EqualTo(22));
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void MageProjectile_WithSupportedValues_IsValid()
        {
            SkillBalanceProfile balance = CreateBalance();
            MageRoleDefinition mage = ScriptableObject.CreateInstance<MageRoleDefinition>();
            SkillDefinition skill = CreateSkill(SkillSlot.Q, SkillType.Projectile);

            SkillValidationResult result = SkillValidator.Validate(skill, mage, balance);

            Assert.That(result.IsValid, Is.True, string.Join("\n", result.Errors));
            Object.DestroyImmediate(balance);
            Object.DestroyImmediate(mage);
        }

        [Test]
        public void UnsupportedCastDelay_IsRejected()
        {
            SkillBalanceProfile balance = CreateBalance();
            SkillDefinition skill = new SkillDefinition(
                "Invalid Delay", SkillSlot.Q, SkillType.Projectile,
                1.2f, 5f, 0.5f, 6f, 0.5f, 10f,
                CrowdControlType.None, SkillElement.Fire, 1);

            SkillValidationResult result = SkillValidator.Validate(skill, null, balance);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(error => error.Contains("Cast delay")), Is.True);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void Tank_NonUltimateProjectile_IsRejected()
        {
            SkillBalanceProfile balance = CreateBalance();
            TankRoleDefinition tank = ScriptableObject.CreateInstance<TankRoleDefinition>();

            SkillValidationResult normal = SkillValidator.Validate(
                CreateSkill(SkillSlot.Q, SkillType.Projectile), tank, balance);
            SkillValidationResult ultimate = SkillValidator.Validate(
                CreateSkill(SkillSlot.Ultimate, SkillType.Projectile), tank, balance);

            Assert.That(normal.IsValid, Is.False);
            Assert.That(ultimate.IsValid, Is.True, string.Join("\n", ultimate.Errors));
            Object.DestroyImmediate(balance);
            Object.DestroyImmediate(tank);
        }

        [Test]
        public void Assassin_AreaPlanClampsRadius()
        {
            AssassinRoleDefinition assassin = ScriptableObject.CreateInstance<AssassinRoleDefinition>();
            SkillDefinition skill = CreateSkill(SkillSlot.E, SkillType.GroundArea, radius: 8f);

            bool created = SkillCastPlanner.TryCreate(
                skill,
                assassin,
                Vector3.zero,
                Vector3.forward * 3f,
                out SkillCastPlan plan);

            Assert.That(created, Is.True);
            Assert.That(plan.Radius, Is.EqualTo(3f));
            Object.DestroyImmediate(assassin);
        }

        [TestCase(SkillType.SelfArea, 1f, 1.2f)]
        [TestCase(SkillType.SelfArea, 3f, 1f)]
        [TestCase(SkillType.SelfArea, 5f, 0.8f)]
        [TestCase(SkillType.GroundArea, 1f, 1f)]
        [TestCase(SkillType.GroundArea, 3f, 0.8f)]
        [TestCase(SkillType.GroundArea, 5f, 0.6f)]
        public void AreaFalloff_UsesSteppedMultiplier(
            SkillType type,
            float distance,
            float expected)
        {
            Assert.That(SkillAreaFalloff.GetMultiplier(type, distance), Is.EqualTo(expected));
        }

        [Test]
        public void Mage_AreaSkillDamageUsesRoleCap()
        {
            SkillBalanceProfile balance = CreateBalance();
            MageRoleDefinition mage = ScriptableObject.CreateInstance<MageRoleDefinition>();
            SkillDefinition skill = CreateSkill(
                SkillSlot.E,
                SkillType.GroundArea,
                coefficient: 3f);

            DamageRequest request = SkillRuntimeRules.CreateDamageRequest(
                null, 500f, skill, mage, balance, 0f);

            Assert.That(DamageCalculator.Calculate(request), Is.EqualTo(300f));
            Object.DestroyImmediate(balance);
            Object.DestroyImmediate(mage);
        }

        [Test]
        public void Loadout_RejectsDuplicateSlotsAndExcessPointCost()
        {
            SkillBalanceProfile balance = CreateBalance();
            SkillDefinition first = CreateSkill(SkillSlot.Q, SkillType.Projectile, pointCost: 60);
            SkillDefinition second = CreateSkill(SkillSlot.Q, SkillType.Projectile, pointCost: 60);

            SkillValidationResult result = SkillLoadoutValidator.Validate(
                new[] { first, second }, null, balance);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Any(error => error.Contains("assigned more than once")), Is.True);
            Assert.That(result.Errors.Any(error => error.Contains("Total skill point")), Is.True);
            Object.DestroyImmediate(balance);
        }

        [Test]
        public void CastPlanner_RejectsAimBeyondRange()
        {
            SkillDefinition skill = CreateSkill(SkillSlot.Q, SkillType.Projectile);

            bool created = SkillCastPlanner.TryCreate(
                skill, null, Vector3.zero, Vector3.forward * 10f, out _);

            Assert.That(created, Is.False);
        }

        [Test]
        public void CooldownTracker_BlocksUntilReady()
        {
            var cooldown = new SkillCooldownTracker();

            Assert.That(cooldown.TryStart(SkillSlot.Q, 10f, 5f), Is.True);
            Assert.That(cooldown.TryStart(SkillSlot.Q, 14.9f, 5f), Is.False);
            Assert.That(cooldown.GetRemaining(SkillSlot.Q, 14f), Is.EqualTo(1f));
            Assert.That(cooldown.TryStart(SkillSlot.Q, 15f, 5f), Is.True);
        }

        private static SkillBalanceProfile CreateBalance()
        {
            return ScriptableObject.CreateInstance<SkillBalanceProfile>();
        }

        private static SkillDefinition CreateSkill(
            SkillSlot slot,
            SkillType type,
            float coefficient = 1.2f,
            float radius = 1f,
            int pointCost = 1)
        {
            return new SkillDefinition(
                "Test Skill",
                slot,
                type,
                coefficient,
                5f,
                0.3f,
                6f,
                radius,
                10f,
                CrowdControlType.None,
                SkillElement.Fire,
                pointCost);
        }
    }
}
