using UnityEngine;

namespace Axiom.AI
{
    public abstract class AISkillUserBehaviour : MonoBehaviour
    {
        public abstract bool CanUseSkill { get; }
        public abstract bool TryUseSkill(Transform target);
    }
}
