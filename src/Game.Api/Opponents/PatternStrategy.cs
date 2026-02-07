using Game.Shared.Models;
using System.Threading;

namespace Game.Api.Opponents;

public sealed class PatternStrategy : IOpponentStrategy
{
    private readonly MoveType[] _pattern =
    {
        MoveType.Rock,
        MoveType.Paper,
        MoveType.Scissors,
        MoveType.Scissors
    };

    private int _index = -1;

    public OpponentProfile Profile { get; } = new()
    {
        Id = "rex",
        Name = "Rhythm Rex",
        BehaviorType = "Pattern",
        Difficulty = "Medium",
        Description = "Repeats a four-move riff — crack the rhythm to win."
    };

    public MoveType GetNextMove(PlayRequest request, PlayerState playerState)
    {
        var nextIndex = Interlocked.Increment(ref _index);
        return _pattern[Math.Abs(nextIndex) % _pattern.Length];
    }
}
