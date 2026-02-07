using Game.Shared.Models;

namespace Game.Api.Services;

public sealed class RoundResolver : IRoundResolver
{
    public RoundWinner Resolve(MoveType playerMove, MoveType opponentMove)
    {
        if (playerMove == opponentMove)
        {
            return RoundWinner.Tie;
        }

        return (playerMove, opponentMove) switch
        {
            (MoveType.Rock, MoveType.Scissors) => RoundWinner.Player,
            (MoveType.Paper, MoveType.Rock) => RoundWinner.Player,
            (MoveType.Scissors, MoveType.Paper) => RoundWinner.Player,
            _ => RoundWinner.Opponent
        };
    }
}
