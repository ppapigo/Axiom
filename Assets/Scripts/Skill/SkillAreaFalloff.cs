using System;

namespace Axiom.Skill
{
    public static class SkillAreaFalloff
    {
        public static float GetMultiplier(SkillType type, float distance)
        {
            if (distance < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(distance));
            }

            if (type == SkillType.SelfArea)
            {
                return distance <= 2f ? 1.2f : distance <= 4f ? 1f : 0.8f;
            }

            if (type == SkillType.GroundArea)
            {
                return distance <= 2f ? 1f : distance <= 4f ? 0.8f : 0.6f;
            }

            return 1f;
        }
    }
}
