using UnityEngine;
using UnityEngine.InputSystem;

namespace Axiom.Input
{
    [DisallowMultipleComponent]
    public sealed class InputActionMovementSource : MonoBehaviour, IMovementInputSource
    {
        [SerializeField] private InputActionReference moveAction;

        public Vector2 ReadMovement()
        {
            InputAction action = moveAction?.action;
            return action == null ? Vector2.zero : action.ReadValue<Vector2>();
        }

        private void OnEnable()
        {
            moveAction?.action?.Enable();
        }

        private void OnDisable()
        {
            moveAction?.action?.Disable();
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
