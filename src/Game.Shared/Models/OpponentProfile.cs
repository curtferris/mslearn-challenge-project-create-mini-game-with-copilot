namespace Game.Shared.Models;

public sealed class OpponentProfile
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string BehaviorType { get; init; }

    public required string Difficulty { get; init; }

    public string? Description { get; init; }
}
