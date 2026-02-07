using Game.Shared.Models;

namespace Game.Api.Services;

public interface IPlayerStateService
{
    PlayerStatsResponse GetStats();

    PlayerState GetSnapshot();

    PlayerState Reset();

    RoundResult ApplyRound(PlayRequest request, MoveType opponentMove, RoundWinner winner);
}
