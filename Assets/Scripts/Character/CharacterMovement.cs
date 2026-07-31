using Axiom.Data;
using Axiom.Input;
using UnityEngine;

namespace Axiom.Character
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class CharacterMovement : MonoBehaviour
    {
        [SerializeField] private CharacterMovementProfile movementProfile;
        [SerializeField] private MonoBehaviour inputSourceBehaviour;

        private readonly CharacterMovementMotor _motor = new CharacterMovementMotor();
        private CharacterController _characterController;
        private IMovementInputSource _inputSource;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _inputSource = inputSourceBehaviour as IMovementInputSource;
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

            Vector3 velocity = _motor.Tick(
                _inputSource.ReadMovement(),
                _characterController.isGrounded,
                movementProfile.Parameters,
                Time.deltaTime);

            _characterController.Move(velocity * Time.deltaTime);
        }

        private void OnValidate()
        {
            if (inputSourceBehaviour != null && inputSourceBehaviour is not IMovementInputSource)
            {
                Debug.LogWarning(
                    $"{nameof(inputSourceBehaviour)}는 {nameof(IMovementInputSource)}를 구현해야 합니다.",
                    this);
            }
        }
    }
}

