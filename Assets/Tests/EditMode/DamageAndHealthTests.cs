using System;
using Axiom.Combat;
using NUnit.Framework;

namespace Axiom.Tests.EditMode
{
    public sealed class DamageAndHealthTests
    {
        [Test]
        public void DamageCalculator_UsesCompleteFormula()
        {
            var request = new DamageRequest(
                attacker: null,
                attackPower: 100f,
                damageCoefficient: 0.8f,
                castDelayBonus: 1.5f,
                distanceMultiplier: 0.75f);

            float damage = DamageCalculator.Calculate(request);

            Assert.That(damage, Is.EqualTo(90f).Within(0.001f));
        }

        [Test]
        public void DamageCalculator_AppliesVulnerabilityBeforeDamageLimit()
        {
            var request = new DamageRequest(null, 100f, 1f, 1f, 1f, 110f);

            float damage = DamageCalculator.Calculate(request, 1.2f);

            Assert.That(damage, Is.EqualTo(110f));
        }

        [Test]
        public void Health_ClampsDamageAtZero()
        {
            var health = new HealthModel(100f);

            float applied = health.ApplyDamage(150f);

            Assert.That(applied, Is.EqualTo(100f));
            Assert.That(health.CurrentHealth, Is.EqualTo(0f));
            Assert.That(health.IsDead, Is.True);
        }

        [Test]
        public void Health_DeadTargetIgnoresAdditionalDamage()
        {
            var health = new HealthModel(100f);
            health.ApplyDamage(100f);

            Assert.That(health.ApplyDamage(10f), Is.EqualTo(0f));
        }

        [Test]
        public void Health_RestoreClampsAtMaximum()
        {
            var health = new HealthModel(100f);
            health.ApplyDamage(40f);

            float restored = health.Restore(80f);

            Assert.That(restored, Is.EqualTo(40f));
            Assert.That(health.CurrentHealth, Is.EqualTo(100f));
        }

        [Test]
        public void Health_ResetRestoresMaximumHealth()
        {
            var health = new HealthModel(100f);
            health.ApplyDamage(100f);

            health.Reset();

            Assert.That(health.CurrentHealth, Is.EqualTo(100f));
            Assert.That(health.IsDead, Is.False);
        }

        [Test]
        public void Health_RejectsNegativeDamage()
        {
            var health = new HealthModel(100f);

            Assert.Throws<ArgumentOutOfRangeException>(() => health.ApplyDamage(-1f));
        }

        [Test]
        public void DamageRequest_RejectsNegativeFormulaFactors()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new DamageRequest(null, -1f, 0.8f, 1f, 1f));
        }
    }
}
