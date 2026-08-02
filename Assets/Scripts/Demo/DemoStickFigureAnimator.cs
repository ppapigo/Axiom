using Axiom.Combat;
using Axiom.Role;
using UnityEngine;

namespace Axiom.Demo
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(BasicAttackController))]
    [RequireComponent(typeof(CharacterRole))]
    public sealed class DemoStickFigureAnimator : MonoBehaviour
    {
        [Header("Walk")]
        [SerializeField, Min(0.01f)] private float referenceSpeed = 4f;
        [SerializeField, Min(0f)] private float walkFrequency = 9f;
        [SerializeField, Range(0f, 60f)] private float maximumLimbSwing = 28f;
        [SerializeField, Range(0f, 0.1f)] private float idleBobHeight = 0.012f;
        [SerializeField, Range(0f, 0.15f)] private float movingBobHeight = 0.045f;

        [Header("Attack")]
        [SerializeField, Min(0.05f)] private float attackDuration = 0.2f;
        [SerializeField, Range(0f, 90f)] private float attackSwing = 55f;

        private CharacterController _controller;
        private BasicAttackController _basicAttack;
        private CharacterRole _role;
        private Transform _visualRoot;
        private Transform _leftArm;
        private Transform _rightArm;
        private Transform _leftLeg;
        private Transform _rightLeg;
        private Transform _tankShield;
        private Transform _tankShieldEmblem;
        private Transform _mageStaff;
        private Transform _mageOrb;
        private Transform _leftDagger;
        private Transform _rightDagger;
        private Vector3 _rootBasePosition;
        private Quaternion _leftArmBaseRotation;
        private Quaternion _rightArmBaseRotation;
        private Quaternion _leftLegBaseRotation;
        private Quaternion _rightLegBaseRotation;
        private Quaternion _tankShieldBaseRotation;
        private Quaternion _tankShieldEmblemBaseRotation;
        private Quaternion _mageStaffBaseRotation;
        private Quaternion _mageOrbBaseRotation;
        private Quaternion _leftDaggerBaseRotation;
        private Quaternion _rightDaggerBaseRotation;
        private Vector3 _tankShieldBasePosition;
        private Vector3 _tankShieldEmblemBasePosition;
        private Vector3 _mageStaffBasePosition;
        private Vector3 _mageOrbBasePosition;
        private Vector3 _leftDaggerBasePosition;
        private Vector3 _rightDaggerBasePosition;
        private float _attackStartedAt = float.NegativeInfinity;
        private bool _subscribed;

        public bool IsConfigured => _visualRoot != null &&
                                    _leftArm != null &&
                                    _rightArm != null &&
                                    _leftLeg != null &&
                                    _rightLeg != null;

        public void Configure(Transform visualRoot)
        {
            _visualRoot = visualRoot;
            if (_visualRoot == null)
            {
                return;
            }

            _rootBasePosition = _visualRoot.localPosition;
            _leftArm = _visualRoot.Find("Stick Left Arm");
            _rightArm = _visualRoot.Find("Stick Right Arm");
            _leftLeg = _visualRoot.Find("Stick Left Leg");
            _rightLeg = _visualRoot.Find("Stick Right Leg");
            _tankShield = _visualRoot.Find("Tank Shield");
            _tankShieldEmblem = _visualRoot.Find("Tank Shield Emblem");
            _mageStaff = _visualRoot.Find("Mage Staff");
            _mageOrb = _visualRoot.Find("Mage Orb");
            _leftDagger = _visualRoot.Find("Assassin Left Dagger");
            _rightDagger = _visualRoot.Find("Assassin Right Dagger");
            _leftArmBaseRotation = GetLocalRotation(_leftArm);
            _rightArmBaseRotation = GetLocalRotation(_rightArm);
            _leftLegBaseRotation = GetLocalRotation(_leftLeg);
            _rightLegBaseRotation = GetLocalRotation(_rightLeg);
            _tankShieldBaseRotation = GetLocalRotation(_tankShield);
            _tankShieldEmblemBaseRotation = GetLocalRotation(_tankShieldEmblem);
            _mageStaffBaseRotation = GetLocalRotation(_mageStaff);
            _mageOrbBaseRotation = GetLocalRotation(_mageOrb);
            _leftDaggerBaseRotation = GetLocalRotation(_leftDagger);
            _rightDaggerBaseRotation = GetLocalRotation(_rightDagger);
            _tankShieldBasePosition = GetLocalPosition(_tankShield);
            _tankShieldEmblemBasePosition = GetLocalPosition(_tankShieldEmblem);
            _mageStaffBasePosition = GetLocalPosition(_mageStaff);
            _mageOrbBasePosition = GetLocalPosition(_mageOrb);
            _leftDaggerBasePosition = GetLocalPosition(_leftDagger);
            _rightDaggerBasePosition = GetLocalPosition(_rightDagger);
        }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _basicAttack = GetComponent<BasicAttackController>();
            _role = GetComponent<CharacterRole>();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            ResetPose();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (!IsConfigured || _controller == null)
            {
                return;
            }

            Vector3 velocity = _controller.velocity;
            float planarSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            StickFigureMotionPose pose = StickFigureMotionCalculator.Calculate(
                planarSpeed,
                Time.time,
                Time.time - _attackStartedAt,
                referenceSpeed,
                walkFrequency,
                maximumLimbSwing,
                idleBobHeight,
                movingBobHeight,
                attackDuration);
            ApplyPose(pose);
        }

        private void ApplyPose(in StickFigureMotionPose pose)
        {
            _visualRoot.localPosition = _rootBasePosition +
                                        (Vector3.up * pose.BobOffset);
            SetRotation(
                _leftArm,
                _leftArmBaseRotation,
                Quaternion.Euler(pose.LimbSwingDegrees, 0f, 0f));
            SetRotation(
                _rightArm,
                _rightArmBaseRotation,
                Quaternion.Euler(-pose.LimbSwingDegrees, 0f, 0f));
            SetRotation(
                _leftLeg,
                _leftLegBaseRotation,
                Quaternion.Euler(-pose.LimbSwingDegrees, 0f, 0f));
            SetRotation(
                _rightLeg,
                _rightLegBaseRotation,
                Quaternion.Euler(pose.LimbSwingDegrees, 0f, 0f));

            if (_role == null || !_role.IsConfigured)
            {
                ResetEquipmentPose();
                return;
            }

            float swing = attackSwing * pose.AttackWeight;
            Vector3 leftPivot = _leftArm.localPosition;
            Vector3 rightPivot = _rightArm.localPosition;
            switch (_role.Definition.RoleId)
            {
                case CharacterRoleId.Tank:
                    AddRotation(_leftArm, Quaternion.Euler(-swing, 0f, 0f));
                    Quaternion shieldOffset = Quaternion.Euler(
                        pose.LimbSwingDegrees - swing,
                        0f,
                        0f);
                    SetAroundPivot(
                        _tankShield,
                        _tankShieldBasePosition,
                        _tankShieldBaseRotation,
                        leftPivot,
                        shieldOffset);
                    SetAroundPivot(
                        _tankShieldEmblem,
                        _tankShieldEmblemBasePosition,
                        _tankShieldEmblemBaseRotation,
                        leftPivot,
                        shieldOffset);
                    break;
                case CharacterRoleId.Mage:
                    AddRotation(_rightArm, Quaternion.Euler(-swing, 0f, 0f));
                    Quaternion staffOffset = Quaternion.Euler(
                        -pose.LimbSwingDegrees - swing,
                        0f,
                        0f);
                    SetAroundPivot(
                        _mageStaff,
                        _mageStaffBasePosition,
                        _mageStaffBaseRotation,
                        rightPivot,
                        staffOffset);
                    SetAroundPivot(
                        _mageOrb,
                        _mageOrbBasePosition,
                        _mageOrbBaseRotation,
                        rightPivot,
                        staffOffset);
                    break;
                case CharacterRoleId.Assassin:
                    AddRotation(_leftArm, Quaternion.Euler(-swing, 0f, 0f));
                    AddRotation(_rightArm, Quaternion.Euler(swing, 0f, 0f));
                    SetAroundPivot(
                        _leftDagger,
                        _leftDaggerBasePosition,
                        _leftDaggerBaseRotation,
                        leftPivot,
                        Quaternion.Euler(pose.LimbSwingDegrees - swing, 0f, 0f));
                    SetAroundPivot(
                        _rightDagger,
                        _rightDaggerBasePosition,
                        _rightDaggerBaseRotation,
                        rightPivot,
                        Quaternion.Euler(-pose.LimbSwingDegrees + swing, 0f, 0f));
                    break;
            }
        }

        private void HandleAttackPerformed()
        {
            _attackStartedAt = Time.time;
        }

        private void Subscribe()
        {
            if (_subscribed || _basicAttack == null)
            {
                return;
            }

            _basicAttack.AttackPerformed += HandleAttackPerformed;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed || _basicAttack == null)
            {
                return;
            }

            _basicAttack.AttackPerformed -= HandleAttackPerformed;
            _subscribed = false;
        }

        private void ResetPose()
        {
            if (_visualRoot != null)
            {
                _visualRoot.localPosition = _rootBasePosition;
            }
            SetRotation(_leftArm, _leftArmBaseRotation, Quaternion.identity);
            SetRotation(_rightArm, _rightArmBaseRotation, Quaternion.identity);
            SetRotation(_leftLeg, _leftLegBaseRotation, Quaternion.identity);
            SetRotation(_rightLeg, _rightLegBaseRotation, Quaternion.identity);
            ResetEquipmentPose();
        }

        private void ResetEquipmentPose()
        {
            ResetTransform(
                _tankShield,
                _tankShieldBasePosition,
                _tankShieldBaseRotation);
            ResetTransform(
                _tankShieldEmblem,
                _tankShieldEmblemBasePosition,
                _tankShieldEmblemBaseRotation);
            ResetTransform(_mageStaff, _mageStaffBasePosition, _mageStaffBaseRotation);
            ResetTransform(_mageOrb, _mageOrbBasePosition, _mageOrbBaseRotation);
            ResetTransform(_leftDagger, _leftDaggerBasePosition, _leftDaggerBaseRotation);
            ResetTransform(_rightDagger, _rightDaggerBasePosition, _rightDaggerBaseRotation);
        }

        private static Quaternion GetLocalRotation(Transform target)
        {
            return target == null ? Quaternion.identity : target.localRotation;
        }

        private static Vector3 GetLocalPosition(Transform target)
        {
            return target == null ? Vector3.zero : target.localPosition;
        }

        private static void SetRotation(
            Transform target,
            Quaternion baseRotation,
            Quaternion offset)
        {
            if (target != null)
            {
                target.localRotation = baseRotation * offset;
            }
        }

        private static void AddRotation(Transform target, Quaternion offset)
        {
            if (target != null)
            {
                target.localRotation *= offset;
            }
        }

        private static void SetAroundPivot(
            Transform target,
            Vector3 basePosition,
            Quaternion baseRotation,
            Vector3 pivot,
            Quaternion offset)
        {
            if (target == null)
            {
                return;
            }

            target.localPosition = pivot + (offset * (basePosition - pivot));
            target.localRotation = baseRotation * offset;
        }

        private static void ResetTransform(
            Transform target,
            Vector3 basePosition,
            Quaternion baseRotation)
        {
            if (target == null)
            {
                return;
            }

            target.localPosition = basePosition;
            target.localRotation = baseRotation;
        }
    }
}
