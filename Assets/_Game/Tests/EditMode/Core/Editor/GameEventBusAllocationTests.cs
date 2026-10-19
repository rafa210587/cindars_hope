using System;
using CindarsHope.Core;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Core
{
    public class GameEventBusAllocationTests
    {
        private readonly struct AllocationProbeEvent
        {
            public AllocationProbeEvent(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        [TearDown]
        public void TearDown()
        {
            GameEventBus.Clear<AllocationProbeEvent>();
        }

        [Test]
        public void WarmDispatch_DoesNotAllocatePerEvent()
        {
            int observed = 0;
            GameEventBus.Subscribe<AllocationProbeEvent>(evt => observed += evt.Value);
            GameEventBus.Publish(new AllocationProbeEvent(1));

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1000; i++)
            {
                GameEventBus.Publish(new AllocationProbeEvent(1));
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(observed, Is.EqualTo(1001));
            Assert.That(allocated, Is.Zero, "Warm EventBus dispatch must not allocate.");
        }
    }
}
