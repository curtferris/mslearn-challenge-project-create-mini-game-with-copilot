using Game.Api.Opponents;
using Game.Shared.Models;
using FluentAssertions;

namespace Game.Domain.Tests;

public class OpponentStrategyTests
{
    [Fact]
    public void MartyStrategy_UsesRandomMove()
    {
        var random = new TestRandom();
        random.EnqueueInt((int)MoveType.Scissors);
        var strategy = new MartyStrategy(random);

        var move = strategy.GetNextMove(new PlayRequest { PlayerMove = MoveType.Rock, OpponentId = "marty", BetAmount = 5 }, new PlayerState());

        move.Should().Be(MoveType.Scissors);
    }

    [Fact]
    public void WeightedStrategy_FavorsPaper_WhenRollBelowWeight()
    {
        var random = new TestRandom();
        random.EnqueueDouble(0.2);
        var strategy = new WeightedStrategy(random);

        var move = strategy.GetNextMove(new PlayRequest { PlayerMove = MoveType.Rock, OpponentId = "lena", BetAmount = 5 }, new PlayerState());

        move.Should().Be(MoveType.Paper);
    }

    [Fact]
    public void PatternStrategy_FollowsSequence()
    {
        var strategy = new PatternStrategy();

        var first = strategy.GetNextMove(new PlayRequest { PlayerMove = MoveType.Rock, OpponentId = "rex", BetAmount = 5 }, new PlayerState());
        var second = strategy.GetNextMove(new PlayRequest { PlayerMove = MoveType.Rock, OpponentId = "rex", BetAmount = 5 }, new PlayerState());
        var third = strategy.GetNextMove(new PlayRequest { PlayerMove = MoveType.Rock, OpponentId = "rex", BetAmount = 5 }, new PlayerState());

        first.Should().Be(MoveType.Rock);
        second.Should().Be(MoveType.Paper);
        third.Should().Be(MoveType.Scissors);
    }

    [Fact]
    public void CheaterStrategy_CountersPlayer_WhenCheatRollHits()
    {
        var random = new TestRandom();
        random.EnqueueDouble(0.1); // force cheat path
        var strategy = new CheaterStrategy(random);

        var move = strategy.GetNextMove(new PlayRequest { PlayerMove = MoveType.Rock, OpponentId = "sid", BetAmount = 5 }, new PlayerState());

        move.Should().Be(MoveType.Paper);
    }
}
