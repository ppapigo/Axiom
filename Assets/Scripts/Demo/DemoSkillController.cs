using System.Collections;
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
        private bool _isCasting;
        private GameObject _castIndicator;

        public SkillDefinition QSkillDefinition => CreateQSkill();
        public SkillDefinition ESkillDefinition => CreateESkill();
        public SkillDefinition UltimateDefinition => CreateUltimate();
        public bool IsCasting => _isCasting;

        public float GetCooldownRemaining(SkillSlot slot, float currentTime)
        {
            return _cooldowns.GetRemaining(slot, currentTime);
        }

        public bool CanCast(SkillSlot slot, float currentTime)
        {
            return _balance != null && _role != null && _role.IsConfigured &&
                   !_isCasting && (_status == null || !_status.IsActionBlocked) &&
                   _cooldowns.GetRemaining(slot, currentTime) <= 0f;
        }

        public void SetDraft(in SkillDraft draft)
        {
            ApplyDraft(draft);
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
            StopAllCoroutines();
            _isCasting = false;
            if (_castIndicator != null)
            {
                Destroy(_castIndicator);
                _castIndicator = null;
            }
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
                TryCastFromPlayerAim(SkillSlot.Q);
            }
            else if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryCastFromPlayerAim(SkillSlot.E);
            }
            else if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                TryCastFromPlayerAim(SkillSlot.Ultimate);
            }
        }

        public bool TryCastAt(
            SkillSlot slot,
            Vector3 aimPoint,
            float currentTime)
        {
            SkillDefinition definition = GetSkillDefinition(slot);
            if (!CanCast(slot, currentTime) ||
                !SkillCastPlanner.TryCreate(
                    definition,
                    _role.Definition,
                    transform.position,
                    aimPoint,
                    out SkillCastPlan plan) ||
                !_cooldowns.TryStart(slot, currentTime, definition.Cooldown))
            {
                return false;
            }

            _isCasting = true;
            _castIndicator = ShowCastIndicator(definition, plan, aimPoint);
            StartCoroutine(ExecuteCast(definition, plan, aimPoint));
            return true;
        }

        private IEnumerator ExecuteCast(
            SkillDefinition definition,
            SkillCastPlan plan,
            Vector3 aimPoint)
        {
            if (definition.CastDelay > 0f)
            {
                yield return new WaitForSeconds(definition.CastDelay);
            }

            if (_castIndicator != null)
            {
                Destroy(_castIndicator);
                _castIndicator = null;
            }

            if (!isActiveAndEnabled || (_status != null && _status.IsActionBlocked))
            {
                _isCasting = false;
                yield break;
            }

            ApplyUtilityEffects(definition, aimPoint);
            if (definition.Element == SkillElement.Water && _health != null)
            {
                _health.RestoreHealth(
                    _health.MaximumHealth * _balance.WaterHealingRatio);
            }

            if (definition.Type == SkillType.Projectile)
            {
                SpawnProjectile(definition, plan);
            }
            else
            {
                ResolveHits(definition, plan);
                ShowEffect(plan, definition.Element);
            }

            _isCasting = false;
        }

        private void TryCastFromPlayerAim(SkillSlot slot)
        {
            if (TryGetAimPoint(out Vector3 aimPoint))
            {
                TryCastAt(slot, aimPoint, Time.time);
            }
        }

        public SkillDefinition GetSkillDefinition(SkillSlot slot)
        {
            return slot switch
            {
                SkillSlot.Q => CreateQSkill(),
                SkillSlot.E => CreateESkill(),
                SkillSlot.Ultimate => CreateUltimate(),
                _ => throw new System.ArgumentOutOfRangeException(nameof(slot), slot, null)
            };
        }

        private void SpawnProjectile(
            in SkillDefinition definition,
            in SkillCastPlan plan)
        {
            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = $"{definition.Slot} Projectile";
            Collider projectileCollider = projectileObject.GetComponent<Collider>();
            projectileCollider.enabled = false;
            Destroy(projectileCollider);
            float collisionRadius = Mathf.Clamp(definition.Radius * 0.2f, 0.12f, 0.35f);
            projectileObject.transform.position = plan.Origin + (Vector3.up * 0.65f);
            projectileObject.transform.localScale = Vector3.one * collisionRadius * 2f;
            projectileObject.GetComponent<Renderer>().material =
                DemoArenaBootstrap.CreateDemoMaterial(GetElementColor(definition.Element));
            DemoProjectile projectile = projectileObject.AddComponent<DemoProjectile>();
            SkillDefinition projectileDefinition = definition;
            Vector3 projectileDirection = plan.Direction;
            projectile.Initialize(
                transform,
                projectileDirection,
                definition.ProjectileSpeed,
                collisionRadius,
                definition.Range,
                impactPoint => ResolveProjectileImpact(
                    projectileDefinition,
                    projectileDirection,
                    impactPoint));
        }

        private void ResolveProjectileImpact(
            in SkillDefinition definition,
            Vector3 direction,
            Vector3 impactPoint)
        {
            if (this == null || !isActiveAndEnabled)
            {
                return;
            }

            float explosionRadius = Mathf.Max(0.6f, definition.Radius);
            var impactPlan = new SkillCastPlan(
                SkillType.GroundArea,
                impactPoint,
                direction,
                impactPoint,
                explosionRadius);
            ResolveHits(definition, impactPlan);
            ShowEffect(impactPlan, definition.Element);
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

                if (plan.Type == SkillType.Cone)
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
                ShowHitEffect(health.transform.position, definition.Element);
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
            if (plan.Type == SkillType.GroundArea ||
                plan.Type == SkillType.SelfArea)
            {
                return Physics.OverlapSphere(plan.Center, plan.Radius);
            }

            if (plan.Type == SkillType.Global)
            {
                return Physics.OverlapSphere(plan.Origin, 1000f);
            }

            if (plan.Type == SkillType.Target)
            {
                return Physics.OverlapSphere(
                    plan.Center,
                    Mathf.Max(0.5f, definition.Radius));
            }

            if (plan.Type == SkillType.Cone)
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
            SkillDefinition baseDefinition = DemoSkillDefinitionFactory.Create(
                _role.Definition.RoleId,
                SkillSlot.Q);
            return SkillDraftApplier.Apply(
                baseDefinition,
                _qDraft,
                _role == null ? null : _role.Definition,
                _balance);
        }

        private SkillDefinition CreateESkill()
        {
            SkillDefinition baseDefinition = DemoSkillDefinitionFactory.Create(
                _role.Definition.RoleId,
                SkillSlot.E);
            return SkillDraftApplier.Apply(
                baseDefinition,
                _eDraft,
                _role == null ? null : _role.Definition,
                _balance);
        }

        private SkillDefinition CreateUltimate()
        {
            SkillDefinition baseDefinition = DemoSkillDefinitionFactory.Create(
                _role.Definition.RoleId,
                SkillSlot.Ultimate);
            return SkillDraftApplier.Apply(
                baseDefinition,
                _ultimateDraft,
                _role == null ? null : _role.Definition,
                _balance);
        }

        private static GameObject ShowCastIndicator(
            in SkillDefinition definition,
            in SkillCastPlan plan,
            Vector3 aimPoint)
        {
            GameObject indicator = CreateVisual(
                PrimitiveType.Cylinder,
                "Cast Range Indicator",
                new Color(1f, 0.75f, 0.15f));
            float radius = plan.Type == SkillType.Global
                ? 9f
                : plan.Type == SkillType.Cone
                    ? Mathf.Max(0.5f, definition.Range * 0.5f)
                    : Mathf.Max(0.5f, plan.Radius);
            Vector3 center = plan.Type == SkillType.SelfArea ||
                             plan.Type == SkillType.Global
                ? plan.Origin
                : plan.Type == SkillType.Cone
                    ? plan.Origin + (plan.Direction * radius)
                    : aimPoint;
            center.y = 0.03f;
            indicator.transform.position = center;
            indicator.transform.localScale = new Vector3(radius, 0.015f, radius);
            return indicator;
        }

        private static void ShowEffect(
            in SkillCastPlan plan,
            SkillElement element)
        {
            GameObject effect = CreateVisual(
                PrimitiveType.Sphere,
                "Skill Impact",
                GetElementColor(element));
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
            Object.Destroy(effect, 0.3f);
        }

        private static void ShowHitEffect(Vector3 position, SkillElement element)
        {
            GameObject effect = CreateVisual(
                PrimitiveType.Sphere,
                "Skill Hit VFX",
                GetElementColor(element));
            effect.transform.position = position + (Vector3.up * 1f);
            effect.transform.localScale = Vector3.one * 0.45f;
            Object.Destroy(effect, 0.25f);
        }

        private static GameObject CreateVisual(
            PrimitiveType primitive,
            string visualName,
            Color color)
        {
            GameObject visual = GameObject.CreatePrimitive(primitive);
            visual.name = visualName;
            Collider collider = visual.GetComponent<Collider>();
            collider.enabled = false;
            Object.Destroy(collider);
            visual.GetComponent<Renderer>().material =
                DemoArenaBootstrap.CreateDemoMaterial(color);
            return visual;
        }

        private static Color GetElementColor(SkillElement element)
        {
            return element switch
            {
                SkillElement.Fire => new Color(1f, 0.25f, 0.08f),
                SkillElement.Ice => new Color(0.35f, 0.85f, 1f),
                SkillElement.Lightning => new Color(0.8f, 0.55f, 1f),
                SkillElement.Poison => new Color(0.45f, 1f, 0.2f),
                SkillElement.Water => new Color(0.15f, 0.55f, 1f),
                SkillElement.Wind => new Color(0.55f, 1f, 0.75f),
                SkillElement.Earth => new Color(0.65f, 0.4f, 0.18f),
                _ => Color.white
            };
        }
    }
}
