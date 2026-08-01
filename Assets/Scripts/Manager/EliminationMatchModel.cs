using System;

namespace Axiom.Manager
{
    public sealed class EliminationMatchModel
    {
        private readonly int _teamSize;
        private readonly int _winsRequired;
        private int _teamAAlive;
        private int _teamBAlive;

        public EliminationMatchModel(int teamSize = 3, int winsRequired = 2)
        {
            if (teamSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(teamSize));
            }

            if (winsRequired <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(winsRequired));
            }

            _teamSize = teamSize;
            _winsRequired = winsRequired;
        }

        public MatchPhase Phase { get; private set; } = MatchPhase.Waiting;
        public int RoundNumber { get; private set; }
        public int TeamAWins { get; private set; }
        public int TeamBWins { get; private set; }
        public int TeamAAlive => _teamAAlive;
        public int TeamBAlive => _teamBAlive;

        public void StartMatch()
        {
            if (Phase != MatchPhase.Waiting)
            {
                throw new InvalidOperationException("Match has already started.");
            }

            TeamAWins = 0;
            TeamBWins = 0;
            RoundNumber = 0;
            StartNextRound();
        }

        public void StartNextRound()
        {
            if (Phase != MatchPhase.Waiting && Phase != MatchPhase.RoundBreak)
            {
                throw new InvalidOperationException("A new round cannot start in the current phase.");
            }

            RoundNumber++;
            _teamAAlive = _teamSize;
            _teamBAlive = _teamSize;
            Phase = MatchPhase.RoundActive;
        }

        public bool TryReportElimination(TeamId defeatedTeam, out RoundResult result)
        {
            result = default;
            if (Phase != MatchPhase.RoundActive)
            {
                return false;
            }

            if (defeatedTeam == TeamId.TeamA)
            {
                _teamAAlive = Math.Max(0, _teamAAlive - 1);
                if (_teamAAlive > 0)
                {
                    return false;
                }

                TeamBWins++;
                result = CompleteRound(TeamId.TeamB);
                return true;
            }

            _teamBAlive = Math.Max(0, _teamBAlive - 1);
            if (_teamBAlive > 0)
            {
                return false;
            }

            TeamAWins++;
            result = CompleteRound(TeamId.TeamA);
            return true;
        }

        private RoundResult CompleteRound(TeamId winner)
        {
            bool completesMatch = TeamAWins >= _winsRequired || TeamBWins >= _winsRequired;
            Phase = completesMatch ? MatchPhase.MatchComplete : MatchPhase.RoundBreak;
            return new RoundResult(
                RoundNumber,
                winner,
                TeamAWins,
                TeamBWins,
                completesMatch);
        }
    }
}
