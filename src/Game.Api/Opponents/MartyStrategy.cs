using Game.Api.Services;
using Game.Shared.Models;

namespace Game.Api.Opponents;

public sealed class MartyStrategy : IOpponentStrategy
{
    private readonly IGameRandom _random;

    public MartyStrategy(IGameRandom random)
    {
        _random = random;
    }

    public OpponentProfile Profile { get; } = new()
    {
        Id = "marty",
        Name = "Marty the Maverick",
        BehaviorType = "Random",
        Difficulty = "Easy",
        Description = "Pure RNG — picks whichever move the universe whispers.",
        PortraitUrl = "/images/opponents/marty.svg"
    };

    public MoveType GetNextMove(PlayRequest request, PlayerState playerState)
        => (MoveType)_random.Next(0, 3);
}
