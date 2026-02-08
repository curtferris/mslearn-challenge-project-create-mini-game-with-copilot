using System;
using Game.Shared.Models;

namespace Game.Client.Services;

public sealed class SessionSnapshot
{
    public string? OpponentId { get; set; }

    public MoveType SelectedMove { get; set; } = MoveType.Rock;

    public int BetAmount { get; set; } = 5;

    public PlayerStatsResponse? Stats { get; set; }

    public RoundResult? LastRound { get; set; }

    public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.UtcNow;
}
