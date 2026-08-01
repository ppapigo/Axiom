using UnityEngine;

namespace Axiom.Role
{
    [DisallowMultipleComponent]
    public sealed class CharacterRole : MonoBehaviour
    {
        [SerializeField] private CharacterRoleDefinition definition;

        public bool IsConfigured => definition != null;
        public CharacterRoleDefinition Definition => definition;
    }
}

