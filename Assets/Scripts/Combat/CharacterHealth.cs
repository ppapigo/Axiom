using System;
using Axiom.Character;
using UnityEngine;

namespace Axiom.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterStats))]
    public sealed class CharacterHealth : MonoBehaviour, IDamageable, IBasicAttackReceiver
    {
        private CharacterStats _stats;
        private HealthModel _health;

        public event Action<float, float> HealthChanged;
        public event Action Died;

        public float CurrentHealth => _health?.CurrentHealth ?? 0f;
        public float MaximumHealth => _health?.MaximumHealth ?? 0f;
        public bool IsDead => _health?.IsDead ?? false;

        private void Awake()
        {
            _stats = GetComponent<CharacterStats>();
            if (!_stats.IsConfigured)
            {
                Debug.LogError("CharacterStatsProfile이 설정되지 않았습니다.", this);
                enabled = false;
                return;
            }

            _health = new HealthModel(_stats.MaximumHealth);
        }

        public float ApplyDamage(in DamageRequest request)
        {
            if (_health == null || _health.IsDead)
            {
                return 0f;
            }

            bool wasAlive = !_health.IsDead;
            float appliedDamage = _health.ApplyDamage(DamageCalculator.Calculate(request));

            if (appliedDamage > 0f)
            {
                HealthChanged?.Invoke(_health.CurrentHealth, _health.MaximumHealth);
            }

            if (wasAlive && _health.IsDead)
            {
                Died?.Invoke();
            }

            return appliedDamage;
        }

        public void ReceiveBasicAttack(in BasicAttackHit hit)
        {
            var request = new DamageRequest(
                hit.Attacker,
                hit.AttackPower,
                hit.DamageCoefficient,
                hit.CastDelayBonus,
                hit.DistanceMultiplier);

            ApplyDamage(request);
        }

        public float RestoreHealth(float amount)
        {
            if (_health == null)
            {
                return 0f;
            }

            float restored = _health.Restore(amount);
            if (restored > 0f)
            {
                HealthChanged?.Invoke(_health.CurrentHealth, _health.MaximumHealth);
            }

            return restored;
        }

        public void ResetHealth()
        {
            if (_health == null)
            {
                return;
            }

            _health.Reset();
            HealthChanged?.Invoke(_health.CurrentHealth, _health.MaximumHealth);
        }
    }
}

