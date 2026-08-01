using System;
using System.Collections;
using System.Collections.Generic;
using Axiom.Combat;
using UnityEngine;

namespace Axiom.Manager
{
    [DisallowMultipleComponent]
    public sealed class ThreeVsThreeMatchManager : MonoBehaviour
    {
        private const int RequiredTeamSize = 3;

        [Header("Rules")]
        [SerializeField, Min(1)] private int winsRequired = 2;
        [SerializeField, Min(0f)] private float nextRoundDelay = 2f;
        [SerializeField] private bool startAutomatically = true;

        [Header("Teams")]
        [SerializeField] private MatchParticipant[] teamA = new MatchParticipant[RequiredTeamSize];
        [SerializeField] private MatchParticipant[] teamB = new MatchParticipant[RequiredTeamSize];

        private readonly Dictionary<CharacterHealth, Action> _deathHandlers =
            new Dictionary<CharacterHealth, Action>();
        private EliminationMatchModel _match;

        public event Action<int> RoundStarted;
        public event Action<RoundResult> RoundEnded;
        public event Action<TeamId> MatchEnded;

        public MatchPhase Phase => _match?.Phase ?? MatchPhase.Waiting;
        public int RoundNumber => _match?.RoundNumber ?? 0;
        public int TeamAWins => _match?.TeamAWins ?? 0;
        public int TeamBWins => _match?.TeamBWins ?? 0;

        private void Start()
        {
            if (!ValidateConfiguration())
            {
                enabled = false;
                return;
            }

            SubscribeToDeaths(teamA);
            SubscribeToDeaths(teamB);

            if (startAutomatically)
            {
                StartMatch();
            }
        }

        private void OnDestroy()
        {
            foreach (KeyValuePair<CharacterHealth, Action> entry in _deathHandlers)
            {
                if (entry.Key != null)
                {
                    entry.Key.Died -= entry.Value;
                }
            }

            _deathHandlers.Clear();
        }

        public void StartMatch()
        {
            if (_match != null && _match.Phase != MatchPhase.Waiting &&
                _match.Phase != MatchPhase.MatchComplete)
            {
                return;
            }

            _match = new EliminationMatchModel(RequiredTeamSize, winsRequired);
            _match.StartMatch();
            PrepareRound();
        }

        private void OnParticipantDied(TeamId defeatedTeam)
        {
            if (_match == null ||
                !_match.TryReportElimination(defeatedTeam, out RoundResult result))
            {
                return;
            }

            SetCombatEnabled(false);
            RoundEnded?.Invoke(result);

            if (result.CompletesMatch)
            {
                MatchEnded?.Invoke(result.Winner);
                return;
            }

            StartCoroutine(StartNextRoundAfterDelay());
        }

        private IEnumerator StartNextRoundAfterDelay()
        {
            if (nextRoundDelay > 0f)
            {
                yield return new WaitForSeconds(nextRoundDelay);
            }

            _match.StartNextRound();
            PrepareRound();
        }

        private void PrepareRound()
        {
            SetCombatEnabled(false);
            ResetParticipants(teamA);
            ResetParticipants(teamB);
            SetCombatEnabled(true);
            RoundStarted?.Invoke(_match.RoundNumber);
        }

        private void SubscribeToDeaths(IEnumerable<MatchParticipant> participants)
        {
            foreach (MatchParticipant participant in participants)
            {
                CharacterHealth health = participant.Health;
                TeamId team = participant.Team;
                Action handler = () => OnParticipantDied(team);
                health.Died += handler;
                _deathHandlers.Add(health, handler);
            }
        }

        private void SetCombatEnabled(bool enabledState)
        {
            SetCombatEnabled(teamA, enabledState);
            SetCombatEnabled(teamB, enabledState);
        }

        private static void SetCombatEnabled(
            IEnumerable<MatchParticipant> participants,
            bool enabledState)
        {
            foreach (MatchParticipant participant in participants)
            {
                participant.SetCombatEnabled(enabledState);
            }
        }

        private static void ResetParticipants(IEnumerable<MatchParticipant> participants)
        {
            foreach (MatchParticipant participant in participants)
            {
                participant.ResetForRound();
            }
        }

        private bool ValidateConfiguration()
        {
            if (teamA == null || teamA.Length != RequiredTeamSize ||
                teamB == null || teamB.Length != RequiredTeamSize)
            {
                Debug.LogError("3vs3 경기는 각 팀에 정확히 3명이 필요합니다.", this);
                return false;
            }

            return ValidateTeam(teamA, TeamId.TeamA) &&
                   ValidateTeam(teamB, TeamId.TeamB);
        }

        private bool ValidateTeam(
            IReadOnlyList<MatchParticipant> participants,
            TeamId expectedTeam)
        {
            var uniqueMembers = new HashSet<TeamMember>();
            for (int i = 0; i < participants.Count; i++)
            {
                MatchParticipant participant = participants[i];
                if (participant == null || !participant.IsValid(expectedTeam) ||
                    !uniqueMembers.Add(participant.TeamMember))
                {
                    Debug.LogError(
                        $"{expectedTeam}의 참가자, 팀, 체력 또는 스폰 설정이 올바르지 않습니다.",
                        this);
                    return false;
                }
            }

            return true;
        }

        private void OnValidate()
        {
            winsRequired = Mathf.Max(1, winsRequired);
            nextRoundDelay = Mathf.Max(0f, nextRoundDelay);
        }
    }
}
