using System.Collections.Generic;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Equipment;
using CindarsHope.Inventory;
using CindarsHope.Tools;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    // TODO_INTEGRATION_NOT_FINAL — debug provisioner for smoke-testing WAVE04/05/06 playable slices.
    // Must be removed or disabled before production build. Do not wire to gameplay systems.
    public static class DebugLoadoutProvisioner
    {
        // (item ID, amount to add, hotbar slot index — -1 means don't bind to hotbar)
        private static readonly (string Id, int Amount, int HotbarSlot)[] SmokeLoadout =
        {
            // Farm / crop (WAVE05)
            ("item_seed_carrot",               20,  5),
            ("item_crop_carrot",               10, -1),
            ("item_seed_wheat",                10, -1),
            ("item_seed_sunpepper",            10, -1),
            // Tools — spec IDs (may not exist; IsKnownItem gates them)
            ("item_tool_hoe_basic",             1,  0),
            ("item_tool_watering_can_basic",    1,  1),
            ("item_tool_axe_basic",             1,  2),
            ("item_tool_pickaxe_basic",         1,  3),
            ("item_tool_fishing_rod_basic",     1,  4),
            ("item_tool_sickle_basic",          1, -1),
            // Shop-sourced tool alias (confirmed in asset scan)
            ("item_shop_tool_hoe_basic",        1, -1),
            // Resources — spec IDs (legacy aliases; real IDs also included below)
            ("item_wood",                      20, -1),
            ("item_stone",                     20, -1),
            // Confirmed material IDs from asset scan
            ("item_material_wood",             10, -1),
            ("item_material_stone",            10, -1),
            ("item_material_copper_ore",        5, -1),
            ("item_material_iron_ore",          5, -1),
            // Forage / fish — spec IDs (may not exist)
            ("item_fiber",                     20, -1),
            ("item_forage_basic",              10, -1),
            ("item_fish_basic",                10, -1),
            // Confirmed fish
            ("item_fish_common",                5, -1),
            // Consumables
            ("item_consumable_potion_hp_small",  5, -1),
            ("item_consumable_food_bread",        3, -1),
            // Combat
            ("item_weapon_bow_basic",            1, -1),
            ("item_ammo_arrow_basic",           20, -1),
            ("item_spell_fireball_test",          1, -1),
        };

        [MenuItem("CindarsHope/Debug/Provision Farm Smoke Loadout", priority = 60)]
        public static void ProvisionSmokeLoadout()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[DebugLoadout] Must be in Play Mode. Enter Play Mode, open FarmScene, then run this menu.");
                return;
            }

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                Debug.LogError("[DebugLoadout] GameBootstrap.Instance is null. Is FarmScene loaded and playing?");
                return;
            }

            var inv = bootstrap.InventoryManager;
            if (inv == null || !inv.IsInitialized)
            {
                Debug.LogError("[DebugLoadout] InventoryManager is null or not initialized.");
                return;
            }

            var hotbar = bootstrap.SaveManager?.HotbarState;
            var eq = bootstrap.EquipmentManager;

            var added = new List<string>();
            var skipped = new List<string>();
            var hotbarSet = new List<string>();

            foreach (var entry in SmokeLoadout)
            {
                if (!inv.IsKnownItem(entry.Id))
                {
                    skipped.Add(entry.Id);
                    continue;
                }

                bool ok = inv.AddItem(entry.Id, entry.Amount);
                if (ok)
                {
                    added.Add($"{entry.Id} x{entry.Amount}");
                    if (entry.HotbarSlot >= 0 && hotbar != null)
                    {
                        hotbar.SetSlot(entry.HotbarSlot, entry.Id);
                        hotbarSet.Add($"[{entry.HotbarSlot}]={entry.Id}");
                    }
                }
                else
                {
                    // AddItem false = inventory full or at max stack — try hotbar bind if item was pre-existing
                    Debug.LogWarning($"[DebugLoadout] AddItem false: {entry.Id} x{entry.Amount} (inventory full or max stack).");
                    if (entry.HotbarSlot >= 0 && hotbar != null && inv.HasItem(entry.Id))
                    {
                        hotbar.SetSlot(entry.HotbarSlot, entry.Id);
                        hotbarSet.Add($"[{entry.HotbarSlot}]={entry.Id}(pre-existing)");
                    }
                }
            }

            TryEquipFirstKnownTool(eq, inv);

            Debug.Log(
                $"[DebugLoadout] Farm Smoke Loadout complete.\n" +
                $"Added ({added.Count}): {string.Join(", ", added)}\n" +
                $"Skipped/unknown ({skipped.Count}): {string.Join(", ", skipped)}\n" +
                $"Hotbar: {(hotbar != null ? string.Join(", ", hotbarSet) : "SaveManager.HotbarState null — skipped")}");
        }

        [MenuItem("CindarsHope/Debug/Provision All Known Items (Debug)", priority = 61)]
        public static void ProvisionAllKnownItems()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[DebugLoadout] Must be in Play Mode.");
                return;
            }

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null || bootstrap.InventoryManager == null || bootstrap.ItemDatabase == null)
            {
                Debug.LogError("[DebugLoadout] GameBootstrap, InventoryManager, or ItemDatabase is null.");
                return;
            }

            var inv = bootstrap.InventoryManager;
            var db = bootstrap.ItemDatabase;

            if (!inv.IsInitialized)
            {
                Debug.LogError("[DebugLoadout] InventoryManager not initialized.");
                return;
            }

            var added = new List<string>();
            var skipped = new List<string>();

            foreach (var item in db.All)
            {
                if (inv.AddItem(item.Id, 1))
                    added.Add(item.Id);
                else
                    skipped.Add(item.Id);
            }

            Debug.Log(
                $"[DebugLoadout] All known items provisioned: {added.Count} added, {skipped.Count} skipped.\n" +
                $"Added: {string.Join(", ", added)}\n" +
                $"Skipped (full/unknown): {string.Join(", ", skipped)}");
        }

        private static void TryEquipFirstKnownTool(EquipmentManager eq, InventoryManager inv)
        {
            if (eq == null)
            {
                Debug.Log("[DebugLoadout] EquipmentManager null — skipping equipment step.");
                return;
            }

            var candidates = new (string Id, ToolType Type)[]
            {
                ("item_tool_hoe_basic",           ToolType.Hoe),
                ("item_shop_tool_hoe_basic",       ToolType.Hoe),
                ("item_tool_axe_basic",            ToolType.Axe),
                ("item_tool_fishing_rod_basic",    ToolType.FishingRod),
                ("item_tool_pickaxe_basic",        ToolType.Pickaxe),
                ("item_tool_watering_can_basic",   ToolType.WateringCan),
            };

            foreach (var (id, toolType) in candidates)
            {
                if (inv.HasItem(id))
                {
                    eq.EquipTool(id, toolType, ToolTier.Basic);
                    Debug.Log($"[DebugLoadout] Equipped {id} via legacy EquipTool({toolType}, Basic).");
                    return;
                }
            }

            Debug.Log("[DebugLoadout] No tool items in inventory to equip — equipment step skipped.");
        }
    }
}
