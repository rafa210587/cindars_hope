using CindarsHope.Foundation.Random;

namespace CindarsHope.Core.Random
{
    public sealed class SeededGameplayRandomSource : IGameplayRandomSource
    {
        private readonly System.Random _random;

        public SeededGameplayRandomSource(int seed)
        {
            _random = new System.Random(seed);
        }

        public float NextFloat() => (float)_random.NextDouble();
        public int NextInt(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);
    }

    public sealed class SeededWorldRandomSource : IWorldRandomSource
    {
        private readonly System.Random _random;

        public SeededWorldRandomSource(int seed)
        {
            _random = new System.Random(seed);
        }

        public float NextFloat() => (float)_random.NextDouble();
        public int NextInt(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);
    }

    /// <summary>Somente efeitos visuais podem consumir o estado global não determinístico do Unity.</summary>
    public sealed class UnityVisualRandomSource : IVisualRandomSource
    {
        public static readonly UnityVisualRandomSource Shared = new UnityVisualRandomSource();

        private UnityVisualRandomSource()
        {
        }

        public float NextFloat() => UnityEngine.Random.value;
        public int NextInt(int minInclusive, int maxExclusive) =>
            UnityEngine.Random.Range(minInclusive, maxExclusive);
    }

    /// <summary>Adapter de compatibilidade para rolls de gameplay ainda baseados no estado Unity.</summary>
    public sealed class UnityGameplayRandomSource : IGameplayRandomSource
    {
        public static readonly UnityGameplayRandomSource Shared = new UnityGameplayRandomSource();

        private UnityGameplayRandomSource()
        {
        }

        public float NextFloat() => UnityEngine.Random.value;
        public int NextInt(int minInclusive, int maxExclusive) =>
            UnityEngine.Random.Range(minInclusive, maxExclusive);
    }
}
