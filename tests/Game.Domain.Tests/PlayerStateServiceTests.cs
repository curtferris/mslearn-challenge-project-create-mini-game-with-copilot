using Game.Api.Services;
using Game.Shared.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace Game.Domain.Tests;

public class PlayerStateServiceTests
{
    private readonly IBetValidator _validator = new BetValidator();

    [Fact]
    public void ApplyRound_IncrementsCoins_OnWin()
    {
        var service = new PlayerStateService(_validator);
        var request = new PlayRequest { OpponentId = "marty", PlayerMove = MoveType.Rock, BetAmount = 10 };

        var round = service.ApplyRound(request, MoveType.Scissors, RoundWinner.Player);
        var snapshot = service.GetSnapshot();

        round.CoinDelta.Should().Be(10);
        snapshot.Coins.Should().Be(60);
        snapshot.Wins.Should().Be(1);
        snapshot.CurrentStreak.Should().Be(1);
        snapshot.OpponentStats.Should().ContainKey("marty");
    }

    [Fact]
    public void ApplyRound_DecrementsCoins_OnLoss()
    {
        var service = new PlayerStateService(_validator);
        var request = new PlayRequest { OpponentId = "marty", PlayerMove = MoveType.Rock, BetAmount = 5 };

        var round = service.ApplyRound(request, MoveType.Paper, RoundWinner.Opponent);
        var snapshot = service.GetSnapshot();

        round.CoinDelta.Should().Be(-5);
        snapshot.Coins.Should().Be(45);
        snapshot.Losses.Should().Be(1);
        snapshot.CurrentStreak.Should().Be(-1);
    }

    [Fact]
    public void ApplyRound_Throws_WhenBetInvalid()
    {
        var service = new PlayerStateService(_validator);
        var request = new PlayRequest { OpponentId = "marty", PlayerMove = MoveType.Rock, BetAmount = 1000 };

        var act = () => service.ApplyRound(request, MoveType.Scissors, RoundWinner.Player);

        act.Should().Throw<BadHttpRequestException>();
    }

    [Fact]
    public void Reset_RestoresStartingState()
    {
        var service = new PlayerStateService(_validator);
        service.ApplyRound(new PlayRequest { OpponentId = "marty", PlayerMove = MoveType.Rock, BetAmount = 5 }, MoveType.Scissors, RoundWinner.Player);

        var reset = service.Reset();

        reset.Coins.Should().Be(50);
        reset.Wins.Should().Be(0);
        reset.OpponentStats.Should().BeEmpty();
    }
}
