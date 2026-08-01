using System;
using Axiom.Camera;
using NUnit.Framework;
using UnityEngine;

namespace Axiom.Tests.EditMode
{
    public sealed class QuarterViewCameraPoseCalculatorTests
    {
        private static readonly CameraFollowParameters Parameters = new CameraFollowParameters(
            height: 10f,
            pitchAngle: 50f,
            yawAngle: 45f,
            lookAtHeight: 1f,
            smoothTime: 0.12f);

        [Test]
        public void Calculate_PlacesCameraAtConfiguredHeightAboveFocus()
        {
            Vector3 target = new Vector3(2f, 0f, -3f);

            CameraPose pose = QuarterViewCameraPoseCalculator.Calculate(target, Parameters);

            Assert.That(pose.Position.y, Is.EqualTo(11f).Within(0.001f));
        }

        [Test]
        public void Calculate_MovingTargetPreservesCameraOffset()
        {
            Vector3 movement = new Vector3(4f, 0f, -7f);
            CameraPose first = QuarterViewCameraPoseCalculator.Calculate(Vector3.zero, Parameters);
            CameraPose second = QuarterViewCameraPoseCalculator.Calculate(movement, Parameters);

            Assert.That(
                Vector3.Distance(second.Position - first.Position, movement),
                Is.LessThan(0.001f));
            Assert.That(Quaternion.Angle(first.Rotation, second.Rotation), Is.LessThan(0.001f));
        }

        [Test]
        public void Calculate_RotationLooksAtConfiguredFocus()
        {
            Vector3 target = new Vector3(-2f, 3f, 5f);
            Vector3 focus = target + (Vector3.up * Parameters.LookAtHeight);

            CameraPose pose = QuarterViewCameraPoseCalculator.Calculate(target, Parameters);
            Vector3 directionToFocus = (focus - pose.Position).normalized;

            Assert.That(Vector3.Angle(pose.Rotation * Vector3.forward, directionToFocus),
                Is.LessThan(0.001f));
        }

        [TestCase(0f)]
        [TestCase(90f)]
        public void Constructor_RejectsInvalidPitch(float pitch)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new CameraFollowParameters(10f, pitch, 45f, 1f, 0.1f));
        }
    }
}
