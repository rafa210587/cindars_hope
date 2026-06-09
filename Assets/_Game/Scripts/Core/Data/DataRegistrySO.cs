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
        private bool _indexBuilt;

        public IReadOnlyCollection<T> All
        {
            get
            {
                EnsureIndex();
                return _validItems;
            }
        }

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
            if (!_indexBuilt || _itemsById == null)
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
                _indexBuilt = true;
                return;
            }

            // SPEC 14A-FIX11: detailed diagnostic so the offender is obvious.
            int nullCount = 0;
            int emptyIdCount = 0;
            int duplicateCount = 0;
            int validCount = 0;
            string registryPath = ResolveRegistryAssetPath();
            string typeName = typeof(T).Name;
            for (int i = 0; i < _items.Length; i++)
            {
                var item = _items[i];
                if (item == null)
                {
                    nullCount++;
                    Debug.LogError($"DataRegistry FAIL: Null item in {name} (type {typeName}). Index={i}, TotalSlots={_items.Length}, RegistryAssetPath={registryPath}. Likely cause: deleted asset reference still in the array or YAML 'type' field wrong (asset refs need type:2). Open Advanced/Validate Registries to repair.", this);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.Id))
                {
                    emptyIdCount++;
                    Debug.LogError($"DataRegistry FAIL: Item without Id in {name} (type {typeName}). Index={i}, AssetName={item.name}, RegistryAssetPath={registryPath}.", item);
                    continue;
                }

                if (_itemsById.ContainsKey(item.Id))
                {
                    duplicateCount++;
                    Debug.LogError($"DataRegistry FAIL: Duplicate Id in {name} (type {typeName}). Index={i}, Id='{item.Id}', AssetName={item.name}, RegistryAssetPath={registryPath}.", item);
                    continue;
                }

                _itemsById.Add(item.Id, item);
                _validItems.Add(item);
                validCount++;
            }

            if (nullCount + emptyIdCount + duplicateCount > 0)
            {
                Debug.LogWarning($"DataRegistry summary: {name} (type {typeName}). Valid={validCount}, Null={nullCount}, EmptyId={emptyIdCount}, Duplicate={duplicateCount}, TotalSlots={_items.Length}.", this);
            }

            _indexBuilt = true;
        }

        private string ResolveRegistryAssetPath()
        {
#if UNITY_EDITOR
            var path = UnityEditor.AssetDatabase.GetAssetPath(this);
            return string.IsNullOrEmpty(path) ? "<in-memory>" : path;
#else
            return "<runtime>";
#endif
        }

#if UNITY_EDITOR
        // SPEC 14A-FIX11: editor-only repair helpers used by CindarsHope > Repair & Validate Project.
        // Operate via SerializedObject so changes persist into the .asset file.
        public int RemoveNullEntries()
        {
            var so = new UnityEditor.SerializedObject(this);
            var prop = so.FindProperty("_items");
            if (prop == null || !prop.isArray) return 0;
            int removed = 0;
            for (int i = prop.arraySize - 1; i >= 0; i--)
            {
                var element = prop.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue == null)
                {
                    prop.DeleteArrayElementAtIndex(i);
                    removed++;
                }
            }
            if (removed > 0)
            {
                so.ApplyModifiedProperties();
                UnityEditor.EditorUtility.SetDirty(this);
                RebuildIndex();
            }
            return removed;
        }

        public RegistryValidationReport ValidateRegistry()
        {
            var report = new RegistryValidationReport { RegistryName = name, RegistryAssetPath = ResolveRegistryAssetPath(), TypeName = typeof(T).Name };
            if (_items == null) return report;
            var seen = new Dictionary<string, int>();
            for (int i = 0; i < _items.Length; i++)
            {
                var item = _items[i];
                report.TotalSlots++;
                if (item == null) { report.NullEntries.Add(i); continue; }
                if (string.IsNullOrWhiteSpace(item.Id)) { report.EmptyIdEntries.Add(i); continue; }
                if (seen.ContainsKey(item.Id)) { report.DuplicateIds.Add((i, item.Id, seen[item.Id])); continue; }
                seen.Add(item.Id, i);
                report.ValidCount++;
            }
            return report;
        }
#endif
    }

#if UNITY_EDITOR
    public sealed class RegistryValidationReport
    {
        public string RegistryName;
        public string RegistryAssetPath;
        public string TypeName;
        public int TotalSlots;
        public int ValidCount;
        public System.Collections.Generic.List<int> NullEntries = new System.Collections.Generic.List<int>();
        public System.Collections.Generic.List<int> EmptyIdEntries = new System.Collections.Generic.List<int>();
        public System.Collections.Generic.List<(int Index, string Id, int FirstIndex)> DuplicateIds = new System.Collections.Generic.List<(int, string, int)>();

        public bool HasIssues => NullEntries.Count + EmptyIdEntries.Count + DuplicateIds.Count > 0;

        public string Summarize()
        {
            return $"{RegistryName} ({TypeName}) at '{RegistryAssetPath}': Valid={ValidCount}/{TotalSlots}, Null={NullEntries.Count}, EmptyId={EmptyIdEntries.Count}, Duplicate={DuplicateIds.Count}";
        }
    }
#endif
}
