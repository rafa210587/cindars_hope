using System;
using System.Collections.Generic;
using CindarsHope.Cave.Runtime;
using CindarsHope.Craft;
using CindarsHope.Enemy;
using CindarsHope.Foundation;
using CindarsHope.Farm;
using CindarsHope.Farm.Runtime;
using CindarsHope.Inventory;
using CindarsHope.Player;
using CindarsHope.Player.Death;
using CindarsHope.Player.Progression;
using CindarsHope.Skills;
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
        public CindarsHope.NPC.NpcManagerSaveData Npcs;
        public GameTimeSaveData GameTime;
        public PlayerStatusEffectsSaveData PlayerStatusEffects;
        public ActiveSkillSlotsSaveData ActiveSkillSlots;
        public SkillTreeSaveData SkillTree;
        public CindarsHope.Skills.Runtime.SurvivalSkillSaveData SurvivalSkills;
        // Phase 18: escopo e tentativas consumidas dos rolls seeded de passivas de crafting.
        // Secao aditiva; saves legados restauram escopo do slot e ledger vazio.
        public CindarsHope.Skills.Runtime.CraftingPassiveRngSaveData CraftingPassiveRng;
        // Skills 20T: shared Living Forge daily charge and pending job reservations.
        public CindarsHope.Skills.Runtime.CraftingSkillSaveData CraftingSkills;
        // Phase 19B: janela temporária Kanthor/Kaand, carga de cura e dedupe causal.
        public CindarsHope.Skills.Runtime.CombatCapstoneSaveData CombatCapstones;
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
        // fable_62: hints de onboarding ja vistos (1x/save). Secao ADITIVA, so List<string> de ids
        // estaveis (ADR-0006 / save-dto-simple-types-only). Ausente em save legado = lista vazia =
        // todos os hints elegiveis de novo (seguro). Sem migracao. Owner: OnboardingHintService.
        public OnboardingHintsSaveData OnboardingHints;
        // spec_farm_till_anywhere_tilemap: estado dos tiles araveis por coordenada (TileX,TileY).
        // Secao ADITIVA, so ints/strings e tipos simples (ADR-0006 / save-dto-simple-types-only).
        // Ausente em save legado = sem tiles arados = fazenda sem plots livres (CA-3). Sem migracao.
        // Owner: FarmTilesSectionProvider (via FarmTileGrid + FarmPlotLogic).
        public FarmTilesSaveData FarmTiles;
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
        // Campo aditivo: saves anteriores desserializam como 0 e preservam o comportamento legado.
        public float HungerFractionalDrainAccumulator;
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

    // arch: InventorySaveData/InventorySlotSaveData/InventoryItemSaveData movidos para
    // CindarsHope.Inventory (InventorySaveData.cs) — quebra do ciclo mutuo Inventory|Save. Ver
    // campo Inventory acima.

    // arch: FarmSaveData movido para CindarsHope.Farm (Farm/FarmSaveData.cs) — quebra do ciclo mutuo
    // Farm|Save (spec_arch_farm_save_cycle_reduction). Ver campo Farm acima.

    [Serializable]
    public class WorldSaveData
    {
        public List<ItemPickupSaveData> Pickups = new List<ItemPickupSaveData>();
        public List<TreeSaveData> Trees = new List<TreeSaveData>();
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

    // arch: EquipmentDurabilitySaveData/DurabilityEntryData movidos para CindarsHope.Foundation
    // (Foundation/SaveSchema/EquipmentSaveDtos.cs) — quebra do ciclo mutuo Equipment|Save
    // (spec_arch_equipment_save_cycle_reduction_v29). Ver campo EquipmentDurability acima.

    // arch: NpcManagerSaveData/NpcSaveData movidos para CindarsHope.NPC (NpcManagerSaveData.cs) —
    // quebra do ciclo mutuo NPC|Save. Ver campo Npcs acima.

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

    // arch: EquipmentSaveData/EquipmentSlotSaveData/EquipmentUpgradeSaveData movidos para
    // CindarsHope.Foundation (Foundation/SaveSchema/EquipmentSaveDtos.cs) — quebra do ciclo mutuo
    // Equipment|Save (spec_arch_equipment_save_cycle_reduction_v29). Ver campo Equipment acima.
    // WeaponInfusionSaveData ja estava em CindarsHope.Foundation.SaveSchema.EconomySaveDtos (arch:
    // quebra do ciclo Economy|Save).

    [Serializable]
    public class ActiveSkillSlotsSaveData
    {
        public string SlotRSkillActionId = string.Empty;
        public string SlotTSkillActionId = string.Empty;
        public string SlotYSkillActionId = string.Empty;
        public string SlotGSkillActionId = string.Empty;
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
    public class QuestDynamicRewardSaveData
    {
        public string RewardId;
        public int RewardType;
        public string TargetId;
        public int Quantity;
        public string GrantedFlagId;
        public int IdempotencyPolicy;
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
        public int Source = (int)QuestSource.Npc;
        public bool IsDynamicInstance;
        public string TemplateId;
        public string InstanceTargetId;
        public int InstanceQuantity;
        public int QuestLevel;
        public int InstanceRewardGold;
        public int InstanceRewardXp;
        public int GeneratedForDay;
        public List<QuestDynamicRewardSaveData> DynamicRewards = new List<QuestDynamicRewardSaveData>();
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
