using System;
using Axiom.Role;
using UnityEngine;

namespace Axiom.Combat
{
    public static class RoleDamageRules
    {
        public static bool CanApply(
            CharacterRoleDefinition role,
            DamageApplicationType applicationType)
        {
            return applicationType != DamageApplicationType.Area ||
                role == null ||
                role.AllowsAreaDamage;
        }

        public static float GetDamageLimit(
            CharacterRoleDefinition role,
            DamageApplicationType applicationType)
        {
            if (applicationType == DamageApplicationType.Area &&
                role is MageRoleDefinition mage)
            {
                return mage.MaximumAreaDamage;
            }

            return float.PositiveInfinity;
        }

        public static float ClampAreaRadius(
            CharacterRoleDefinition role,
            float requestedRadius)
        {
            if (requestedRadius < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(requestedRadius));
            }

            return role == null
                ? requestedRadius
                : Mathf.Min(requestedRadius, role.MaximumAreaRadius);
        }
    }
}
