using UnityEngine;

namespace Axiom.Input
{
    public interface IBasicAttackInputSource
    {
        bool WasBasicAttackPressedThisFrame();
        bool TryGetAimPoint(out Vector3 worldPoint);
    }
}

