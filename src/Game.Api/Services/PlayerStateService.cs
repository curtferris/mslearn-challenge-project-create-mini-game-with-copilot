using System.Collections.ObjectModel;
using System.Linq;
using Game.Shared.Models;

namespace Game.Api.Services;

public sealed class PlayerStateService : IPlayerStateService
{
    private const int MaxHistory = 20;

    private readonly object _sync = new();
    private readonly PlayerState _state = new();
    private readonly Dictionary<string, OpponentStats> _opponentStats = new();
    private readonly LinkedList<RoundResult> _history = new();
    private readonly IBetValidator _betValidator;

    public PlayerStateService(IBetValidator betValidator)
    {
        _betValidator = betValidator;
    }

    public PlayerStatsResponse GetStats()
    {
        lock (_sync)
        {
            return new PlayerStatsResponse
            {
                Player = CloneState(),
                RecentRounds = _history.ToArray()
            };
        }
    }

    public PlayerState GetSnapshot()
    {
        lock (_sync)
        {
            return CloneState();
        }
    }

    public PlayerState Reset()
    {
        lock (_sync)
        {
            _state.Coins = 50;
            _state.Wins = 0;
            _state.Losses = 0;
            _state.Ties = 0;
            _state.CurrentStreak = 0;
            _state.BestStreak = 0;
            _opponentStats.Clear();
            _history.Clear();
            _state.OpponentStats = new ReadOnlyDictionary<string, OpponentStats>(new Dictionary<string, OpponentStats>());
            return CloneState();
        }
    }

    public RoundResult ApplyRound(PlayRequest request, MoveType opponentMove, RoundWinner winner)
    {
        lock (_sync)
        {
            _betValidator.Validate(_state, request);

            var coinDelta = winner switch
            {
                RoundWinner.Player => request.BetAmount,
                RoundWinner.Opponent => -request.BetAmount,
                _ => 0
            };

            _state.Coins += coinDelta;

            switch (winner)
            {
                case RoundWinner.Player:
                    _state.Wins++;
                    _state.CurrentStreak = _state.CurrentStreak >= 0 ? _state.CurrentStreak + 1 : 1;
                    _state.BestStreak = Math.Max(_state.BestStreak, _state.CurrentStreak);
                    break;
                case RoundWinner.Opponent:
                    _state.Losses++;
                    _state.CurrentStreak = _state.CurrentStreak <= 0 ? _state.CurrentStreak - 1 : -1;
                    break;
                default:
                    _state.Ties++;
                    _state.CurrentStreak = 0;
                    break;
            }

            var opponentStats = GetOrCreateOpponentStats(request.OpponentId);
            switch (winner)
            {
                case RoundWinner.Player:
                    opponentStats.Losses++;
                    opponentStats.NetCoins += request.BetAmount;
                    break;
                case RoundWinner.Opponent:
                    opponentStats.Wins++;
                    opponentStats.NetCoins -= request.BetAmount;
                    break;
                default:
                    opponentStats.Ties++;
                    break;
            }

            opponentStats.LastPlayed = DateTimeOffset.UtcNow;

            var round = new RoundResult
            {
                PlayerMove = request.PlayerMove,
                OpponentMove = opponentMove,
                Winner = winner,
                CoinDelta = coinDelta,
                Timestamp = opponentStats.LastPlayed.Value,
                OpponentId = request.OpponentId
            };

            _history.AddLast(round);
            if (_history.Count > MaxHistory)
            {
                _history.RemoveFirst();
            }

            return round;
        }
    }

    private PlayerState CloneState()
    {
        var opponentStatsCopy = _opponentStats.ToDictionary(
            kvp => kvp.Key,
            kvp => new OpponentStats
            {
                OpponentId = kvp.Value.OpponentId,
                Wins = kvp.Value.Wins,
                Losses = kvp.Value.Losses,
                Ties = kvp.Value.Ties,
                NetCoins = kvp.Value.NetCoins,
                LastPlayed = kvp.Value.LastPlayed
            });

        return new PlayerState
        {
            Coins = _state.Coins,
            Wins = _state.Wins,
            Losses = _state.Losses,
            Ties = _state.Ties,
            CurrentStreak = _state.CurrentStreak,
            BestStreak = _state.BestStreak,
            OpponentStats = new ReadOnlyDictionary<string, OpponentStats>(opponentStatsCopy)
        };
    }

    private OpponentStats GetOrCreateOpponentStats(string opponentId)
    {
        if (string.IsNullOrWhiteSpace(opponentId))
        {
            opponentId = "unknown";
        }

        if (!_opponentStats.TryGetValue(opponentId, out var stats))
        {
            stats = new OpponentStats
            {
                OpponentId = opponentId
            };
            _opponentStats[opponentId] = stats;
        }

        return stats;
    }
}
