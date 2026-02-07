using Game.Api.Services;

namespace Game.Domain.Tests;

internal sealed class TestRandom : IGameRandom
{
    private readonly Queue<int> _ints = new();
    private readonly Queue<double> _doubles = new();

    public void EnqueueInt(int value) => _ints.Enqueue(value);

    public void EnqueueDouble(double value) => _doubles.Enqueue(value);

    public int Next(int minInclusive, int maxExclusive)
    {
        return _ints.Count > 0 ? _ints.Dequeue() : minInclusive;
    }

    public double NextDouble()
    {
        return _doubles.Count > 0 ? _doubles.Dequeue() : 0d;
    }
}
