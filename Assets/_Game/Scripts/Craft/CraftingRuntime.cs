using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Craft.Data;
using CindarsHope.Foundation;
using CindarsHope.Inventory;
using CindarsHope.Player;
using CindarsHope.Skills;
using CindarsHope.Skills.Runtime;
using UnityEngine;

namespace CindarsHope.Craft
{
    [DisallowMultipleComponent]
    public sealed class CraftingRuntime : MonoBehaviour, ISalvageRecipeProvider
    {
        private const string PocketStationId = "player_pocket";

        /// <summary>
        /// Static registry of active CraftingRuntime instances.
        /// Populated via OnEnable/OnDisable — avoids global scene searches.
        /// CraftingStationRuntimeBootstrap reads from this list instead of FindObjectsOfType.
        /// </summary>
        public static readonly List<CraftingRuntime> ActiveInstances = new List<CraftingRuntime>();

        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private RecipeDatabaseSO _recipeDatabase;
        [SerializeField] private StaminaManager _staminaManager;

        private readonly Dictionary<string, CraftingStation> _stations = new Dictionary<string, CraftingStation>();
        private long _nextCraftedItemSequence = 1;
        private CraftingSkillState _craftingSkillState;

        public bool IsInitialized { get; private set; }
        public RecipeDatabaseSO RecipeDatabase => _recipeDatabase;
        public InventoryManager InventoryManager => _inventoryManager;
        public int LivingForgeRank => SkillTreeManager.Instance != null
            ? SkillTreeManager.Instance.GetRank(LivingForgeCapstoneResolver.NodeId)
            : 0;

        public bool IsLivingForgeChargeAvailable
        {
            get
            {
                EnsureCraftingSkillState();
                return _craftingSkillState.IsChargeAvailable(
                    _craftingSkillState.CurrentDayIndex);
            }
        }

        public bool IsLivingForgeCommonIngredient(string itemId) =>
            _inventoryManager != null &&
            _inventoryManager.TryGetItemData(itemId, out var item) &&
            CraftingPassiveConsumers.IsCommonIngredient(item);

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            if (!ActiveInstances.Contains(this))
                ActiveInstances.Add(this);
            DomainManagerRegistry.Register<ISalvageRecipeProvider>(this);
            EnsureCraftingSkillState();
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
        }

        private void OnDisable()
        {
            ActiveInstances.Remove(this);
            DomainManagerRegistry.Unregister<ISalvageRecipeProvider>(this);
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
        }

        public bool TryGetCanonicalSalvageRecipe(string outputItemId, out SalvageRecipeDefinition definition)
        {
            definition = null;
            if (_recipeDatabase == null || string.IsNullOrWhiteSpace(outputItemId)) return false;
            RecipeDataSO selected = null;
            foreach (var candidate in _recipeDatabase.All)
            {
                if (candidate == null || candidate.OutputAmount != 1 ||
                    !string.Equals(candidate.OutputItemId, outputItemId, StringComparison.Ordinal)) continue;
                if (selected == null || string.CompareOrdinal(candidate.Id, selected.Id) < 0) selected = candidate;
            }
            if (selected == null) return false;
            definition = new SalvageRecipeDefinition
            {
                RecipeId = selected.Id,
                OutputItemId = selected.OutputItemId,
                OutputAmount = selected.OutputAmount
            };
            if (selected.Ingredients != null)
                foreach (var ingredient in selected.Ingredients)
                    definition.Ingredients.Add(new SalvageIngredient(ingredient.ItemId, ingredient.Amount));
            return true;
        }

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            if (_inventoryManager == null || _recipeDatabase == null)
            {
                Debug.LogError("CraftingRuntime requires InventoryManager and RecipeDatabaseSO.", this);
                return;
            }

