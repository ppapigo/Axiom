using System;

namespace Axiom.Character
{
    public readonly struct MovementParameters
    {
        public MovementParameters(
            float maximumSpeed,
            float acceleration,
            float deceleration,
            float gravity,
            float groundedVerticalSpeed)
        {
            if (maximumSpeed < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumSpeed));
            }

            if (acceleration < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(acceleration));
            }

            if (deceleration < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deceleration));
            }

            MaximumSpeed = maximumSpeed;
            Acceleration = acceleration;
            Deceleration = deceleration;
            Gravity = gravity;
            GroundedVerticalSpeed = groundedVerticalSpeed;
        }

        public float MaximumSpeed { get; }
        public float Acceleration { get; }
        public float Deceleration { get; }
        public float Gravity { get; }
        public float GroundedVerticalSpeed { get; }
    }
}

