using UnityEngine;

namespace Axiom.Character
{
    public sealed class CharacterMovementMotor
    {
        private Vector3 _planarVelocity;
        private float _verticalVelocity;

        public Vector3 Velocity => _planarVelocity + (Vector3.up * _verticalVelocity);

        public Vector3 Tick(
            Vector2 movementInput,
            bool isGrounded,
            in MovementParameters parameters,
            float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return Velocity;
            }

            Vector2 input = Vector2.ClampMagnitude(movementInput, 1f);
            Vector3 desiredDirection = new Vector3(input.x, 0f, input.y);
            Vector3 targetVelocity = desiredDirection * parameters.MaximumSpeed;
            float changeRate = input.sqrMagnitude > 0f
                ? parameters.Acceleration
                : parameters.Deceleration;

            _planarVelocity = Vector3.MoveTowards(
                _planarVelocity,
                targetVelocity,
                changeRate * deltaTime);

            _verticalVelocity = isGrounded && _verticalVelocity <= 0f
                ? parameters.GroundedVerticalSpeed
                : _verticalVelocity + (parameters.Gravity * deltaTime);

            return Velocity;
        }

        public void Reset()
        {
            _planarVelocity = Vector3.zero;
            _verticalVelocity = 0f;
        }
    }
}

