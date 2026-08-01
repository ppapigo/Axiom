using Axiom.Skill;
using UnityEngine;

namespace Axiom.Data
{
    [CreateAssetMenu(fileName = "SkillData", menuName = "Axiom/Skill/Skill Data")]
    public sealed class SkillData : ScriptableObject
    {
        [SerializeField] private string displayName = "New Skill";
        [SerializeField] private SkillSlot slot = SkillSlot.Q;
        [SerializeField] private SkillType type = SkillType.Projectile;
        [SerializeField, Min(0f)] private float damageCoefficient = 1.2f;
        [SerializeField, Min(0f)] private float cooldown = 5f;
        [SerializeField, Min(0f)] private float castDelay = 0.3f;
        [SerializeField, Min(0f)] private float range = 6f;
        [SerializeField, Min(0f)] private float radius = 0.5f;
        [SerializeField, Min(0f)] private float projectileSpeed = 10f;
        [SerializeField] private CrowdControlType crowdControl;
        [SerializeField] private SkillElement element = SkillElement.Fire;
        [SerializeField, Min(0), Tooltip("Fallback used only without a Skill Balance Profile.")]
        private int pointCost = 1;
        [Header("100 Point Build Modifiers")]
        [SerializeField] private SkillPointModifiers pointModifiers;

        public SkillDefinition Definition => new SkillDefinition(
            displayName,
            slot,
            type,
            damageCoefficient,
            cooldown,
            castDelay,
            range,
            radius,
            projectileSpeed,
            crowdControl,
            element,
            pointCost);

        public SkillDefinition CreateDefinition(SkillBalanceProfile balance)
        {
            int calculatedPointCost = balance == null
                ? pointCost
                : balance.CalculatePointCost(pointModifiers, 1);
            return new SkillDefinition(
                displayName,
                slot,
                type,
                damageCoefficient,
                cooldown,
                castDelay,
                range,
                radius,
                projectileSpeed,
                crowdControl,
                element,
                calculatedPointCost);
        }

        private void OnValidate()
        {
            damageCoefficient = Mathf.Max(0f, damageCoefficient);
            cooldown = Mathf.Max(0f, cooldown);
            castDelay = Mathf.Max(0f, castDelay);
            range = Mathf.Max(0f, range);
            radius = Mathf.Max(0f, radius);
            projectileSpeed = Mathf.Max(0f, projectileSpeed);
            pointCost = Mathf.Max(0, pointCost);
        }
    }
}
