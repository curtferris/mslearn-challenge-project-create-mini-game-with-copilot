using Game.Api.Services;
using Game.Shared.Models;

namespace Game.Api.Opponents;

public sealed class CheaterStrategy : IOpponentStrategy
{
    private readonly IGameRandom _random;
    private readonly double _cheatChance;

    public CheaterStrategy(IGameRandom random)
    {
        _random = random;
        _cheatChance = 0.65;
    }

    public OpponentProfile Profile { get; } = new()
    {
        Id = "sid",
        Name = "Sneaky Sid",
        BehaviorType = "Cheater",
        Difficulty = "Hard",
        Description = "Peeks at your move ~65% of the time and counters it.",
        PortraitUrl = "/images/opponents/sid.svg"
    };

    public MoveType GetNextMove(PlayRequest request, PlayerState playerState)
    {
        if (_random.NextDouble() < _cheatChance)
        {
            return request.PlayerMove switch
            {
                MoveType.Rock => MoveType.Paper,
                MoveType.Paper => MoveType.Scissors,
                MoveType.Scissors => MoveType.Rock,
                _ => (MoveType)_random.Next(0, 3)
            };
        }

        return (MoveType)_random.Next(0, 3);
    }
}
