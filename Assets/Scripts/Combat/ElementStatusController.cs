using Axiom.Data;
using Axiom.Skill;
using UnityEngine;

namespace Axiom.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterHealth))]
    public sealed class ElementStatusController : MonoBehaviour
    {
        [SerializeField] private SkillBalanceProfile balance;
        private readonly ElementDamageOverTimeState _damageOverTime =
            new ElementDamageOverTimeState();
        private CharacterHealth _health;
        private CharacterStatusController _crowdControl;
        private GameObject _lastSource;
        private SkillElement? _markedElement;
        private float _markExpiresAt;
        private ElementReactionType _lastReaction;
        private float _reactionDisplayUntil;

        public SkillElement? ActiveDamageOverTime =>
            _damageOverTime.GetActiveElement(Time.time);
        public SkillElement? ActiveElementMark => Time.time < _markExpiresAt
            ? _markedElement
            : null;
        public ElementReactionType LastReaction => Time.time < _reactionDisplayUntil
            ? _lastReaction
            : ElementReactionType.None;

        public void Configure(SkillBalanceProfile skillBalance)
        {
            balance = skillBalance;
        }

        private void Awake()
        {
            _health = GetComponent<CharacterHealth>();
            _crowdControl = GetComponent<CharacterStatusController>();
        }

        public ElementReactionResult ApplyOnHit(
            SkillElement element,
            GameObject source,
            float attackerAttackPower,
            float currentTime)
        {
            if (!enabled || balance == null || _health == null || _health.IsDead)
            {
                return ElementReactionResult.None;
            }

            _lastSource = source;
            ElementReactionResult reaction = ResolveReaction(element, currentTime);
            if (reaction.Triggered)
            {
                _markedElement = null;
                _markExpiresAt = 0f;
                _lastReaction = reaction.Type;
                _reactionDisplayUntil = currentTime + 1f;
                if (_crowdControl != null &&
                    reaction.CrowdControl != CrowdControlType.None)
                {
                    _crowdControl.Apply(reaction.CrowdControl, currentTime);
                }
            }
            else
            {
                _markedElement = element;
                _markExpiresAt = currentTime + balance.ElementMarkDuration;
            }

            switch (element)
            {
                case SkillElement.Fire:
                    _damageOverTime.ApplyBurn(
                        currentTime,
                        balance.BurnDuration,
                        balance.ElementTickInterval,
                        attackerAttackPower * balance.BurnAttackCoefficient);
                    break;
                case SkillElement.Poison:
                    _damageOverTime.ApplyPoison(
                        currentTime,
                        balance.PoisonDuration,
                        balance.ElementTickInterval,
                        _health.MaximumHealth * balance.PoisonMaximumHealthCoefficient);
                    break;
                case SkillElement.Ice:
                    _crowdControl?.Apply(CrowdControlType.Slow, currentTime);
                    break;
            }

            return reaction;
        }

        private ElementReactionResult ResolveReaction(
            SkillElement incoming,
            float currentTime)
        {
            if (!_markedElement.HasValue || currentTime >= _markExpiresAt)
            {
                return ElementReactionResult.None;
            }

            return ElementReactionResolver.Resolve(
                _markedElement.Value,
                incoming,
                balance.FireWaterDamageMultiplier,
                balance.WaterIceDamageMultiplier,
                balance.FireIceDamageMultiplier);
        }

        public void Clear()
        {
            _damageOverTime.Clear();
            _lastSource = null;
            _markedElement = null;
            _markExpiresAt = 0f;
            _lastReaction = ElementReactionType.None;
            _reactionDisplayUntil = 0f;
        }

        private void Update()
        {
            if (_health == null || _health.IsDead)
            {
                return;
            }

            float damage = _damageOverTime.ConsumeDamage(Time.time);
            if (damage > 0f)
            {
                _health.ApplyDamage(new DamageRequest(_lastSource, damage, 1f, 1f, 1f));
            }
        }

        private void OnDisable()
        {
            Clear();
        }
    }
}
