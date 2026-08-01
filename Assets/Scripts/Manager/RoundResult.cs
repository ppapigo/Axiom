namespace Axiom.Manager
{
    public readonly struct RoundResult
    {
        public RoundResult(
            int roundNumber,
            TeamId winner,
            int teamAWins,
            int teamBWins,
            bool completesMatch)
        {
            RoundNumber = roundNumber;
            Winner = winner;
            TeamAWins = teamAWins;
            TeamBWins = teamBWins;
            CompletesMatch = completesMatch;
        }

        public int RoundNumber { get; }
        public TeamId Winner { get; }
        public int TeamAWins { get; }
        public int TeamBWins { get; }
        public bool CompletesMatch { get; }
    }
}
