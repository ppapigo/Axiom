using UnityEngine;

namespace Axiom.Combat
{
    public readonly struct BasicAttackHit
    {
        public BasicAttackHit(
            GameObject attacker,
            Vector3 origin,
            Vector3 direction,
            float attackPower,
            float damageCoefficient,
            float castDelayBonus,
            float distanceMultiplier)
        {
            Attacker = attacker;
            Origin = origin;
            Direction = direction;
            AttackPower = attackPower;
            DamageCoefficient = damageCoefficient;
            CastDelayBonus = castDelayBonus;
            DistanceMultiplier = distanceMultiplier;
        }

        public GameObject Attacker { get; }
        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public float AttackPower { get; }
        public float DamageCoefficient { get; }
        public float CastDelayBonus { get; }
        public float DistanceMultiplier { get; }
    }
}
