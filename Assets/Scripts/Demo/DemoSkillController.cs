using System.Collections.Generic;
using Axiom.Character;
using Axiom.Combat;
using Axiom.Data;
using Axiom.Manager;
using Axiom.Role;
using Axiom.Skill;
using Axiom.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Axiom.Demo
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterStats))]
    [RequireComponent(typeof(CharacterRole))]
    [RequireComponent(typeof(TeamMember))]
    public sealed class DemoSkillController : MonoBehaviour
    {
        private readonly SkillCooldownTracker _cooldowns = new SkillCooldownTracker();
        private readonly HashSet<CharacterHealth> _hitTargets = new HashSet<CharacterHealth>();
        private UnityEngine.Camera _aimCamera;
        private SkillBalanceProfile _balance;
        private CharacterStats _stats;
        private CharacterRole _role;
        private TeamMember _team;
        private SkillBuilderPanel _skillBuilder;
        private SkillDraft _qDraft;
        private CharacterStatusController _status;
        private CharacterHealth _health;

        public SkillDefinition QSkillDefinition => CreateQSkill();
        public SkillDefinition ESkillDefinition => CreateESkill();
        public SkillDefinition UltimateDefinition => CreateUltimate();

        public float GetCooldownRemaining(SkillSlot slot, float currentTime)
        {
            return _cooldowns.GetRemaining(slot, currentTime);
        }

        public void Configure(
            UnityEngine.Camera aimCamera,
            SkillBalanceProfile balance,
            SkillBuilderPanel skillBuilder = null)
        {
            if (_skillBuilder != null)
            {
                _skillBuilder.DraftSaved -= ApplyQDraft;
            }

            _aimCamera = aimCamera;
            _balance = balance;
            _skillBuilder = skillBuilder;
            if (_skillBuilder != null)
            {
                _skillBuilder.DraftSaved += ApplyQDraft;
                if (_skillBuilder.HasSavedDraft)
                {
                    ApplyQDraft(_skillBuilder.SavedDraft);
                }
            }
        }

        private void Awake()
        {
            _stats = GetComponent<CharacterStats>();
            _role = GetComponent<CharacterRole>();
            _team = GetComponent<TeamMember>();
            _status = GetComponent<CharacterStatusController>();
            _health = GetComponent<CharacterHealth>();
        }

        private void OnDisable()
        {
            _cooldowns.Reset();
        }

        private void OnDestroy()
        {
            if (_skillBuilder != null)
            {
                _skillBuilder.DraftSaved -= ApplyQDraft;
            }
        }

        private void ApplyQDraft(SkillDraft draft)
        {
            _qDraft = draft;
        }

        private void Update()
        {
            if (_aimCamera == null || _balance == null || Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                TryCast(CreateQSkill());
            }
            else if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryCast(CreateESkill());
            }
            else if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                TryCast(CreateUltimate());
            }
        }

        private void TryCast(in SkillDefinition definition)
        {
            if ((_status != null && _status.IsActionBlocked) ||
                !_cooldowns.TryStart(definition.Slot, Time.time, definition.Cooldown) ||
                !TryGetAimPoint(out Vector3 aimPoint) ||
                !SkillCastPlanner.TryCreate(
                    definition,
                    _role.Definition,
                    transform.position,
                    aimPoint,
                    out SkillCastPlan plan))
            {
                return;
            }

            ResolveHits(definition, plan);
            if (definition.Element == SkillElement.Water && _health != null)
            {
                _health.RestoreHealth(_health.MaximumHealth * _balance.WaterHealingRatio);
            }
            ShowEffect(plan);
        }

        private void ResolveHits(in SkillDefinition definition, in SkillCastPlan plan)
        {
            _hitTargets.Clear();
            Collider[] colliders = GetColliders(definition, plan);
            foreach (Collider hitCollider in colliders)
            {
                CharacterHealth health = hitCollider.GetComponentInParent<CharacterHealth>();
                TeamMember targetTeam = health == null ? null : health.GetComponent<TeamMember>();
                if (health == null || health.IsDead || targetTeam == null ||
                    targetTeam.Team == _team.Team || !_hitTargets.Add(health))
                {
                    continue;
                }

                if (definition.Type == SkillType.Cone)
                {
                    Vector3 toTarget = health.transform.position - plan.Origin;
                    toTarget.y = 0f;
                    if (Vector3.Angle(plan.Direction, toTarget) > 45f)
                    {
                        continue;
                    }
                }

                float distance = Vector3.Distance(health.transform.position, plan.Center);
                DamageRequest request = SkillRuntimeRules.CreateDamageRequest(
                    gameObject,
                    _stats.AttackPower,
                    definition,
                    _role.Definition,
                    _balance,
                    distance);
                health.ApplyDamage(request);
                CharacterStatusController targetStatus =
                    health.GetComponent<CharacterStatusController>();
                if (targetStatus != null)
                {
                    targetStatus.Apply(definition.CrowdControl, Time.time);
                }

                ElementStatusController elementStatus =
                    health.GetComponent<ElementStatusController>();
                if (elementStatus != null)
                {
                    elementStatus.ApplyOnHit(
                        definition.Element,
                        gameObject,
                        _stats.AttackPower,
                        Time.time);
                }
            }
        }

        private static Collider[] GetColliders(
            in SkillDefinition definition,
            in SkillCastPlan plan)
        {
            if (definition.Type == SkillType.GroundArea ||
                definition.Type == SkillType.SelfArea)
            {
                return Physics.OverlapSphere(plan.Center, plan.Radius);
            }

            if (definition.Type == SkillType.Cone)
            {
                return Physics.OverlapSphere(plan.Origin, definition.Range);
            }

            Vector3 end = plan.Origin + (plan.Direction * definition.Range);
            return Physics.OverlapCapsule(plan.Origin, end, Mathf.Max(0.3f, definition.Radius));
        }

        private bool TryGetAimPoint(out Vector3 point)
        {
            if (Mouse.current == null)
            {
                point = transform.position + transform.forward;
                return true;
            }

            Ray ray = _aimCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            var plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }

            point = default;
            return false;
        }

        private SkillDefinition CreateQSkill()
        {
            SkillType type = _role.Definition.RoleId == CharacterRoleId.Tank
                ? SkillType.Cone
                : SkillType.Projectile;
            float range = type == SkillType.Cone ? 3f : 7f;
            SkillDefinition baseDefinition = CreateDefinition(
                "Q Skill", SkillSlot.Q, type, 1.2f, 4f, range, 1.5f,
                GetDefaultQElement(_role.Definition.RoleId));
            return SkillDraftApplier.Apply(
                baseDefinition,
                _qDraft,
                _role == null ? null : _role.Definition,
                _balance);
        }

        private SkillDefinition CreateESkill()
        {
            SkillType type = _role.Definition.RoleId == CharacterRoleId.Tank
                ? SkillType.Cone
                : SkillType.GroundArea;
            float range = type == SkillType.Cone ? 3f : 6f;
            SkillElement element = GetDefaultEElement(_role.Definition.RoleId);
            return CreateDefinition(
                "E Skill", SkillSlot.E, type, 1.8f, 7f, range, 3f, element);
        }

        private SkillDefinition CreateUltimate()
        {
            SkillType type = _role.Definition.RoleId == CharacterRoleId.Mage
                ? SkillType.GroundArea
                : SkillType.Projectile;
            SkillElement element = GetDefaultUltimateElement(_role.Definition.RoleId);
            return CreateDefinition(
                "Ultimate", SkillSlot.Ultimate, type, 3f, 15f, 8f, 3f,
                element);
        }

        public static SkillElement GetDefaultQElement(CharacterRoleId role)
        {
            return role switch
            {
                CharacterRoleId.Mage => SkillElement.Fire,
                CharacterRoleId.Assassin => SkillElement.Poison,
                _ => SkillElement.Earth
            };
        }

        public static SkillElement GetDefaultEElement(CharacterRoleId role)
        {
            return role == CharacterRoleId.Mage
                ? SkillElement.Ice
                : SkillElement.Wind;
        }

        public static SkillElement GetDefaultUltimateElement(CharacterRoleId role)
        {
            return role switch
            {
                CharacterRoleId.Mage => SkillElement.Fire,
                CharacterRoleId.Assassin => SkillElement.Poison,
                _ => SkillElement.Earth
            };
        }

        private static SkillDefinition CreateDefinition(
            string name,
            SkillSlot slot,
            SkillType type,
            float coefficient,
            float cooldown,
            float range,
            float radius,
            SkillElement element)
        {
            return new SkillDefinition(
                name, slot, type, coefficient, cooldown, 0.3f,
                range, radius, 12f, CrowdControlType.None, element, 1);
        }

        private static void ShowEffect(in SkillCastPlan plan)
        {
            GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effect.name = "SkillEffect";
            Object.Destroy(effect.GetComponent<Collider>());
            effect.transform.position = plan.Type == SkillType.GroundArea ||
                                        plan.Type == SkillType.SelfArea
                ? plan.Center + (Vector3.up * 0.15f)
                : plan.Origin + (plan.Direction * 2f) + (Vector3.up * 0.5f);
            float size = plan.Type == SkillType.GroundArea || plan.Type == SkillType.SelfArea
                ? plan.Radius * 2f
                : 0.8f;
            effect.transform.localScale = new Vector3(size, 0.2f, size);
            Renderer renderer = effect.GetComponent<Renderer>();
            renderer.material = DemoArenaBootstrap.CreateDemoMaterial(
                new Color(0.2f, 0.8f, 1f, 0.7f));
            Object.Destroy(effect, 0.3f);
        }
    }
}
