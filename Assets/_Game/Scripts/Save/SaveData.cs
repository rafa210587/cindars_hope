using System;
using System.Collections.Generic;
using CindarsHope.Craft;
using CindarsHope.Enemy;
using CindarsHope.Equipment;
using CindarsHope.Farm;
using CindarsHope.Farm.Runtime;
using CindarsHope.Player;
using CindarsHope.Player.Death;
using CindarsHope.Player.Progression;
using CindarsHope.Skills;
using CindarsHope.UI.Hotbar;
using CindarsHope.World;
using UnityEngine;

namespace CindarsHope.Save
{
    [Serializable]
    public class GameSaveData
    {
        public int SchemaVersion;
        public int CurrentDay;
        public string CurrentSceneName;
        public string CurrentScenePath;
        public PlayerSaveData Player;
        public InventorySaveData Inventory;
        public EquipmentSaveData Equipment;
        public HotbarSaveData Hotbar;
        public PlayerProgressionSaveData Progression;
        public FarmSaveData Farm;
        public WorldSaveData World;
        public FonteSaveData Fonte;
        public CindarsHope.Cave.Runtime.CaveRunSaveData CaveRun;
        public CaveSaveData Cave;
        public DeathSaveData Death;
        public EconomySaveData Economy;
        public CraftingRuntimeSaveData Crafting;
        public StaminaSaveData Stamina;
        public EquipmentDurabilitySaveData EquipmentDurability;
        public NpcManagerSaveData Npcs;
        public GameTimeSaveData GameTime;
        public PlayerStatusEffectsSaveData PlayerStatusEffects;
        public ActiveSkillSlotsSaveData ActiveSkillSlots;
        public SkillTreeSaveData SkillTree;
        public BestiarySaveData Bestiary;
        public CompanionManagerSaveData Companions;
        public QuestStateSectionSaveData Quests;
        public FarmDailyGoalsSaveData DailyGoals;
        // fable_07: grimório (knownSpellIds + progresso de tomo). Seção aditiva sem migration:
        // saves legados (sem o campo) carregam com grimório vazio.
        public CindarsHope.Magic.SpellbookSaveData Spellbook;
    }

    [Serializable]
    public class PlayerSaveData
    {
        public int CurrentHP;
        public int MaxHP;
        public int Gold;
        public int CurrentHunger;
        public int MaxHunger;
        public int CurrentMana;
        public int MaxMana;
        public Vector2 PlayerPosition;
        public float Fatigue;
    }

    // F17: estado da Fonte de Anya (seção aditiva; saves legados carregam com defaults).
    [Serializable]
    public class FonteSaveData
    {
        public int FonteState;
        public List<int> UnlockedFunctions = new List<int>();
        public bool LivingWaterUnlocked;
        public int LivingWaterCharges;
        public int LastGrantDay = -1;
        public List<int> IntegratedFragments = new List<int>();
    }

    [Serializable]
    public class InventorySaveData
    {
        public int Capacity;
        public List<InventorySlotSaveData> Slots = new List<InventorySlotSaveData>();
        public List<InventoryItemSaveData> Items = new List<InventoryItemSaveData>();
    }

    [Serializable]
    public class InventorySlotSaveData
    {
        public int SlotIndex;
        public string ItemId;
        public int Amount;
        public bool IsEquipped;
        public string EquipmentBindingId;
    }

    [Serializable]
    public class InventoryItemSaveData
    {
        public string ItemId;
        public int Amount;
    }

    [Serializable]
    public class FarmSaveData
    {
        public List<FarmPlotSaveData> Plots = new List<FarmPlotSaveData>();
        public List<TreeSaveData> Trees = new List<TreeSaveData>();
    }

    [Serializable]
    public class WorldSaveData
    {
        public List<ItemPickupSaveData> Pickups = new List<ItemPickupSaveData>();
        public List<TreeSaveData> Trees = new List<TreeSaveData>();
    }

    [Serializable]
    public class ShopStockSaveData
    {
        public string ShopId;
        public List<ShopItemStockEntry> Items = new List<ShopItemStockEntry>();
        public int LastRestockDay;
    }

    [Serializable]
    public class ShopItemStockEntry
    {
        public string ItemId;
        public int CurrentStock;
    }

    [Serializable]
    public class EconomySaveData
    {
        public List<ShopStockSaveData> Shops = new List<ShopStockSaveData>();
    }

    [Serializable]
    public class StaminaSaveData
    {
        public int CurrentStamina;
        public int MaxStamina;
    }

    [Serializable]
    public class EquipmentDurabilitySaveData
    {
        public List<DurabilityEntryData> EquipmentDurabilities = new List<DurabilityEntryData>();
    }

