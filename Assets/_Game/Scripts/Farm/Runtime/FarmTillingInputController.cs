using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Farm.Watering;
using CindarsHope.Player;
using CindarsHope.Save;
using CindarsHope.Tools;
using UnityEngine;

namespace CindarsHope.Farm.Runtime
{
    /// <summary>
    /// Controlador de input de ferramentas de fazenda (enxada/regador/semente/colher) para o
    /// sistema de solo aravel por tile (Spec B). Fecha o gap T006 da Spec B.
    ///
    /// Uso: ao pressionar [F] (acao de ferramenta de fazenda), detecta o tile sob/a frente do
    /// jogador e chama FarmTilledSoilService.TillTile / WaterTile / HarvestTile conforme a
    /// ferramenta equipada no hotbar.
    ///
    /// Wiring: referencias injetadas pelo gerador de cena (sem FindObjectOfType).
    /// Criado em 2026-06-26 (spec_farm_scene_relayout_v4 + spec_farm_till_anywhere_tilemap).
    /// Fase 8 (2026-06-26): checagem real de ferramenta via EquipmentManager.HasTool em vez de flag.
    /// spec_codex_03 (2026-07-03): stamina real via StaminaManager.TrySpendStamina/CurrentStamina
    /// e dia real via TimeManager.CurrentDay, substituindo os literais staminaOk:true/currentDay:1.
    /// </summary>
    [DisallowMultipleComponent]
    public class FarmTillingInputController : MonoBehaviour
    {
        // Tecla de acao de fazenda (prototipo; mover para InputConstants em spec futura).
        private const KeyCode FarmActionKey = KeyCode.F;

        // Custo de stamina por acao (placeholder de balance, sujeito a tuning futuro —
        // nao existe PlayerNeedsBalanceSO equivalente para farm actions hoje; consts locais
        // seguem a rule no-magic-balance-values e a escada de minimalismo).
        private const int TillStaminaCost = 10;
        private const int WaterStaminaCost = 8;

        private const string InsufficientStaminaMessage = "Stamina insuficiente para usar a ferramenta.";

        [SerializeField] private SaveManager _saveManager;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private EquipmentManager _equipmentManager;
        [SerializeField] private StaminaManager _staminaManager;
        [SerializeField] private CindarsHope.Core.Time.TimeManager _timeManager;

        // Servico de solo aravel (obtido do SaveManager na inicializacao).
        private FarmTilledSoilService _soilService;

        private void Start()
        {
            if (_saveManager == null || _playerTransform == null)
            {
                Debug.LogWarning("[FarmTillingInputController] SaveManager ou PlayerTransform nao wired. " +
                                 "Regenere a FarmScene via CindarsHope/Inicializar Projeto.");
                enabled = false;
                return;
            }

            var grid = _saveManager.FarmTileGrid;
            var wateringService = new FarmWateringService();
            _soilService = new FarmTilledSoilService(grid, wateringService);

            Debug.Log("[FarmTillingInputController] Inicializado. Pressione [F] sobre um tile para arar.");
        }

        private void Update()
        {
            if (!Input.GetKeyDown(FarmActionKey))
            {
                return;
            }

            if (_soilService == null || _saveManager == null)
            {
                return;
            }

            var grid = _saveManager.FarmTileGrid;
            var playerPos = _playerTransform.position;
            var tile = grid.WorldToTile(playerPos.x, playerPos.y);

            // Checagem real de ferramenta via EquipmentManager (fase 8).
            // Se EquipmentManager nao estiver wired, cai no fallback permissivo com aviso.
            bool hasHoe;
            bool hasWateringCan;
            if (_equipmentManager != null)
            {
                hasHoe = _equipmentManager.HasTool(ToolType.Hoe);
                hasWateringCan = _equipmentManager.HasTool(ToolType.WateringCan);
            }
            else
            {
                Debug.LogWarning("[FarmTillingInputController] EquipmentManager nao wired — " +
                                 "permitindo acao sem verificacao de ferramenta (residual). " +
                                 "Regenere a FarmScene via CindarsHope/Inicializar Projeto.");
                hasHoe = true;
                hasWateringCan = true;
            }

            var staminaManager = ResolveStaminaManager();

            // Checagem real de stamina (padrao TreeNode.Interact): checa CurrentStamina como
            // pre-condicao ANTES de chamar TillTile/WaterTile (o parametro staminaOk e
            // pre-condicao, nao pos-condicao — nao ha estado a reverter no controller).
            bool tillStaminaOk = HasEnoughStamina(staminaManager, TillStaminaCost);

            // Tentativa de arar o tile sob o jogador.
            // spec_codex_03: temporarySliceMode desligado — exigencia de ferramenta agora e real
            // (o fallback permissivo de EquipmentManager ausente acima cobre cena nao-regenerada).
            bool tilled = _soilService.TillTile(tile.x, tile.y,
                hasTool: hasHoe,
                temporarySliceMode: false,
                staminaOk: tillStaminaOk);

            if (tilled)
            {
                SpendStaminaIfPresent(staminaManager, TillStaminaCost);
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Solo arado!", 1.5f));
                Debug.Log($"[FarmTillingInputController] Tile ({tile.x},{tile.y}) arado com sucesso.");
                return;
            }

            if (hasHoe && !tillStaminaOk)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent(InsufficientStaminaMessage, 1.5f));
                return;
            }

