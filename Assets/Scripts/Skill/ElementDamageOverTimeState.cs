using System;
using UnityEngine;

namespace Axiom.Skill
{
    public sealed class ElementDamageOverTimeState
    {
        private float _burnEndTime;
        private float _burnNextTick;
        private float _burnTickInterval;
        private float _burnDamagePerTick;
        private float _poisonEndTime;
        private float _poisonNextTick;
        private float _poisonTickInterval;
        private float _poisonDamagePerTick;
        private float _lightningEndTime;
        private float _lightningNextTick;
        private float _lightningTickInterval;
        private float _lightningDamagePerTick;

        public void ApplyBurn(
            float currentTime,
            float duration,
            float tickInterval,
            float damagePerTick)
        {
            Validate(currentTime, tickInterval, damagePerTick);
            _burnEndTime = currentTime + Mathf.Max(0f, duration);
            _burnTickInterval = tickInterval;
            _burnNextTick = currentTime + tickInterval;
            _burnDamagePerTick = damagePerTick;
        }

        public void ApplyPoison(
            float currentTime,
            float duration,
            float tickInterval,
            float damagePerTick)
        {
            Validate(currentTime, tickInterval, damagePerTick);
            _poisonEndTime = currentTime + Mathf.Max(0f, duration);
            _poisonTickInterval = tickInterval;
            _poisonNextTick = currentTime + tickInterval;
            _poisonDamagePerTick = damagePerTick;
        }

        public void ApplyLightning(
            float currentTime,
            float duration,
            float tickInterval,
            float damagePerTick)
        {
            Validate(currentTime, tickInterval, damagePerTick);
            _lightningEndTime = currentTime + Mathf.Max(0f, duration);
            _lightningTickInterval = tickInterval;
            _lightningNextTick = currentTime + tickInterval;
            _lightningDamagePerTick = damagePerTick;
        }

        public void MultiplyActiveBurnDamage(
            float currentTime,
            float multiplier)
        {
            if (multiplier < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(multiplier));
            }

            if (_burnNextTick > 0f && currentTime < _burnEndTime)
            {
                _burnDamagePerTick *= multiplier;
            }
        }

        public float ConsumeDamage(float currentTime)
        {
            float damage = Consume(
                currentTime,
                ref _burnNextTick,
                _burnEndTime,
                _burnTickInterval,
                _burnDamagePerTick);
            damage += Consume(
                currentTime,
                ref _poisonNextTick,
                _poisonEndTime,
                _poisonTickInterval,
                _poisonDamagePerTick);
            damage += Consume(
                currentTime,
                ref _lightningNextTick,
                _lightningEndTime,
                _lightningTickInterval,
                _lightningDamagePerTick);
            return damage;
        }

        public SkillElement? GetActiveElement(float currentTime)
        {
            if (_burnNextTick > 0f && currentTime < _burnEndTime)
            {
                return SkillElement.Fire;
            }

            if (_lightningNextTick > 0f && currentTime < _lightningEndTime)
            {
                return SkillElement.Lightning;
            }

            return _poisonNextTick > 0f && currentTime < _poisonEndTime
                ? SkillElement.Poison
                : null;
        }

        public void Clear()
        {
            _burnEndTime = 0f;
            _burnNextTick = 0f;
            _burnTickInterval = 0f;
            _burnDamagePerTick = 0f;
            _poisonEndTime = 0f;
            _poisonNextTick = 0f;
            _poisonTickInterval = 0f;
            _poisonDamagePerTick = 0f;
            _lightningEndTime = 0f;
            _lightningNextTick = 0f;
            _lightningTickInterval = 0f;
            _lightningDamagePerTick = 0f;
        }

        private static float Consume(
            float currentTime,
            ref float nextTick,
            float endTime,
            float interval,
            float damagePerTick)
        {
            if (nextTick <= 0f || interval <= 0f)
            {
                return 0f;
            }

            float damage = 0f;
            while (nextTick <= currentTime && nextTick <= endTime)
            {
                damage += damagePerTick;
                nextTick += interval;
            }

            return damage;
        }

        private static void Validate(float currentTime, float tickInterval, float damagePerTick)
        {
            if (currentTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(currentTime));
            }

            if (tickInterval <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(tickInterval));
            }

            if (damagePerTick < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(damagePerTick));
            }
        }
    }
}
