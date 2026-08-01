using System;
using UnityEngine;

namespace Axiom.Combat
{
    public readonly struct DamageRequest
    {
        public DamageRequest(
            GameObject attacker,
            float attackPower,
            float damageCoefficient,
            float castDelayBonus,
            float distanceMultiplier,
            float damageLimit = float.PositiveInfinity)
        {
            if (attackPower < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(attackPower));
            }

            if (damageCoefficient < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(damageCoefficient));
            }

            if (castDelayBonus < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(castDelayBonus));
            }

            if (distanceMultiplier < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(distanceMultiplier));
            }

            if (damageLimit < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(damageLimit));
            }

            Attacker = attacker;
            AttackPower = attackPower;
            DamageCoefficient = damageCoefficient;
            CastDelayBonus = castDelayBonus;
            DistanceMultiplier = distanceMultiplier;
            DamageLimit = damageLimit;
        }

        public GameObject Attacker { get; }
        public float AttackPower { get; }
        public float DamageCoefficient { get; }
        public float CastDelayBonus { get; }
        public float DistanceMultiplier { get; }
        public float DamageLimit { get; }
    }
}
