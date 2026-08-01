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

        public bool IsConfigured => ResolveRoleDefinition() != null || statsProfile != null;
        public float MaximumHealth => ResolveRoleDefinition()?.MaximumHealth
            ?? (statsProfile == null ? 0f : statsProfile.MaximumHealth);
        public float AttackPower => ResolveRoleDefinition()?.AttackPower
            ?? (statsProfile == null ? 0f : statsProfile.AttackPower);

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
