using CindarsHope.Save;
using UnityEngine;

namespace CindarsHope.Farm.Runtime
{
    /// <summary>
    /// Bootstrap da FarmScene para o sistema de solo aravel por tile (Spec B).
    ///
    /// No Start() da FarmScene:
    ///  1. Chama SetBounds no FarmTileGrid do SaveManager (bounds v7: 64x44, origem centrada).
    ///  2. Registra as zonas nao-araveis via FarmNonArableZones (footprints de construcoes,
    ///     agua, montanha, interior de estruturas exceto estufa, 3 areas de expansao reservadas).
    ///     MIOLO x[-18,16] y[-15,16] = totalmente aravel (princípio P1/P2 v7).
    ///  3. Registra interior da estufa como aravel (GreenhouseRect).
    ///
    /// Wiring: referencia ao SaveManager injetada pelo gerador de cena (sem FindObjectOfType).
    /// Criado em 2026-06-26 (spec_farm_scene_relayout_v4 — fecha gap T006 da Spec B).
    /// Atualizado para v5 (56x40) em 2026-06-26 (spec_farm_scene_relayout_v4 §15.2 v5).
    /// Atualizado para v6 (64x44) em 2026-06-26 (spec_farm_scene_relayout_v4 §15.2 v6).
    /// Atualizado para v7 (borda aberta, miolo livre) em 2026-06-27 (spec_farm_scene_relayout_v4 §15.5 v7).
    /// </summary>
    [DisallowMultipleComponent]
    public class FarmSceneRuntimeBootstrap : MonoBehaviour
    {
        [SerializeField] private SaveManager _saveManager;

        // Tile size = 1 Unity unit = 32px. Bounds v6: 64x44, origin centrada (0,0).
        // Tile (0,0) = canto inferior esquerdo = world (-32, -22).
        private const int WidthTiles = 64;
        private const int HeightTiles = 44;
        private const float WorldOriginX = -32f;
        private const float WorldOriginY = -22f;
        private const float TileSizeUnits = 1f;

