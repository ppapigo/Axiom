using System;
using UnityEngine;

namespace Axiom.Character
{
    public sealed class AttackPowerModifierState
    {
        private float _multiplier = 1f;
        private float _expiresAt;

        public float GetMultiplier(float currentTime)
        {
            return currentTime < _expiresAt ? _multiplier : 1f;
        }

        public void Apply(float multiplier, float currentTime, float duration)
        {
            if (multiplier < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(multiplier));
            }

            if (currentTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(currentTime));
            }

            bool isActive = currentTime < _expiresAt;
            _multiplier = isActive
                ? Mathf.Min(_multiplier, multiplier)
                : multiplier;
            _expiresAt = Mathf.Max(_expiresAt, currentTime + Mathf.Max(0f, duration));
        }

        public void Clear()
        {
            _multiplier = 1f;
            _expiresAt = 0f;
        }
    }
}
