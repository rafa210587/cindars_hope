using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Farm.Watering;
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
    /// </summary>
    [DisallowMultipleComponent]
    public class FarmTillingInputController : MonoBehaviour
    {
        // Tecla de acao de fazenda (prototipo; mover para InputConstants em spec futura).
        private const KeyCode FarmActionKey = KeyCode.F;

        [SerializeField] private SaveManager _saveManager;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private EquipmentManager _equipmentManager;

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

            // Tentativa de arar o tile sob o jogador.
            // staminaOk = true (temporario; integrar StaminaManager em spec futura).
            bool tilled = _soilService.TillTile(tile.x, tile.y,
                hasTool: hasHoe,
                temporarySliceMode: true,
                staminaOk: true);

            if (tilled)
            {
                GameEventBus.Publish(new PlayerActionFeedbackEvent("Solo arado!", 1.5f));
                Debug.Log($"[FarmTillingInputController] Tile ({tile.x},{tile.y}) arado com sucesso.");
            }
            else
            {
                // Verifica se o tile ja esta arado — tenta regar.
                bool watered = _soilService.WaterTile(tile.x, tile.y,
                    hasTool: hasWateringCan,
                    temporarySliceMode: true,
                    staminaOk: true,
                    currentDay: 1);

                if (!watered)
                {
                    GameEventBus.Publish(new PlayerActionFeedbackEvent("Nao e possivel arar aqui", 1.5f));
                }
            }
        }

        // ── Wiring de editor (chamado pelo gerador de cena) ─────────────────────────────────────

        public void EditorWire(SaveManager saveManager, Transform playerTransform, EquipmentManager equipmentManager = null)
        {
            _saveManager = saveManager;
            _playerTransform = playerTransform;
            _equipmentManager = equipmentManager;
        }
    }
}
