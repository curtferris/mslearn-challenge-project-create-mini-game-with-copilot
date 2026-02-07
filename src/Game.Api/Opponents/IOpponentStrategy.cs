using Game.Shared.Models;

namespace Game.Api.Opponents;

public interface IOpponentStrategy
{
    OpponentProfile Profile { get; }

    MoveType GetNextMove(PlayRequest request, PlayerState playerState);
}
