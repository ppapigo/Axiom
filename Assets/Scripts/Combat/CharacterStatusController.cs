using Axiom.Data;
using Axiom.Skill;
using UnityEngine;

namespace Axiom.Combat
{
    [DisallowMultipleComponent]
    public sealed class CharacterStatusController : MonoBehaviour
    {
        [SerializeField] private SkillBalanceProfile balance;
        private readonly CrowdControlState _state = new CrowdControlState();

        public CrowdControlType ActiveEffect => _state.GetActiveEffect(Time.time);
        public float ActiveRemainingDuration => _state.GetRemainingDuration(Time.time);
        public bool IsMovementBlocked => _state.IsMovementBlocked(Time.time);
        public bool IsActionBlocked => _state.IsActionBlocked(Time.time);
        public float MovementSpeedMultiplier => balance == null
            ? 1f
            : _state.GetMovementSpeedMultiplier(Time.time, balance.SlowMovementMultiplier);

        public void Configure(SkillBalanceProfile skillBalance)
        {
            balance = skillBalance;
        }

        public bool Apply(CrowdControlType type, float currentTime)
        {
            if (!enabled || balance == null || type == CrowdControlType.None)
            {
                return false;
            }

            float duration = balance.GetCrowdControlDuration(type);
            if (duration <= 0f)
            {
                return false;
            }

            _state.Apply(type, currentTime, duration);
            return true;
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
