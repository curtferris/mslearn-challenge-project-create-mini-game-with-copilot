using System.Collections.Generic;
using System.Linq;
using Game.Shared.Models;
using Microsoft.AspNetCore.Http;

namespace Game.Api.Opponents;

public sealed class OpponentService : IOpponentService
{
    private readonly IReadOnlyDictionary<string, IOpponentStrategy> _strategies;
    private readonly IReadOnlyCollection<OpponentProfile> _profiles;

    public OpponentService(IEnumerable<IOpponentStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.Profile.Id, StringComparer.OrdinalIgnoreCase);
        _profiles = _strategies.Values
            .Select(s => s.Profile)
            .OrderBy(p => p.Name)
            .ToArray();
    }

    public IReadOnlyCollection<OpponentProfile> GetProfiles() => _profiles;

    public IOpponentStrategy GetStrategy(string opponentId)
    {
        if (string.IsNullOrWhiteSpace(opponentId))
        {
            throw new BadHttpRequestException("Opponent id is required.");
        }

        if (_strategies.TryGetValue(opponentId, out var strategy))
        {
            return strategy;
        }

        throw new BadHttpRequestException($"Opponent '{opponentId}' is not available.");
    }
}
