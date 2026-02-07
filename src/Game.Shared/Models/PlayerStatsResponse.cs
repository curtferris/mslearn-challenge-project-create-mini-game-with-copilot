namespace Game.Shared.Models;

public sealed class PlayerStatsResponse
{
    public PlayerState Player { get; init; } = new();

    public IReadOnlyCollection<RoundResult> RecentRounds { get; init; } = Array.Empty<RoundResult>();
}
