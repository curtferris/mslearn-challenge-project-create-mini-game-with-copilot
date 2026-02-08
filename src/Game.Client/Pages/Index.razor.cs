using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Client.Services;
using Game.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Game.Client.Pages;

public partial class Index : ComponentBase
{
    [Inject]
    private GameApiClient ApiClient { get; set; } = default!;

    [Inject]
    private ILogger<Index> Logger { get; set; } = default!;

    [Inject]
    private SessionStateCache SessionCache { get; set; } = default!;

    private readonly List<OpponentProfile> _opponents = new();
    private PlayerStatsResponse? stats;
    private RoundResult? lastRound;
    private string? selectedOpponentId;
    private MoveType selectedMove = MoveType.Rock;
    private int betAmount = 5;
    private bool isInitializing = true;
    private bool isBusy;
    private bool isResetting;
    private string? errorMessage;
    private string? tauntMessage;
    private string tauntTone = "neutral";
    private bool showJankenOverlay;
    private bool isRevealPhase;
    private RoundResult? overlayRoundResult;
    private static readonly TimeSpan CountdownDuration = TimeSpan.FromMilliseconds(1350);
    private static readonly TimeSpan RevealDuration = TimeSpan.FromSeconds(4);

    protected override async Task OnInitializedAsync()
    {
        await LoadInitialDataAsync();
    }

    private IReadOnlyList<OpponentProfile> AvailableOpponents => _opponents;

    private int AvailableOpponentsCount => _opponents.Count;

    private int CurrentCoins => stats?.Player.Coins ?? 50;

    private string OverallRecord => stats is null
        ? "0-0-0"
        : $"{stats.Player.Wins}-{stats.Player.Losses}-{stats.Player.Ties}";

    private string CurrentStreakLabel => FormatStreak(stats?.Player.CurrentStreak ?? 0);

    private bool CanPlayRound => !isBusy && !isResetting && !string.IsNullOrWhiteSpace(selectedOpponentId) && CurrentCoins > 0;

    private bool IsResetDisabled => isBusy || isResetting;

    private async Task LoadInitialDataAsync()
    {
        errorMessage = null;
        await HydrateFromCacheAsync();
        await LoadOpponentsAsync();
        await RefreshStatsAsync();

        if (string.IsNullOrEmpty(selectedOpponentId) && _opponents.Count > 0)
        {
            selectedOpponentId = _opponents[0].Id;
        }

        ClampBet();
        UpdateTaunt();
        await PersistSessionAsync();
        isInitializing = false;
    }

