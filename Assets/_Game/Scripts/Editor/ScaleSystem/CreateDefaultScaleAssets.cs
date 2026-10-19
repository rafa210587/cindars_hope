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
        // Standalone entry so the path referenced by every "run this first" message (ScaleProfileLibrary,
        // both validators) is a real menu — generate/refresh just the scale profiles without the full
        // destructive project init.
        [MenuItem("CindarsHope/Generate/Data/Create Default Scale Assets")]
        public static void CreateAllMenu() => CreateAll();

        private const string ScaleDataPath = "Assets/_Game/Data/Scale";
        private const string CameraDataPath = "Assets/_Game/Data/Camera";
        private const string ConfigDataPath = "Assets/_Game/Data/Config";

        // The PLAYER is the size reference for the whole game. Every other category below is declared as a
        // multiple of the player ("playerRelative"), and the absolute VisualScale = PlayerReferenceScale ×
        // playerRelative. This is the single knob: bump PlayerReferenceScale and the entire world rescales
        // proportionally; change one ratio and only that prop class moves. No magic absolutes in scene code.
        // (Enemies size through EnemySizeProfileSO — the Enemy* rows here are cosmetic placeholders only.)
        public const float PlayerReferenceScale = 2.0f;

        public static void CreateAll()
        {
            EnsureDirectory(ScaleDataPath);
            EnsureDirectory(CameraDataPath);
            EnsureDirectory(ConfigDataPath);

            // category                                    id                    displayName          ×player  collider  nameplateY
            CreateProfile(EntityScaleCategory.Player,           "player",             "Player",            1.00f,  1.0f, 1.4f);
            CreateProfile(EntityScaleCategory.NPC,              "npc",                "NPC",               1.00f,  1.0f, 1.4f);
            CreateProfile(EntityScaleCategory.EnemyTiny,        "enemy_tiny",         "Enemy Tiny",        0.50f,  1.0f, 0.8f);
            CreateProfile(EntityScaleCategory.EnemySmall,       "enemy_small",        "Enemy Small",       0.75f,  1.0f, 1.0f);
            CreateProfile(EntityScaleCategory.EnemyMedium,      "enemy_medium",       "Enemy Medium",      1.00f,  1.0f, 1.4f);
            CreateProfile(EntityScaleCategory.EnemyLarge,       "enemy_large",        "Enemy Large",       1.50f,  1.0f, 2.0f);
            CreateProfile(EntityScaleCategory.EnemyHuge,        "enemy_huge",         "Enemy Huge",        2.00f,  1.0f, 2.6f);
            CreateProfile(EntityScaleCategory.EnemyBoss,        "enemy_boss",         "Enemy Boss",        2.00f,  1.0f, 2.6f);
            CreateProfile(EntityScaleCategory.TreeSmall,        "tree_small",         "Tree Small",        1.00f,  0.4f, 1.8f);
            CreateProfile(EntityScaleCategory.TreeMedium,       "tree_medium",        "Tree Medium",       1.50f,  0.4f, 2.4f);
            CreateProfile(EntityScaleCategory.TreeLarge,        "tree_large",         "Tree Large",        2.00f,  0.4f, 3.0f);
            CreateProfile(EntityScaleCategory.RockSmall,        "rock_small",         "Rock Small",        0.75f,  1.0f, 1.0f);
            CreateProfile(EntityScaleCategory.RockMedium,       "rock_medium",        "Rock Medium",       1.00f,  1.0f, 1.4f);
            CreateProfile(EntityScaleCategory.Pickup,           "pickup",             "Pickup",            0.45f,  1.0f, 0.7f); // small ground item
            CreateProfile(EntityScaleCategory.Chest,            "chest",              "Chest",             0.75f,  1.0f, 1.2f);
            CreateProfile(EntityScaleCategory.Workbench,        "workbench",          "Workbench",         0.65f,  1.0f, 1.2f); // waist/chest-high
            CreateProfile(EntityScaleCategory.Forge,            "forge",              "Forge",             0.65f,  1.0f, 1.2f);
            CreateProfile(EntityScaleCategory.CookingStation,   "cooking_station",    "Cooking Station",   0.65f,  1.0f, 1.2f);
            CreateProfile(EntityScaleCategory.FarmObject,       "farm_object",        "Farm Object",       0.75f,  1.0f, 1.2f);
            CreateProfile(EntityScaleCategory.CavePortal,       "cave_portal",        "Cave Portal",       1.10f,  1.0f, 2.0f); // landmark gate, slightly taller than player
            CreateProfile(EntityScaleCategory.CheckpointPortal, "checkpoint_portal",  "Checkpoint Portal", 0.90f,  1.0f, 1.8f);
            CreateProfile(EntityScaleCategory.Corpse,           "corpse",             "Corpse",            1.00f,  0.5f, 0.6f);
            CreateProfile(EntityScaleCategory.ResourceTree,     "resource_tree",      "Resource Tree",     1.20f,  1.0f, 1.8f); // taller than player
            CreateProfile(EntityScaleCategory.ResourceRock,     "resource_rock",      "Resource Rock",     0.65f,  1.0f, 1.0f);
            CreateProfile(EntityScaleCategory.ForagePoint,      "forage_point",       "Forage Point",      0.55f,  1.0f, 0.9f);
            CreateProfile(EntityScaleCategory.ShippingBin,      "shipping_bin",       "Shipping Bin",      0.90f,  1.0f, 1.3f);
            CreateProfile(EntityScaleCategory.ContractBoard,    "contract_board",     "Contract Board",    0.72f,  1.0f, 1.4f);
            // Tamanhos OFICIAIS por raca (2026-06-30), relativos ao player. Anao 0.90, smallfolk
            // (halfling/goblin/gnome) 0.85, orc/meio-orc 1.10. Dragonborn (1.07) e demais (elfo/humano/
            // tiefling/nymiriano = 1.00) usam o profile "npc" padrao. Ver memoria project_npc_race_sizes_official.
            CreateProfile(EntityScaleCategory.NpcDwarf,          "npc_dwarf",          "NPC Dwarf",         0.90f,  1.0f, 1.2f);
            CreateProfile(EntityScaleCategory.NpcSmallfolk,      "npc_smallfolk",      "NPC Smallfolk",     0.85f,  1.0f, 1.0f);
            CreateProfile(EntityScaleCategory.NpcOrc,            "npc_orc",            "NPC Orc",           1.30f,  1.0f, 1.7f);
            // Dragonborn: 1.07x player (tamanho oficial raca — spec_npc_physics_cat_companion 2026-06-30).
            CreateProfile(EntityScaleCategory.NpcDragonborn,     "npc_dragonborn",     "NPC Dragonborn",    1.07f,  1.0f, 1.5f);

            CreateCameraConfig();
            CreateGameScaleConfig();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            // Drop any cached lookup so scene creators run later in the same orchestrator pass see the
            // freshly generated/updated profiles instead of a stale (or empty) cache.
            ScaleProfileLibrary.InvalidateCache();
            Debug.Log("[CreateDefaultScaleAssets] All scale assets created.");
        }

        private static void CreateProfile(
            EntityScaleCategory category,
            string id,
            string displayName,
            float playerRelative,
            float colliderScale,
            float nameplateY)
        {
            var visualScale = PlayerReferenceScale * playerRelative;
            var path = $"{ScaleDataPath}/VisualScaleProfile_{id}.asset";
            // Idempotent + authoritative: update an existing profile's values instead of skipping it. These
            // profiles were never wired into scenes before, so there are no hand-tuned values to clobber, and
            // re-running the generator must converge on the canonical numbers (e.g. after a balance change).
            var asset = AssetDatabase.LoadAssetAtPath<VisualScaleProfileSO>(path);
            var isNew = asset == null;
            if (isNew)
            {
                asset = ScriptableObject.CreateInstance<VisualScaleProfileSO>();
            }

            asset.ProfileId = id;
            asset.DisplayName = displayName;
            asset.Category = category;
            asset.VisualScale = visualScale;
            asset.ColliderScale = colliderScale;
            asset.NameplateOffset = new Vector2(0f, nameplateY);
            asset.HintOffset = new Vector2(0f, nameplateY + 0.2f);
            asset.DamageNumberOffset = new Vector2(0f, nameplateY - 0.2f);

            if (isNew)
            {
                AssetDatabase.CreateAsset(asset, path);
            }
            else
            {
                EditorUtility.SetDirty(asset);
            }
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
