using Game.Api.Services;
using Game.Shared.Models;
using FluentAssertions;

namespace Game.Domain.Tests;

public class RoundResolverTests
{
    private readonly RoundResolver _resolver = new();

    [Theory]
    [InlineData(MoveType.Rock, MoveType.Scissors)]
    [InlineData(MoveType.Paper, MoveType.Rock)]
    [InlineData(MoveType.Scissors, MoveType.Paper)]
    public void Resolve_PlayerWins_WhenMoveBeatsOpponent(MoveType playerMove, MoveType opponentMove)
    {
        var winner = _resolver.Resolve(playerMove, opponentMove);
        winner.Should().Be(RoundWinner.Player);
    }

    [Theory]
    [InlineData(MoveType.Rock, MoveType.Paper)]
    [InlineData(MoveType.Paper, MoveType.Scissors)]
    [InlineData(MoveType.Scissors, MoveType.Rock)]
    public void Resolve_OpponentWins_WhenMoveLoses(MoveType playerMove, MoveType opponentMove)
    {
        var winner = _resolver.Resolve(playerMove, opponentMove);
        winner.Should().Be(RoundWinner.Opponent);
    }

    [Fact]
    public void Resolve_Tie_WhenMovesMatch()
    {
        _resolver.Resolve(MoveType.Rock, MoveType.Rock).Should().Be(RoundWinner.Tie);
    }
}
