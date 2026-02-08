using Game.Api.Services;
using Game.Shared.Models;

namespace Game.Api.Opponents;

public sealed class WeightedStrategy : IOpponentStrategy
{
    private readonly IGameRandom _random;
    private readonly MoveType _favoredMove;
    private readonly double _favoredWeight;

    public WeightedStrategy(IGameRandom random)
    {
        _random = random;
        _favoredMove = MoveType.Paper;
        _favoredWeight = 0.6;
    }

    public OpponentProfile Profile { get; } = new()
    {
        Id = "lena",
        Name = "Lucky Lena",
        BehaviorType = "Weighted",
        Difficulty = "Medium",
        Description = "Prefers Paper but occasionally mixes things up.",
        PortraitUrl = "/images/opponents/lena.svg"
    };

    public MoveType GetNextMove(PlayRequest request, PlayerState playerState)
    {
        var roll = _random.NextDouble();
        if (roll < _favoredWeight)
        {
            return _favoredMove;
        }

        return _favoredMove switch
        {
            MoveType.Rock => (MoveType)_random.Next(1, 3),
            MoveType.Paper => _random.Next(0, 2) == 0 ? MoveType.Rock : MoveType.Scissors,
            _ => (MoveType)_random.Next(0, 2)
        };
    }
}
