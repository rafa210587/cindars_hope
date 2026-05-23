#if UNITY_EDITOR
using CindarsHope.Combat;
using CindarsHope.Enemy.AI;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor
{
    [InitializeOnLoad]
    public class EnemyDataInitializer
    {
        private const string EnemyPath = "Assets/_Game/Data/Enemy/";
        private const string AIPath = "Assets/_Game/Data/Enemy/AI/";
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
            CreateAIBehaviors();
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
            if (!AssetDatabase.IsValidFolder(enemyDir + "/AI"))
            {
                AssetDatabase.CreateFolder(enemyDir, "AI");
            }
        }

        private static void CreateAIBehaviors()
        {
            var behaviors = new[]
            {
                ("ai_patrol_basic", "Basic Patrol", AIType.Patrol),
                ("ai_aggressive_melee", "Aggressive Melee", AIType.Aggressive),
                ("ai_ranged_cautious", "Ranged Cautious", AIType.Ranged),
            };

            foreach (var (id, name, type) in behaviors)
            {
                CreateAIBehavior(id, name, type);
            }
        }

        private static void CreateEnemies()
        {
            var enemies = new[]
            {
                ("enemy_slime_basic", "Basic Slime", 1, 20, 3, 0, 10, EnemyDifficulty.Easy, "ai_patrol_basic"),
                ("enemy_goblin_scout", "Goblin Scout", 5, 30, 5, 1, 20, EnemyDifficulty.Normal, "ai_aggressive_melee"),
                ("enemy_orc_warrior", "Orc Warrior", 10, 50, 10, 3, 40, EnemyDifficulty.Hard, "ai_aggressive_melee"),
                ("enemy_spider_ice", "Ice Spider", 12, 35, 8, 2, 35, EnemyDifficulty.Hard, "ai_ranged_cautious"),
            };

            foreach (var (id, name, level, hp, dmg, def, xp, difficulty, aiBehavior) in enemies)
            {
                CreateEnemy(id, name, level, hp, dmg, def, xp, difficulty, aiBehavior);
            }
        }

        private static void CreateAIBehavior(string id, string name, AIType type)
        {
            var path = $"{AIPath}{id}.asset";
            if (AssetDatabase.LoadAssetAtPath<AIBehaviorSO>(path) != null)
            {
                return;
            }

            var asset = ScriptableObject.CreateInstance<AIBehaviorSO>();
            asset.Id = id;
            asset.BehaviorName = name;
            asset.Type = type;

            AssetDatabase.CreateAsset(asset, path);
        }

        private static void CreateEnemy(
            string id,
            string name,
            int level,
            int hp,
            int damage,
            int defense,
            int xpOverride,
            EnemyDifficulty difficulty,
            string aiBehavior)
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
            asset.aiBehaviorId = aiBehavior;
            asset.lootTableId = string.Empty;

            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
#endif