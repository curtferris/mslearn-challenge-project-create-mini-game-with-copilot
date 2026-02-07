using Game.Shared.Models;

namespace Game.Api.Opponents;

public interface IOpponentService
{
    IReadOnlyCollection<OpponentProfile> GetProfiles();

    IOpponentStrategy GetStrategy(string opponentId);
}
