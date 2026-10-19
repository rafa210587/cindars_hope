using CindarsHope.Foundation;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Core
{
    public sealed class DomainManagerRegistryTests
    {
        private sealed class TestManager
        {
        }

        [TearDown]
        public void TearDown()
        {
            DomainManagerRegistry.Unregister<TestManager>();
        }

        [Test]
        public void UnregisterWithInstance_DoesNotRemoveNewerRegisteredInstance()
        {
            var oldInstance = new TestManager();
            var newInstance = new TestManager();

            DomainManagerRegistry.Register(oldInstance);
            DomainManagerRegistry.Register(newInstance);
            DomainManagerRegistry.Unregister(oldInstance);

            Assert.That(DomainManagerRegistry.Get<TestManager>(), Is.SameAs(newInstance));
        }

        [Test]
        public void UnregisterWithCurrentInstance_RemovesRegistration()
        {
            var instance = new TestManager();

            DomainManagerRegistry.Register(instance);
            DomainManagerRegistry.Unregister(instance);

            Assert.That(DomainManagerRegistry.Get<TestManager>(), Is.Null);
        }
    }
}