            // Verifica se o tile ja esta arado — tenta regar.
            bool waterStaminaOk = HasEnoughStamina(staminaManager, WaterStaminaCost);
            int currentDay = ResolveCurrentDay();

            bool watered = _soilService.WaterTile(tile.x, tile.y,
                hasTool: hasWateringCan,
                temporarySliceMode: false,
                staminaOk: waterStaminaOk,
                currentDay: currentDay);

            if (watered)
            {
                SpendStaminaIfPresent(staminaManager, WaterStaminaCost);
                return;
            }

            if (hasWateringCan && !waterStaminaOk)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent(InsufficientStaminaMessage, 1.5f));
                return;
            }

            if (!hasHoe && !hasWateringCan)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Precisa de enxada ou regador equipado.", 1.5f));
                return;
            }

            GameEventBus.Publish(new PlayerActionFeedbackEvent("Nao e possivel arar aqui", 1.5f));
        }

        private StaminaManager ResolveStaminaManager()
        {
            if (_staminaManager != null)
            {
                return _staminaManager;
            }

            return GameBootstrap.Instance != null ? GameBootstrap.Instance.StaminaManager : null;
        }

        private CindarsHope.Core.Time.TimeManager ResolveTimeManager()
        {
            if (_timeManager != null)
            {
                return _timeManager;
            }

            return GameBootstrap.Instance != null ? GameBootstrap.Instance.TimeManager : null;
        }

        private static bool HasEnoughStamina(StaminaManager staminaManager, int cost)
        {
            // Fallback permissivo (com aviso) se StaminaManager nao estiver wired — mesmo
            // padrao ja usado para EquipmentManager ausente nesta classe.
            if (staminaManager == null)
            {
                Debug.LogWarning("[FarmTillingInputController] StaminaManager nao wired — " +
                                  "permitindo acao sem verificacao de stamina (residual). " +
                                  "Regenere a FarmScene via CindarsHope/Inicializar Projeto.");
                return true;
            }

            return staminaManager.CurrentStamina >= cost;
        }

        private static void SpendStaminaIfPresent(StaminaManager staminaManager, int cost)
        {
            if (staminaManager == null)
            {
                return;
            }

            staminaManager.TrySpendStamina(cost);
        }

        private int ResolveCurrentDay()
        {
            var timeManager = ResolveTimeManager();
            if (timeManager == null)
            {
                Debug.LogWarning("[FarmTillingInputController] TimeManager nao wired — " +
                                  "usando dia 1 como fallback (residual). " +
                                  "Regenere a FarmScene via CindarsHope/Inicializar Projeto.");
                return 1;
            }

            return timeManager.CurrentDay;
        }

        // ── Wiring de editor (chamado pelo gerador de cena) ─────────────────────────────────────

        public void EditorWire(SaveManager saveManager, Transform playerTransform, EquipmentManager equipmentManager = null,
            StaminaManager staminaManager = null, CindarsHope.Core.Time.TimeManager timeManager = null)
        {
            _saveManager = saveManager;
            _playerTransform = playerTransform;
            _equipmentManager = equipmentManager;
            _staminaManager = staminaManager;
            _timeManager = timeManager;
        }
    }
}
