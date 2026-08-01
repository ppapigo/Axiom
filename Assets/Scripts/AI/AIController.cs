using System.Collections.Generic;
using Axiom.Character;
using Axiom.Combat;
using Axiom.Data;
using Axiom.Manager;
using Axiom.Role;
using UnityEngine;

namespace Axiom.AI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CharacterStats))]
    [RequireComponent(typeof(CharacterHealth))]
    [RequireComponent(typeof(CharacterRole))]
    [RequireComponent(typeof(TeamMember))]
    [RequireComponent(typeof(BasicAttackController))]
    public sealed class AIController : MonoBehaviour
    {
        [SerializeField] private AIBehaviourProfile behaviourProfile;
        [SerializeField] private AISkillUserBehaviour skillUser;

        private readonly AIStateMachine _stateMachine = new AIStateMachine();
        private readonly List<AITargetCandidate> _candidates = new List<AITargetCandidate>();
        private readonly HashSet<CharacterHealth> _seenEnemies = new HashSet<CharacterHealth>();
        private readonly Collider[] _senseResults = new Collider[32];

        private CharacterController _characterController;
        private CharacterStats _stats;
        private CharacterHealth _health;
        private CharacterRole _role;
        private TeamMember _team;
        private BasicAttackController _basicAttack;
        private Transform _target;
        private float _nextThinkTime;

        public AIState CurrentState => _stateMachine.CurrentState;
        public Transform CurrentTarget => _target;

        public void Configure(
            AIBehaviourProfile profile,
            AISkillUserBehaviour configuredSkillUser = null)
        {
            behaviourProfile = profile;
            skillUser = configuredSkillUser;
        }

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _stats = GetComponent<CharacterStats>();
            _health = GetComponent<CharacterHealth>();
            _role = GetComponent<CharacterRole>();
            _team = GetComponent<TeamMember>();
            _basicAttack = GetComponent<BasicAttackController>();
        }

        private void Update()
        {
            if (behaviourProfile == null || !_role.IsConfigured || !_stats.IsConfigured)
            {
                return;
            }

            if (Time.time >= _nextThinkTime)
            {
                Think();
                _nextThinkTime = Time.time + behaviourProfile.ThinkInterval;
            }

            Act();
        }

        private void Think()
        {
            SenseEnemies();
            _target = AITargetSelector.Select(
                _role.Definition.RoleId,
                transform.position,
                _candidates);

            float distance = _target == null
                ? float.MaxValue
                : PlanarDistance(transform.position, _target.position);
            float healthRatio = _health.MaximumHealth <= 0f
                ? 0f
                : _health.CurrentHealth / _health.MaximumHealth;
            bool shouldUseSkill = skillUser != null && skillUser.CanUseSkill &&
                AIRoleTactics.ShouldUseSkill(
                    _role.Definition.RoleId,
                    distance,
                    CountEnemiesNearTarget(),
                    behaviourProfile.TankTauntRange,
                    behaviourProfile.MageClusterCount);

            _stateMachine.Evaluate(new AIDecisionContext(
                _health.IsDead,
                _target != null,
                healthRatio <= behaviourProfile.RetreatHealthRatio,
                shouldUseSkill,
                distance <= _basicAttack.AttackRange));
        }

        private void Act()
        {
            switch (CurrentState)
            {
                case AIState.Move:
                    MoveForRole();
                    break;
                case AIState.Attack:
                    AttackTarget();
                    break;
                case AIState.UseSkill:
                    skillUser.TryUseSkill(_target);
                    break;
                case AIState.Retreat:
                    MoveAwayFromTarget();
                    break;
            }
        }

        private void SenseEnemies()
        {
            _candidates.Clear();
            _seenEnemies.Clear();
            int count = Physics.OverlapSphereNonAlloc(
                transform.position,
                behaviourProfile.DetectionRange,
                _senseResults,
                behaviourProfile.TargetLayers,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                CharacterHealth enemyHealth = _senseResults[i].GetComponentInParent<CharacterHealth>();
                if (enemyHealth == null || enemyHealth.IsDead || !_seenEnemies.Add(enemyHealth))
                {
                    continue;
                }

                TeamMember enemyTeam = enemyHealth.GetComponent<TeamMember>();
                if (enemyTeam == null || enemyTeam.Team == _team.Team)
                {
                    continue;
                }

                float ratio = enemyHealth.MaximumHealth <= 0f
                    ? 0f
                    : enemyHealth.CurrentHealth / enemyHealth.MaximumHealth;
                _candidates.Add(new AITargetCandidate(enemyHealth.transform, ratio));
            }
        }

        private int CountEnemiesNearTarget()
        {
            if (_target == null)
            {
                return 0;
            }

            int count = 0;
            float radiusSquared = behaviourProfile.MageClusterRadius *
                                  behaviourProfile.MageClusterRadius;
            for (int i = 0; i < _candidates.Count; i++)
            {
                Vector3 offset = _candidates[i].Transform.position - _target.position;
                offset.y = 0f;
                if (offset.sqrMagnitude <= radiusSquared)
                {
                    count++;
                }
            }

            return count;
        }

        private void MoveForRole()
        {
            if (_target == null)
            {
                return;
            }

            if (_role.Definition.RoleId == CharacterRoleId.Mage)
            {
                float distance = PlanarDistance(transform.position, _target.position);
                if (distance < behaviourProfile.PreferredMinimumRange)
                {
                    MoveInDirection(transform.position - _target.position);
                }
                else if (distance > behaviourProfile.PreferredMaximumRange)
                {
                    MoveInDirection(_target.position - transform.position);
                }

                return;
            }

            Vector3 destination = AIRoleTactics.GetApproachPoint(
                _role.Definition.RoleId,
                _target.position,
                _target.forward,
                behaviourProfile.AssassinRearOffset);
            if (PlanarDistance(transform.position, destination) > behaviourProfile.ArrivalDistance)
            {
                MoveInDirection(destination - transform.position);
            }
        }

        private void AttackTarget()
        {
            if (_target == null)
            {
                return;
            }

            _basicAttack.TryAttack(_target.position - transform.position, Time.time);
        }

        private void MoveAwayFromTarget()
        {
            if (_target != null)
            {
                MoveInDirection(transform.position - _target.position);
            }
        }

        private void MoveInDirection(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            direction.Normalize();
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            float speed = behaviourProfile.BaseMoveSpeed *
                          _role.Definition.MovementSpeedMultiplier;
            var velocity = direction * speed;
            velocity.y = _characterController.isGrounded ? -2f : -9.81f;
            _characterController.Move(velocity * Time.deltaTime);
        }

        private static float PlanarDistance(Vector3 first, Vector3 second)
        {
            first.y = 0f;
            second.y = 0f;
            return Vector3.Distance(first, second);
        }
    }
}
