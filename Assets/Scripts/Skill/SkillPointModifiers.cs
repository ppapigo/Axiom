using System;
using UnityEngine;

namespace Axiom.Skill
{
    [Serializable]
    public struct SkillPointModifiers
    {
        [SerializeField, Min(0f)] private float damageIncreasePercent;
        [SerializeField, Min(0f)] private float radiusIncrease;
        [SerializeField, Min(0f)] private float rangeIncrease;
        [SerializeField, Min(0f)] private float cooldownReduction;
        [SerializeField] private bool appliesBurnOrPoison;
        [SerializeField] private bool appliesSlow;
        [SerializeField] private bool appliesStun;
        [SerializeField] private bool appliesKnockUp;
        [SerializeField] private bool addsMobility;
        [SerializeField] private bool createsShield;
        [SerializeField] private bool heals;

        public SkillPointModifiers(
            float damageIncreasePercent = 0f,
            float radiusIncrease = 0f,
            float rangeIncrease = 0f,
            float cooldownReduction = 0f,
            bool appliesBurnOrPoison = false,
            bool appliesSlow = false,
            bool appliesStun = false,
            bool appliesKnockUp = false,
            bool addsMobility = false,
            bool createsShield = false,
            bool heals = false)
        {
            this.damageIncreasePercent = Mathf.Max(0f, damageIncreasePercent);
            this.radiusIncrease = Mathf.Max(0f, radiusIncrease);
            this.rangeIncrease = Mathf.Max(0f, rangeIncrease);
            this.cooldownReduction = Mathf.Max(0f, cooldownReduction);
            this.appliesBurnOrPoison = appliesBurnOrPoison;
            this.appliesSlow = appliesSlow;
            this.appliesStun = appliesStun;
            this.appliesKnockUp = appliesKnockUp;
            this.addsMobility = addsMobility;
            this.createsShield = createsShield;
            this.heals = heals;
        }

        public float DamageIncreasePercent => Mathf.Max(0f, damageIncreasePercent);
        public float RadiusIncrease => Mathf.Max(0f, radiusIncrease);
        public float RangeIncrease => Mathf.Max(0f, rangeIncrease);
        public float CooldownReduction => Mathf.Max(0f, cooldownReduction);
        public bool AppliesBurnOrPoison => appliesBurnOrPoison;
        public bool AppliesSlow => appliesSlow;
        public bool AppliesStun => appliesStun;
        public bool AppliesKnockUp => appliesKnockUp;
        public bool AddsMobility => addsMobility;
        public bool CreatesShield => createsShield;
        public bool Heals => heals;
    }
}
