using UnityEngine;

namespace Axiom.Skill
{
    public readonly struct SkillCastPlan
    {
        public SkillCastPlan(
            SkillType type,
            Vector3 origin,
            Vector3 direction,
            Vector3 center,
            float radius)
        {
            Type = type;
            Origin = origin;
            Direction = direction;
            Center = center;
            Radius = radius;
        }

        public SkillType Type { get; }
        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public Vector3 Center { get; }
        public float Radius { get; }
    }
}
