using Microsoft.Extensions.Options;

namespace Game.Api.Services;

public sealed class SeededGameRandom : IGameRandom
{
    private readonly Random _random;
    private readonly object _sync = new();

    public SeededGameRandom(IOptions<RandomOptions> options)
    {
        _random = new Random(options.Value.Seed);
    }

    public int Next(int minInclusive, int maxExclusive)
    {
        lock (_sync)
        {
            return _random.Next(minInclusive, maxExclusive);
        }
    }

    public double NextDouble()
    {
        lock (_sync)
        {
            return _random.NextDouble();
        }
    }
}
