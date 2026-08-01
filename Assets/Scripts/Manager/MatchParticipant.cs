using System;
using Axiom.Combat;
using UnityEngine;

namespace Axiom.Manager
{
    [Serializable]
    public sealed class MatchParticipant
    {
        [SerializeField] private TeamMember teamMember;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Behaviour[] roundBehaviours;

        public TeamMember TeamMember => teamMember;
        public Transform SpawnPoint => spawnPoint;
        public TeamId Team => teamMember.Team;
        public CharacterHealth Health => teamMember == null
            ? null
            : teamMember.GetComponent<CharacterHealth>();

        public bool IsValid(TeamId expectedTeam)
        {
            return teamMember != null && spawnPoint != null &&
                   Health != null && Team == expectedTeam;
        }

        public void SetCombatEnabled(bool enabled)
        {
            if (roundBehaviours == null)
            {
                return;
            }

            foreach (Behaviour behaviour in roundBehaviours)
            {
                if (behaviour != null)
                {
                    behaviour.enabled = enabled;
                }
            }
        }

        public void ResetForRound()
        {
            CharacterController controller = teamMember.GetComponent<CharacterController>();
            bool restoreController = controller != null && controller.enabled;
            if (restoreController)
            {
                controller.enabled = false;
            }

            teamMember.transform.SetPositionAndRotation(
                spawnPoint.position,
                spawnPoint.rotation);

            if (restoreController)
            {
                controller.enabled = true;
            }

            Health.ResetHealth();
        }
    }
}
