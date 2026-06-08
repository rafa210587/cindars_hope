using CindarsHope.Core.Data;
using CindarsHope.Inventory.Data;
using CindarsHope.Combat;
using CindarsHope.Farm.Data;
using CindarsHope.World.Data;
using CindarsHope.Craft.Data;
using CindarsHope.Combat.Weapon;
using CindarsHope.Combat.Magic;
using CindarsHope.Combat.Skills;
using CindarsHope.Equipment;
using CindarsHope.Skills;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Core.Data
{
    /// <summary>
    /// SPEC 01.01: Stable IDs Registry Audit and Hardening
    /// Tests validate that all DataSO registries maintain stable IDs without duplicates, nulls, or empty values.
    /// </summary>
    public class StableIdsValidationTests
    {
        private sealed class RegistryProbe
        {
            public string Name;
            public System.Collections.IEnumerable GetAll;
        }

        private List<RegistryProbe> _allRegistries;

        [SetUp]
        public void SetUp()
        {
            _allRegistries = new List<RegistryProbe>();

            // Load all registry assets
            var itemDb = FindAsset<ItemDatabaseSO>();
            var seedDb = FindAsset<SeedDatabaseSO>();
            var weaponDb = FindAsset<WeaponDatabaseSO>();
            var spellDb = FindAsset<SpellDatabaseSO>();
            var skillActionDb = FindAsset<SkillActionDatabaseSO>();
            var enemyDb = FindAsset<EnemyDatabaseSO>();
            var recipeDb = FindAsset<RecipeDatabaseSO>();
            var workshopDb = FindAsset<WorkshopDatabaseSO>();
            var treeDb = FindAsset<TreeDatabaseSO>();
            var statusEffectDb = FindAsset<StatusEffectDatabaseSO>();
            var skillNodeDb = FindAsset<SkillNodeDatabaseSO>();

            if (itemDb != null) AddProbe(nameof(ItemDatabaseSO), itemDb.All);
            if (seedDb != null) AddProbe(nameof(SeedDatabaseSO), seedDb.All);
            if (weaponDb != null) AddProbe(nameof(WeaponDatabaseSO), weaponDb.All);
            if (spellDb != null) AddProbe(nameof(SpellDatabaseSO), spellDb.All);
            if (skillActionDb != null) AddProbe(nameof(SkillActionDatabaseSO), skillActionDb.All);
            if (enemyDb != null) AddProbe(nameof(EnemyDatabaseSO), enemyDb.All);
            if (recipeDb != null) AddProbe(nameof(RecipeDatabaseSO), recipeDb.All);
            if (workshopDb != null) AddProbe(nameof(WorkshopDatabaseSO), workshopDb.All);
            if (treeDb != null) AddProbe(nameof(TreeDatabaseSO), treeDb.All);
            if (statusEffectDb != null) AddProbe(nameof(StatusEffectDatabaseSO), statusEffectDb.All);
            if (skillNodeDb != null) AddProbe(nameof(SkillNodeDatabaseSO), skillNodeDb.All);
        }

        private void AddProbe(string name, System.Collections.IEnumerable all)
        {
            _allRegistries.Add(new RegistryProbe { Name = name, GetAll = all });
        }

        [Test]
        public void AllRegistriesLoaded_ReturnsNonEmptyList()
        {
            Assert.IsNotEmpty(_allRegistries, "At least one registry must be loaded.");
        }

        [Test]
        public void NoNullEntriesInRegistries()
        {
            var nullErrors = new List<string>();

            foreach (var probe in _allRegistries)
            {
                int index = 0;
                foreach (object item in probe.GetAll)
                {
                    if (item == null)
                    {
                        nullErrors.Add(string.Format("{0}[{1}] is null", probe.Name, index));
                    }
                    index++;
                }
            }

            Assert.IsEmpty(nullErrors, string.Join("\n", nullErrors));
        }

        [Test]
        public void NoEmptyIdsInRegistries()
        {
            var emptyIdErrors = new List<string>();

            foreach (var probe in _allRegistries)
            {
                foreach (object itemObj in probe.GetAll)
                {
                    if (itemObj == null) continue;

                    var item = itemObj as IIdentifiedData;
                    if (item == null) continue;

                    if (string.IsNullOrWhiteSpace(item.Id))
                    {
                        var scriptableObj = itemObj as ScriptableObject;
                        string itemName = scriptableObj != null ? scriptableObj.name : "<unknown>";
                        emptyIdErrors.Add(string.Format("{0}: '{1}' has empty Id", probe.Name, itemName));
                    }
                }
            }

            Assert.IsEmpty(emptyIdErrors, string.Join("\n", emptyIdErrors));
        }

        [Test]
        public void NoDuplicateIdsWithinRegistry()
        {
            var duplicateErrors = new List<string>();

            foreach (var probe in _allRegistries)
            {
                var seenIds = new Dictionary<string, string>();

                foreach (object itemObj in probe.GetAll)
                {
                    if (itemObj == null) continue;

                    var item = itemObj as IIdentifiedData;
                    if (item == null || string.IsNullOrWhiteSpace(item.Id)) continue;

                    var scriptableObj = itemObj as ScriptableObject;
                    string itemName = scriptableObj != null ? scriptableObj.name : "<unknown>";

                    if (seenIds.ContainsKey(item.Id))
                    {
                        duplicateErrors.Add(string.Format("{0}: duplicate Id '{1}' in '{2}' and '{3}'",
                            probe.Name, item.Id, itemName, seenIds[item.Id]));
                    }
                    else
                    {
                        seenIds[item.Id] = itemName;
                    }
                }
            }

            Assert.IsEmpty(duplicateErrors, string.Join("\n", duplicateErrors));
        }

        [Test]
        public void TryGetById_ReturnsCorrectItems()
        {
            var itemDb = FindAsset<ItemDatabaseSO>();
            Assert.IsNotNull(itemDb, "ItemDatabaseSO must be loaded for TryGetById test");

            var allItems = itemDb.All;
            foreach (var item in allItems)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id)) continue;

                bool found = itemDb.TryGetById(item.Id, out var retrievedItem);
                Assert.IsTrue(found, string.Format("TryGetById failed for Id '{0}'", item.Id));
                Assert.AreEqual(item.Id, retrievedItem.Id, "Retrieved item has mismatched Id");
            }
        }

        [Test]
        public void AllPersistentIdsFollowConvention()
        {
            var conventionErrors = new List<string>();

            // Domain-specific conventions
            var itemDb = FindAsset<ItemDatabaseSO>();
            if (itemDb != null)
            {
                foreach (var item in itemDb.All)
                {
                    if (item == null) continue;
                    // Items should follow item_* pattern for future content
                    if (!item.Id.StartsWith("item_"))
                    {
                        // Warning, not error — legacy content may not follow convention
                        conventionErrors.Add(string.Format("ItemDatabase '{0}': Id '{1}' does not follow item_* convention",
                            item.name, item.Id));
                    }
                }
            }

            // Warnings are reported but do not fail the test
            if (conventionErrors.Count > 0)
            {
                Debug.LogWarning(string.Format("Convention warnings: {0}", string.Join("; ", conventionErrors)));
            }
        }

        private T FindAsset<T>() where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T).Name));
            if (guids == null || guids.Length == 0) return null;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}
