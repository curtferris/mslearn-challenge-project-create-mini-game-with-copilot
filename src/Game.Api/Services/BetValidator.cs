using Game.Shared.Models;
using Microsoft.AspNetCore.Http;

namespace Game.Api.Services;

public sealed class BetValidator : IBetValidator
{
    public void Validate(PlayerState currentState, PlayRequest request)
    {
        if (request.BetAmount <= 0)
        {
            throw new BadHttpRequestException("Bet amount must be greater than zero.");
        }

        if (request.BetAmount > currentState.Coins)
        {
            throw new BadHttpRequestException("Bet amount cannot exceed the current coin balance.");
        }
    }
}
