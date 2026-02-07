namespace Game.Shared.Models;

public sealed class PlayRequest
{
    public required string OpponentId { get; init; }

    public MoveType PlayerMove { get; init; }

    public int BetAmount { get; init; }
}
