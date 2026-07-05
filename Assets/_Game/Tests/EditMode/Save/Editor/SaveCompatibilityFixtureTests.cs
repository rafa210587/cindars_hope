using System.Collections.Generic;
using System.IO;
using System.Linq;
using CindarsHope.Save;
using CindarsHope.Save.Migrations;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Save
{
    public class SaveCompatibilityFixtureTests
    {
        private const int CurrentSchemaVersion = 5;

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void LegacyFixture_MigratesToCurrentSchemaAndRoundTrips(int sourceVersion)
        {
            string sourceJson = ReadFixture(sourceVersion);
            GameSaveData source = JsonUtility.FromJson<GameSaveData>(sourceJson);
            Assert.That(source, Is.Not.Null);
            Assert.That(source.SchemaVersion, Is.EqualTo(sourceVersion));
            Assert.That(source.Player, Is.Not.Null);

            var registry = CreateRegistry();
            Assert.That(registry.CanMigrate(sourceVersion, CurrentSchemaVersion), Is.True);

            var context = new SaveMigrationContext(
                $"fixture-v{sourceVersion}.json",
                $"fixture-v{sourceVersion}.backup.json",
                sourceVersion,
                CurrentSchemaVersion,
                sourceJson);
            bool migrated = registry.TryMigrate(context, out SaveMigrationResult result);

            Assert.That(migrated, Is.True, result?.ErrorMessage);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.True, result.ErrorMessage);
            Assert.That(result.TargetSchemaVersion, Is.EqualTo(CurrentSchemaVersion));

            GameSaveData current = JsonUtility.FromJson<GameSaveData>(result.MigratedJson);
            AssertCurrentSaveRoundTrips(current);
        }

        [Test]
        public void VersionOneFixture_PreservesLegacyInventoryAmountAcrossSlotMigration()
        {
            string sourceJson = ReadFixture(1);
            var registry = CreateRegistry();
            var context = new SaveMigrationContext(
                "fixture-v1.json",
                "fixture-v1.backup.json",
                1,
                CurrentSchemaVersion,
                sourceJson);

            Assert.That(registry.TryMigrate(context, out SaveMigrationResult result), Is.True);
            GameSaveData current = JsonUtility.FromJson<GameSaveData>(result.MigratedJson);

            Assert.That(current.Inventory, Is.Not.Null);
            Assert.That(current.Inventory.Slots.Sum(slot => slot.Amount), Is.EqualTo(120));
            Assert.That(current.Inventory.Slots.All(slot => slot.Amount > 0 && slot.Amount <= 99), Is.True);
        }

        [Test]
        public void CurrentFixture_RoundTripsWithoutMigration()
        {
            GameSaveData current = JsonUtility.FromJson<GameSaveData>(ReadFixture(CurrentSchemaVersion));
            AssertCurrentSaveRoundTrips(current);
        }

        private static SaveMigrationRegistry CreateRegistry()
        {
            return new SaveMigrationRegistry(new ISaveMigration[]
            {
                new InventorySlotsV1ToV2Migration(),
                new SaveV2ToV3Migration(),
                new SaveV3ToV4Migration(),
                new SaveV4ToV5Migration()
            });
        }

        private static void AssertCurrentSaveRoundTrips(GameSaveData current)
        {
            Assert.That(current, Is.Not.Null);
            Assert.That(current.SchemaVersion, Is.EqualTo(CurrentSchemaVersion));
            Assert.That(current.Player, Is.Not.Null);

            string roundTripJson = JsonUtility.ToJson(current, true);
            GameSaveData roundTripped = JsonUtility.FromJson<GameSaveData>(roundTripJson);

            Assert.That(roundTripped, Is.Not.Null);
            Assert.That(roundTripped.SchemaVersion, Is.EqualTo(CurrentSchemaVersion));
            Assert.That(roundTripped.Player, Is.Not.Null);
            Assert.That(roundTripped.CurrentDay, Is.EqualTo(current.CurrentDay));
            Assert.That(roundTripped.Player.Gold, Is.EqualTo(current.Player.Gold));
        }

        private static string ReadFixture(int schemaVersion)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            Assert.That(projectRoot, Is.Not.Null.And.Not.Empty);
            string path = Path.Combine(
                projectRoot,
                "tools",
                "architecture",
                "save-fixtures",
                $"save-v{schemaVersion}.json");
            Assert.That(File.Exists(path), Is.True, $"Save fixture not found: {path}");
            return File.ReadAllText(path);
        }
    }
}
