using System.IO;
using CindarsHope.Cave.Data;
using CindarsHope.Combat;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.CaveData
{
    public static class CreateCaveBossAssets
    {
        private const string CaveDataPath = "Assets/_Game/Data/Cave";
        private const string EnemiesDataPath = "Assets/_Game/Data/Enemies";
        private const string EnemyDatabasePath = "Assets/_Game/Data/Combat/EnemyDatabase.asset";
        private const string RegistryPath = "Assets/_Game/Data/Cave/CaveBossGateRegistry.asset";

        [MenuItem("CindarsHope/Advanced/Legacy/Generate/Scenes/Create Boss Gate Assets")]
        public static void CreateAll()
        {
            EnsureDirectory(CaveDataPath);
            EnsureDirectory(EnemiesDataPath);

            var bossEnemyData = EnsureBossEnemyData();
            var gate15 = EnsureBossGate(15, "boss_gate_level_15", 0, false);
            var gate30 = EnsureBossGate(30, "boss_gate_level_30", 15, false);
            var gate45 = EnsureBossGate(45, "boss_gate_level_45", 30, false);
            var gate60 = EnsureBossGate(60, "boss_gate_level_60", 45, false);
            var gate75 = EnsureBossGate(75, "boss_gate_level_75", 60, false);
            var gate90 = EnsureBossGate(90, "boss_gate_level_90", 75, true);

            WireRegistry(gate15, gate30, gate45, gate60, gate75, gate90);
            AddBossToEnemyDatabase(bossEnemyData);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CreateCaveBossAssets] All boss gate assets created and registry wired. Regenerate CaveScene to activate.");
        }

        private static EnemyDataSO EnsureBossEnemyData()
        {
            var path = $"{EnemiesDataPath}/enemy_meteor_ooze_king.asset";
            var existing = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
            if (existing != null)
            {
                Debug.Log($"[CreateCaveBossAssets] enemy_meteor_ooze_king already exists at {path}.");
                return existing;
            }

            var asset = ScriptableObject.CreateInstance<EnemyDataSO>();
            asset.enemyId = "enemy_meteor_ooze_king";
            asset.DisplayName = "Meteor Ooze King";
            asset.Description = "A massive ooze crystallized by years of exposure to cave minerals. Guards the deepest passages with crushing force.";
            asset.FactionId = "faction_beast";
            asset.PrimaryRole = EnemyRole.Boss;
            asset.CaveBand = 1;
            asset.BiomeTags = new[] { "stone_cavern" };
            asset.EnvironmentTags = new[] { "underground" };
            asset.SizeProfileId = "size_huge";
            asset.MovementProfileId = "movement_slow_heavy";
            asset.aiBehaviorId = "ai_boss_chaser";
            asset.maxHp = 200;
            asset.contactDamage = 12;
            asset.contactDamageCooldownSeconds = 1.0f;
            asset.contactKnockbackForce = 6f;
            asset.defense = 5;
            asset.receivedKnockbackResistance = 0.6f;
            asset.receivedKnockbackMultiplier = 0.4f;
            asset.moveSpeed = 2.5f;
            asset.detectionRadius = 10f;
            asset.stopDistance = 0.6f;
            asset.hitFlashColor = new Color(1f, 0.3f, 0f, 1f);
            asset.hitFlashDuration = 0.15f;
            asset.dropItemId = "item_nothing";
            asset.dropAmount = 0;
            asset.lootTableId = "loot_boss_meteor_ooze_king";
            asset.enemyLevel = 15;
            asset.baseDifficulty = EnemyDifficulty.Boss;
            asset.xpReward = 150;
            asset.IsBoss = true;
            AssetDatabase.CreateAsset(asset, path);
            Debug.Log($"[CreateCaveBossAssets] Created {path}.");
            return asset;
        }

        private static CaveBossGateDataSO EnsureBossGate(int level, string id, int requiredPreviousGateLevel, bool isLast)
        {
            var path = $"{CaveDataPath}/BossGate_Level{level}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<CaveBossGateDataSO>(path);

            CaveBossGateDataSO asset;
            if (existing != null)
            {
                asset = existing;
            }
            else
            {
                asset = ScriptableObject.CreateInstance<CaveBossGateDataSO>();
                AssetDatabase.CreateAsset(asset, path);
                Debug.Log($"[CreateCaveBossAssets] Created {path}.");
            }

            // Use SerializedObject to set all fields including read-only properties backed by private fields
            var so = new SerializedObject(asset);
            so.FindProperty("_id").stringValue = id;
            so.FindProperty("_caveLevel").intValue = level;
            so.FindProperty("_biomeId").stringValue = "biome_cave_earth";
            so.FindProperty("_bossEnemyId").stringValue = "enemy_meteor_ooze_king";
            so.FindProperty("_checkpointUnlockedOnDefeat").intValue = level;
            so.FindProperty("_unlocksCheckpointPortalDestination").intValue = level;

            if (requiredPreviousGateLevel > 0)
            {
                so.FindProperty("_requiredPreviousGateId").stringValue = CaveBossGateRegistrySO.BuildDefaultGateId(requiredPreviousGateLevel);
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);

            if (existing != null && (existing.Id != id || existing.CaveLevel != level))
            {
                Debug.Log($"[CreateCaveBossAssets] Fixed BossGate_Level{level} serialization.");
            }

            return asset;
        }

        private static void WireRegistry(params CaveBossGateDataSO[] gates)
        {
            var registry = AssetDatabase.LoadAssetAtPath<CaveBossGateRegistrySO>(RegistryPath);
            if (registry == null)
            {
                registry = ScriptableObject.CreateInstance<CaveBossGateRegistrySO>();
                AssetDatabase.CreateAsset(registry, RegistryPath);
            }

            var serializedObject = new SerializedObject(registry);
            var gatesProp = serializedObject.FindProperty("_gates");
            gatesProp.ClearArray();

            for (var i = 0; i < gates.Length; i++)
            {
                gatesProp.InsertArrayElementAtIndex(i);
                gatesProp.GetArrayElementAtIndex(i).objectReferenceValue = gates[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(registry);
            Debug.Log($"[CreateCaveBossAssets] Registry wired with {gates.Length} gates.");
        }

        private static void AddBossToEnemyDatabase(EnemyDataSO bossData)
        {
            var db = AssetDatabase.LoadAssetAtPath<EnemyDatabaseSO>(EnemyDatabasePath);
            if (db == null)
            {
                Debug.LogWarning($"[CreateCaveBossAssets] EnemyDatabase not found at {EnemyDatabasePath}. Cannot add boss enemy.");
                return;
            }

            var serializedObject = new SerializedObject(db);
            var itemsProp = serializedObject.FindProperty("_items");

            for (var i = 0; i < itemsProp.arraySize; i++)
            {
                if (itemsProp.GetArrayElementAtIndex(i).objectReferenceValue == bossData)
                {
                    Debug.Log("[CreateCaveBossAssets] Boss enemy already in EnemyDatabase.");
                    return;
                }
            }

            var idx = itemsProp.arraySize;
            itemsProp.InsertArrayElementAtIndex(idx);
            itemsProp.GetArrayElementAtIndex(idx).objectReferenceValue = bossData;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(db);
            Debug.Log("[CreateCaveBossAssets] Boss enemy added to EnemyDatabase.");
        }

        private static void EnsureDirectory(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parts = path.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
