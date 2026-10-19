namespace CindarsHope.Foundation.Random
{
    public interface IRandomSource
    {
        float NextFloat();
        int NextInt(int minInclusive, int maxExclusive);
    }

    public interface IGameplayRandomSource : IRandomSource
    {
    }

    public interface IWorldRandomSource : IRandomSource
    {
    }

    public interface IVisualRandomSource : IRandomSource
    {
    }
}
