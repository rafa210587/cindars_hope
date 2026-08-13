using System;
using System.Collections.Generic;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Equipment;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Tools;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor.Validation
{
    internal readonly struct DebugLoadoutEntry
    {
        public DebugLoadoutEntry(string id, int amount, int hotbarSlot, ToolType toolType = ToolType.None)
        {
            Id = id;
            Amount = amount;
            HotbarSlot = hotbarSlot;
            ToolType = toolType;
        }

        public string Id { get; }
        public int Amount { get; }
        public int HotbarSlot { get; }
        public ToolType ToolType { get; }
    }

    // fable_66 — DECISÃO AUDITADA: ferramenta de DEBUG de editor PERMANENTE (não é débito de integração).
    // Vive em Editor/ (fora do build de produção por definição); expõe dois MenuItems Play-Mode-only
    // para validação manual de slices. Nunca é referenciada por código de gameplay runtime.
    public static class DebugLoadoutProvisioner
    {
        private const string CanonicalHoeId = "item_shop_tool_hoe_basic";
        private const int FirstHotbarSlot = 0;
        private const int LastHotbarSlot = 5;

        private static readonly DebugLoadoutEntry[] SmokeLoadoutEntries =
        {
            // Slots visíveis 1..6: ferramentas de farm, arma e sementes.
            new DebugLoadoutEntry(CanonicalHoeId,                         1, 0, ToolType.Hoe),
            new DebugLoadoutEntry("item_shop_tool_watering_can_basic",  1, 1, ToolType.WateringCan),
            new DebugLoadoutEntry("item_tool_pickaxe_iron",             1, 2, ToolType.Pickaxe),
            new DebugLoadoutEntry("item_tool_fishing_rod_basic",        1, 3, ToolType.FishingRod),
            new DebugLoadoutEntry("item_weapon_bow_basic",              1, 4),
            new DebugLoadoutEntry("item_seed_carrot",                  20, 5),

            // Requisitos de combate, recuperação, crafting e venda fora da hotbar.
            new DebugLoadoutEntry("item_ammo_arrow_basic",             30, -1),
            new DebugLoadoutEntry("item_consumable_potion_hp_small",    5, -1),
            new DebugLoadoutEntry("item_consumable_food_bread",         3, -1),
            new DebugLoadoutEntry("item_material_wood",                20, -1),
            new DebugLoadoutEntry("item_material_stone",               20, -1),
            new DebugLoadoutEntry("item_material_copper_ore",          10, -1),
            new DebugLoadoutEntry("item_material_iron_ore",            10, -1),
            new DebugLoadoutEntry("item_crop_carrot",                  10, -1),
            new DebugLoadoutEntry("item_fish_common",                   5, -1),
        };

        internal static IReadOnlyList<DebugLoadoutEntry> SmokeLoadout => SmokeLoadoutEntries;

        internal static bool TryValidateSmokeLoadout(out string failureReason)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var occupiedSlots = new HashSet<int>();
            var toolTypes = new HashSet<ToolType>();
            bool hasSeed = false;
            bool hasWeapon = false;
            bool hasAmmo = false;
            bool hasConsumable = false;
            int materialCount = 0;

            for (int i = 0; i < SmokeLoadoutEntries.Length; i++)
            {
                var entry = SmokeLoadoutEntries[i];
                if (string.IsNullOrWhiteSpace(entry.Id))
                {
                    failureReason = $"Entry {i} has an empty item ID.";
                    return false;
                }

                if (!ids.Add(entry.Id))
                {
                    failureReason = $"Duplicate item ID: {entry.Id}.";
                    return false;
                }

                if (entry.Amount < 1)
                {
                    failureReason = $"Item {entry.Id} has invalid amount {entry.Amount}.";
                    return false;
                }

                if (entry.HotbarSlot < -1 || entry.HotbarSlot > LastHotbarSlot)
                {
                    failureReason = $"Item {entry.Id} has invalid hotbar slot {entry.HotbarSlot}.";
                    return false;
                }

                if (entry.HotbarSlot >= FirstHotbarSlot && !occupiedSlots.Add(entry.HotbarSlot))
                {
                    failureReason = $"Duplicate hotbar slot: {entry.HotbarSlot}.";
                    return false;
                }

                if (entry.ToolType != ToolType.None)
                {
                    toolTypes.Add(entry.ToolType);
                }

                hasSeed |= entry.Id.StartsWith("item_seed_", StringComparison.Ordinal);
                hasWeapon |= entry.Id.StartsWith("item_weapon_", StringComparison.Ordinal);
                hasAmmo |= entry.Id.StartsWith("item_ammo_", StringComparison.Ordinal);
                hasConsumable |= entry.Id.StartsWith("item_consumable_", StringComparison.Ordinal);
                if (entry.Id.StartsWith("item_material_", StringComparison.Ordinal))
                {
                    materialCount++;
                }
            }

            var missing = new List<string>();
            RequireTool(toolTypes, ToolType.Hoe, missing);
            RequireTool(toolTypes, ToolType.WateringCan, missing);
            RequireTool(toolTypes, ToolType.Pickaxe, missing);
            RequireTool(toolTypes, ToolType.FishingRod, missing);
            if (!hasSeed) missing.Add("seeds");
            if (!hasWeapon) missing.Add("weapon");
            if (!hasAmmo) missing.Add("ammo");
            if (!hasConsumable) missing.Add("consumable");
            if (materialCount < 2) missing.Add("crafting/sale materials");

            if (missing.Count > 0)
            {
                failureReason = $"Missing smoke requirements: {string.Join(", ", missing)}.";
                return false;
            }

            failureReason = string.Empty;
            return true;
        }

        [MenuItem("CindarsHope/Integration/Debug/Provision Farm Smoke Loadout")]
        public static void ProvisionSmokeLoadout()
        {
            if (!TryValidateSmokeLoadout(out string validationFailure))
            {
                Debug.LogError($"[DebugLoadout] Invalid smoke loadout definition: {validationFailure}");
                return;
            }

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

            var inv = bootstrap.InventoryManager as InventoryManager;
            if (inv == null || !inv.IsInitialized)
            {
                Debug.LogError("[DebugLoadout] InventoryManager is null or not initialized.");
                return;
            }

            var hotbar = bootstrap.SaveManager?.HotbarState;
            var eq = EquipmentManager.Instance;
            var added = new List<string>();
            var present = new List<string>();
            var hotbarSet = new List<string>();
            var requiredFailures = new List<string>();

            if (hotbar == null)
            {
                requiredFailures.Add("SaveManager.HotbarState is null");
            }

            foreach (var entry in SmokeLoadoutEntries)
            {
                if (!inv.IsKnownItem(entry.Id))
                {
                    requiredFailures.Add($"unknown item: {entry.Id}");
                    continue;
                }

                bool addedNow = inv.AddItem(entry.Id, entry.Amount);
                bool isPresent = addedNow || inv.HasItem(entry.Id);
                if (!isPresent)
                {
                    requiredFailures.Add($"not added (inventory full): {entry.Id} x{entry.Amount}");
                    continue;
                }

                if (addedNow)
                {
                    added.Add($"{entry.Id} x{entry.Amount}");
                }
                else
                {
                    present.Add(entry.Id);
                }

                if (entry.HotbarSlot >= FirstHotbarSlot && hotbar != null)
                {
                    if (hotbar.SetSlot(entry.HotbarSlot, entry.Id))
                    {
                        hotbarSet.Add($"[{entry.HotbarSlot + 1}]={entry.Id}");
                    }
                    else
                    {
                        requiredFailures.Add($"hotbar slot {entry.HotbarSlot + 1}: {entry.Id}");
                    }
                }
            }

            if (eq == null)
            {
                requiredFailures.Add("EquipmentManager is null; hoe not equipped");
            }
            else if (!inv.HasItem(CanonicalHoeId))
            {
                requiredFailures.Add($"hoe missing from inventory: {CanonicalHoeId}");
            }
            else
            {
                eq.EquipTool(CanonicalHoeId, ToolType.Hoe, ToolTier.Basic);
                if (!eq.HasTool(ToolType.Hoe))
                {
                    requiredFailures.Add($"failed to equip hoe: {CanonicalHoeId}");
                }
            }

            string result = requiredFailures.Count == 0
                ? "READY FOR PLAYTEST"
                : $"REQUIRED FAILURES ({requiredFailures.Count}): {string.Join("; ", requiredFailures)}";

            Debug.Log(
                $"[DebugLoadout] {result}\n" +
                $"Added ({added.Count}): {string.Join(", ", added)}\n" +
                $"Already present ({present.Count}): {string.Join(", ", present)}\n" +
                $"Hotbar: {(hotbar != null ? string.Join(", ", hotbarSet) : "unavailable")}");
        }

        [MenuItem("CindarsHope/Integration/Debug/Provision All Known Items")]
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

            var inv = bootstrap.InventoryManager as InventoryManager;
            var db = bootstrap.ItemDatabase as ItemDatabaseSO;
            if (inv == null || db == null || !inv.IsInitialized)
            {
                Debug.LogError("[DebugLoadout] InventoryManager, ItemDatabase, or initialization state is invalid.");
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

        private static void RequireTool(HashSet<ToolType> toolTypes, ToolType required, List<string> missing)
        {
            if (!toolTypes.Contains(required))
            {
                missing.Add(required.ToString());
            }
        }
    }
}
