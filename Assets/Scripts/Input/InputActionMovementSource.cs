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
            return moveAction == null
                ? Vector2.zero
                : moveAction.action.ReadValue<Vector2>();
        }

        private void OnEnable()
        {
            if (moveAction != null)
            {
                moveAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (moveAction != null)
            {
                moveAction.action.Disable();
            }
        }

        private void OnValidate()
        {
            if (moveAction != null && moveAction.action.expectedControlType != "Vector2")
            {
                Debug.LogWarning("Move Action은 Vector2 액션이어야 합니다.", this);
            }
        }
    }
}

