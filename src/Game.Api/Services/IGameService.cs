using Game.Shared.Models;

namespace Game.Api.Services;

public interface IGameService
{
    PlayResponse PlayRound(PlayRequest request);

    PlayerStatsResponse GetStats();

    PlayerState Reset();
}
