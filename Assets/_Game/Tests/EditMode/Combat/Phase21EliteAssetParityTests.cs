using CindarsHope.Combat;
using CindarsHope.Combat.Bestiary;
using NUnit.Framework;
using UnityEditor;

namespace CindarsHope.Tests.EditMode.Combat
{
    public sealed class Phase21EliteAssetParityTests
    {
        [Test]
        public void CanonicalEnemyAssets_PreserveBestiaryEliteRoleForKillTelemetry()
        {
            foreach (var definition in CanonicalBestiaryCatalog.All)
            {
                var asset = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(
                    $"Assets/_Game/Data/Enemies/Canonical/{definition.EnemyId}.asset");
                Assert.That(asset, Is.Not.Null, definition.EnemyId);
                Assert.That(asset.IsElite, Is.EqualTo(definition.Role == EnemyRole.Elite),
                    $"{definition.EnemyId}: kill events read EnemyDataSO.IsElite.");
            }
        }
    }
}