    private async Task LoadOpponentsAsync()
    {
        try
        {
            var fetched = await ApiClient.GetOpponentsAsync();
            _opponents.Clear();
            if (fetched != null && fetched.Count > 0)
            {
                _opponents.AddRange(fetched);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load opponents");
            errorMessage = "Opponents could not be loaded. Ensure the API is running and refresh.";
            _opponents.Clear();
        }
    }

    private async Task RefreshStatsAsync(bool updateTaunt = true)
    {
        try
        {
            stats = await ApiClient.GetStatsAsync() ?? new PlayerStatsResponse();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load stats");
            errorMessage ??= "Stats are temporarily unavailable. Try again shortly.";
        }
        finally
        {
            if (updateTaunt)
            {
                UpdateTaunt();
            }
        }
    }

    private async Task HandlePlayAsync()
    {
        if (!CanPlayRound)
        {
            errorMessage = string.IsNullOrWhiteSpace(selectedOpponentId)
                ? "Pick an opponent before wagering."
                : "You need coins before you can duel again.";
            return;
        }

        isBusy = true;
        errorMessage = null;
        RoundResult? resolvedRound = null;

        try
        {
            await ShowCountdownAsync();

            var response = await ApiClient.PlayRoundAsync(new PlayRequest
            {
                OpponentId = selectedOpponentId!,
                PlayerMove = selectedMove,
                BetAmount = betAmount
            });

            if (response != null)
            {
                resolvedRound = response.Round;
                lastRound = response.Round;
                await RefreshStatsAsync();
                ClampBet();
                await PersistSessionAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Play request failed");
            errorMessage = "That round could not be resolved. Try again in a moment.";
        }
        finally
        {
            if (resolvedRound != null)
            {
                await ShowRevealAsync(resolvedRound);
            }
            else
            {
                await HideOverlayAsync();
            }

            isBusy = false;
        }
    }

    private async Task HandleResetAsync()
    {
        if (IsResetDisabled)
        {
            return;
        }

        isResetting = true;
        errorMessage = null;

        try
        {
            await ApiClient.ResetAsync();
            lastRound = null;
            betAmount = 5;
            await RefreshStatsAsync();
            ClampBet();
            await PersistSessionAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Reset failed");
            errorMessage = "Reset failed. Please try again.";
        }
        finally
        {
            isResetting = false;
        }
    }

    private void ClampBet()
    {
        var ceiling = Math.Max(1, CurrentCoins);
        betAmount = Math.Clamp(betAmount, 1, ceiling);
    }

    private void UpdateTaunt()
    {
        if (stats == null)
        {
            tauntMessage = "Systems idle. Challenge someone to gather intel.";
            tauntTone = "neutral";
            return;
        }

        var streak = stats.Player.CurrentStreak;
        if (streak >= 3)
        {
            tauntMessage = $"Heat check! You're on a W{streak} tear.";
            tauntTone = "hype";
            return;
        }

        if (streak <= -2)
        {
            tauntMessage = $"Warning: L{Math.Abs(streak)} skid. Reset the pattern!";
            tauntTone = "danger";
            return;
        }

        if (!string.IsNullOrEmpty(selectedOpponentId)
            && stats.Player.OpponentStats.TryGetValue(selectedOpponentId, out var opponentStats)
            && Math.Abs(opponentStats.NetCoins) >= 10)
        {
            var opponentName = GetOpponentHandle(selectedOpponentId);
            if (opponentStats.NetCoins >= 0)
            {
                tauntMessage = $"{opponentName} is wobbling after losing {opponentStats.NetCoins} coins.";
                tauntTone = "hype";
            }
            else
            {
                tauntMessage = $"{opponentName} siphoned {Math.Abs(opponentStats.NetCoins)} coins—time to adapt.";
                tauntTone = "danger";
            }
            return;
        }

        if (lastRound != null)
        {
            var opponentName = GetOpponentHandle(lastRound.OpponentId);
            switch (lastRound.Winner)
            {
                case RoundWinner.Player:
                    tauntMessage = $"Clutch read! {opponentName} never saw {lastRound.PlayerMove} coming.";
                    tauntTone = "hype";
                    return;
                case RoundWinner.Opponent:
                    tauntMessage = $"{opponentName} flexes that win—switch rhythms.";
                    tauntTone = "danger";
                    return;
                default:
                    tauntMessage = "Stalemate. Syndicate analysts crave more data.";
                    tauntTone = "neutral";
                    return;
            }
        }

        tauntMessage = "Pick a rival and open with confidence.";
        tauntTone = "neutral";
    }

    private string GetOpponentHandle(string? opponentId)
        => _opponents.FirstOrDefault(o => o.Id == opponentId)?.Name ?? opponentId ?? "Opponent";

    private async Task HydrateFromCacheAsync()
    {
        try
        {
            var snapshot = await SessionCache.LoadAsync();
            if (snapshot == null)
            {
                return;
            }

            selectedOpponentId = snapshot.OpponentId ?? selectedOpponentId;
            selectedMove = snapshot.SelectedMove;
            betAmount = snapshot.BetAmount;
            stats = snapshot.Stats ?? stats;
            lastRound = snapshot.LastRound ?? lastRound;
            ClampBet();
            UpdateTaunt();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to hydrate session cache");
        }
    }

    private async Task PersistSessionAsync()
    {
        try
        {
            var snapshot = new SessionSnapshot
            {
                OpponentId = selectedOpponentId,
                SelectedMove = selectedMove,
                BetAmount = betAmount,
                Stats = stats,
                LastRound = lastRound,
                SavedAt = DateTimeOffset.UtcNow
            };

            await SessionCache.SaveAsync(snapshot);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to persist session cache");
        }
    }

    private async Task ShowCountdownAsync()
    {
        showJankenOverlay = true;
        isRevealPhase = false;
        overlayRoundResult = null;
        await InvokeAsync(StateHasChanged);
        await Task.Delay(CountdownDuration);
    }

    private async Task ShowRevealAsync(RoundResult round)
    {
        overlayRoundResult = round;
        isRevealPhase = true;
        await InvokeAsync(StateHasChanged);
        await Task.Delay(RevealDuration);
        await HideOverlayAsync();
    }

    private async Task HideOverlayAsync()
    {
        showJankenOverlay = false;
        isRevealPhase = false;
        overlayRoundResult = null;
        await InvokeAsync(StateHasChanged);
    }

    private static string FormatStreak(int streak)
    {
        if (streak > 0)
        {
            return $"W{streak}";
        }

        if (streak < 0)
        {
            return $"L{Math.Abs(streak)}";
        }

        return "Neutral";
    }
}
