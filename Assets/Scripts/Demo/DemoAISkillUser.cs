using System;
using Axiom.AI;
using Axiom.Combat;
using Axiom.Data;
using Axiom.Role;
using Axiom.Skill;
using UnityEngine;

namespace Axiom.Demo
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterRole))]
    [RequireComponent(typeof(CharacterHealth))]
    [RequireComponent(typeof(DemoSkillController))]
    public sealed class DemoAISkillUser : AISkillUserBehaviour
    {
        [SerializeField, Min(1)] private int tankNearbyEnemyCount = 2;
        [SerializeField, Min(1)] private int mageNearbyEnemyCount = 2;
        [SerializeField, Range(0f, 1f)] private float assassinHealthThreshold = 0.4f;

        private DemoSkillController _skills;
        private CharacterRole _role;

        public override bool CanUseSkill
        {
            get
            {
                if (!IsConfigured())
                {
                    return false;
                }

                return _skills.CanCast(GetPreferredSlot(), Time.time);
            }
        }

        public void Configure(SkillBalanceProfile balance)
        {
            if (balance == null)
            {
                throw new ArgumentNullException(nameof(balance));
            }

            CacheComponents();
            _skills.Configure(
                aimCamera: null,
                balance: balance,
                skillBuilder: null);
            EquipRolePresets();
        }

        public override bool CanUseSkillOn(Transform target, int nearbyEnemyCount)
        {
            if (!CanUseSkill || target == null)
            {
                return false;
            }

            CharacterHealth targetHealth = target.GetComponent<CharacterHealth>();
            if (targetHealth == null || targetHealth.IsDead)
            {
                return false;
            }

            SkillSlot slot = GetPreferredSlot();
            SkillDefinition definition = _skills.GetSkillDefinition(slot);
            float healthRatio = targetHealth.MaximumHealth <= 0f
                ? 0f
                : targetHealth.CurrentHealth / targetHealth.MaximumHealth;
            Vector3 offset = target.position - transform.position;
            offset.y = 0f;
            return AIRoleTactics.ShouldUseGeneratedSkill(
                _role.Definition.RoleId,
                offset.magnitude,
                nearbyEnemyCount,
                healthRatio,
                definition.Range,
                tankNearbyEnemyCount,
                mageNearbyEnemyCount,
                assassinHealthThreshold);
        }

        public override bool TryUseSkill(Transform target)
        {
            return target != null && CanUseSkill &&
                   _skills.TryCastAt(
                       GetPreferredSlot(),
                       target.position,
                       Time.time);
        }

        private void Awake()
        {
            CacheComponents();
        }

        private void CacheComponents()
        {
            _skills ??= GetComponent<DemoSkillController>();
            _role ??= GetComponent<CharacterRole>();
        }

        private bool IsConfigured()
        {
            return _skills != null && _role != null && _role.IsConfigured;
        }

        private SkillSlot GetPreferredSlot()
        {
            return _role.Definition.RoleId switch
            {
                CharacterRoleId.Mage => SkillSlot.E,
                _ => SkillSlot.Q
            };
        }

        private void EquipRolePresets()
        {
            CharacterRoleId role = _role.Definition.RoleId;
            _skills.SetDraft(CreatePreset(role, SkillSlot.Q));
            _skills.SetDraft(CreatePreset(role, SkillSlot.E));
            _skills.SetDraft(CreatePreset(role, SkillSlot.Ultimate));
        }

        public static SkillDraft CreatePreset(CharacterRoleId role, SkillSlot slot)
        {
            SkillPointModifiers modifiers = role switch
            {
                CharacterRoleId.Tank when slot == SkillSlot.Q =>
                    new SkillPointModifiers(appliesStun: true, createsShield: true),
                CharacterRoleId.Tank when slot == SkillSlot.E =>
                    new SkillPointModifiers(appliesSlow: true),
                CharacterRoleId.Mage when slot == SkillSlot.E =>
                    new SkillPointModifiers(appliesSlow: true),
                CharacterRoleId.Mage when slot == SkillSlot.Ultimate =>
                    new SkillPointModifiers(
                        damageIncreasePercent: 20f,
                        appliesStun: true),
                CharacterRoleId.Assassin when slot == SkillSlot.Q =>
                    new SkillPointModifiers(
                        damageIncreasePercent: 20f,
                        rangeIncrease: 1f,
                        addsMobility: true),
                CharacterRoleId.Assassin when slot == SkillSlot.E =>
                    new SkillPointModifiers(appliesSlow: true),
                _ => new SkillPointModifiers(damageIncreasePercent: 10f)
            };

            SkillDefinition baseDefinition = DemoSkillDefinitionFactory.Create(role, slot);
            return new SkillDraft(
                modifiers,
                baseDefinition.Element,
                baseDefinition.Type,
                slot);
        }
    }
}
