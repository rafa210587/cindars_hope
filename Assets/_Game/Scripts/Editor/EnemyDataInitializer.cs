#if UNITY_EDITOR
using CindarsHope.Combat;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor
{
    [InitializeOnLoad]
    public class EnemyDataInitializer
    {
        private const string EnemyPath = "Assets/_Game/Data/Enemy/";
        private const string InitKey = "EnemyDataInitialized";

        static EnemyDataInitializer()
        {
            if (!SessionState.GetBool(InitKey, false))
            {
                SessionState.SetBool(InitKey, true);
                GenerateDefaultEnemyData();
            }
        }

        private static void GenerateDefaultEnemyData()
        {
            CreateEnemyFolders();
            CreateEnemies();
            AssetDatabase.SaveAssets();
        }

        private static void CreateEnemyFolders()
        {
            string enemyDir = "Assets/_Game/Data/Enemy";
            if (!AssetDatabase.IsValidFolder(enemyDir))
            {
                AssetDatabase.CreateFolder("Assets/_Game/Data", "Enemy");
            }
        }

        private static void CreateEnemies()
        {
            var enemies = new[]
            {
                ("enemy_slime_basic", "Basic Slime", 1, 20, 3, 0, 10, EnemyDifficulty.Easy),
                ("enemy_goblin_scout", "Goblin Scout", 5, 30, 5, 1, 20, EnemyDifficulty.Normal),
                ("enemy_orc_warrior", "Orc Warrior", 10, 50, 10, 3, 40, EnemyDifficulty.Hard),
                ("enemy_spider_ice", "Ice Spider", 12, 35, 8, 2, 35, EnemyDifficulty.Hard),
            };

            foreach (var (id, name, level, hp, dmg, def, xp, difficulty) in enemies)
            {
                CreateEnemy(id, name, level, hp, dmg, def, xp, difficulty);
            }
        }

        private static void CreateEnemy(
            string id,
            string name,
            int level,
            int hp,
            int damage,
            int defense,
            int xpOverride,
            EnemyDifficulty difficulty)
        {
            var path = $"{EnemyPath}{id}.asset";
            if (AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path) != null)
            {
                return;
            }

            var asset = ScriptableObject.CreateInstance<EnemyDataSO>();
            asset.enemyId = id;
            asset.DisplayName = name;
            asset.Description = $"Enemy: {name}";
            asset.enemyLevel = level;
            asset.maxHp = hp;
            asset.contactDamage = damage;
            asset.defense = defense;
            asset.xpRewardOverride = xpOverride;
            asset.baseDifficulty = difficulty;
            asset.lootTableId = string.Empty;

            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
#endif
