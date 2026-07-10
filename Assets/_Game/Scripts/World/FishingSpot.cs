using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using CindarsHope.Loot;
using CindarsHope.Player;
using CindarsHope.Tools;
using CindarsHope.World.Fishing;
using CindarsHope.World.Weather;
using UnityEngine;

namespace CindarsHope.World
{
    [DisallowMultipleComponent]
    public class FishingSpot : MonoBehaviour, IInteractable
    {
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private StaminaManager _staminaManager;
        [SerializeField] private string _requiredToolId = "item_tool_fishing_rod_basic";
        [SerializeField] private string _fishItemId = "item_fish_common";
        [SerializeField] private int _fishAmount = 1;
        [SerializeField] private LootTableSO _lootTable;
        [SerializeField] private float _castDelaySeconds = 0.5f;
        [SerializeField] private float _timingWindowSeconds = 1.25f;
        [SerializeField] private bool _requireEdgeInteraction = true;
        [SerializeField] private Vector2 _edgeInteractionOuterHalfExtents = new Vector2(0.45f, 0.45f);
        [SerializeField] private Vector2 _edgeInteractionInnerHalfExtents = new Vector2(0.35f, 0.35f);

        [Header("fable_50 — Pesca v2 (tabela/clima/minigame)")]
        [Tooltip("Tabela de pesca por contexto (FishingTableSO). Vazio/null = comportamento v1 (_fishItemId/_lootTable).")]
        [SerializeField] private FishingTableSO _fishingTable;
        [Tooltip("Id estável usado no seed determinístico do resolver. Vazio = nome do GameObject.")]
        [SerializeField] private string _fishingSpotId;
        [Tooltip("Quando true e há tabela, usa o resolver v2 + minigame Perfect/Good/Miss.")]
        [SerializeField] private bool _enableFishingV2 = true;
        [Tooltip("Quando true, este spot é externo: Storm o bloqueia (ex.: açude da fazenda).")]
        [SerializeField] private bool _isOutdoorSpot = true;
        [Tooltip("Estação atual fixa/derivada (token). Vazio = sem restrição de estação.")]
        [SerializeField] private string _seasonOverride = string.Empty;
        [Tooltip("Referência opcional ao relógio (hora-do-dia). Sem ela, assume meio-dia.")]
        [SerializeField] private GameTimeManager _timeManager;

        private bool _isFishing;
        private float _windowOpenTime;
        private int _castIndex;
        private FishingTimingMinigame _minigame;

        public string InteractionPrompt => "Pescar";

        public bool CanInteract(GameObject interactor)
        {
            if (_inventoryManager == null)
            {
                return false;
            }

            return !_requireEdgeInteraction || IsInteractorOnEdge(interactor);
        }

        public void Interact(GameObject interactor)
        {
            const int fishCastStaminaCost = 20;

            if (_inventoryManager == null)
            {
                Debug.LogWarning("FishingSpot cannot fish because InventoryManager is missing.", this);
                return;
            }

            if (!HasRequiredTool())
            {
                const string message = "Requires Fishing Rod.";
                GameEventBus.Publish(new PlayerActionFeedbackEvent(message));
                Debug.Log($"FishingSpot blocked fishing. {message} Expected tool id hint '{_requiredToolId}'.", this);
                return;
            }

            if (!_isFishing)
            {
                // fable_50: Storm bloqueia spot externo (açude da fazenda). Spot de caverna é interno.
                if (_isOutdoorSpot && IsStormActive())
                {
                    GameEventBus.Publish(new PlayerActionFeedbackEvent("A tempestade fechou a pesca aqui."));
                    return;
                }

                if (_staminaManager != null && _staminaManager.CurrentStamina < fishCastStaminaCost)
                {
                    GameEventBus.Publish(new PlayerActionFeedbackEvent("Not enough stamina to fish."));
                    return;
                }

                StartCoroutine(FishingRoutine(fishCastStaminaCost));
                return;
            }

            TryConfirmFishing();
        }

