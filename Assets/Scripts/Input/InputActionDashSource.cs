using UnityEngine;
using UnityEngine.InputSystem;

namespace Axiom.Input
{
    [DisallowMultipleComponent]
    public sealed class InputActionDashSource : MonoBehaviour, IDashInputSource
    {
        [SerializeField] private InputActionReference dashAction;

        public bool WasDashPressedThisFrame()
        {
            return dashAction?.action?.WasPressedThisFrame() ?? false;
        }

        private void OnEnable()
        {
            dashAction?.action?.Enable();
        }

        private void OnDisable()
        {
            dashAction?.action?.Disable();
        }
    }
}

