using System.Collections.Generic;
using Axiom.AI;
using Axiom.Role;
using NUnit.Framework;
using UnityEngine;

namespace Axiom.Tests.EditMode
{
    public sealed class AIBehaviourTests
    {
        [TestCase(true, true, false, false, false, AIState.Dead)]
        [TestCase(false, false, false, false, false, AIState.FindTarget)]
        [TestCase(false, true, true, true, true, AIState.Retreat)]
        [TestCase(false, true, false, true, true, AIState.UseSkill)]
        [TestCase(false, true, false, false, true, AIState.Attack)]
        [TestCase(false, true, false, false, false, AIState.Move)]
        public void StateMachine_UsesExpectedPriority(
            bool isDead,
            bool hasTarget,
            bool shouldRetreat,
            bool shouldUseSkill,
            bool inAttackRange,
            AIState expected)
        {
            var machine = new AIStateMachine();
            var context = new AIDecisionContext(
                isDead,
                hasTarget,
                shouldRetreat,
                shouldUseSkill,
                inAttackRange);

            Assert.That(machine.Evaluate(context), Is.EqualTo(expected));
            Assert.That(machine.CurrentState, Is.EqualTo(expected));
        }

        [Test]
        public void Tank_SelectsClosestEnemy()
        {
            GameObject close = new GameObject("Close");
            GameObject far = new GameObject("Far");
            close.transform.position = new Vector3(2f, 0f, 0f);
            far.transform.position = new Vector3(8f, 0f, 0f);
            var candidates = new List<AITargetCandidate>
            {
                new AITargetCandidate(far.transform, 0.1f),
                new AITargetCandidate(close.transform, 1f)
            };

            Transform selected = AITargetSelector.Select(
                CharacterRoleId.Tank,
                Vector3.zero,
                candidates);

            Assert.That(selected, Is.EqualTo(close.transform));
            Object.DestroyImmediate(close);
            Object.DestroyImmediate(far);
        }

        [Test]
        public void Assassin_SelectsLowestHealthEnemy()
        {
            GameObject healthy = new GameObject("Healthy");
            GameObject wounded = new GameObject("Wounded");
            healthy.transform.position = Vector3.right;
            wounded.transform.position = Vector3.right * 8f;
            var candidates = new List<AITargetCandidate>
            {
                new AITargetCandidate(healthy.transform, 0.9f),
                new AITargetCandidate(wounded.transform, 0.2f)
            };

            Transform selected = AITargetSelector.Select(
                CharacterRoleId.Assassin,
                Vector3.zero,
                candidates);

            Assert.That(selected, Is.EqualTo(wounded.transform));
            Object.DestroyImmediate(healthy);
            Object.DestroyImmediate(wounded);
        }

        [Test]
        public void Assassin_ApproachesBehindTarget()
        {
            Vector3 destination = AIRoleTactics.GetApproachPoint(
                CharacterRoleId.Assassin,
                new Vector3(5f, 0f, 5f),
                Vector3.forward,
                2f);

            Assert.That(destination, Is.EqualTo(new Vector3(5f, 0f, 3f)));
        }

        [Test]
        public void Mage_UsesSkillWhenEnemiesAreClustered()
        {
            bool shouldUse = AIRoleTactics.ShouldUseSkill(
                CharacterRoleId.Mage,
                10f,
                3,
                3f,
                2);

            Assert.That(shouldUse, Is.True);
        }
    }
}