        public void RebindInventoryManager(InventoryManager inventoryManager)
        {
            if (inventoryManager == null)
            {
                Debug.LogWarning("FishingSpot received null InventoryManager for rebind.", this);
                return;
            }

            _inventoryManager = inventoryManager;
        }

        public void RebindStaminaManager(StaminaManager staminaManager)
        {
            _staminaManager = staminaManager;
        }

        private void OnValidate()
        {
            _fishAmount = Mathf.Max(1, _fishAmount);
            _castDelaySeconds = Mathf.Max(0f, _castDelaySeconds);
            _timingWindowSeconds = Mathf.Max(0.1f, _timingWindowSeconds);
            _edgeInteractionOuterHalfExtents = Max(_edgeInteractionOuterHalfExtents, new Vector2(0.01f, 0.01f));
            _edgeInteractionInnerHalfExtents = Vector2.Min(_edgeInteractionInnerHalfExtents, _edgeInteractionOuterHalfExtents);
            _edgeInteractionInnerHalfExtents = Max(_edgeInteractionInnerHalfExtents, Vector2.zero);
        }

        private bool IsInteractorOnEdge(GameObject interactor)
        {
            if (interactor == null)
            {
                return false;
            }

            var localPosition = transform.InverseTransformPoint(interactor.transform.position);
            var absoluteLocal = new Vector2(Mathf.Abs(localPosition.x), Mathf.Abs(localPosition.y));
            var insideOuter = absoluteLocal.x <= _edgeInteractionOuterHalfExtents.x
                && absoluteLocal.y <= _edgeInteractionOuterHalfExtents.y;
            var outsideInnerWater = absoluteLocal.x >= _edgeInteractionInnerHalfExtents.x
                || absoluteLocal.y >= _edgeInteractionInnerHalfExtents.y;

            return insideOuter && outsideInnerWater;
        }

        private static Vector2 Max(Vector2 value, Vector2 minimum)
        {
            return new Vector2(Mathf.Max(value.x, minimum.x), Mathf.Max(value.y, minimum.y));
        }

        private bool UsesFishingV2 => _enableFishingV2 && _fishingTable != null;

        private System.Collections.IEnumerator FishingRoutine(int staminaCost)
        {
            _isFishing = true;
            if (_staminaManager != null && !_staminaManager.TrySpendStamina(staminaCost))
            {
                _isFishing = false;
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Not enough stamina to fish."));
                yield break;
            }

            GameEventBus.Publish(new PlayerActionFeedbackEvent("Fishing..."));
            yield return new WaitForSeconds(_castDelaySeconds);
            _windowOpenTime = Time.time;

            // fable_50: minigame de barra (Perfect/Good/Miss). A view rica fica para F14/F20;
            // aqui é lógica + feedback por prompt. Sem tabela/flag off ⇒ caminho v1 intocado.
            _minigame = UsesFishingV2 ? new FishingTimingMinigame(_timingWindowSeconds) : null;
            _minigame?.Start();
            GameEventBus.Publish(new PlayerActionFeedbackEvent(UsesFishingV2 ? "Pressione E no centro!" : "Press E now!"));

            while (_isFishing && Time.time - _windowOpenTime <= _timingWindowSeconds)
            {
                _minigame?.Tick(Time.deltaTime);
                yield return null;
            }

            if (_isFishing)
            {
                // Estourou a janela sem confirmar = Miss (timeout). Mitiga "estado preso em Cast".
                _isFishing = false;
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Fishing failed."));
            }
        }

        private void TryConfirmFishing()
        {
            if (Time.time - _windowOpenTime > _timingWindowSeconds)
            {
                _isFishing = false;
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Fishing failed."));
                return;
            }

            if (UsesFishingV2)
            {
                ConfirmFishingV2();
                return;
            }

            _isFishing = false;
            var itemId = _fishItemId;
            var amount = _fishAmount;
            if (_lootTable != null && _lootTable.TryRoll(out var rolledItemId, out var rolledAmount))
            {
                itemId = rolledItemId;
                amount = rolledAmount;
            }

            if (!_inventoryManager.AddItem(itemId, amount))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Inventory full. Catch kept in the water."));
                Debug.LogWarning($"FishingSpot could not add fish '{itemId}' x{amount}; catch was not consumed.", this);
                return;
            }

