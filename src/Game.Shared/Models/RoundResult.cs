namespace Game.Shared.Models;

public sealed class RoundResult
{
    public MoveType PlayerMove { get; init; }

    public MoveType OpponentMove { get; init; }

    public RoundWinner Winner { get; init; }

    public int CoinDelta { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    public string OpponentId { get; init; } = string.Empty;
}
