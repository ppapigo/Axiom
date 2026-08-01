using System;

namespace Axiom.Combat
{
    public readonly struct BasicAttackParameters
    {
        public BasicAttackParameters(
            float damageCoefficient,
            float cooldown,
            float range,
            float radius,
            float castDelayBonus,
            float distanceMultiplier)
        {
            if (damageCoefficient < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(damageCoefficient));
            }

            if (cooldown < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(cooldown));
            }

            if (range < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(range));
            }

            if (radius < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(radius));
            }

            if (castDelayBonus < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(castDelayBonus));
            }

            if (distanceMultiplier < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(distanceMultiplier));
            }

            DamageCoefficient = damageCoefficient;
            Cooldown = cooldown;
            Range = range;
            Radius = radius;
            CastDelayBonus = castDelayBonus;
            DistanceMultiplier = distanceMultiplier;
        }

        public float DamageCoefficient { get; }
        public float Cooldown { get; }
        public float Range { get; }
        public float Radius { get; }
        public float CastDelayBonus { get; }
        public float DistanceMultiplier { get; }
    }
}
