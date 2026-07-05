using System;
using UnityEngine;

namespace CindarsHope.Core
{
    /// <summary>
    /// Reusable buffer for Physics2D overlap queries. It grows only when a query fills the current
    /// capacity, then reuses the expanded array on subsequent calls.
    /// </summary>
    public sealed class Physics2DOverlapBuffer
    {
        private const int DefaultCapacity = 32;

        private Collider2D[] _results;

        public Physics2DOverlapBuffer(int initialCapacity = DefaultCapacity)
        {
            _results = new Collider2D[Mathf.Max(1, initialCapacity)];
        }

        public int Count { get; private set; }
        public int Capacity => _results.Length;
        public Collider2D this[int index] => _results[index];

        public int QueryCircle(Vector2 center, float radius, ContactFilter2D filter)
        {
            while (true)
            {
                int count = Physics2D.OverlapCircle(center, radius, filter, _results);
                if (count < _results.Length)
                {
                    Count = count;
                    return count;
                }

                Array.Resize(ref _results, checked(_results.Length * 2));
            }
        }
    }
}
