using System;
using System.Collections.Generic;
using System.IO;
using CindarsHope.Core.Data;
using CindarsHope.Economy;
using CindarsHope.Inventory.Data;
using CindarsHope.NPC;
using CindarsHope.NPC.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.Validation
{
    public static class ValidateRefinedCanonicalNpcTownPopulation
    {
        private const string TownScenePath = "Assets/_Game/Scenes/TownScene.unity";
        private const string ItemDatabasePath = "Assets/_Game/Data/Registries/ItemDatabase.asset";

        private static readonly ExpectedNpc[] ExpectedNpcs =
        {
            new("npc_corvus", "Padre Corvus", "Stationary/TemplePatrol", "shop_corvus"),
            new("npc_mara", "Mara Vellum", "Stationary/RegistryDesk", "shop_mara"),
            new("npc_sylveth", "Sylveth", "ShopKeeperFixed/FarmVisit", "shop_sylveth"),
            new("npc_brumdar", "Brumdar Ferro-Quieto", "ShopKeeperFixed", "shop_brumdar"),
            new("npc_nimble", "Nimble Galhobaixo", "Patrol/WorkshopDesk", "shop_nimble"),
            new("npc_gurd", "Gurd Carvalho-Torto", "Patrol/HeavyWorkZone", "shop_gurd"),
            new("npc_hund", "Hund Carvalho-Torto", "Patrol/TownRoad", "shop_hund"),
            new("npc_ozzra", "Ozzra Fumacazul", "WanderWithinZone/Lab", "shop_ozzra"),
            new("npc_gruta", "Gruta Panela-Funda", "ShopKeeperFixed/TavernStage", "shop_gruta"),
            new("npc_zrix", "Zrix das Estradas", "Patrol/CaveRoad", "shop_zrix"),
            new("npc_yael", "Yael Noite-Mansa", "NightOnly/WanderHidden", "shop_yael"),
            new("npc_thalindra", "Thalindra Veu-de-Lua", "Stationary/ArchiveDesk", "shop_thalindra"),
            new("npc_dagna", "Dagna Rocha-Morna", "Patrol/QuarryRoad", "shop_dagna"),
            new("npc_pip", "Pip Semente-Solta", "WanderWithinZone", "shop_pip"),
            new("npc_alaric", "Ser Alaric Veyr", "Patrol/TownGate", ""),
            new("npc_mirela", "Mirela dos Lacos", "ShopKeeperFixed", "shop_mirela"),
            new("npc_renko", "Renko Tres-Sorrisos", "ShopKeeperFixed", "shop_renko"),
            new("npc_eiran", "Eiran Valeclaro", "WanderWithinZone/AnimalArea", "shop_eiran"),
            new("npc_liora", "Liora Canta-Rio", "WanderWithinZone/EveningStage", ""),
            new("npc_orlan", "Orlan Pouso-Curto", "ShopKeeperFixed", "shop_orlan"),
            new("npc_savra", "Savra Escama-Verde", "Patrol/HerbRoute", "shop_savra"),
            new("npc_tovin", "Tovin Maos-de-Selo", "Stationary/PermitDesk", "shop_tovin"),
            new("npc_maelor", "Maelor Cinza", "NightOnly/WanderHidden", ""),
        };

        public static void Run()
        {
            var errors = new List<string>();
            ValidateAssets(errors);
            ValidateRequiredDocs(errors);
            ValidateSceneMarkers(errors);

            if (errors.Count > 0)
            {
                throw new InvalidOperationException("WAVE12C refined canonical NPC validation failed:\n- " + string.Join("\n- ", errors));
            }

            Debug.Log("WAVE12C refined canonical NPC validation passed.");
        }

        private static void ValidateAssets(List<string> errors)
        {
            var itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabaseSO>(ItemDatabasePath);
            if (itemDatabase == null)
            {
                errors.Add($"ItemDatabaseSO not found at '{ItemDatabasePath}'.");
            }

            foreach (var expected in ExpectedNpcs)
            {
                var npc = FindAssetById<NpcDataSO>(expected.NpcId, "Assets/_Game/Data/NPCs");
                if (npc == null)
                {
                    errors.Add($"Missing NpcDataSO for {expected.NpcId}.");
                    continue;
                }

                if (npc.DisplayName != expected.DisplayName)
                {
                    errors.Add($"{expected.NpcId} DisplayName mismatch: '{npc.DisplayName}'.");
                }

                if (npc.DialogueTree == null)
                {
                    errors.Add($"{expected.NpcId} DialogueTree is null.");
                }
                else if (npc.DialogueTree.Nodes == null || npc.DialogueTree.Nodes.Count < 10)
                {
                    errors.Add($"{expected.NpcId} DialogueTree has fewer than 10 nodes.");
                }

                if (!string.IsNullOrWhiteSpace(expected.ShopId))
                {
                    if (npc.ShopId != expected.ShopId)
                    {
                        errors.Add($"{expected.NpcId} ShopId mismatch: '{npc.ShopId}' expected '{expected.ShopId}'.");
                    }

                    var shop = FindAssetById<ShopDataSO>(expected.ShopId, "Assets/_Game/Data/Economy");
                    if (shop == null)
                    {
                        errors.Add($"Missing ShopDataSO {expected.ShopId} for {expected.NpcId}.");
                    }
                    else
                    {
                        if (shop.NpcId != expected.NpcId)
                        {
                            errors.Add($"{expected.ShopId} NpcId mismatch: '{shop.NpcId}' expected '{expected.NpcId}'.");
                        }

                        if (shop.Items == null || shop.Items.Length == 0)
                        {
                            errors.Add($"{expected.ShopId} has no stock items.");
                        }
                        else if (itemDatabase != null)
                        {
                            ValidateShopStock(expected, shop, itemDatabase, errors);
                        }
                    }
                }
            }
        }

        private static void ValidateShopStock(ExpectedNpc expected, ShopDataSO shop, ItemDatabaseSO itemDatabase, List<string> errors)
        {
            for (var i = 0; i < shop.Items.Length; i++)
            {
                var entry = shop.Items[i];
                if (entry == null)
                {
                    errors.Add($"{expected.ShopId} stock[{i}] is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.ItemId))
                {
                    errors.Add($"{expected.ShopId} stock[{i}] has empty ItemId.");
                    continue;
                }

                if (!itemDatabase.TryGetById(entry.ItemId, out ItemDataSO itemData) || itemData == null)
                {
                    errors.Add($"{expected.ShopId} stock[{i}] item '{entry.ItemId}' is absent from ItemDatabaseSO.");
                    continue;
                }

                if (entry.BuyPriceOverride <= 0 && itemData.BaseValue <= 0)
                {
                    errors.Add($"{expected.ShopId} stock[{i}] item '{entry.ItemId}' has no valid buy price.");
                }
            }
        }

        private static void ValidateRequiredDocs(List<string> errors)
        {
            var docs = new[]
            {
                "docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_CANONICAL_ROSTER.md",
                "docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_TOWN_PLACEMENT_MAP.md",
                "docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_DIALOGUE_SETS.md",
                "docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_MOVEMENT_SCHEDULES.md",
                "docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_SHOP_SERVICES.md",
                "docs/validation/WAVE_INTEGRATION_12C_REFINED_NPC_IMPLEMENTATION_REPORT.md",
                "docs/validation/WAVE_INTEGRATION_12C_HUMAN_PLAYMODE_CHECKLIST.md"
            };

            foreach (var doc in docs)
            {
                if (!File.Exists(doc))
                {
                    errors.Add($"Missing required WAVE12C doc: {doc}.");
                }
            }
        }

        private static void ValidateSceneMarkers(List<string> errors)
        {
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.path != TownScenePath)
            {
                var scene = EditorSceneManager.OpenScene(TownScenePath, OpenSceneMode.Single);
                if (!scene.IsValid())
                {
                    errors.Add("TownScene could not be opened for marker validation.");
                    return;
                }
            }

            var markers = UnityEngine.Object.FindObjectsByType<NpcScenePlacementMarker>();
            foreach (var expected in ExpectedNpcs)
            {
                var matches = 0;
                foreach (var marker in markers)
                {
                    if (marker != null && marker.NpcId == expected.NpcId)
                    {
                        matches++;
                        if (marker.SceneId != "TownScene")
                        {
                            errors.Add($"{expected.NpcId} marker SceneId mismatch: '{marker.SceneId}'.");
                        }

                        if (marker.MovementProfile != expected.MovementProfile)
                        {
                            errors.Add($"{expected.NpcId} movement profile mismatch: '{marker.MovementProfile}' expected '{expected.MovementProfile}'.");
                        }
                    }
                }

                if (matches != 1)
                {
                    errors.Add($"{expected.NpcId} expected exactly one TownScene placement marker, found {matches}.");
                }
            }
        }

        private static T FindAssetById<T>(string id, string searchFolder) where T : ScriptableObject
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { searchFolder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                switch (asset)
                {
                    case NpcDataSO npc when npc.NpcId == id:
                        return asset;
                    case ShopDataSO shop when shop.Id == id:
                        return asset;
                }
            }

            return null;
        }

        private readonly struct ExpectedNpc
        {
            public ExpectedNpc(string npcId, string displayName, string movementProfile, string shopId)
            {
                NpcId = npcId;
                DisplayName = displayName;
                MovementProfile = movementProfile;
                ShopId = shopId;
            }

            public string NpcId { get; }
            public string DisplayName { get; }
            public string MovementProfile { get; }
            public string ShopId { get; }
        }
    }
}
