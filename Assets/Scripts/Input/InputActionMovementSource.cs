using UnityEngine;
using UnityEngine.InputSystem;

namespace Axiom.Input
{
    [DisallowMultipleComponent]
    public sealed class InputActionMovementSource : MonoBehaviour, IMovementInputSource
    {
        [SerializeField] private InputActionReference moveAction;
        private InputAction _runtimeAction;

        public void Configure(InputAction action)
        {
            _runtimeAction = action;
            if (isActiveAndEnabled)
            {
                _runtimeAction?.Enable();
            }
        }

        public Vector2 ReadMovement()
        {
            InputAction action = moveAction?.action ?? _runtimeAction;
            return action == null ? Vector2.zero : action.ReadValue<Vector2>();
        }

        private void OnEnable()
        {
            moveAction?.action?.Enable();
            _runtimeAction?.Enable();
        }

        private void OnDisable()
        {
            moveAction?.action?.Disable();
            _runtimeAction?.Disable();
        }

        private void OnValidate()
        {
            InputAction action = moveAction?.action;
            if (action != null && action.expectedControlType != "Vector2")
            {
                Debug.LogWarning("Move Action은 Vector2 액션이어야 합니다.", this);
            }
        }
    }
}
