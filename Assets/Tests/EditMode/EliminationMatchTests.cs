using System;
using Axiom.Manager;
using NUnit.Framework;

namespace Axiom.Tests.EditMode
{
    public sealed class EliminationMatchTests
    {
        [Test]
        public void Match_StartsWithThreeAliveOnEachTeam()
        {
            var match = new EliminationMatchModel();

            match.StartMatch();

            Assert.That(match.Phase, Is.EqualTo(MatchPhase.RoundActive));
            Assert.That(match.RoundNumber, Is.EqualTo(1));
            Assert.That(match.TeamAAlive, Is.EqualTo(3));
            Assert.That(match.TeamBAlive, Is.EqualTo(3));
        }

        [Test]
        public void EliminatingAllThreeEnemies_AwardsOneRoundWin()
        {
            var match = new EliminationMatchModel();
            match.StartMatch();

            Assert.That(match.TryReportElimination(TeamId.TeamB, out _), Is.False);
            Assert.That(match.TryReportElimination(TeamId.TeamB, out _), Is.False);
            bool roundEnded = match.TryReportElimination(
                TeamId.TeamB,
                out RoundResult result);

            Assert.That(roundEnded, Is.True);
            Assert.That(result.Winner, Is.EqualTo(TeamId.TeamA));
            Assert.That(result.TeamAWins, Is.EqualTo(1));
            Assert.That(result.CompletesMatch, Is.False);
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.RoundBreak));
        }

        [Test]
        public void WinningTwoRounds_CompletesBestOfThreeMatch()
        {
            var match = new EliminationMatchModel();
            match.StartMatch();
            EliminateTeam(match, TeamId.TeamB);
            match.StartNextRound();

            RoundResult finalResult = EliminateTeam(match, TeamId.TeamB);

            Assert.That(finalResult.CompletesMatch, Is.True);
            Assert.That(finalResult.Winner, Is.EqualTo(TeamId.TeamA));
            Assert.That(match.TeamAWins, Is.EqualTo(2));
            Assert.That(match.TeamBWins, Is.Zero);
            Assert.That(match.Phase, Is.EqualTo(MatchPhase.MatchComplete));
        }

        [Test]
        public void SplitRounds_AllowsDecidingThirdRound()
        {
            var match = new EliminationMatchModel();
            match.StartMatch();
            EliminateTeam(match, TeamId.TeamB);
            match.StartNextRound();
            EliminateTeam(match, TeamId.TeamA);
            match.StartNextRound();

            RoundResult finalResult = EliminateTeam(match, TeamId.TeamB);

            Assert.That(match.RoundNumber, Is.EqualTo(3));
            Assert.That(finalResult.Winner, Is.EqualTo(TeamId.TeamA));
            Assert.That(match.TeamAWins, Is.EqualTo(2));
            Assert.That(match.TeamBWins, Is.EqualTo(1));
            Assert.That(finalResult.CompletesMatch, Is.True);
        }

        [Test]
        public void StartingRoundDuringActiveRound_IsRejected()
        {
            var match = new EliminationMatchModel();
            match.StartMatch();

            Assert.Throws<InvalidOperationException>(() => match.StartNextRound());
        }

        private static RoundResult EliminateTeam(
            EliminationMatchModel match,
            TeamId defeatedTeam)
        {
            match.TryReportElimination(defeatedTeam, out _);
            match.TryReportElimination(defeatedTeam, out _);
            Assert.That(
                match.TryReportElimination(defeatedTeam, out RoundResult result),
                Is.True);
            return result;
        }
    }
}
