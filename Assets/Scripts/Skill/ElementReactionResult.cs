namespace Axiom.Skill
{
    public readonly struct ElementReactionResult
    {
        public ElementReactionResult(
            ElementReactionType type,
            float damageMultiplier,
            CrowdControlType crowdControl)
        {
            Type = type;
            DamageMultiplier = damageMultiplier;
            CrowdControl = crowdControl;
        }

        public ElementReactionType Type { get; }
        public float DamageMultiplier { get; }
        public CrowdControlType CrowdControl { get; }
        public bool Triggered => Type != ElementReactionType.None;

        public static ElementReactionResult None => new ElementReactionResult(
            ElementReactionType.None,
            1f,
            CrowdControlType.None);
    }
}
