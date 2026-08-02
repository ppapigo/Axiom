using Axiom.Data;
using Axiom.Role;
using UnityEngine;

namespace Axiom.Character
{
    [DisallowMultipleComponent]
    public sealed class CharacterStats : MonoBehaviour
    {
        [SerializeField] private CharacterStatsProfile statsProfile;
        [SerializeField] private CharacterRole characterRole;
        private readonly AttackPowerModifierState _attackPowerModifier =
            new AttackPowerModifierState();

        public bool IsConfigured => ResolveRoleDefinition() != null || statsProfile != null;
        public float MaximumHealth => ResolveRoleDefinition()?.MaximumHealth
            ?? (statsProfile == null ? 0f : statsProfile.MaximumHealth);
        public float AttackPower => BaseAttackPower * AttackPowerMultiplier;
        public float AttackPowerMultiplier => _attackPowerModifier.GetMultiplier(Time.time);

        private float BaseAttackPower => ResolveRoleDefinition()?.AttackPower
            ?? (statsProfile == null ? 0f : statsProfile.AttackPower);

        public void ApplyAttackPowerMultiplier(
            float multiplier,
            float currentTime,
            float duration)
        {
            _attackPowerModifier.Apply(multiplier, currentTime, duration);
        }

        public void ClearModifiers()
        {
            _attackPowerModifier.Clear();
        }

        private CharacterRoleDefinition ResolveRoleDefinition()
        {
            if (characterRole == null)
            {
                characterRole = GetComponent<CharacterRole>();
            }

            return characterRole != null && characterRole.IsConfigured
                ? characterRole.Definition
                : null;
        }
    }
}
