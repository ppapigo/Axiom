namespace Axiom.AI
{
    public sealed class AIStateMachine
    {
        public AIState CurrentState { get; private set; } = AIState.Idle;

        public AIState Evaluate(in AIDecisionContext context)
        {
            if (context.IsDead)
            {
                CurrentState = AIState.Dead;
            }
            else if (!context.HasTarget)
            {
                CurrentState = AIState.FindTarget;
            }
            else if (context.ShouldRetreat)
            {
                CurrentState = AIState.Retreat;
            }
            else if (context.ShouldUseSkill)
            {
                CurrentState = AIState.UseSkill;
            }
            else if (context.IsInAttackRange)
            {
                CurrentState = AIState.Attack;
            }
            else
            {
                CurrentState = AIState.Move;
            }

            return CurrentState;
        }
    }
}
