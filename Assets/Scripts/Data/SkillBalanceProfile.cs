using UnityEngine;

namespace Axiom.Data
{
    [CreateAssetMenu(fileName = "SkillBalanceProfile", menuName = "Axiom/Skill/Balance Profile")]
    public sealed class SkillBalanceProfile : ScriptableObject
    {
        [SerializeField, Min(0)] private int loadoutPointBudget = 10;
        [SerializeField, Min(0f)] private float tankMaximumNonUltimateRange = 3f;
        [SerializeField] private AnimationCurve castDelayBonus =
            AnimationCurve.Linear(0f, 1f, 1.5f, 1.5f);

        public int LoadoutPointBudget => loadoutPointBudget;
        public float TankMaximumNonUltimateRange => tankMaximumNonUltimateRange;

        public float EvaluateCastDelayBonus(float castDelay)
        {
            return Mathf.Max(0f, castDelayBonus.Evaluate(Mathf.Max(0f, castDelay)));
        }

        private void OnValidate()
        {
            loadoutPointBudget = Mathf.Max(0, loadoutPointBudget);
            tankMaximumNonUltimateRange = Mathf.Max(0f, tankMaximumNonUltimateRange);
            castDelayBonus ??= AnimationCurve.Linear(0f, 1f, 1.5f, 1.5f);
        }
    }
}
