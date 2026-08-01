using Axiom.Role;
using UnityEngine;

namespace Axiom.AI
{
    public static class AIRoleTactics
    {
        public static Vector3 GetApproachPoint(
            CharacterRoleId role,
            Vector3 targetPosition,
            Vector3 targetForward,
            float assassinRearOffset)
        {
            if (role != CharacterRoleId.Assassin)
            {
                return targetPosition;
            }

            targetForward.y = 0f;
            return targetPosition - (targetForward.normalized * assassinRearOffset);
        }

        public static bool ShouldUseSkill(
            CharacterRoleId role,
            float targetDistance,
            int enemiesNearTarget,
            float tankTauntRange,
            int mageClusterCount)
        {
            return role switch
            {
                CharacterRoleId.Tank => targetDistance <= tankTauntRange,
                CharacterRoleId.Mage => enemiesNearTarget >= mageClusterCount,
                _ => false
            };
        }
    }
}
