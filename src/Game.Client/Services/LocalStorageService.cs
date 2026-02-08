using System.Text.Json;
using Microsoft.JSInterop;

namespace Game.Client.Services;

public sealed class LocalStorageService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SetItemAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(value, SerializerOptions);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, key, payload);
    }

    public async Task<T?> GetItemAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, key);
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json, SerializerOptions);
    }

    public Task RemoveItemAsync(string key, CancellationToken cancellationToken = default)
        => _jsRuntime.InvokeVoidAsync("localStorage.removeItem", cancellationToken, key).AsTask();
}
