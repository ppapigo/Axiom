namespace Axiom.AI
{
    public readonly struct AIDecisionContext
    {
        public AIDecisionContext(
            bool isDead,
            bool hasTarget,
            bool shouldRetreat,
            bool shouldUseSkill,
            bool isInAttackRange)
        {
            IsDead = isDead;
            HasTarget = hasTarget;
            ShouldRetreat = shouldRetreat;
            ShouldUseSkill = shouldUseSkill;
            IsInAttackRange = isInAttackRange;
        }

        public bool IsDead { get; }
        public bool HasTarget { get; }
        public bool ShouldRetreat { get; }
        public bool ShouldUseSkill { get; }
        public bool IsInAttackRange { get; }
    }
}
