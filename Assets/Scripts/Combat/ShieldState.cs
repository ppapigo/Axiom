using UnityEngine;

namespace Axiom.Combat
{
    public sealed class ShieldState
    {
        private float _amount;
        private float _expiresAt;

        public float GetAmount(float currentTime)
        {
            return currentTime < _expiresAt ? _amount : 0f;
        }

        public void Apply(float amount, float currentTime, float duration)
        {
            _amount = Mathf.Max(_amount, Mathf.Max(0f, amount));
            _expiresAt = Mathf.Max(_expiresAt, currentTime + Mathf.Max(0f, duration));
        }

        public float Absorb(float damage, float currentTime)
        {
            float remainingDamage = Mathf.Max(0f, damage);
            if (GetAmount(currentTime) <= 0f)
            {
                _amount = 0f;
                return remainingDamage;
            }

            float absorbed = Mathf.Min(_amount, remainingDamage);
            _amount -= absorbed;
            return remainingDamage - absorbed;
        }

        public void Clear()
        {
            _amount = 0f;
            _expiresAt = 0f;
        }
    }
}
