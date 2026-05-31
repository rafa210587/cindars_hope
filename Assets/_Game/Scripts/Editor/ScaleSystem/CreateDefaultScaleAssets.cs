using System.IO;
using CindarsHope.Camera;
using CindarsHope.Core.Data;
using CindarsHope.World.Scale;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.ScaleSystem
{
    public static class CreateDefaultScaleAssets
    {
        private const string ScaleDataPath = "Assets/_Game/Data/Scale";
        private const string CameraDataPath = "Assets/_Game/Data/Camera";
        private const string ConfigDataPath = "Assets/_Game/Data/Config";

        [MenuItem("CindarsHope/Advanced/Legacy/Generate/Data/Create Default Scale Assets")]
        public static void CreateAll()
        {
            EnsureDirectory(ScaleDataPath);
            EnsureDirectory(CameraDataPath);
            EnsureDirectory(ConfigDataPath);

            CreateProfile(EntityScaleCategory.Player,           "player",             "Player",            visualScale: 2.0f,  colliderScale: 1.0f, nameplateY: 1.4f);
            CreateProfile(EntityScaleCategory.NPC,              "npc",                "NPC",               visualScale: 2.0f,  colliderScale: 1.0f, nameplateY: 1.4f);
            CreateProfile(EntityScaleCategory.EnemyTiny,        "enemy_tiny",         "Enemy Tiny",        visualScale: 1.0f,  colliderScale: 1.0f, nameplateY: 0.8f);
            CreateProfile(EntityScaleCategory.EnemySmall,       "enemy_small",        "Enemy Small",       visualScale: 1.5f,  colliderScale: 1.0f, nameplateY: 1.0f);
            CreateProfile(EntityScaleCategory.EnemyMedium,      "enemy_medium",       "Enemy Medium",      visualScale: 2.0f,  colliderScale: 1.0f, nameplateY: 1.4f);
            CreateProfile(EntityScaleCategory.EnemyLarge,       "enemy_large",        "Enemy Large",       visualScale: 3.0f,  colliderScale: 1.0f, nameplateY: 2.0f);
            CreateProfile(EntityScaleCategory.EnemyHuge,        "enemy_huge",         "Enemy Huge",        visualScale: 4.0f,  colliderScale: 1.0f, nameplateY: 2.6f);
            CreateProfile(EntityScaleCategory.EnemyBoss,        "enemy_boss",         "Enemy Boss",        visualScale: 4.0f,  colliderScale: 1.0f, nameplateY: 2.6f);
            CreateProfile(EntityScaleCategory.TreeSmall,        "tree_small",         "Tree Small",        visualScale: 2.0f,  colliderScale: 0.4f, nameplateY: 1.8f);
            CreateProfile(EntityScaleCategory.TreeMedium,       "tree_medium",        "Tree Medium",       visualScale: 3.0f,  colliderScale: 0.4f, nameplateY: 2.4f);
            CreateProfile(EntityScaleCategory.TreeLarge,        "tree_large",         "Tree Large",        visualScale: 4.0f,  colliderScale: 0.4f, nameplateY: 3.0f);
            CreateProfile(EntityScaleCategory.RockSmall,        "rock_small",         "Rock Small",        visualScale: 1.5f,  colliderScale: 1.0f, nameplateY: 1.0f);
            CreateProfile(EntityScaleCategory.RockMedium,       "rock_medium",        "Rock Medium",       visualScale: 2.0f,  colliderScale: 1.0f, nameplateY: 1.4f);
            CreateProfile(EntityScaleCategory.Pickup,           "pickup",             "Pickup",            visualScale: 1.0f,  colliderScale: 1.0f, nameplateY: 0.7f);
            CreateProfile(EntityScaleCategory.Chest,            "chest",              "Chest",             visualScale: 1.5f,  colliderScale: 1.0f, nameplateY: 1.2f);
            CreateProfile(EntityScaleCategory.Workbench,        "workbench",          "Workbench",         visualScale: 2.0f,  colliderScale: 1.0f, nameplateY: 1.6f);
            CreateProfile(EntityScaleCategory.Forge,            "forge",              "Forge",             visualScale: 2.0f,  colliderScale: 1.0f, nameplateY: 1.6f);
            CreateProfile(EntityScaleCategory.CookingStation,   "cooking_station",    "Cooking Station",   visualScale: 2.0f,  colliderScale: 1.0f, nameplateY: 1.6f);
            CreateProfile(EntityScaleCategory.FarmObject,       "farm_object",        "Farm Object",       visualScale: 1.5f,  colliderScale: 1.0f, nameplateY: 1.2f);
            CreateProfile(EntityScaleCategory.CavePortal,       "cave_portal",        "Cave Portal",       visualScale: 2.5f,  colliderScale: 1.0f, nameplateY: 2.0f);
            CreateProfile(EntityScaleCategory.CheckpointPortal, "checkpoint_portal",  "Checkpoint Portal", visualScale: 2.0f,  colliderScale: 1.0f, nameplateY: 1.8f);
            CreateProfile(EntityScaleCategory.Corpse,           "corpse",             "Corpse",            visualScale: 2.0f,  colliderScale: 0.5f, nameplateY: 0.6f);

            CreateCameraConfig();
            CreateGameScaleConfig();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CreateDefaultScaleAssets] All scale assets created.");
        }

        private static void CreateProfile(
            EntityScaleCategory category,
            string id,
            string displayName,
            float visualScale,
            float colliderScale,
            float nameplateY)
        {
            var path = $"{ScaleDataPath}/VisualScaleProfile_{id}.asset";
            if (File.Exists(Path.Combine(Application.dataPath, "..", path)))
            {
                return;
            }

            var asset = ScriptableObject.CreateInstance<VisualScaleProfileSO>();
            asset.ProfileId = id;
            asset.DisplayName = displayName;
            asset.Category = category;
            asset.VisualScale = visualScale;
            asset.ColliderScale = colliderScale;
            asset.NameplateOffset = new Vector2(0f, nameplateY);
            asset.HintOffset = new Vector2(0f, nameplateY + 0.2f);
            asset.DamageNumberOffset = new Vector2(0f, nameplateY - 0.2f);

            AssetDatabase.CreateAsset(asset, path);
        }

        private static void CreateCameraConfig()
        {
            var path = $"{CameraDataPath}/CameraScaleConfig.asset";
            if (File.Exists(Path.Combine(Application.dataPath, "..", path)))
            {
                return;
            }

            var asset = ScriptableObject.CreateInstance<CameraScaleConfigSO>();
            asset.DefaultOrthographicSize = 8.5f;
            asset.FarmOrthographicSize = 8.5f;
            asset.TownOrthographicSize = 8.0f;
            asset.CaveOrthographicSize = 7.0f;
            asset.BossArenaOrthographicSize = 10.0f;
            asset.MinOrthographicSize = 3f;
            asset.MaxOrthographicSize = 20f;
            asset.SizeTransitionTime = 0.5f;

            AssetDatabase.CreateAsset(asset, path);
        }

        private static void CreateGameScaleConfig()
        {
            var path = $"{ConfigDataPath}/GameScaleConfig.asset";
            if (File.Exists(Path.Combine(Application.dataPath, "..", path)))
            {
                return;
            }

            var asset = ScriptableObject.CreateInstance<GameScaleConfigSO>();
            asset.PlayerReferenceScale = 1f;
            asset.TreeScale = 3f;
            asset.LakeScale = 6f;
            asset.BossScale = 2.5f;
            asset.BossMinScale = 2f;
            asset.BossMaxScale = 3f;
            asset.NormalEnemySmallScale = 1.15f;
            asset.NormalEnemyMediumScale = 1.35f;
            asset.NormalEnemyLargeScale = 1.65f;
            asset.CaveWidthMultiplier = 2;
            asset.CaveHeightMultiplier = 2;
            asset.CaveRoomSizeMultiplier = 2;
            asset.CaveCorridorWidthMultiplier = 2;

            AssetDatabase.CreateAsset(asset, path);
            Debug.Log($"[CreateDefaultScaleAssets] Created {path}.");
        }

        private static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
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
}
