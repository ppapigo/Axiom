namespace Axiom.Skill
{
    public readonly struct SkillDefinition
    {
        public SkillDefinition(
            string displayName,
            SkillSlot slot,
            SkillType type,
            float damageCoefficient,
            float cooldown,
            float castDelay,
            float range,
            float radius,
            float projectileSpeed,
            CrowdControlType crowdControl,
            SkillElement element,
            int pointCost,
            bool addsMobility = false,
            bool createsShield = false,
            bool heals = false)
        {
            DisplayName = displayName;
            Slot = slot;
            Type = type;
            DamageCoefficient = damageCoefficient;
            Cooldown = cooldown;
            CastDelay = castDelay;
            Range = range;
            Radius = radius;
            ProjectileSpeed = projectileSpeed;
            CrowdControl = crowdControl;
            Element = element;
            PointCost = pointCost;
            AddsMobility = addsMobility;
            CreatesShield = createsShield;
            Heals = heals;
        }

        public string DisplayName { get; }
        public SkillSlot Slot { get; }
        public SkillType Type { get; }
        public float DamageCoefficient { get; }
        public float Cooldown { get; }
        public float CastDelay { get; }
        public float Range { get; }
        public float Radius { get; }
        public float ProjectileSpeed { get; }
        public CrowdControlType CrowdControl { get; }
        public SkillElement Element { get; }
        public int PointCost { get; }
        public bool AddsMobility { get; }
        public bool CreatesShield { get; }
        public bool Heals { get; }
    }
}
