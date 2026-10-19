using System;

namespace CindarsHope.Gameplay.Input
{
    public enum GameplayInputBlockReason
    {
        FarmActionMenu = 0
    }

    /// <summary>
    /// Pure tokenized gate shared by gameplay owners and input adapters. A lease only releases its
    /// own claim, so nested UI owners cannot accidentally re-enable movement for each other.
    /// </summary>
    public static class GameplayInputBlocker
    {
        private static readonly int[] Counts =
            new int[Enum.GetValues(typeof(GameplayInputBlockReason)).Length];
        private static int _totalCount;

        public static bool IsBlocked => _totalCount > 0;

        public static bool IsBlockedBy(GameplayInputBlockReason reason)
        {
            int index = (int)reason;
            return index >= 0 && index < Counts.Length && Counts[index] > 0;
        }

        public static IDisposable Acquire(GameplayInputBlockReason reason)
        {
            int index = (int)reason;
            if (index < 0 || index >= Counts.Length)
                throw new ArgumentOutOfRangeException(nameof(reason));

            Counts[index]++;
            _totalCount++;
            return new Lease(reason);
        }

        public static void Reset()
        {
            Array.Clear(Counts, 0, Counts.Length);
            _totalCount = 0;
        }

        private static void Release(GameplayInputBlockReason reason)
        {
            int index = (int)reason;
            if (index < 0 || index >= Counts.Length || Counts[index] <= 0) return;
            Counts[index]--;
            if (_totalCount > 0) _totalCount--;
        }

        private sealed class Lease : IDisposable
        {
            private readonly GameplayInputBlockReason _reason;
            private bool _disposed;

            public Lease(GameplayInputBlockReason reason) => _reason = reason;

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                Release(_reason);
            }
        }
    }
}
