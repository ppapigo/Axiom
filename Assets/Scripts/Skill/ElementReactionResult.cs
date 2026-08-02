namespace Axiom.Skill
{
    public readonly struct ElementReactionResult
    {
        public ElementReactionResult(
            ElementReactionType type,
            float damageMultiplier,
            CrowdControlType crowdControl,
            SkillElement? spreadElement = null)
        {
            Type = type;
            DamageMultiplier = damageMultiplier;
            CrowdControl = crowdControl;
            SpreadElement = spreadElement;
        }

        public ElementReactionType Type { get; }
        public float DamageMultiplier { get; }
        public CrowdControlType CrowdControl { get; }
        public SkillElement? SpreadElement { get; }
        public bool Triggered => Type != ElementReactionType.None;

        public static ElementReactionResult None => new ElementReactionResult(
            ElementReactionType.None,
            1f,
            CrowdControlType.None);
    }
}
