using System;
using System.Collections.Generic;
using Axiom.Character;
using Axiom.Data;
using Axiom.Input;
using Axiom.Manager;
using Axiom.Role;
using UnityEngine;

namespace Axiom.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterStats))]
    public sealed class BasicAttackController : MonoBehaviour
    {
        public event Action AttackPerformed;

        [SerializeField] private BasicAttackProfile attackProfile;
        [SerializeField] private InputActionBasicAttackSource inputSource;
        [SerializeField] private Transform attackOrigin;

        private readonly BasicAttackCooldown _cooldown = new BasicAttackCooldown();
        private CharacterStats _characterStats;
        private CharacterRole _characterRole;
        private CharacterStatusController _status;

        public void Configure(
            BasicAttackProfile profile,
            InputActionBasicAttackSource configuredInputSource = null,
            Transform configuredAttackOrigin = null)
        {
            attackProfile = profile;
            inputSource = configuredInputSource;
            attackOrigin = configuredAttackOrigin;
        }

        public float AttackRange => attackProfile == null
            ? 0f
            : _characterRole != null && _characterRole.IsConfigured
                ? _characterRole.Definition.BasicAttackRange
                : attackProfile.Parameters.Range;
        public float AttackSpeedMultiplier => _characterRole != null &&
                                              _characterRole.IsConfigured
            ? _characterRole.Definition.AttackSpeedMultiplier
            : 1f;
        public float EffectiveCooldown => attackProfile == null
            ? 0f
            : BasicAttackSpeedRules.GetCooldown(
                attackProfile.Parameters.Cooldown,
                AttackSpeedMultiplier);

        private void Awake()
        {
            _characterStats = GetComponent<CharacterStats>();
            _characterRole = GetComponent<CharacterRole>();
            _status = GetComponent<CharacterStatusController>();
        }

        private void OnDisable()
        {
            _cooldown.Reset();
        }

        private void Update()
        {
            if (inputSource == null)
            {
                return;
            }

            Transform originTransform = attackOrigin == null ? transform : attackOrigin;
            if (!inputSource.TryGetAimPoint(out Vector3 aimPoint) ||
                !BasicAttackAim.TryGetPlanarDirection(
                    originTransform.position,
                    aimPoint,
                    out Vector3 direction))
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

            if (!inputSource.WasBasicAttackPressedThisFrame())
            {
                return;
            }

            TryAttack(direction, Time.time);
        }

        public bool TryAttack(Vector3 direction, float currentTime)
        {
            if (attackProfile == null || _characterStats == null ||
                !_characterStats.IsConfigured ||
                (_status != null && _status.IsActionBlocked))
            {
                return false;
            }

            direction.y = 0f;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return false;
            }

            direction.Normalize();
            BasicAttackParameters parameters = attackProfile.Parameters;
            if (!RoleAttackRules.CanUseBasicAttack(
                    _characterRole == null ? null : _characterRole.Definition,
                    attackProfile.DeliveryType) ||
                !_cooldown.TryStart(
                    currentTime,
                    BasicAttackSpeedRules.GetCooldown(
                        parameters.Cooldown,
                        AttackSpeedMultiplier)))
            {
                return false;
            }

            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            Transform originTransform = attackOrigin == null ? transform : attackOrigin;
            ExecuteAttack(
                originTransform.position,
                direction,
                AttackRange,
                parameters);
            AttackPerformed?.Invoke();
            return true;
        }

        private void ExecuteAttack(
            Vector3 origin,
            Vector3 direction,
            float range,
            in BasicAttackParameters parameters)
        {
            Vector3 end = origin + (direction * Mathf.Max(0f, range));
            Collider[] colliders = Physics.OverlapCapsule(
                origin,
                end,
                parameters.Radius,
                attackProfile.TargetLayers,
                attackProfile.TriggerInteraction);

            var notifiedReceivers = new HashSet<IBasicAttackReceiver>();
            TeamMember attackerTeam = GetComponent<TeamMember>();
            var hit = new BasicAttackHit(
                gameObject,
                origin,
                direction,
                _characterStats.AttackPower,
                parameters.DamageCoefficient,
                parameters.CastDelayBonus,
                parameters.DistanceMultiplier);

            foreach (Collider hitCollider in colliders)
            {
                var receiverComponent = hitCollider.GetComponentInParent(typeof(IBasicAttackReceiver));
                if (receiverComponent is not IBasicAttackReceiver receiver ||
                    receiverComponent.transform.root == transform.root ||
                    !notifiedReceivers.Add(receiver))
                {
                    continue;
                }

                TeamMember receiverTeam = receiverComponent.GetComponent<TeamMember>();
                if (attackerTeam != null && receiverTeam != null &&
                    attackerTeam.Team == receiverTeam.Team)
                {
                    continue;
                }

                receiver.ReceiveBasicAttack(hit);
            }
        }
    }
}