    [Serializable]
    public class DurabilityEntryData
    {
        public string ItemInstanceId;
        public int CurrentDurability;
        public int MaxDurability;
    }

    [Serializable]
    public class NpcManagerSaveData
    {
        public List<NpcSaveData> Npcs = new List<NpcSaveData>();
    }

    [Serializable]
    public class NpcSaveData
    {
        public string NpcId;
        public string SceneId;
        public Vector2 Position;
        public bool HasMet;
    }

    [Serializable]
    public class GameTimeSaveData
    {
        public int CurrentDay;
        public int CurrentPhase;
        public float PhaseElapsedSeconds;
    }

    [Serializable]
    public class PlayerStatusEffectsSaveData
    {
        public List<StatusEffectEntryData> ActiveEffects = new List<StatusEffectEntryData>();
    }

    [Serializable]
    public class StatusEffectEntryData
    {
        public string EffectId;
        public float RemainingSeconds;
    }

    [Serializable]
    public class DurabilityEntry
    {
        public int CurrentDurability;
        public int MaxDurability;
    }

    [Serializable]
    public class EquipmentSaveData
    {
        public string EquippedToolId;
        public List<EquipmentSlotSaveData> Slots = new List<EquipmentSlotSaveData>();
        // fable_22 (aditivo): infusões de têmpera por instância de arma/ferramenta. Default VAZIO =>
        // saves legados carregam sem têmpera, sem migration. Strings/ints simples (sem refs Unity).
        public List<WeaponInfusionSaveData> Infusions = new List<WeaponInfusionSaveData>();
    }

    [Serializable]
    public class EquipmentSlotSaveData
    {
        public EquipmentSlot SlotType;
        public string ItemInstanceId;
    }

    // fable_22: entrada aditiva de infusão (itemInstanceId → element/tier). Tipos simples, sem
    // refs Unity, compatível com JsonUtility. Ausência da entrada = arma sem têmpera.
    [Serializable]
    public class WeaponInfusionSaveData
    {
        public string ItemInstanceId;
        public string InfusionElement; // estável: fire/ice/toxic/lightning/arcane/void
        public int InfusionTier;        // 0 = sem; 1 ou 2
    }

    [Serializable]
    public class ActiveSkillSlotsSaveData
    {
        public string SlotRSkillActionId;
        public string SlotTSkillActionId;
        public string SlotYSkillActionId;
        public string SlotGSkillActionId;
    }

    [Serializable]
    public class CompanionManagerSaveData
    {
        public List<CompanionSaveEntry> Companions = new List<CompanionSaveEntry>();
    }

    [Serializable]
    public class CompanionSaveEntry
    {
        public string CompanionId;
        public string NpcId;
        public int UnlockState;
        public List<string> UnlockedRoles = new List<string>();
        public List<string> UnlockedByQuestIds = new List<string>();
        public int BondLevel;
        public int TrustPoints;
        public int Fatigue;
        public int InjuryState;
        public int LastInteractionDay;
        public int JobRank;
        public int CaveRank;
    }

    // Quest save DTOs — [Serializable] with public fields for JsonUtility compatibility.
    // QuestStateSection uses properties (not JsonUtility-compatible) so we maintain parallel
    // serializable copies here and convert in SaveManager.

    [Serializable]
    public class QuestObjectiveStateSaveData
    {
        public string ObjectiveId;
        public int CurrentProgress;
        public int RequiredProgress;
        public bool IsCompleted;
        public bool IsFailed;
        public bool IsKnown;
    }

    [Serializable]
    public class QuestStateSaveData
    {
        public string QuestId;
        public int State;
        public string CurrentStepId;
        public List<string> CompletedStepIds = new List<string>();
        public List<string> FailedStepIds = new List<string>();
        public List<QuestObjectiveStateSaveData> ObjectiveStates = new List<QuestObjectiveStateSaveData>();
        public List<string> KnownObjectiveIds = new List<string>();
        public List<string> KnownHints = new List<string>();
        public int StartedAtDay;
        public int StartedAtTime;
        public int CompletedAtDay;
        public bool Tracked;
        public bool Discovered;
        public string FailureReason;
        public List<string> GrantedRewardIds = new List<string>();
        public List<string> GrantedFlagIds = new List<string>();
        public string RepeatInstanceId;
    }

    [Serializable]
    public class QuestStateSectionSaveData
    {
        public int Version = 1;
        public List<QuestStateSaveData> QuestStates = new List<QuestStateSaveData>();
        public List<string> GlobalKnownHints = new List<string>();
    }
}
