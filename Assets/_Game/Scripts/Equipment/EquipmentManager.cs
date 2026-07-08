using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Tools;
using UnityEngine;

namespace CindarsHope.Equipment
{
    [DisallowMultipleComponent]
    public class EquipmentManager : MonoBehaviour
    {
        // arch: Core|Equipment (spec_arch_core_equipment_cycle_reduction_v35) — self-registro estatico
        // (molde Craft/Economy/Skills) para o GameBootstrap parar de segurar esta referencia serializada.
        public static EquipmentManager Instance { get; private set; }

        [SerializeField] private ItemDatabaseSO _itemDatabase;

        // Legacy fields - kept for backward compat in save/load only
        [SerializeField] private string _equippedToolId = string.Empty;
        [SerializeField] private ToolType _equippedToolType = ToolType.None;
        [SerializeField] private ToolTier _equippedToolTier = ToolTier.None;

        private EquipmentDurabilityTracker _durabilityTracker;
        private Dictionary<EquipmentSlot, string> _slots = new(); // itemInstanceId per slot

        // fable_22: registro de infusões de têmpera por instância (paralelo à durabilidade).
        // Capturado/restaurado de forma aditiva no DTO de equipment; exposto à leitura de combate
        // pelo acessor estático WeaponInfusionRegistry.Active.
        private CindarsHope.Economy.WeaponInfusionRegistry _infusionRegistry;
        public CindarsHope.Economy.WeaponInfusionRegistry InfusionRegistry => _infusionRegistry;

        // fable_49: registro de upgrade focado (+1/+2/+3) por instância, paralelo à têmpera/durabilidade,
        // e serviço de receitas aprendidas (gating de craft de tier alto). Ambos persistidos de forma
        // aditiva no MESMO DTO de equipment (sem tocar o SaveManager core) e expostos por acessor estático.
        private CindarsHope.Crafting.EquipmentUpgradeRegistry _upgradeRegistry;
        private CindarsHope.Crafting.RecipeUnlockService _recipeUnlockService;
        public CindarsHope.Crafting.EquipmentUpgradeRegistry UpgradeRegistry => _upgradeRegistry;
        public CindarsHope.Crafting.RecipeUnlockService RecipeUnlockService => _recipeUnlockService;

        // fable_23: roteador de efeitos de acessórios/relíquias (não-stack por tipo). Recomputado a
        // cada equip/unequip/restore a partir dos 3 slots tipo-acessório (Ring1/Ring2/Accessory) e
        // publicado no acessor estático AccessoryEffectRouter.Active para os hooks pontuais (gold,
        // durabilidade, loot, fadiga, comida, knockback) consultarem sem busca global de cena.
        private AccessoryEffectRouter _accessoryRouter;
        public AccessoryEffectRouter AccessoryRouter => _accessoryRouter;

        // Legacy properties - deprecated, use GetEquippedItem() instead
        public string EquippedToolId => _equippedToolId;
        public ToolType EquippedToolType => _equippedToolType;
        public ToolTier EquippedToolTier => _equippedToolTier;
        public EquipmentDurabilityTracker DurabilityTracker => _durabilityTracker;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (_durabilityTracker == null)
            {
                _durabilityTracker = new EquipmentDurabilityTracker();
            }

            // fable_22: cria o registro de infusões e o publica como acessor único para o ponto de
            // leitura de combate (sem busca global). DontDestroyOnLoad: o último Awake vence.
            if (_infusionRegistry == null)
            {
                _infusionRegistry = new CindarsHope.Economy.WeaponInfusionRegistry();
            }
            CindarsHope.Economy.WeaponInfusionRegistry.Active = _infusionRegistry;

            // fable_49: cria/publica o registro de upgrade e o serviço de receitas aprendidas como
            // acessores únicos (sem busca global). DontDestroyOnLoad: o último Awake vence.
            if (_upgradeRegistry == null)
            {
                _upgradeRegistry = new CindarsHope.Crafting.EquipmentUpgradeRegistry();
            }
            CindarsHope.Crafting.EquipmentUpgradeRegistry.Active = _upgradeRegistry;

            if (_recipeUnlockService == null)
            {
                _recipeUnlockService = new CindarsHope.Crafting.RecipeUnlockService();
            }
            CindarsHope.Crafting.RecipeUnlockService.Active = _recipeUnlockService;

