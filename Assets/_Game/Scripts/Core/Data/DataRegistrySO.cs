using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Core.Data
{
    public abstract class DataRegistrySO<T> : ScriptableObject, IDataRegistry<T>
        where T : ScriptableObject, IIdentifiedData
    {
        [SerializeField] private T[] _items;

        private readonly List<T> _validItems = new List<T>();
        private Dictionary<string, T> _itemsById = new Dictionary<string, T>();

        public IReadOnlyCollection<T> All => _validItems;

        private void OnEnable()
        {
            RebuildIndex();
        }

        public bool TryGetById(string id, out T data)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                data = null;
                return false;
            }

            EnsureIndex();
            return _itemsById.TryGetValue(id, out data);
        }

        public T GetRequired(string id)
        {
            if (TryGetById(id, out var data))
            {
                return data;
            }

            throw new KeyNotFoundException($"Data id not found in {name}: {id}");
        }

        private void EnsureIndex()
        {
            if (_itemsById == null)
            {
                RebuildIndex();
            }
        }

        private void RebuildIndex()
        {
            _validItems.Clear();
            _itemsById = new Dictionary<string, T>();

            if (_items == null)
            {
                return;
            }

            foreach (var item in _items)
            {
                if (item == null)
                {
                    Debug.LogError($"Null item found in data registry {name}.", this);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.Id))
                {
                    Debug.LogError($"Data item without Id found in registry {name}: {item.name}.", item);
                    continue;
                }

                if (_itemsById.ContainsKey(item.Id))
                {
                    Debug.LogError($"Duplicate data Id found in registry {name}: {item.Id}.", item);
                    continue;
                }

                _itemsById.Add(item.Id, item);
                _validItems.Add(item);
            }
        }
    }
}
