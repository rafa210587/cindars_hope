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
        // fable_41: posse dos lotes de expansão da fazenda (domínio farm). Campo ADITIVO, só IDs
        // (ADR-0006). Ausente em save legado = lista vazia = todos os lotes Locked (CA-4). Sem migração.
        public CindarsHope.Farm.Lots.FarmLotsSaveData FarmLots;
        // fable_12: animais de fazenda (id/tipo/abrigo/saude/fed/dias-sem-comida). Seção ADITIVA, só
        // IDs e tipos simples (ADR-0006 / save-dto-simple-types-only). Ausente em save legado = lista
        // vazia = zero animais (CA-3). Sem migração. Owner: FarmAnimalRegistry.
        public CindarsHope.Farm.Animals.FarmAnimalsSaveData FarmAnimals;
        // fable_26: amizade por NPC (npcId + pontos + marcadores de dia dos caps). Seção ADITIVA, só
        // strings/ints e IDs estáveis (ADR-0006). Ausente em save legado = lista vazia = todos os NPCs
        // nível 0 (Unknown) (CA-3). Sem migração. Owner: FriendshipService.
        public CindarsHope.NPC.Friendship.FriendshipSaveData Friendship;
        // fable_25: pendências dos serviços únicos de NPC (encomendas de livro, contrato de caca
        // semanal, prato do dia usado, pasto premium ativo). Seção ADITIVA, só strings/ints e dias/
        // semanas absolutos (ADR-0006). Ausente em save legado = sem pendências (CA-5). Sem migração.
        // Owner: NpcServiceRuntime.
        public CindarsHope.NPC.Services.NpcServicesSaveData NpcServices;
        // fable_43: estado do endgame (Ato 5) da MainProgressionSection — ato atual, gate 100->101,
        // status da escolha final e id do epilogo persistido. Secao ADITIVA, so ints/strings (enums
        // por valor; ending por id; ADR-0006 / save-dto-simple-types-only). Ausente em save legado =
        // endgame nao iniciado (defaults: ato None, gate Locked, escolha Unavailable). Sem migracao.
        // Owner runtime: FonteRuntimeService.Progression (host da MainProgressionSection).
        public MainProgressionSaveData MainProgression;
    }

    // fable_43: DTO aditivo do endgame (Ato 5). Tipos simples apenas — sem refs Unity. Os fragmentos
    // integrados continuam persistidos via FonteSaveData.IntegratedFragments; este DTO adiciona o
    // estado do gate/escolha/epilogo que antes nao era salvo (Fase 0: MainProgressionSection nao
    // persistia). Saves legados (campo nulo) carregam com defaults = endgame nao iniciado.
    [Serializable]
    public class MainProgressionSaveData
    {
        public int CurrentAct;          // MainAct enum value (default 0 = None)
        public int Level100GateState;   // Level100GateStatus enum value (default 0 = Locked)
        public int Level101AccessState; // Level101AccessStatus enum value (default 0 = Locked)
        public int FinalChoiceState;    // FinalChoiceStatus enum value (default 0 = Unavailable)
        public string PostGameWorldState; // ending id (ending_protect|seal|use) or null
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

        // fable_55: jobs de processamento ativos (queijaria/barril). Campo ADITIVO na seção farm;
        // só IDs/ints (ADR-0006). Ausente em save legado = sem jobs (CA-5). Dono: FarmProcessingStationService.
        public CindarsHope.Farm.Processing.FarmProcessingSaveData Processing = new CindarsHope.Farm.Processing.FarmProcessingSaveData();

        // fable_54: batch de envio pendente da caixa de shipping (venda overnight). Campo ADITIVO na
        // seção farm; só ids/ints/floats (ADR-0006). Ausente em save legado = lista vazia = nenhum
        // batch pendente (CA-4). Dono: ShippingBinRuntimeService.
        public CindarsHope.Farm.Shipping.PendingShippingSaveData PendingShipping = new CindarsHope.Farm.Shipping.PendingShippingSaveData();

        // fable_54: estado dos pontos de forrageio do dia. Campo ADITIVO na seção farm; só ids/ints
        // (ADR-0006). Ausente em save legado = lista vazia = spawns regeneram no próximo DayStarted
        // (CA-4). Dono: FarmForageRuntimeService.
        public CindarsHope.Farm.Forage.ForageSpawnsSaveData ForageSpawns = new CindarsHope.Farm.Forage.ForageSpawnsSaveData();
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
        // fable_49 (aditivo): upgrade focado (+1/+2/+3) por instância de equipamento. Default VAZIO =>
        // saves legados carregam sem upgrade (level 0), sem migration. Tipos simples (sem refs Unity).
        public List<EquipmentUpgradeSaveData> Upgrades = new List<EquipmentUpgradeSaveData>();
        // fable_49 (aditivo): receitas de tier alto APRENDIDas via first-kill (slugs estáveis). Default
        // VAZIO => save legado sem receitas, sem migration. Persistido aqui (mesmo owner do equipment
        // save) para não tocar o SaveManager core; consultado pelo gating de craft.
        public List<string> UnlockedRecipeIds = new List<string>();
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

    // fable_49: entrada aditiva de upgrade (itemInstanceId → level/focus). Tipos simples, sem refs
    // Unity, compatível com JsonUtility. Ausência da entrada = item sem upgrade (level 0). Derivados
    // (dano/durabilidade) NUNCA persistidos (§45) — recalculados no load a partir deste registro.
    [Serializable]
    public class EquipmentUpgradeSaveData
    {
        public string ItemInstanceId;
        public int UpgradeLevel;   // 0 = sem; 1..3
        public string UpgradeFocus; // estável: damage/durability/weight/stamina/block
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

        // fable_34 — additive simple-type fields for the source channel + dynamic instances.
        // Defaults keep legacy saves loading safely (Source = Npc, not a dynamic instance).
        public int Source = (int)CindarsHope.Quests.QuestSource.Npc;
        public bool IsDynamicInstance;
        public string TemplateId;
        public string InstanceTargetId;
        public int InstanceQuantity;
        public int QuestLevel;
        public int InstanceRewardGold;
        public int InstanceRewardXp;
        public int GeneratedForDay;
    }

    [Serializable]
    public class QuestStateSectionSaveData
    {
        public int Version = 1;
        public List<QuestStateSaveData> QuestStates = new List<QuestStateSaveData>();
        public List<string> GlobalKnownHints = new List<string>();

        // fable_34 — quest source channels that the player has discovered (cave secrets).
        // A secret quest only appears in the log after its id is recorded here.
        public List<string> DiscoveredSecretQuestIds = new List<string>();
        // fable_34 — main-quest acts already rewarded with +1 skill point (idempotency).
        public List<string> RewardedMainActIds = new List<string>();
    }
}
