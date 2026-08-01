using System;
using UnityEngine;

namespace Axiom.Combat
{
    public sealed class HealthModel
    {
        public HealthModel(float maximumHealth)
        {
            if (maximumHealth <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumHealth));
            }

            MaximumHealth = maximumHealth;
            CurrentHealth = maximumHealth;
        }

        public float MaximumHealth { get; }
        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;

        public float ApplyDamage(float amount)
        {
            if (amount < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (IsDead)
            {
                return 0f;
            }

            float previousHealth = CurrentHealth;
            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            return previousHealth - CurrentHealth;
        }

        public float Restore(float amount)
        {
            if (amount < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            float previousHealth = CurrentHealth;
            CurrentHealth = Mathf.Min(MaximumHealth, CurrentHealth + amount);
            return CurrentHealth - previousHealth;
        }

        public void Reset()
        {
            CurrentHealth = MaximumHealth;
        }
    }
}

