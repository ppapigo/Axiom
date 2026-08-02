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
        private readonly HashSet<CharacterHealth> _spreadTargets = new HashSet<CharacterHealth>();
        private UnityEngine.Camera _aimCamera;
        private SkillBalanceProfile _balance;
        private CharacterStats _stats;
        private CharacterRole _role;
        private TeamMember _team;
        private SkillBuilderPanel _skillBuilder;
        private SkillDraft _qDraft;
        private SkillDraft _eDraft;
        private SkillDraft _ultimateDraft;
        private CharacterStatusController _status;
        private CharacterHealth _health;
        private CharacterShieldController _shield;
        private CharacterController _characterController;

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
                _skillBuilder.DraftSaved -= ApplyDraft;
            }

            _aimCamera = aimCamera;
            _balance = balance;
            _skillBuilder = skillBuilder;
            if (_skillBuilder != null)
            {
                _skillBuilder.DraftSaved += ApplyDraft;
                ApplySavedDraft(SkillSlot.Q);
                ApplySavedDraft(SkillSlot.E);
                ApplySavedDraft(SkillSlot.Ultimate);
            }
        }

        private void Awake()
        {
            _stats = GetComponent<CharacterStats>();
            _role = GetComponent<CharacterRole>();
            _team = GetComponent<TeamMember>();
            _status = GetComponent<CharacterStatusController>();
            _health = GetComponent<CharacterHealth>();
            _shield = GetComponent<CharacterShieldController>();
            _characterController = GetComponent<CharacterController>();
        }

        private void OnDisable()
        {
            _cooldowns.Reset();
        }

        private void OnDestroy()
        {
            if (_skillBuilder != null)
            {
                _skillBuilder.DraftSaved -= ApplyDraft;
            }
        }

        private void ApplySavedDraft(SkillSlot slot)
        {
            if (_skillBuilder.TryGetSavedDraft(slot, out SkillDraft draft))
            {
                ApplyDraft(draft);
            }
        }

        private void ApplyDraft(SkillDraft draft)
        {
            switch (draft.Slot)
            {
                case SkillSlot.Q:
                    _qDraft = draft;
                    break;
                case SkillSlot.E:
                    _eDraft = draft;
                    break;
                case SkillSlot.Ultimate:
                    _ultimateDraft = draft;
                    break;
            }
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
            ApplyUtilityEffects(definition, aimPoint);
            if (definition.Element == SkillElement.Water && _health != null)
            {
                _health.RestoreHealth(_health.MaximumHealth * _balance.WaterHealingRatio);
            }
            ShowEffect(plan);
        }

        private void ApplyUtilityEffects(
            in SkillDefinition definition,
            Vector3 aimPoint)
        {
            if (definition.Heals && _health != null)
            {
                _health.RestoreHealth(
                    _health.MaximumHealth * _balance.SkillHealingMaximumHealthRatio);
            }

            if (definition.CreatesShield && _shield != null && _health != null)
            {
                _shield.ApplySkillShield(_health.MaximumHealth);
            }

            if (!definition.AddsMobility || _characterController == null ||
                (_status != null && _status.IsMovementBlocked))
            {
                return;
            }

            Vector3 direction = aimPoint - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                direction = transform.forward;
            }

            _characterController.Move(
                direction.normalized * _balance.SkillMobilityDistance);
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

                ElementStatusController elementStatus =
                    health.GetComponent<ElementStatusController>();
                ElementReactionResult reaction = elementStatus == null
                    ? ElementReactionResult.None
                    : elementStatus.ApplyOnHit(
                        definition.Element,
                        gameObject,
                        _stats.AttackPower,
                        Time.time);
                float distance = Vector3.Distance(health.transform.position, plan.Center);
                DamageRequest request = SkillRuntimeRules.CreateDamageRequest(
                    gameObject,
                    _stats.AttackPower,
                    definition,
                    _role.Definition,
                    _balance,
                    distance);
                if (reaction.Triggered)
                {
                    request = MultiplyDamage(request, reaction.DamageMultiplier);
                }
                health.ApplyDamage(request);
                if (reaction.Type == ElementReactionType.WindSpread &&
                    reaction.SpreadElement.HasValue)
                {
                    SpreadElement(
                        health,
                        reaction.SpreadElement.Value,
                        Time.time);
                }
                CharacterStatusController targetStatus =
                    health.GetComponent<CharacterStatusController>();
                if (targetStatus != null)
                {
                    targetStatus.Apply(definition.CrowdControl, Time.time);
                }

                if (definition.Type == SkillType.Target)
                {
                    break;
                }
            }
        }

        private void SpreadElement(
            CharacterHealth origin,
            SkillElement element,
            float currentTime)
        {
            _spreadTargets.Clear();
            Collider[] colliders = Physics.OverlapSphere(
                origin.transform.position,
                _balance.WindSpreadRadius);
            foreach (Collider collider in colliders)
            {
                CharacterHealth target = collider.GetComponentInParent<CharacterHealth>();
                TeamMember targetTeam = target == null ? null : target.GetComponent<TeamMember>();
                if (target == null || target == origin || target.IsDead ||
                    targetTeam == null || targetTeam.Team == _team.Team ||
                    !_spreadTargets.Add(target))
                {
                    continue;
                }

                ElementStatusController elementStatus =
                    target.GetComponent<ElementStatusController>();
                elementStatus?.ApplyOnHit(
                    element,
                    gameObject,
                    _stats.AttackPower,
                    currentTime);
            }
        }

        private static DamageRequest MultiplyDamage(
            in DamageRequest request,
            float multiplier)
        {
            return new DamageRequest(
                request.Attacker,
                request.AttackPower,
                request.DamageCoefficient * Mathf.Max(0f, multiplier),
                request.CastDelayBonus,
                request.DistanceMultiplier,
                request.DamageLimit);
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

            if (definition.Type == SkillType.Global)
            {
                return Physics.OverlapSphere(plan.Origin, 1000f);
            }

            if (definition.Type == SkillType.Target)
            {
                return Physics.OverlapSphere(
                    plan.Center,
                    Mathf.Max(0.5f, definition.Radius));
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
            SkillDefinition baseDefinition = CreateDefinition(
                "E Skill", SkillSlot.E, type, 1.8f, 7f, range, 3f, element);
            return SkillDraftApplier.Apply(
                baseDefinition,
                _eDraft,
                _role == null ? null : _role.Definition,
                _balance);
        }

        private SkillDefinition CreateUltimate()
        {
            SkillType type = _role.Definition.RoleId == CharacterRoleId.Mage
                ? SkillType.GroundArea
                : SkillType.Projectile;
            SkillElement element = GetDefaultUltimateElement(_role.Definition.RoleId);
            SkillDefinition baseDefinition = CreateDefinition(
                "Ultimate", SkillSlot.Ultimate, type, 3f, 15f, 8f, 3f,
                element);
            return SkillDraftApplier.Apply(
                baseDefinition,
                _ultimateDraft,
                _role == null ? null : _role.Definition,
                _balance);
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
                                        plan.Type == SkillType.SelfArea ||
                                        plan.Type == SkillType.Global
                ? plan.Center + (Vector3.up * 0.15f)
                : plan.Origin + (plan.Direction * 2f) + (Vector3.up * 0.5f);
            float size = plan.Type == SkillType.Global
                ? 18f
                : plan.Type == SkillType.GroundArea || plan.Type == SkillType.SelfArea
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
