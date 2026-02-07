namespace Game.Api.Services;

public interface IGameRandom
{
    int Next(int minInclusive, int maxExclusive);

    double NextDouble();
}
