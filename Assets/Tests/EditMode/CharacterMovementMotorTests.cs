using System;
using Axiom.Character;
using NUnit.Framework;
using UnityEngine;

namespace Axiom.Tests.EditMode
{
    public sealed class CharacterMovementMotorTests
    {
        private static readonly MovementParameters Parameters = new MovementParameters(
            maximumSpeed: 5f,
            acceleration: 10f,
            deceleration: 20f,
            gravity: -25f,
            groundedVerticalSpeed: -2f);

        [Test]
        public void Tick_NormalizesDiagonalInput()
        {
            var motor = new CharacterMovementMotor();

            Vector3 velocity = motor.Tick(Vector2.one, true, Parameters, 1f);

            Assert.That(new Vector2(velocity.x, velocity.z).magnitude, Is.EqualTo(5f).Within(0.001f));
        }

        [Test]
        public void Tick_AcceleratesUsingDeltaTime()
        {
            var motor = new CharacterMovementMotor();

            Vector3 velocity = motor.Tick(Vector2.right, true, Parameters, 0.25f);

            Assert.That(velocity.x, Is.EqualTo(2.5f).Within(0.001f));
        }

        [Test]
        public void Tick_DeceleratesWhenInputStops()
        {
            var motor = new CharacterMovementMotor();
            motor.Tick(Vector2.right, true, Parameters, 1f);

            Vector3 velocity = motor.Tick(Vector2.zero, true, Parameters, 0.1f);

            Assert.That(velocity.x, Is.EqualTo(3f).Within(0.001f));
        }

        [Test]
        public void Tick_UsesGroundedVerticalSpeedWhileGrounded()
        {
            var motor = new CharacterMovementMotor();
            motor.Tick(Vector2.zero, false, Parameters, 0.5f);

            Vector3 velocity = motor.Tick(Vector2.zero, true, Parameters, 0.1f);

            Assert.That(velocity.y, Is.EqualTo(-2f).Within(0.001f));
        }

        [Test]
        public void Tick_WithZeroDeltaTime_DoesNotChangeState()
        {
            var motor = new CharacterMovementMotor();

            Vector3 velocity = motor.Tick(Vector2.right, false, Parameters, 0f);

            Assert.That(velocity, Is.EqualTo(Vector3.zero));
        }

        [TestCase(-1f, 1f, 1f)]
        [TestCase(1f, -1f, 1f)]
        [TestCase(1f, 1f, -1f)]
        public void Constructor_RejectsNegativePlanarValues(
            float speed,
            float acceleration,
            float deceleration)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new MovementParameters(speed, acceleration, deceleration, -25f, -2f));
        }
    }
}
