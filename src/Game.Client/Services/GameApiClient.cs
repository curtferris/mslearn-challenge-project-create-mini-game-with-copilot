using System.Net.Http.Json;
using Game.Shared.Models;

namespace Game.Client.Services;

public sealed class GameApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<GameApiClient> _logger;

    public GameApiClient(HttpClient http, ILogger<GameApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<IReadOnlyList<OpponentProfile>> GetOpponentsAsync(CancellationToken cancellationToken = default)
        => await SendAsync(() => _http.GetFromJsonAsync<IReadOnlyList<OpponentProfile>>("opponents", cancellationToken))
           ?? Array.Empty<OpponentProfile>();

    public Task<PlayerStatsResponse?> GetStatsAsync(CancellationToken cancellationToken = default)
        => SendAsync(() => _http.GetFromJsonAsync<PlayerStatsResponse>("stats", cancellationToken));

    public async Task<PlayerState?> ResetAsync(CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("reset", new { }, cancellationToken);
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<PlayerState>(cancellationToken: cancellationToken);
    }

    public async Task<PlayResponse?> PlayRoundAsync(PlayRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsJsonAsync("play", request, cancellationToken);
        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<PlayResponse>(cancellationToken: cancellationToken);
    }

    private async Task<T?> SendAsync<T>(Func<Task<T?>> action)
    {
        try
        {
            return await action();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API request failed");
            throw;
        }
    }

    private static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Request failed: {(int)response.StatusCode} - {body}");
        }
    }
}