        private void Start()
        {
            if (_saveManager == null)
            {
                Debug.LogError("[FarmSceneRuntimeBootstrap] SaveManager nao esta wired. " +
                               "Regenere a FarmScene via CindarsHope/Inicializar Projeto.");
                return;
            }

            var grid = _saveManager.FarmTileGrid;
            var zones = grid.NonArableZones;

            // 1. Configura bounds v5 da fazenda.
            // originTileX/Y = 0 (canto inferior esquerdo do grid). worldOrigin = (-28, -20).
            grid.SetBounds(
                originTileX: 0,
                originTileY: 0,
                widthTiles: WidthTiles,
                heightTiles: HeightTiles,
                worldOriginX: WorldOriginX,
                worldOriginY: WorldOriginY);

            // 2. Registra footprints nao-araveis v7 (em coordenadas de tile).
            // Tile = floor((worldPos - worldOrigin) / tileSize).
            // Coordenadas v7 (64x44, origem centrada, worldOrigin = (-32,-22)):
            // Tile X = floor((worldX + 32) / 1), Tile Y = floor((worldY + 22) / 1).
            // PRINCIPIO P1/P2 v7: MIOLO x[-18,16] y[-15,16] = totalmente aravel. Nada nas bordas corta o miolo.

            // Montanha (faixa norte inteira: y world in [18,22], largura total 64).
            // x in [-32,32], y in [18,22] => cobre tudo acima de y=18.
            RegisterBlockedWorldRect(zones, grid, -32f, 18f, 64f, 4f);

            // Casa WALK-IN v7: centro (28,9), footprint ~7x6 => x in [24.5,31.5], y in [6,12].
            RegisterBlockedWorldRect(zones, grid, 24.5f, 6f, 7f, 6f);

            // Estufa v7: centro (24,10), exterior footprint ~5x4 => x in [21.5,26.5], y in [8,12].
            // Interior aravel (GreenhouseRect abaixo); exterior nao-aravel.
            RegisterBlockedWorldRect(zones, grid, 21.5f, 8f, 5f, 4f);

            // Coop_01 v7: centro (-22,-19), footprint 6x3 => x in [-25,-19], y in [-20.5,-17.5].
            RegisterBlockedWorldRect(zones, grid, -25f, -20.5f, 6f, 3f);

            // Barn_01 v7: centro (-13,-19), footprint 7x4 => x in [-16.5,-9.5], y in [-21,-17].
            RegisterBlockedWorldRect(zones, grid, -16.5f, -21f, 7f, 4f);

            // CheesePress v7: centro (-28,-19), footprint ~2x2 => x in [-29,-27], y in [-20,-18].
            RegisterBlockedWorldRect(zones, grid, -29f, -20f, 2f, 2f);

            // WineBarrel v7: centro (-8,-19), footprint ~2x2 => x in [-9,-7], y in [-20,-18].
            RegisterBlockedWorldRect(zones, grid, -9f, -20f, 2f, 2f);

            // Acude/Nascente v7: centro (22,18), footprint ~4x3 => x in [20,24], y in [16.5,19.5].
            // Fica na faixa da montanha; registro garante nao-aravel.
            RegisterBlockedWorldRect(zones, grid, 20f, 16.5f, 4f, 3f);

            // Rio v7 — BORDA LESTE, rota (22,18)→(21,8)→(20,-2)→(19,-8)→foz lago (19,-6).
            // NUNCA cruza o miolo x[-18,16]. Todos os segmentos em x > 18.
            // Seg_N v7: x in [20.6,22.4], y in [8,18].
            RegisterBlockedWorldRect(zones, grid, 20.6f, 8f, 1.8f, 10f);
            // Seg_C_Upper v7: x in [20.1,21.9], y in [4,8].
            RegisterBlockedWorldRect(zones, grid, 20.1f, 4f, 1.8f, 4f);
            // Seg_C_Lower v7 (abaixo da ponte): x in [19.1,20.9], y in [-2,2].
            RegisterBlockedWorldRect(zones, grid, 19.1f, -2f, 1.8f, 4f);
            // Seg_Lower2 v7: x in [18.6,20.4], y in [-8,-2].
            RegisterBlockedWorldRect(zones, grid, 18.6f, -8f, 1.8f, 6f);
            // Seg_Delta v7 (foz): x in [18.1,19.9], y in [-8,-6].
            RegisterBlockedWorldRect(zones, grid, 18.1f, -8f, 1.8f, 2f);

            // Lago organico SE v7: bounding box conservadora x in [5,31], y in [-20,-6].
            RegisterBlockedWorldRect(zones, grid, 5f, -20f, 26f, 14f);

            // Ponte v7: centro (21,3), footprint ~3x2 => x in [19.5,22.5], y in [2,4].
            // Corredor sem colisor sob a ponte (vao aberto — passagem real).
            zones.UnregisterBlockedRect(
                WorldToTileX(grid, 19.5f), WorldToTileY(grid, 2f),
                3, 2);

            // 3 areas de expansao reservadas v7 (nas BORDAS — fora do miolo):
            // Exp_North: centro (-2,16.5), footprint 8x4 => x in [-6,2], y in [14.5,18.5].
            RegisterBlockedWorldRect(zones, grid, -6f, 14.5f, 8f, 4f);
            // Exp_NE: centro (16,16.5), footprint 7x4 => x in [12.5,19.5], y in [14.5,18.5].
            RegisterBlockedWorldRect(zones, grid, 12.5f, 14.5f, 7f, 4f);
            // Exp_West: centro (-30,-10), footprint 5x8 => x in [-32.5,-27.5], y in [-14,-6].
            RegisterBlockedWorldRect(zones, grid, -32.5f, -14f, 5f, 8f);

            // 3. Interior da estufa v7 (centro 24,10 footprint 5x4: 21.5,8 → 26.5,12) como aravel.
            RegisterGreenhouseWorldRect(zones, grid, 21.5f, 8f, 5f, 4f);

            Debug.Log("[FarmSceneRuntimeBootstrap] FarmTileGrid configurado: " +
                      $"bounds {WidthTiles}x{HeightTiles}, worldOrigin ({WorldOriginX},{WorldOriginY}), " +
                      "zonas nao-araveis v7 registradas (construcoes borda+agua borda+montanha+3 areas expansao), " +
                      "miolo x[-18,16] y[-15,16] totalmente aravel, interior da estufa aravel.");
        }

        // ── Helpers de conversao world -> tile ──────────────────────────────────────────────────

        private static int WorldToTileX(FarmTileGrid grid, float worldX)
        {
            return Mathf.FloorToInt((worldX - WorldOriginX) / TileSizeUnits);
        }

        private static int WorldToTileY(FarmTileGrid grid, float worldY)
        {
            return Mathf.FloorToInt((worldY - WorldOriginY) / TileSizeUnits);
        }

        private static void RegisterBlockedWorldRect(FarmNonArableZones zones, FarmTileGrid grid,
            float worldX, float worldY, float worldWidth, float worldHeight)
        {
            int tileX = WorldToTileX(grid, worldX);
            int tileY = WorldToTileY(grid, worldY);
            int tileW = Mathf.CeilToInt(worldWidth / TileSizeUnits);
            int tileH = Mathf.CeilToInt(worldHeight / TileSizeUnits);
            zones.RegisterBlockedRect(tileX, tileY, tileW, tileH);
        }

        private static void RegisterGreenhouseWorldRect(FarmNonArableZones zones, FarmTileGrid grid,
            float worldX, float worldY, float worldWidth, float worldHeight)
        {
            int tileX = WorldToTileX(grid, worldX);
            int tileY = WorldToTileY(grid, worldY);
            int tileW = Mathf.CeilToInt(worldWidth / TileSizeUnits);
            int tileH = Mathf.CeilToInt(worldHeight / TileSizeUnits);
            zones.RegisterGreenhouseRect(tileX, tileY, tileW, tileH);
        }

        // ── Wiring de editor (chamado pelo gerador de cena) ─────────────────────────────────────

        public void EditorWire(SaveManager saveManager)
        {
            _saveManager = saveManager;
        }
    }
}
