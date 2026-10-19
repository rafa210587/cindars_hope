using System.Collections.Generic;
using CindarsHope.Core;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    public class Physics2DOverlapBufferTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject instance in _objects)
            {
                Object.DestroyImmediate(instance);
            }

            _objects.Clear();
        }

        [Test]
        public void QueryCircle_GrowsWhenFullAndReusesExpandedCapacity()
        {
            var center = new Vector2(10000f, 10000f);
            for (int i = 0; i < 40; i++)
            {
                var instance = new GameObject($"PhaseOneCollider_{i}");
                instance.transform.position = center;
                instance.AddComponent<CircleCollider2D>().radius = 0.1f;
                _objects.Add(instance);
            }

            Physics2D.SyncTransforms();
            var buffer = new Physics2DOverlapBuffer(initialCapacity: 4);

            int firstCount = buffer.QueryCircle(center, 1f, ContactFilter2D.noFilter);
            int expandedCapacity = buffer.Capacity;
            int secondCount = buffer.QueryCircle(center, 1f, ContactFilter2D.noFilter);

            Assert.That(firstCount, Is.EqualTo(40));
            Assert.That(secondCount, Is.EqualTo(40));
            Assert.That(expandedCapacity, Is.GreaterThanOrEqualTo(40));
            Assert.That(buffer.Capacity, Is.EqualTo(expandedCapacity));
        }
    }
}
