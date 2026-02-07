using System.Collections.ObjectModel;

namespace Game.Shared.Models;

public sealed class PlayerState
{
    public int Coins { get; set; } = 50;

    public int Wins { get; set; }

    public int Losses { get; set; }

    public int Ties { get; set; }

    public int CurrentStreak { get; set; }

    public int BestStreak { get; set; }

    public IReadOnlyDictionary<string, OpponentStats> OpponentStats { get; set; }
        = new ReadOnlyDictionary<string, OpponentStats>(new Dictionary<string, OpponentStats>());
}
