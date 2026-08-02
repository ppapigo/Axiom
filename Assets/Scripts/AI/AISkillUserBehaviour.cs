using UnityEngine;

namespace Axiom.AI
{
    public abstract class AISkillUserBehaviour : MonoBehaviour
    {
        public abstract bool CanUseSkill { get; }
        public virtual bool CanUseSkillOn(Transform target, int nearbyEnemyCount)
        {
            return CanUseSkill && target != null;
        }

        public abstract bool TryUseSkill(Transform target);
    }
}
