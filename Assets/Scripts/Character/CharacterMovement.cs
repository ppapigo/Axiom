using Axiom.Data;
using Axiom.Combat;
using Axiom.Input;
using Axiom.Role;
using UnityEngine;

namespace Axiom.Character
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class CharacterMovement : MonoBehaviour
    {
        [SerializeField] private CharacterMovementProfile movementProfile;
        [SerializeField] private InputActionMovementSource inputSourceBehaviour;

        private readonly CharacterMovementMotor _motor = new CharacterMovementMotor();
        private CharacterController _characterController;
        private IMovementInputSource _inputSource;
        private CharacterRole _characterRole;
        private CharacterStatusController _status;

        public void Configure(
            CharacterMovementProfile profile,
            InputActionMovementSource inputSource)
        {
            movementProfile = profile;
            inputSourceBehaviour = inputSource;
            _inputSource = inputSource;
        }

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _inputSource = inputSourceBehaviour;
            _characterRole = GetComponent<CharacterRole>();
            _status = GetComponent<CharacterStatusController>();
        }

        private void OnDisable()
        {
            _motor.Reset();
        }

        private void Update()
        {
            if (movementProfile == null || _inputSource == null)
            {
                return;
            }

            MovementParameters parameters = movementProfile.Parameters;
            if (_characterRole != null && _characterRole.IsConfigured)
            {
                parameters = parameters.WithSpeedMultiplier(
                    _characterRole.Definition.MovementSpeedMultiplier);
            }

            if (_status != null)
            {
                parameters = parameters.WithSpeedMultiplier(
                    _status.IsMovementBlocked ? 0f : _status.MovementSpeedMultiplier);
                if (_status.IsMovementBlocked)
                {
                    _motor.Reset();
                }
            }

            Vector3 velocity = _motor.Tick(
                _status != null && _status.IsMovementBlocked
                    ? Vector2.zero
                    : _inputSource.ReadMovement(),
                _characterController.isGrounded,
                parameters,
                Time.deltaTime);

            _characterController.Move(velocity * Time.deltaTime);
        }

    }
}