            // fable_23: cria/publica o roteador de acessórios como acessor único (sem busca global).
            // DontDestroyOnLoad: o último Awake vence. Rebuild inicial a partir dos slots atuais.
            if (_accessoryRouter == null)
            {
                _accessoryRouter = new AccessoryEffectRouter();
            }
            AccessoryEffectRouter.Active = _accessoryRouter;
            RebuildAccessoryEffects();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<InventoryChangedEvent>(HandleInventoryChanged);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<InventoryChangedEvent>(HandleInventoryChanged);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (GameBootstrap.Instance?.ModalManager?.HasActiveModal == true)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                CycleDebugTool();
            }
        }

        // LEGACY - use EquipItem instead
        public void EquipTool(string toolId, ToolType toolType, ToolTier tier)
        {
            _equippedToolId = toolId ?? string.Empty;
            _equippedToolType = toolType;
            _equippedToolTier = tier;
            EquipItem(EquipmentSlot.LeftHand, toolId);
            Debug.Log($"EquipmentManager: equipped tool {_equippedToolId} ({_equippedToolType}/{_equippedToolTier}) to LeftHand via legacy EquipTool.", this);
        }

        public void EquipItem(EquipmentSlot slot, string itemInstanceId)
        {
            // fable_29 (emenda V3 item 6) — punitive-respec gate READ (not a rewrite of equipment):
            // an item whose required skill tier was re-locked by a respec is non-equippable until
            // the tier is re-unlocked. The rule is owned by Skills (SkillTierEquipGate); here we
            // only consult it. Items with no tier requirement are never blocked.
            if (!string.IsNullOrEmpty(itemInstanceId)
                && CindarsHope.Skills.SkillTierEquipGate.IsEquipBlocked(itemInstanceId))
            {
                Debug.LogWarning($"EquipmentManager: equip de '{itemInstanceId}' bloqueado — tier de skill re-bloqueado por respec. Item permanece no inventario.", this);
                return;
            }

            _slots[slot] = itemInstanceId ?? string.Empty;
            RebuildAccessoryEffects();
            GameEventBus.Publish(new EquipmentSlotChangedEvent(slot, itemInstanceId));
        }

        /// <summary>
        /// fable_23 — equipa um acessório/relíquia VALIDADO num slot tipo-acessório (tipo×slot e
        /// no máximo 1 relíquia). Retorna false sem mutar o estado quando a regra recusa; a mensagem
        /// é entregue à tela de equipamento (F14) como feedback de recusa (ex.: 2ª relíquia).
        /// Itens de mão/armadura continuam pela <see cref="EquipItem"/> normal (sem esta validação).
        /// </summary>
        public bool TryEquipAccessory(EquipmentSlot slot, string itemInstanceId, out string rejectionReason)
        {
            if (!AccessoryEffectRouter.CanEquip(itemInstanceId, slot, EnumerateAccessorySlots(), out rejectionReason))
            {
                Debug.LogWarning($"EquipmentManager: equip de acessório recusado ({rejectionReason}).", this);
                return false;
            }

            EquipItem(slot, itemInstanceId);
            return true;
        }

        public void UnequipSlot(EquipmentSlot slot)
        {
            if (_slots.ContainsKey(slot))
            {
                _slots.Remove(slot);
                RebuildAccessoryEffects();
                GameEventBus.Publish(new EquipmentSlotChangedEvent(slot, null));
            }
        }

        // fable_23: itens atualmente equipados nos 3 slots tipo-acessório (para validação de equip).
        private IEnumerable<(EquipmentSlot Slot, string ItemInstanceId)> EnumerateAccessorySlots()
        {
            foreach (var kvp in _slots)
            {
                if (AccessoryCatalog.IsAccessorySlot(kvp.Key) && !string.IsNullOrEmpty(kvp.Value))
                {
                    yield return (kvp.Key, kvp.Value);
                }
            }
        }

        // fable_23: recomputa o roteador de efeitos a partir dos itens nos slots tipo-acessório.
        // Ponto único chamado após qualquer mutação de slot (equip/unequip/restore).
        private void RebuildAccessoryEffects()
        {
            if (_accessoryRouter == null)
            {
                _accessoryRouter = new AccessoryEffectRouter();
                AccessoryEffectRouter.Active = _accessoryRouter;
            }

            var ids = new List<string>(3);
            foreach (var entry in EnumerateAccessorySlots())
            {
                ids.Add(entry.ItemInstanceId);
            }
            _accessoryRouter.Rebuild(ids);
        }

        public string GetEquippedItem(EquipmentSlot slot)
        {
            return _slots.TryGetValue(slot, out var itemInstanceId) ? itemInstanceId : null;
        }

        public List<EquippedItemSnapshot> GetAllEquippedItems()
        {
            var result = new List<EquippedItemSnapshot>();

            foreach (var kvp in _slots)
            {
                var slot = kvp.Key;
                var itemInstanceId = kvp.Value;

                if (string.IsNullOrEmpty(itemInstanceId))
                {
                    continue;
                }

                var durData = _durabilityTracker?.GetDurability(itemInstanceId);

                result.Add(new EquippedItemSnapshot
                {
                    ItemId = itemInstanceId,
                    ItemInstanceId = itemInstanceId,
                    DurabilityCurrent = durData?.CurrentDurability ?? 1f,
                    DurabilityMax = durData?.MaxDurability ?? 1f,
                    IsBroken = durData?.IsBroken ?? false,
                    SlotType = slot,
                    SlotIndex = (int)slot
                });
            }

            return result;
        }

        public void UnequipAll()
        {
            var slotsToUnequip = new List<EquipmentSlot>(_slots.Keys);
            foreach (var slot in slotsToUnequip)
            {
                UnequipSlot(slot);
            }
        }

        public void RegisterEquipmentUsage()
        {
            RegisterEquipmentUsage(GetEquippedItem(EquipmentSlot.LeftHand) ?? string.Empty);
            RegisterEquipmentUsage(GetEquippedItem(EquipmentSlot.RightHand) ?? string.Empty);
        }

        // fable_23: acumulador determinístico do bônus de durabilidade de ferramenta (Anel de Thoren
        // +15%). Cada uso registra 1.0 de "desgaste"; o bônus (fração 0..1) é creditado de volta e,
        // quando acumula >= 1.0 uso poupado, ESTE uso não consome durabilidade. Sem RNG: ao longo de N
        // usos o consumo efetivo tende a N×(1-bônus). Ponto ÚNICO nomeado (ToolDurabilityModifier).
        private float _toolDurabilitySavingsAccumulator;

        public void RegisterEquipmentUsage(string itemInstanceId)
        {
            if (string.IsNullOrEmpty(itemInstanceId) || _durabilityTracker == null)
                return;

            // fable_23 — ponto único do ToolDurabilityModifier: credita o bônus de Thoren e poupa este
            // uso quando o acumulado fecha 1 uso inteiro (consumo efetivo determinístico, sem if espalhado).
            var bonus = AccessoryEffectRouter.ToolDurabilityModifierSource?.Invoke() ?? 0f;
            if (bonus > 0f)
            {
                _toolDurabilitySavingsAccumulator += UnityEngine.Mathf.Clamp01(bonus);
                if (_toolDurabilitySavingsAccumulator >= 1f)
                {
                    _toolDurabilitySavingsAccumulator -= 1f;
                    return; // uso poupado pela durabilidade do acessório
                }
            }

            _durabilityTracker.TryRegisterUsage(itemInstanceId);

            // Check if broken - auto-unequip
            var durData = _durabilityTracker.GetDurability(itemInstanceId);
            if (durData != null && durData.IsBroken)
            {
                AutoUnequipBrokenItem(itemInstanceId);
            }
        }

        private void AutoUnequipBrokenItem(string itemInstanceId)
        {
            var slotsToUnequip = new List<EquipmentSlot>();
            foreach (var kvp in _slots)
            {
                if (kvp.Value == itemInstanceId)
                {
                    slotsToUnequip.Add(kvp.Key);
                }
            }

            foreach (var slot in slotsToUnequip)
            {
                UnequipSlot(slot);
                Debug.Log($"EquipmentManager: auto-unequipped broken item {itemInstanceId} from {slot}.", this);
            }
        }

        public void RepairItem(string itemInstanceId, int restoreAmount)
        {
            if (_durabilityTracker == null)
                return;

            // fable_47 (follow-up 2): RepairEfficiencyBonus derivado (F18) aumenta a durabilidade
            // restaurada por reparo (ponto único). Fórmula linear simples, clampada e testável.
            var bonus = CindarsHope.Player.PlayerVitalsApplier.RepairEfficiencyBonusSource?.Invoke() ?? 0f;
            var effectiveRestore = CindarsHope.Player.DerivedFollowupFormulas.EffectiveRepairAmount(restoreAmount, bonus);

            _durabilityTracker.RepairEquipment(itemInstanceId, effectiveRestore);
        }

        public DurabilityData GetItemDurability(string itemInstanceId)
        {
            return _durabilityTracker?.GetDurability(itemInstanceId);
        }

        public bool IsItemBroken(string itemInstanceId)
        {
            var durData = _durabilityTracker?.GetDurability(itemInstanceId);
            return durData?.IsBroken ?? false;
        }

        public bool HasTool(ToolType requiredTool)
        {
            return HasTool(requiredTool, ToolTier.None);
        }

        public bool HasTool(ToolType requiredTool, ToolTier minimumTier)
        {
            if (requiredTool == ToolType.None)
            {
                return true;
            }

            // Check primary equipment slots (LeftHand/RightHand for tools)
            var leftHand = GetEquippedItem(EquipmentSlot.LeftHand);
            var rightHand = GetEquippedItem(EquipmentSlot.RightHand);

            if (!string.IsNullOrEmpty(leftHand) && InferToolTypeFromId(leftHand) == requiredTool)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(rightHand) && InferToolTypeFromId(rightHand) == requiredTool)
            {
                return true;
            }

            // Fallback to legacy system for backward compat
            return _equippedToolType == requiredTool && _equippedToolTier >= minimumTier;
        }

        public bool TryGetMissingToolMessage(ToolType requiredTool, ToolTier minimumTier, out string message)
        {
            message = string.Empty;

            if (HasTool(requiredTool, minimumTier))
            {
                return false;
            }

            message = minimumTier > ToolTier.Basic
                ? $"Requires {FormatToolTier(minimumTier)} {FormatToolType(requiredTool)} or better."
                : $"Requires {FormatToolType(requiredTool)}.";
            return true;
        }

        public EquipmentSaveData CaptureSaveData()
        {
            var data = new EquipmentSaveData
            {
                EquippedToolId = _equippedToolId,
                Slots = new List<EquipmentSlotSaveData>()
            };

            foreach (var kvp in _slots)
            {
                data.Slots.Add(new EquipmentSlotSaveData
                {
                    SlotType = kvp.Key,
                    ItemInstanceId = kvp.Value
                });
            }

            // fable_22: persiste infusões de têmpera de forma aditiva (defaults vazios em saves antigos).
            if (_infusionRegistry != null)
            {
                data.Infusions = _infusionRegistry.CaptureSaveData();
            }

            // fable_49: persiste upgrades focados e receitas aprendidas de forma aditiva (defaults vazios
            // em saves antigos). Derivados NUNCA persistidos (§45) — só level/focus e os slugs de unlock.
            if (_upgradeRegistry != null)
            {
                data.Upgrades = _upgradeRegistry.CaptureSaveData();
            }
            if (_recipeUnlockService != null)
            {
                data.UnlockedRecipeIds = _recipeUnlockService.CaptureUnlockedIds();
            }

            return data;
        }

        public void RestoreFromSaveData(EquipmentSaveData saveData)
        {
            if (saveData == null)
            {
                _equippedToolId = string.Empty;
                _equippedToolType = ToolType.None;
                _equippedToolTier = ToolTier.None;
                _slots.Clear();
                _infusionRegistry?.ClearAll();
                _upgradeRegistry?.ClearAll();
                _recipeUnlockService?.ClearAll();
                _accessoryRouter?.Clear();
                return;
            }

            _equippedToolId = saveData.EquippedToolId ?? string.Empty;
            InferEquippedToolFromId();

            _slots.Clear();
            if (saveData.Slots != null)
            {
                foreach (var slotData in saveData.Slots)
                {
                    _slots[slotData.SlotType] = slotData.ItemInstanceId;
                }
            }

            // fable_23: recomputa os efeitos de acessório a partir dos slots restaurados. Saves antigos
            // (sem Ring1/Ring2/Accessory) => slots vazios => roteador neutro, sem migration (CA-4).
            RebuildAccessoryEffects();

            // fable_22: restaura infusões (lista null em save legado => registro vazio, sem migration).
            if (_infusionRegistry == null)
            {
                _infusionRegistry = new CindarsHope.Economy.WeaponInfusionRegistry();
                CindarsHope.Economy.WeaponInfusionRegistry.Active = _infusionRegistry;
            }
            _infusionRegistry.RestoreFromSaveData(saveData.Infusions);

            // fable_49: restaura upgrades e receitas aprendidas (listas null em save legado => registros
            // vazios, sem migration; derivados recalculados pelo fluxo existente lendo estes registros).
            if (_upgradeRegistry == null)
            {
                _upgradeRegistry = new CindarsHope.Crafting.EquipmentUpgradeRegistry();
                CindarsHope.Crafting.EquipmentUpgradeRegistry.Active = _upgradeRegistry;
            }
            _upgradeRegistry.RestoreFromSaveData(saveData.Upgrades);

            if (_recipeUnlockService == null)
            {
                _recipeUnlockService = new CindarsHope.Crafting.RecipeUnlockService();
                CindarsHope.Crafting.RecipeUnlockService.Active = _recipeUnlockService;
            }
            _recipeUnlockService.RestoreUnlockedIds(saveData.UnlockedRecipeIds);
        }

        private void HandleInventoryChanged(InventoryChangedEvent evt)
        {
            var slotsToRemove = new List<EquipmentSlot>();
            foreach (var kvp in _slots)
            {
                if (string.IsNullOrEmpty(kvp.Value))
                {
                    slotsToRemove.Add(kvp.Key);
                }
            }

            foreach (var slot in slotsToRemove)
            {
                UnequipSlot(slot);
            }
        }

        private void CycleDebugTool()
        {
            string currentTool = GetEquippedItem(EquipmentSlot.LeftHand) ?? string.Empty;
            ToolType currentType = InferToolTypeFromId(currentTool);

            switch (currentType)
            {
                case ToolType.None:
                    EquipItem(EquipmentSlot.LeftHand, "item_tool_hoe_basic");
                    break;
                case ToolType.Hoe:
                    EquipItem(EquipmentSlot.LeftHand, "item_tool_watering_can_basic");
                    break;
                case ToolType.WateringCan:
                    EquipItem(EquipmentSlot.LeftHand, "item_tool_axe_basic");
                    break;
                case ToolType.Axe:
                    EquipItem(EquipmentSlot.LeftHand, "item_tool_pickaxe_basic");
                    break;
                case ToolType.Pickaxe:
                    EquipItem(EquipmentSlot.LeftHand, "item_tool_fishing_rod_basic");
                    break;
                default:
                    UnequipSlot(EquipmentSlot.LeftHand);
                    break;
            }
        }

        private void InferEquippedToolFromId()
        {
            _equippedToolType = InferToolTypeFromId(_equippedToolId);
            _equippedToolTier = string.IsNullOrWhiteSpace(_equippedToolId) ? ToolTier.None : ToolTier.Basic;
        }

        private ToolType InferToolTypeFromId(string toolId)
        {
            if (string.IsNullOrWhiteSpace(toolId))
                return ToolType.None;

            if (toolId.Contains("hoe"))
                return ToolType.Hoe;
            if (toolId.Contains("axe"))
                return ToolType.Axe;
            if (toolId.Contains("pickaxe"))
                return ToolType.Pickaxe;
            if (toolId.Contains("fishing_rod"))
                return ToolType.FishingRod;
            if (toolId.Contains("watering_can"))
                return ToolType.WateringCan;
            if (toolId.Contains("sickle"))
                return ToolType.Sickle;

            return ToolType.None;
        }

        private static string FormatToolType(ToolType toolType)
        {
            switch (toolType)
            {
                case ToolType.Axe:
                    return "Axe";
                case ToolType.Pickaxe:
                    return "Pickaxe";
                case ToolType.FishingRod:
                    return "Fishing Rod";
                case ToolType.Hoe:
                    return "Hoe";
                case ToolType.WateringCan:
                    return "Watering Can";
                case ToolType.Sickle:
                    return "Sickle";
                default:
                    return "Tool";
            }
        }

        private static string FormatToolTier(ToolTier tier)
        {
            switch (tier)
            {
                case ToolTier.Copper:
                    return "Copper";
                case ToolTier.Iron:
                    return "Iron";
                case ToolTier.Gold:
                    return "Gold";
                case ToolTier.Diamond:
                    return "Diamond";
                case ToolTier.Basic:
                    return "Basic";
                default:
                    return string.Empty;
            }
        }
    }

    public sealed class EquippedItemSnapshot
    {
        public string ItemId;
        public string ItemInstanceId;
        public float DurabilityCurrent;
        public float DurabilityMax;
        public bool IsBroken;
        public EquipmentSlot SlotType;
        public int SlotIndex;
    }
}
