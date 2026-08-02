using Axiom.Demo;
using NUnit.Framework;

namespace Axiom.Tests.EditMode
{
    public sealed class StickFigureMotionCalculatorTests
    {
        [Test]
        public void StationaryPose_DoesNotSwingLimbs()
        {
            StickFigureMotionPose pose = Calculate(
                planarSpeed: 0f,
                elapsedTime: 0.25f,
                attackElapsed: -1f);

            Assert.That(pose.LimbSwingDegrees, Is.EqualTo(0f));
            Assert.That(pose.AttackWeight, Is.EqualTo(0f));
            Assert.That(pose.BobOffset, Is.InRange(-0.012f, 0.012f));
        }

        [Test]
        public void MovingPose_SwingsLimbsWithinConfiguredLimit()
        {
            StickFigureMotionPose pose = Calculate(
                planarSpeed: 4f,
                elapsedTime: 0.18f,
                attackElapsed: -1f);

            Assert.That(pose.LimbSwingDegrees, Is.Not.EqualTo(0f));
            Assert.That(pose.LimbSwingDegrees, Is.InRange(-28f, 28f));
            Assert.That(pose.BobOffset, Is.InRange(-0.045f, 0.045f));
        }

        [Test]
        public void AttackPose_ReachesPeakAtHalfDuration()
        {
            StickFigureMotionPose pose = Calculate(
                planarSpeed: 0f,
                elapsedTime: 0f,
                attackElapsed: 0.1f);

            Assert.That(pose.AttackWeight, Is.EqualTo(1f).Within(0.001f));
        }

        private static StickFigureMotionPose Calculate(
            float planarSpeed,
            float elapsedTime,
            float attackElapsed)
        {
            return StickFigureMotionCalculator.Calculate(
                planarSpeed,
                elapsedTime,
                attackElapsed,
                referenceSpeed: 4f,
                walkFrequency: 9f,
                maximumLimbSwing: 28f,
                idleBobHeight: 0.012f,
                movingBobHeight: 0.045f,
                attackDuration: 0.2f);
        }
    }
}