            GameEventBus.Publish(new FishCaughtEvent(itemId, amount, Vector2Int.RoundToInt(transform.position)));
            Debug.Log($"FishingSpot caught '{itemId}' x{amount}.", this);
        }

        // fable_50: confirmação v2 — minigame + resolver determinístico (fonte única de captura).
        private void ConfirmFishingV2()
        {
            _isFishing = false;
            var grade = _minigame != null ? _minigame.Submit() : FishingTimingGrade.Good;
            _minigame = null;

            var spotId = ResolveSpotId();
            var context = new FishingContext(
                StableSeed(spotId), 0, spotId, _castIndex++, _seasonOverride, CurrentWeatherToken(), CurrentHour());
            var outcome = FishingCatchResolver.Resolve(_fishingTable, context, grade);

            if (!outcome.Success)
            {
                // Miss ou contexto sem peixe ⇒ sem captura + feedback (nunca exception).
                var reason = grade == FishingTimingGrade.Miss ? "Escapou! (timing)" : "Nada mordeu a isca.";
                GameEventBus.Publish(new PlayerActionFeedbackEvent(reason));
                GameEventBus.Publish(new FishCatchResolvedEvent(spotId, _fishingTable.TableId, null,
                    outcome.Rarity, outcome.Quality, (int)grade));
                return;
            }

            const int amount = 1;
            if (!_inventoryManager.AddItem(outcome.ItemId, amount))
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Inventory full. Catch kept in the water."));
                Debug.LogWarning($"FishingSpot could not add fish '{outcome.ItemId}'; catch was not consumed.", this);
                return;
            }

            var gradeLabel = grade == FishingTimingGrade.Perfect ? "Perfeito!" : "Fisgou!";
            GameEventBus.Publish(new PlayerActionFeedbackEvent($"{gradeLabel} {outcome.ItemId}"));
            GameEventBus.Publish(new FishCaughtEvent(outcome.ItemId, amount, Vector2Int.RoundToInt(transform.position)));
            GameEventBus.Publish(new FishCatchResolvedEvent(spotId, _fishingTable.TableId, outcome.ItemId,
                outcome.Rarity, outcome.Quality, (int)grade));
            Debug.Log($"FishingSpot(v2) caught '{outcome.ItemId}' rarity={outcome.Rarity} grade={grade}.", this);
        }

        private string ResolveSpotId()
        {
            if (!string.IsNullOrWhiteSpace(_fishingSpotId))
            {
                return _fishingSpotId;
            }

            var pos = Vector2Int.RoundToInt(transform.position);
            return $"farm_spot_{pos.x}_{pos.y}";
        }

        private int CurrentHour()
        {
            return _timeManager != null ? _timeManager.CurrentHourOfDay : 12;
        }

        private string CurrentWeatherToken()
        {
            var service = WorldWeatherService.Instance;
            return service != null ? service.CurrentWeather.ToString() : string.Empty;
        }

        private static bool IsStormActive()
        {
            var service = WorldWeatherService.Instance;
            return service != null && service.CurrentWeather == WeatherType.Stormy;
        }

        // Seed estável por spot (FNV-1a) — determinismo independente de processo.
        private static int StableSeed(string spotId)
        {
            unchecked
            {
                const int fnvOffset = (int)2166136261;
                const int fnvPrime = 16777619;
                var hash = fnvOffset;
                foreach (var c in spotId ?? string.Empty)
                {
                    hash ^= c;
                    hash *= fnvPrime;
                }

                return hash == int.MinValue ? 0 : hash;
            }
        }

        private static bool HasRequiredTool()
        {
            // arch: Core|Equipment (spec_arch_core_equipment_cycle_reduction_v35) — EquipmentManager
            // resolvido via EquipmentManager.Instance (self-registro, molde Craft/Economy/Skills).
            var equipmentManager = EquipmentManager.Instance;
            return equipmentManager != null && equipmentManager.HasTool(ToolType.FishingRod, ToolTier.Basic);
        }
    }
}
