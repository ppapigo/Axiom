using Axiom.Combat;
using Axiom.Input;
using Axiom.Role;
using UnityEngine;

namespace Axiom.Character
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CharacterRole))]
    public sealed class CharacterDashController : MonoBehaviour
    {
        [SerializeField] private InputActionDashSource dashInput;
        [SerializeField] private InputActionMovementSource movementInput;

        private readonly DashCooldown _cooldown = new DashCooldown();
        private CharacterController _characterController;
        private CharacterRole _characterRole;
        private CharacterStatusController _status;

        public float CooldownDuration => _characterRole != null && _characterRole.IsConfigured
            ? _characterRole.Definition.DashCooldown
            : 0f;

        public float GetCooldownRemaining(float currentTime)
        {
            return _cooldown.GetRemaining(currentTime);
        }

        public void Configure(
            InputActionDashSource inputSource,
            InputActionMovementSource movementSource)
        {
            dashInput = inputSource;
            movementInput = movementSource;
        }

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _characterRole = GetComponent<CharacterRole>();
            _status = GetComponent<CharacterStatusController>();
        }

        private void OnDisable()
        {
            _cooldown.Reset();
        }

        private void Update()
        {
            if (dashInput == null ||
                movementInput == null ||
                (_status != null && _status.IsMovementBlocked) ||
                !_characterRole.IsConfigured ||
                !dashInput.WasDashPressedThisFrame())
            {
                return;
            }

            CharacterRoleDefinition definition = _characterRole.Definition;
            if (!_cooldown.TryStart(Time.time, definition.DashCooldown))
            {
                return;
            }

            Vector2 movement = Vector2.ClampMagnitude(movementInput.ReadMovement(), 1f);
            Vector3 direction = new Vector3(movement.x, 0f, movement.y);
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                direction = transform.forward;
                direction.y = 0f;
            }

            if (direction.sqrMagnitude > Mathf.Epsilon)
            {
                _characterController.Move(direction.normalized * definition.DashDistance);
            }
        }
    }
}
