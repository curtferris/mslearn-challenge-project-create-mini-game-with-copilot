namespace Game.Shared.Models;

public sealed class OpponentStats
{
    public string OpponentId { get; init; } = string.Empty;

    public int Wins { get; set; }

    public int Losses { get; set; }

    public int Ties { get; set; }

    public int NetCoins { get; set; }

    public DateTimeOffset? LastPlayed { get; set; }
}
