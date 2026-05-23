#if UNITY_EDITOR
using CindarsHope.Enemy;
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
                AssetDatabase.CreateFolder(enemyDir, "AI");
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
                ("enemy_slime_basic", "Basic Slime", 1, 20, 3, 0, 10, "ai_patrol_basic"),
                ("enemy_goblin_scout", "Goblin Scout", 5, 30, 5, 1, 20, "ai_aggressive_melee"),
                ("enemy_orc_warrior", "Orc Warrior", 10, 50, 10, 3, 40, "ai_aggressive_melee"),
                ("enemy_spider_ice", "Ice Spider", 12, 35, 8, 2, 35, "ai_ranged_cautious"),
            };

            foreach (var (id, name, level, hp, dmg, def, xp, aiBehavior) in enemies)
            {
                CreateEnemy(id, name, level, hp, dmg, def, xp, aiBehavior);
            }
        }

        private static void CreateAIBehavior(string id, string name, AIType type)
        {
            var path = $"{AIPath}{id}.asset";
            if (AssetDatabase.LoadAssetAtPath<AIBehaviorSO>(path) != null)
                return;

            var asset = ScriptableObject.CreateInstance<AIBehaviorSO>();
            asset.Id = id;
            asset.BehaviorName = name;
            asset.Type = type;

            AssetDatabase.CreateAsset(asset, path);
        }

        private static void CreateEnemy(string id, string name, int level, int hp, int dmg, int def, int xp, string aiBehavior)
        {
            var path = $"{EnemyPath}{id}.asset";
            if (AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path) != null)
                return;

            var asset = ScriptableObject.CreateInstance<EnemyDataSO>();
            asset.Id = id;
            asset.DisplayName = name;
            asset.Description = $"Enemy: {name}";
            asset.Level = level;
            asset.MaxHP = hp;
            asset.Damage = dmg;
            asset.Defense = def;
            asset.XpReward = xp;
            asset.AIBehaviorId = aiBehavior;

            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
#endif
