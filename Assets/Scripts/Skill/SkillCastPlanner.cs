using Axiom.Role;
using UnityEngine;

namespace Axiom.Skill
{
    public static class SkillCastPlanner
    {
        public static bool TryCreate(
            in SkillDefinition definition,
            CharacterRoleDefinition role,
            Vector3 origin,
            Vector3 aimPoint,
            out SkillCastPlan plan)
        {
            Vector3 offset = aimPoint - origin;
            offset.y = 0f;

            if (definition.Type == SkillType.SelfArea)
            {
                plan = new SkillCastPlan(
                    definition.Type,
                    origin,
                    Vector3.zero,
                    origin,
                    SkillRuntimeRules.GetEffectiveRadius(definition, role));
                return true;
            }

            if (offset.sqrMagnitude <= Mathf.Epsilon || offset.magnitude > definition.Range)
            {
                plan = default;
                return false;
            }

            Vector3 direction = offset.normalized;
            Vector3 center = definition.Type == SkillType.GroundArea ||
                             definition.Type == SkillType.Target
                ? aimPoint
                : origin;
            plan = new SkillCastPlan(
                definition.Type,
                origin,
                direction,
                center,
                SkillRuntimeRules.GetEffectiveRadius(definition, role));
            return true;
        }
    }
}