            EnsureCraftingSkillState();
            IsInitialized = true;
        }

        public void RebindInventoryManager(InventoryManager inventoryManager)
        {
            _inventoryManager = inventoryManager;
            Initialize();
        }

        // arch: quebra do par mutuo Player|SceneManagement (2026-07-15) — aceita MonoBehaviour; cast
        // para o tipo concreto aqui dentro.
        public void RebindStaminaManager(MonoBehaviour staminaManagerRef)
        {
            _staminaManager = staminaManagerRef as StaminaManager;
        }

        public CraftingStation GetOrCreateStation(string stationInstanceId, WorkshopType stationType)
        {
            if (!IsInitialized || string.IsNullOrWhiteSpace(stationInstanceId))
            {
                return null;
            }

            if (_stations.TryGetValue(stationInstanceId, out var station))
            {
                return station;
            }

            station = new CraftingStation(stationInstanceId, stationType);
            _stations.Add(stationInstanceId, station);
            return station;
        }

        public CraftingStation GetPocketStation()
        {
            return GetOrCreateStation(PocketStationId, WorkshopType.None);
        }

        public List<RecipeDataSO> GetRecipesForStation(WorkshopType stationType)
        {
            var recipes = new List<RecipeDataSO>();
            if (_recipeDatabase == null)
            {
                return recipes;
            }

            foreach (var recipe in _recipeDatabase.All)
            {
                if (recipe != null && recipe.RequiredStationType == stationType &&
                    (recipe.IsUnlockedByDefault ||
                     CindarsHope.Crafting.CraftingRecipeGate.IsRecipeUnlocked(
                         recipe.RequiredRecipeUnlockId)))
                {
                    recipes.Add(recipe);
                }
            }

            return recipes;
        }

        public bool TryStartCraft(CraftingStation station, RecipeDataSO recipe, out string failureReason)
            => TryStartCraft(station, recipe, default, out failureReason);

        public bool TryStartCraft(
            CraftingStation station,
            RecipeDataSO recipe,
            LivingForgeCraftSelection livingForgeSelection,
            out string failureReason)
        {
            if (station == null)
            {
                failureReason = "Station is unavailable.";
                return false;
            }

            // fable_47 (follow-up 2): CraftTimeReduction derivada (F18) reduz a duração efetiva do
            // job. Fonte única exposta pelo PlayerVitalsApplier; multiplicador puro e testável.
            var reduction = PlayerVitalsApplier.CraftTimeReductionSource?.Invoke() ?? 0f;
            var craftTimeMultiplier = DerivedFollowupFormulas.CraftTimeMultiplier(reduction);

            int baseStaminaCost = recipe != null ? recipe.StaminaCost : 0;
            int staminaCost = WorkStaminaCostModifierProvider.PreviewCost(
                WorkStaminaChannel.Crafting, baseStaminaCost,
                transform.position.x, transform.position.y, station.StationInstanceId);
            EnsureCraftingSkillState();
            int livingForgeRank = LivingForgeRank;
            return station.TryStartCraft(recipe, _inventoryManager, out failureReason,
                _staminaManager, craftTimeMultiplier, staminaCost,
                charged => WorkStaminaCostModifierProvider.CommitSpend(
                    WorkStaminaChannel.Crafting, baseStaminaCost, charged,
                    transform.position.x, transform.position.y, station.StationInstanceId),
                null, ReserveCraftedItemInstanceId, ResolveBaseDurability,
                CraftedItemDurabilityProvider.CurrentBonus,
                InitializeCraftedItemDurability, livingForgeSelection,
                _craftingSkillState, livingForgeRank,
                _craftingSkillState?.CurrentDayIndex ?? 1);
        }

        public bool TryCollect(CraftingStation station, out string failureReason)
        {
            if (station == null)
            {
                failureReason = "Station is unavailable.";
                return false;
            }

            return station.TryCollectOutput(_inventoryManager, out failureReason,
                InitializeCraftedItemDurability, _craftingSkillState);
        }

        public bool TryCancel(CraftingStation station, out string failureReason)
        {
            if (station == null)
            {
                failureReason = "Station is unavailable.";
                return false;
            }

            return station.TryCancelJob(_inventoryManager, out failureReason,
                _craftingSkillState);
        }

        private void Update()
        {
            if (!IsInitialized)
            {
                return;
            }

            foreach (var station in _stations.Values)
            {
                station.Update(Time.deltaTime);
            }
        }

        public CraftingRuntimeSaveData CaptureSaveData()
        {
            var data = new CraftingRuntimeSaveData
            {
                NextCraftedItemSequence = Math.Max(1, _nextCraftedItemSequence)
            };
            foreach (var station in _stations.Values)
            {
                data.Stations.Add(station.CaptureSaveData());
            }

            return data;
        }

        public void LoadFromSaveData(CraftingRuntimeSaveData saveData)
        {
            if (!IsInitialized || saveData == null)
            {
                return;
            }

            _nextCraftedItemSequence = Math.Max(1, saveData.NextCraftedItemSequence);
            if (saveData.Stations == null)
            {
                return;
            }

            foreach (var stationData in saveData.Stations)
            {
                var station = GetOrCreateStation(stationData.StationInstanceId, (WorkshopType)stationData.StationType);
                station?.LoadFromSaveData(stationData, _recipeDatabase);
                AdvanceSequencePast(station?.Job?.OutputInstanceId);
            }
        }

        internal string ReserveCraftedItemInstanceId(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return string.Empty;
            return $"{itemId}#crafted-{_nextCraftedItemSequence++}";
        }

        private static int? ResolveBaseDurability(string itemId)
            => DomainManagerRegistry.Get<IEquipmentRuntime>()?.ResolveBaseDurability(itemId);

        private static void InitializeCraftedItemDurability(string itemInstanceId, int maxDurability)
            => DomainManagerRegistry.Get<IEquipmentRuntime>()?
                .InitializeCraftedItemDurability(itemInstanceId, maxDurability);

        private void EnsureCraftingSkillState()
        {
            if (_craftingSkillState != null)
                return;

            _craftingSkillState = DomainManagerRegistry.Get<CraftingSkillState>();
            if (_craftingSkillState == null)
            {
                _craftingSkillState = new CraftingSkillState();
                DomainManagerRegistry.Register(_craftingSkillState);
            }
        }

        private void OnDayStarted(DayStartedEvent evt)
        {
            EnsureCraftingSkillState();
            _craftingSkillState.ObserveDay(evt.DayNumber);
        }

        private void AdvanceSequencePast(string itemInstanceId)
        {
            if (string.IsNullOrWhiteSpace(itemInstanceId)) return;
            const string marker = "#crafted-";
            var markerIndex = itemInstanceId.LastIndexOf(marker, StringComparison.Ordinal);
            if (markerIndex < 1) return;
            if (long.TryParse(itemInstanceId.Substring(markerIndex + marker.Length), out var sequence)
                && sequence >= _nextCraftedItemSequence)
            {
                _nextCraftedItemSequence = sequence + 1;
            }
        }
    }

    [Serializable]
    public class CraftingRuntimeSaveData
    {
        public long NextCraftedItemSequence = 1;
        public List<CraftingStationSaveData> Stations = new List<CraftingStationSaveData>();
    }
}
