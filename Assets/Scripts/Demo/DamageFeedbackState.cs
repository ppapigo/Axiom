using UnityEngine;

namespace Axiom.Demo
{
    public sealed class DamageFeedbackState
    {
        public float DamageAmount { get; private set; }
        public float StartedAt { get; private set; }
        public float Duration { get; private set; }

        public bool Register(float damageAmount, float currentTime, float duration)
        {
            if (damageAmount <= 0f || duration <= 0f)
            {
                return false;
            }

            DamageAmount = IsVisible(currentTime)
                ? DamageAmount + damageAmount
                : damageAmount;
            StartedAt = currentTime;
            Duration = duration;
            return true;
        }

        public bool IsVisible(float currentTime)
        {
            float elapsed = currentTime - StartedAt;
            return DamageAmount > 0f &&
                   elapsed >= 0f &&
                   elapsed < Duration &&
                   !Mathf.Approximately(elapsed, Duration);
        }

        public float GetNormalizedAge(float currentTime)
        {
            if (Duration <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01((currentTime - StartedAt) / Duration);
        }

        public void Clear()
        {
            DamageAmount = 0f;
            StartedAt = 0f;
            Duration = 0f;
        }
    }
}
