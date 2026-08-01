using Axiom.Data;
using UnityEngine;

namespace Axiom.Combat
{
    [DisallowMultipleComponent]
    public sealed class CharacterShieldController : MonoBehaviour
    {
        [SerializeField] private SkillBalanceProfile balance;
        private readonly ShieldState _state = new ShieldState();

        public float CurrentShield => _state.GetAmount(Time.time);

        public void Configure(SkillBalanceProfile skillBalance)
        {
            balance = skillBalance;
        }

        public void ApplySkillShield(float maximumHealth)
        {
            if (balance == null)
            {
                return;
            }

            _state.Apply(
                maximumHealth * balance.SkillShieldMaximumHealthRatio,
                Time.time,
                balance.SkillShieldDuration);
        }

        public float AbsorbDamage(float damage)
        {
            return _state.Absorb(damage, Time.time);
        }

        public void Clear()
        {
            _state.Clear();
        }

        private void OnDisable()
        {
            Clear();
        }
    }
}
