using System;
using System.Collections.Generic;

namespace CindarsHope.Save
{
    public interface ISaveSectionDescriptor
    {
        string ProviderId { get; }
        Type SectionType { get; }
        int RestoreOrder { get; }
    }

    public sealed class SaveProviderRegistry
    {
        private readonly Dictionary<string, ISaveSectionDescriptor> _descriptors =
            new Dictionary<string, ISaveSectionDescriptor>(StringComparer.Ordinal);

        public int Count => _descriptors.Count;

        public void Register<TSection>(ISaveSectionProvider provider, int restoreOrder)
            where TSection : class
        {
            if (provider == null || string.IsNullOrWhiteSpace(provider.ProviderId))
            {
                throw new ArgumentException("Save provider and ProviderId are required.", nameof(provider));
            }

            _descriptors[provider.ProviderId] =
                new SaveSectionDescriptor<TSection>(provider, restoreOrder);
        }

        public TSection Capture<TSection>(string providerId, GameSaveData existingSaveData)
            where TSection : class
        {
            return Get<TSection>(providerId).Capture(existingSaveData);
        }

        public void Restore<TSection>(string providerId, TSection section)
            where TSection : class
        {
            Get<TSection>(providerId).Restore(section);
        }

        private SaveSectionDescriptor<TSection> Get<TSection>(string providerId)
            where TSection : class
        {
            if (!_descriptors.TryGetValue(providerId, out ISaveSectionDescriptor descriptor))
            {
                throw new KeyNotFoundException($"Save provider descriptor not registered: {providerId}");
            }

            if (descriptor is not SaveSectionDescriptor<TSection> typed)
            {
                throw new InvalidOperationException(
                    $"Save provider '{providerId}' is registered for {descriptor.SectionType.Name}, " +
                    $"not {typeof(TSection).Name}.");
            }

            return typed;
        }

        private sealed class SaveSectionDescriptor<TSection> : ISaveSectionDescriptor
            where TSection : class
        {
            private readonly ISaveSectionProvider _provider;

            public SaveSectionDescriptor(ISaveSectionProvider provider, int restoreOrder)
            {
                _provider = provider;
                RestoreOrder = restoreOrder;
            }

            public string ProviderId => _provider.ProviderId;
            public Type SectionType => typeof(TSection);
            public int RestoreOrder { get; }

            public TSection Capture(GameSaveData existingSaveData)
            {
                object section = _provider.Capture(existingSaveData);
                if (section == null)
                {
                    return null;
                }

                if (section is TSection typed)
                {
                    return typed;
                }

                throw new InvalidOperationException(
                    $"Provider '{ProviderId}' captured {section.GetType().Name}; expected {typeof(TSection).Name}.");
            }

            public void Restore(TSection section)
            {
                _provider.Restore(section);
            }
        }
    }
}
