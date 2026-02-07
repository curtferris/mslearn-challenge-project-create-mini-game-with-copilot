namespace Game.Shared.Models;

public sealed class PlayResponse
{
    public required RoundResult Round { get; init; }

    public required PlayerState Player { get; init; }
}
