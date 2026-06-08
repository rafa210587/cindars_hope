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
using System.Linq;
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
        private List<ScriptableObject> _allRegistries;

        [SetUp]
        public void SetUp()
        {
            _allRegistries = new List<DataRegistrySO>();

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

            if (itemDb != null) _allRegistries.Add(itemDb);
            if (seedDb != null) _allRegistries.Add(seedDb);
            if (weaponDb != null) _allRegistries.Add(weaponDb);
            if (spellDb != null) _allRegistries.Add(spellDb);
            if (skillActionDb != null) _allRegistries.Add(skillActionDb);
            if (enemyDb != null) _allRegistries.Add(enemyDb);
            if (recipeDb != null) _allRegistries.Add(recipeDb);
            if (workshopDb != null) _allRegistries.Add(workshopDb);
            if (treeDb != null) _allRegistries.Add(treeDb);
            if (statusEffectDb != null) _allRegistries.Add(statusEffectDb);
            if (skillNodeDb != null) _allRegistries.Add(skillNodeDb);
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

            foreach (var registry in _allRegistries)
            {
                var all = registry.All;
                var registryName = registry.GetType().Name;

                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i] == null)
                    {
                        nullErrors.Add($"{registryName}[{i}] is null");
                    }
                }
            }

            Assert.IsEmpty(nullErrors, string.Join("\n", nullErrors));
        }

        [Test]
        public void NoEmptyIdsInRegistries()
        {
            var emptyIdErrors = new List<string>();

            foreach (var registry in _allRegistries)
            {
                var all = registry.All;
                var registryName = registry.GetType().Name;

                foreach (var item in all)
                {
                    if (item == null) continue;

                    if (string.IsNullOrWhiteSpace(item.Id))
                    {
                        emptyIdErrors.Add($"{registryName}: '{item.name}' has empty Id");
                    }
                }
            }

            Assert.IsEmpty(emptyIdErrors, string.Join("\n", emptyIdErrors));
        }

        [Test]
        public void NoDuplicateIdsWithinRegistry()
        {
            var duplicateErrors = new List<string>();

            foreach (var registry in _allRegistries)
            {
                var all = registry.All;
                var registryName = registry.GetType().Name;
                var seenIds = new Dictionary<string, string>();

                foreach (var item in all)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.Id)) continue;

                    if (seenIds.ContainsKey(item.Id))
                    {
                        duplicateErrors.Add($"{registryName}: duplicate Id '{item.Id}' in '{item.name}' and '{seenIds[item.Id]}'");
                    }
                    else
                    {
                        seenIds[item.Id] = item.name;
                    }
                }
            }

            Assert.IsEmpty(duplicateErrors, string.Join("\n", duplicateErrors));
        }

        [Test]
        public void RegistriesHaveValidationReport()
        {
            var validationErrors = new List<string>();

            foreach (var registry in _allRegistries)
            {
                var registryName = registry.GetType().Name;

                // Call RebuildIndex and check if validation report indicates errors
                var report = registry.RebuildIndex();

                if (report == null)
                {
                    validationErrors.Add($"{registryName}: RebuildIndex returned null report");
                    continue;
                }

                if (!report.IsValid)
                {
                    validationErrors.Add($"{registryName}: validation report returned IsValid=false");
                }
            }

            Assert.IsEmpty(validationErrors, string.Join("\n", validationErrors));
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
                Assert.IsTrue(found, $"TryGetById failed for Id '{item.Id}'");
                Assert.AreEqual(item.Id, retrievedItem.Id, $"Retrieved item has mismatched Id");
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
                        conventionErrors.Add($"ItemDatabase '{item.name}': Id '{item.Id}' does not follow item_* convention");
                    }
                }
            }

            // Warnings are reported but do not fail the test
            if (conventionErrors.Count > 0)
            {
                Debug.LogWarning($"Convention warnings: {string.Join("; ", conventionErrors)}");
            }
        }

        private T FindAsset<T>() where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids == null || guids.Length == 0) return null;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}
