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

        public SkillElement? ActiveDamageOverTime =>
            _damageOverTime.GetActiveElement(Time.time);

        public void Configure(SkillBalanceProfile skillBalance)
        {
            balance = skillBalance;
        }

        private void Awake()
        {
            _health = GetComponent<CharacterHealth>();
            _crowdControl = GetComponent<CharacterStatusController>();
        }

        public bool ApplyOnHit(
            SkillElement element,
            GameObject source,
            float attackerAttackPower,
            float currentTime)
        {
            if (!enabled || balance == null || _health == null || _health.IsDead)
            {
                return false;
            }

            _lastSource = source;
            switch (element)
            {
                case SkillElement.Fire:
                    _damageOverTime.ApplyBurn(
                        currentTime,
                        balance.BurnDuration,
                        balance.ElementTickInterval,
                        attackerAttackPower * balance.BurnAttackCoefficient);
                    return true;
                case SkillElement.Poison:
                    _damageOverTime.ApplyPoison(
                        currentTime,
                        balance.PoisonDuration,
                        balance.ElementTickInterval,
                        _health.MaximumHealth * balance.PoisonMaximumHealthCoefficient);
                    return true;
                case SkillElement.Ice:
                    return _crowdControl != null &&
                           _crowdControl.Apply(CrowdControlType.Slow, currentTime);
                default:
                    return false;
            }
        }

        public void Clear()
        {
            _damageOverTime.Clear();
            _lastSource = null;
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
