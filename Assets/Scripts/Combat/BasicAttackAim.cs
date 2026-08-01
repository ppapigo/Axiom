using UnityEngine;

namespace Axiom.Combat
{
    public static class BasicAttackAim
    {
        public static bool TryGetPlanarDirection(
            Vector3 origin,
            Vector3 aimPoint,
            out Vector3 direction)
        {
            Vector3 offset = aimPoint - origin;
            offset.y = 0f;

            if (offset.sqrMagnitude <= Mathf.Epsilon)
            {
                direction = default;
                return false;
            }

            direction = offset.normalized;
            return true;
        }
    }
}

