using UnityEngine;
using UnityEngine.InputSystem;

namespace Axiom.Input
{
    [DisallowMultipleComponent]
    public sealed class InputActionBasicAttackSource : MonoBehaviour, IBasicAttackInputSource
    {
        [SerializeField] private InputActionReference aimAction;
        [SerializeField] private InputActionReference basicAttackAction;
        [SerializeField] private UnityEngine.Camera aimCamera;
        [SerializeField] private LayerMask aimSurfaceLayers = ~0;
        [SerializeField, Min(0.01f)] private float maximumAimDistance = 200f;
        [SerializeField] private float fallbackAimPlaneHeight;

        public bool WasBasicAttackPressedThisFrame()
        {
            return basicAttackAction?.action?.WasPressedThisFrame() ?? false;
        }

        public bool TryGetAimPoint(out Vector3 worldPoint)
        {
            InputAction action = aimAction?.action;
            if (action == null || aimCamera == null)
            {
                worldPoint = default;
                return false;
            }

            Vector2 screenPosition = action.ReadValue<Vector2>();
            Ray ray = aimCamera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    maximumAimDistance,
                    aimSurfaceLayers,
                    QueryTriggerInteraction.Ignore))
            {
                worldPoint = hit.point;
                return true;
            }

            var fallbackPlane = new Plane(Vector3.up, new Vector3(0f, fallbackAimPlaneHeight, 0f));
            if (fallbackPlane.Raycast(ray, out float distance) && distance <= maximumAimDistance)
            {
                worldPoint = ray.GetPoint(distance);
                return true;
            }

            worldPoint = default;
            return false;
        }

        private void OnEnable()
        {
            aimAction?.action?.Enable();
            basicAttackAction?.action?.Enable();
        }

        private void OnDisable()
        {
            aimAction?.action?.Disable();
            basicAttackAction?.action?.Disable();
        }

        private void OnValidate()
        {
            maximumAimDistance = Mathf.Max(0.01f, maximumAimDistance);
        }
    }
}

