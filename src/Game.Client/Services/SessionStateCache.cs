namespace Game.Client.Services;

public sealed class SessionStateCache
{
    private const string StorageKey = "alexkidd.session";
    private readonly LocalStorageService _storage;

    public SessionStateCache(LocalStorageService storage)
    {
        _storage = storage;
    }

    public Task SaveAsync(SessionSnapshot snapshot, CancellationToken cancellationToken = default)
        => _storage.SetItemAsync(StorageKey, snapshot, cancellationToken);

    public Task<SessionSnapshot?> LoadAsync(CancellationToken cancellationToken = default)
        => _storage.GetItemAsync<SessionSnapshot>(StorageKey, cancellationToken);

    public Task ClearAsync(CancellationToken cancellationToken = default)
        => _storage.RemoveItemAsync(StorageKey, cancellationToken);
}
