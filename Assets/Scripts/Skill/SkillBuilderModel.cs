using System;
using Axiom.Data;
using UnityEngine;

namespace Axiom.Skill
{
    public sealed class SkillBuilderModel
    {
        private int _damageSteps;
        private int _radiusSteps;
        private int _rangeSteps;
        private int _cooldownSteps;
        private bool _slow;
        private bool _stun;
        private bool _knockUp;
        private bool _mobility;
        private bool _shield;
        private bool _healing;
        private SkillElement? _element;

        public float DamageIncreasePercent => _damageSteps * 10f;
        public float RadiusIncrease => _radiusSteps;
        public float RangeIncrease => _rangeSteps;
        public float CooldownReduction => _cooldownSteps;
        public SkillElement? Element => _element;
        public int SelectedElementCount => _element.HasValue ? 1 : 0;

        public void AdjustDamage(int steps)
        {
            _damageSteps = Mathf.Max(0, _damageSteps + steps);
        }

        public void AdjustRadius(int meters)
        {
            _radiusSteps = Mathf.Max(0, _radiusSteps + meters);
        }

        public void AdjustRange(int meters)
        {
            _rangeSteps = Mathf.Max(0, _rangeSteps + meters);
        }

        public void AdjustCooldownReduction(int seconds)
        {
            _cooldownSteps = Mathf.Max(0, _cooldownSteps + seconds);
        }

        public bool IsEnabled(SkillPointEffect effect)
        {
            return effect switch
            {
                SkillPointEffect.Slow => _slow,
                SkillPointEffect.Stun => _stun,
                SkillPointEffect.KnockUp => _knockUp,
                SkillPointEffect.Mobility => _mobility,
                SkillPointEffect.Shield => _shield,
                SkillPointEffect.Healing => _healing,
                _ => throw new ArgumentOutOfRangeException(nameof(effect), effect, null)
            };
        }

        public void Toggle(SkillPointEffect effect)
        {
            switch (effect)
            {
                case SkillPointEffect.Slow:
                    _slow = !_slow;
                    break;
                case SkillPointEffect.Stun:
                    _stun = !_stun;
                    break;
                case SkillPointEffect.KnockUp:
                    _knockUp = !_knockUp;
                    break;
                case SkillPointEffect.Mobility:
                    _mobility = !_mobility;
                    break;
                case SkillPointEffect.Shield:
                    _shield = !_shield;
                    break;
                case SkillPointEffect.Healing:
                    _healing = !_healing;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(effect), effect, null);
            }
        }

        public bool IsElementSelected(SkillElement element)
        {
            return _element == element;
        }

        public bool ToggleElement(SkillElement element)
        {
            if (_element == element)
            {
                _element = null;
                return true;
            }

            _element = element;
            return true;
        }

        public int GetPointCost(SkillBalanceProfile balance)
        {
            if (balance == null)
            {
                throw new ArgumentNullException(nameof(balance));
            }

            SkillPointModifiers modifiers = CreateModifiers();
            return balance.CalculatePointCost(modifiers, SelectedElementCount);
        }

        public bool IsWithinBudget(SkillBalanceProfile balance)
        {
            return GetPointCost(balance) <= balance.LoadoutPointBudget;
        }

        public SkillPointModifiers CreateModifiers()
        {
            return new SkillPointModifiers(
                damageIncreasePercent: DamageIncreasePercent,
                radiusIncrease: RadiusIncrease,
                rangeIncrease: RangeIncrease,
                cooldownReduction: CooldownReduction,
                appliesSlow: _slow,
                appliesStun: _stun,
                appliesKnockUp: _knockUp,
                addsMobility: _mobility,
                createsShield: _shield,
                heals: _healing);
        }

        public SkillDraft CreateDraft()
        {
            SkillPointModifiers modifiers = CreateModifiers();
            return new SkillDraft(modifiers, _element);
        }

        public void Reset()
        {
            _damageSteps = 0;
            _radiusSteps = 0;
            _rangeSteps = 0;
            _cooldownSteps = 0;
            _slow = false;
            _stun = false;
            _knockUp = false;
            _mobility = false;
            _shield = false;
            _healing = false;
            _element = null;
        }
    }
}
