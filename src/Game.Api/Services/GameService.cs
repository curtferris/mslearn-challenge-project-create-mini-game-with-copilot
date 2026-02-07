using Game.Api.Opponents;
using Game.Shared.Models;
using Microsoft.AspNetCore.Http;

namespace Game.Api.Services;

public sealed class GameService : IGameService
{
    private readonly IPlayerStateService _playerState;
    private readonly IRoundResolver _roundResolver;
    private readonly IOpponentService _opponents;

    public GameService(
        IPlayerStateService playerState,
        IRoundResolver roundResolver,
        IOpponentService opponents)
    {
        _playerState = playerState;
        _roundResolver = roundResolver;
        _opponents = opponents;
    }

    public PlayResponse PlayRound(PlayRequest request)
    {
        ValidateRequest(request);

        var snapshotBeforeRound = _playerState.GetSnapshot();
        var opponentStrategy = _opponents.GetStrategy(request.OpponentId);
        var opponentMove = opponentStrategy.GetNextMove(request, snapshotBeforeRound);
        var winner = _roundResolver.Resolve(request.PlayerMove, opponentMove);
        var round = _playerState.ApplyRound(request, opponentMove, winner);
        var snapshot = _playerState.GetSnapshot();

        return new PlayResponse
        {
            Round = round,
            Player = snapshot
        };
    }

    public PlayerStatsResponse GetStats() => _playerState.GetStats();

    public PlayerState Reset() => _playerState.Reset();

    private static void ValidateRequest(PlayRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.OpponentId))
        {
            throw new BadHttpRequestException("Opponent ID is required.");
        }

        if (!Enum.IsDefined(typeof(MoveType), request.PlayerMove))
        {
            throw new BadHttpRequestException("Invalid player move.");
        }
    }
}
