using Game.Shared.Models;

namespace Game.Api.Services;

public interface IRoundResolver
{
    RoundWinner Resolve(MoveType playerMove, MoveType opponentMove);
}
