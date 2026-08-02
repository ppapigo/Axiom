using UnityEngine;

namespace Axiom.Combat
{
    public static class DamageCalculator
    {
        public static float Calculate(
            in DamageRequest request,
            float incomingDamageMultiplier = 1f)
        {
            if (incomingDamageMultiplier < 0f)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(incomingDamageMultiplier));
            }

            float damage = request.AttackPower
                * request.DamageCoefficient
                * request.CastDelayBonus
                * request.DistanceMultiplier
                * incomingDamageMultiplier;

            return Mathf.Min(Mathf.Max(0f, damage), request.DamageLimit);
        }
    }
}
