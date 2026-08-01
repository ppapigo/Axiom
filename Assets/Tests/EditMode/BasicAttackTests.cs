using Axiom.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Axiom.Tests.EditMode
{
    public sealed class BasicAttackTests
    {
        [Test]
        public void Cooldown_FirstAttackStartsImmediately()
        {
            var cooldown = new BasicAttackCooldown();

            Assert.That(cooldown.TryStart(0f, 0.8f), Is.True);
        }

        [Test]
        public void Cooldown_BlocksUntilReadyTime()
        {
            var cooldown = new BasicAttackCooldown();
            cooldown.TryStart(1f, 0.8f);

            Assert.That(cooldown.TryStart(1.79f, 0.8f), Is.False);
            Assert.That(cooldown.TryStart(1.8f, 0.8f), Is.True);
        }

        [Test]
        public void Cooldown_RemainingTimeNeverBecomesNegative()
        {
            var cooldown = new BasicAttackCooldown();
            cooldown.TryStart(1f, 0.8f);

            Assert.That(cooldown.GetRemaining(3f), Is.EqualTo(0f));
        }

        [Test]
        public void Aim_IgnoresVerticalDifference()
        {
            bool succeeded = BasicAttackAim.TryGetPlanarDirection(
                Vector3.zero,
                new Vector3(3f, 10f, 4f),
                out Vector3 direction);

            Assert.That(succeeded, Is.True);
            Assert.That(
                Vector3.Distance(direction, new Vector3(0.6f, 0f, 0.8f)),
                Is.LessThan(0.001f));
        }

        [Test]
        public void Aim_RejectsAimPointDirectlyAboveOrigin()
        {
            bool succeeded = BasicAttackAim.TryGetPlanarDirection(
                Vector3.zero,
                Vector3.up,
                out _);

            Assert.That(succeeded, Is.False);
        }
    }
}
