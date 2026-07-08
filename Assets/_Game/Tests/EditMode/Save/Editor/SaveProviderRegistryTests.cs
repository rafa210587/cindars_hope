using System;
using CindarsHope.Inventory;
using CindarsHope.Save;
using CindarsHope.UI.Hotbar;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Save
{
    public class SaveProviderRegistryTests
    {
        private sealed class ProviderStub : ISaveSectionProvider
        {
            private readonly object _captured;

            public ProviderStub(string id, object captured)
            {
                ProviderId = id;
                _captured = captured;
            }

            public string ProviderId { get; }
            public object Restored { get; private set; }
            public object Capture(GameSaveData existingSaveData) => _captured;
            public void Restore(object sectionData) => Restored = sectionData;
        }

        [Test]
        public void TypedDescriptor_CapturesAndRestoresMatchingSection()
        {
            var section = new HotbarSaveData();
            var provider = new ProviderStub("hotbar", section);
            var registry = new SaveProviderRegistry();
            registry.Register<HotbarSaveData>(provider, 1);

            Assert.That(registry.Capture<HotbarSaveData>("hotbar", null), Is.SameAs(section));
            registry.Restore("hotbar", section);
            Assert.That(provider.Restored, Is.SameAs(section));
        }

        [Test]
        public void TypedDescriptor_RejectsMismatchedCaptureType()
        {
            var registry = new SaveProviderRegistry();
            registry.Register<HotbarSaveData>(new ProviderStub("hotbar", new InventorySaveData()), 1);

            Assert.Throws<InvalidOperationException>(
                () => registry.Capture<HotbarSaveData>("hotbar", null));
        }
    }
}
