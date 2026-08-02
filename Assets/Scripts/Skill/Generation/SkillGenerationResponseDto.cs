using System;

namespace Axiom.Skill.Generation
{
    [Serializable]
    public sealed class SkillGenerationResponseDto
    {
        public string displayName;
        public string description;
        public string skillType;
        public string crowdControl;
        public string element;
        public float damageIncreasePercent;
        public float radiusIncrease;
        public float rangeIncrease;
        public float cooldownReduction;
        public bool addsMobility;
        public bool createsShield;
        public bool heals;
    }
}
