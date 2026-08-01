using UnityEngine;

namespace Axiom.Combat
{
    public static class DamageCalculator
    {
        public static float Calculate(in DamageRequest request)
        {
            float damage = request.AttackPower
                * request.DamageCoefficient
                * request.CastDelayBonus
                * request.DistanceMultiplier;

            return Mathf.Min(Mathf.Max(0f, damage), request.DamageLimit);
        }
    }
}
