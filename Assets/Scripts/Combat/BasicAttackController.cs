using System.Collections.Generic;
using Axiom.Character;
using Axiom.Data;
using Axiom.Input;
using Axiom.Role;
using UnityEngine;

namespace Axiom.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterStats))]
    public sealed class BasicAttackController : MonoBehaviour
    {
        [SerializeField] private BasicAttackProfile attackProfile;
        [SerializeField] private InputActionBasicAttackSource inputSource;
        [SerializeField] private Transform attackOrigin;

        private readonly BasicAttackCooldown _cooldown = new BasicAttackCooldown();
        private CharacterStats _characterStats;
        private CharacterRole _characterRole;

        private void Awake()
        {
            _characterStats = GetComponent<CharacterStats>();
            _characterRole = GetComponent<CharacterRole>();
        }

        private void OnDisable()
        {
            _cooldown.Reset();
        }

        private void Update()
        {
            if (attackProfile == null || inputSource == null || !_characterStats.IsConfigured)
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

            BasicAttackParameters parameters = attackProfile.Parameters;
            if (!inputSource.WasBasicAttackPressedThisFrame() ||
                !RoleAttackRules.CanUseBasicAttack(
                    _characterRole == null ? null : _characterRole.Definition,
                    attackProfile.DeliveryType) ||
                !_cooldown.TryStart(Time.time, parameters.Cooldown))
            {
                return;
            }

            ExecuteAttack(originTransform.position, direction, parameters);
        }

        private void ExecuteAttack(
            Vector3 origin,
            Vector3 direction,
            in BasicAttackParameters parameters)
        {
            Vector3 end = origin + (direction * parameters.Range);
            Collider[] colliders = Physics.OverlapCapsule(
                origin,
                end,
                parameters.Radius,
                attackProfile.TargetLayers,
                attackProfile.TriggerInteraction);

            var notifiedReceivers = new HashSet<IBasicAttackReceiver>();
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

                receiver.ReceiveBasicAttack(hit);
            }
        }
    }
}
