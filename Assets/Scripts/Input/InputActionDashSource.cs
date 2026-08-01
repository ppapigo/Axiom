using UnityEngine;
using UnityEngine.InputSystem;

namespace Axiom.Input
{
    [DisallowMultipleComponent]
    public sealed class InputActionDashSource : MonoBehaviour, IDashInputSource
    {
        [SerializeField] private InputActionReference dashAction;
        private InputAction _runtimeAction;

        public void Configure(InputAction action)
        {
            _runtimeAction = action;
            if (isActiveAndEnabled)
            {
                _runtimeAction?.Enable();
            }
        }

        public bool WasDashPressedThisFrame()
        {
            InputAction action = dashAction?.action ?? _runtimeAction;
            return action?.WasPressedThisFrame() ?? false;
        }

        private void OnEnable()
        {
            dashAction?.action?.Enable();
            _runtimeAction?.Enable();
        }

        private void OnDisable()
        {
            dashAction?.action?.Disable();
            _runtimeAction?.Disable();
        }
    }
}
