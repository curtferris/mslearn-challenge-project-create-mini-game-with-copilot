using Game.Api.Services;
using Game.Shared.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace Game.Domain.Tests;

public class BetValidatorTests
{
    private readonly BetValidator _validator = new();

    [Fact]
    public void Validate_AllowsBet_WhenWithinBalance()
    {
        var state = new PlayerState { Coins = 50 };
        var request = new PlayRequest { BetAmount = 25, OpponentId = "marty", PlayerMove = MoveType.Rock };

        var act = () => _validator.Validate(state, request);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_Throws_WhenBetIsZero()
    {
        var state = new PlayerState { Coins = 50 };
        var request = new PlayRequest { BetAmount = 0, OpponentId = "marty", PlayerMove = MoveType.Rock };

        var act = () => _validator.Validate(state, request);

        act.Should().Throw<BadHttpRequestException>()
            .WithMessage("Bet amount must be greater than zero.*");
    }

    [Fact]
    public void Validate_Throws_WhenBetExceedsCoins()
    {
        var state = new PlayerState { Coins = 25 };
        var request = new PlayRequest { BetAmount = 50, OpponentId = "marty", PlayerMove = MoveType.Rock };

        var act = () => _validator.Validate(state, request);

        act.Should().Throw<BadHttpRequestException>()
            .WithMessage("Bet amount cannot exceed the current coin balance.*");
    }
}
