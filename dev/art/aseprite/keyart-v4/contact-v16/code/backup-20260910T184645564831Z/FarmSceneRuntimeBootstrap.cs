using CindarsHope.Foundation;
using CindarsHope.Farm.Scene;
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
    /// Wiring: FarmTileGrid resolvido via DomainManagerRegistry (SaveManager o registra no
    /// Initialize()) — sem FindObjectOfType e sem nomear CindarsHope.Save (quebra do ciclo mutuo
    /// Farm|Save, spec_arch_farm_save_cycle_reduction).
    /// Criado em 2026-06-26 (spec_farm_scene_relayout_v4 — fecha gap T006 da Spec B).
    /// Atualizado para v5 (56x40) em 2026-06-26 (spec_farm_scene_relayout_v4 §15.2 v5).
    /// Atualizado para v6 (64x44) em 2026-06-26 (spec_farm_scene_relayout_v4 §15.2 v6).
    /// Atualizado para v7 (borda aberta, miolo livre) em 2026-06-27 (spec_farm_scene_relayout_v4 §15.5 v7).
    /// </summary>
    [DisallowMultipleComponent]
    public class FarmSceneRuntimeBootstrap : MonoBehaviour
    {
        [SerializeField] private Collider2D[] _authoredSolidBases = System.Array.Empty<Collider2D>();

        // Tile size = 1 Unity unit = 32px. Bounds v6: 64x44, origin centrada (0,0).
        // Tile (0,0) = canto inferior esquerdo = world (-32, -22).
        private const int WidthTiles = (int)FarmLevel1LayoutContract.Level1WidthTiles;
        private const int HeightTiles = (int)FarmLevel1LayoutContract.Level1HeightTiles;
        private const float WorldOriginX = FarmLevel1LayoutContract.MinX;
        private const float WorldOriginY = FarmLevel1LayoutContract.MinY;
        private const float TileSizeUnits = 1f;

        private void Start()
        {
            var grid = DomainManagerRegistry.Get<FarmTileGrid>();
            if (grid == null)
            {
                Debug.LogError("[FarmSceneRuntimeBootstrap] FarmTileGrid nao esta registrado no " +
                               "DomainManagerRegistry (SaveManager.Initialize ainda nao rodou). " +
                               "Regenere a FarmScene via CindarsHope/Inicializar Projeto.");
                return;
            }

            var zones = grid.NonArableZones;
            // Rebuild scene masks only. Existing tilled crops remain in the shared grid.
            zones.Clear();

            // 1. Configura bounds v5 da fazenda.
            // Negative origin tiles expand the envelope while old saved tile(0,0) stays at world(-32,-22).
            grid.SetBounds(
                originTileX: FarmLevel1LayoutContract.GridOriginTileX,
                originTileY: FarmLevel1LayoutContract.GridOriginTileY,
                widthTiles: WidthTiles,
                heightTiles: HeightTiles,
                worldOriginX: WorldOriginX,
                worldOriginY: WorldOriginY);

            // The rectangular envelope is not the clearing: forest corners are permanently solid.
            for (var y = FarmLevel1LayoutContract.GridOriginTileY; y < FarmLevel1LayoutContract.GridOriginTileY + HeightTiles; y++)
            for (var x = FarmLevel1LayoutContract.GridOriginTileX; x < FarmLevel1LayoutContract.GridOriginTileX + WidthTiles; x++)
                if (!FarmEnclosedValleyBoundaryContract.ContainsClearing(grid.TileToWorldCenter(x, y)))
                    zones.RegisterBlockedTile(x, y);

            // 2. Registra footprints nao-araveis v7 (em coordenadas de tile).
            // Tile = floor((worldPos - worldOrigin) / tileSize).
            // Coordenadas v7 (64x44, origem centrada, worldOrigin = (-32,-22)):
            // Tile X = floor((worldX + 32) / 1), Tile Y = floor((worldY + 22) / 1).
            // PRINCIPIO P1/P2 v7: MIOLO x[-18,16] y[-15,16] = totalmente aravel. Nada nas bordas corta o miolo.

            // Montanha (faixa norte inteira: y world in [18,22], largura total 64).
            // x in [-32,32], y in [18,22] => cobre tudo acima de y=18.
            RegisterBlockedPolygon(zones, grid, FarmSceneSpatialContract.Mountain);
            RegisterBlockedSolidRect(zones, grid, FarmSettlementPhysicsContract.FountainBasin);
            RegisterBlockedSolidRect(zones, grid, FarmSettlementPhysicsContract.CentralWell);
            foreach (var body in FarmSettlementPhysicsContract.ServiceBodies) RegisterBlockedSolidRect(zones, grid, body);
            foreach (var fence in FarmSettlementPhysicsContract.AllFenceSegments) RegisterBlockedSolidRect(zones, grid, fence);
            foreach (var entranceBase in FarmEntrancePhysicsContract.CaveSideBases) RegisterBlockedSolidRect(zones, grid, entranceBase);
            // Explicit scene references keep visible prop bases and soil masks on the same footprint.
            foreach (var solid in _authoredSolidBases)
            {
                if (solid == null || solid.isTrigger || !solid.enabled || !solid.gameObject.activeInHierarchy) continue;
                var bounds = solid.bounds;
                RegisterBlockedSolidRect(zones, grid, new FarmSolidRect(solid.name, bounds.center, bounds.size));
            }

            // Casa WALK-IN v7: centro (28,9), footprint ~7x6 => x in [24.5,31.5], y in [6,12].
            RegisterBlockedFootprint(zones, grid, FarmSceneSpatialContract.House);

            // Estufa v7: centro (24,10), exterior footprint ~5x4 => x in [21.5,26.5], y in [8,12].
            // Interior aravel (GreenhouseRect abaixo); exterior nao-aravel.
            var greenhouseExterior = GetFootprintBounds(FarmSceneSpatialContract.Greenhouse);
            RegisterBlockedSolidRect(zones, grid, new FarmSolidRect("GreenhouseExterior", greenhouseExterior.center, greenhouseExterior.size));

            // Coop_01 v7: centro (-22,-19), footprint 6x3 => x in [-25,-19], y in [-20.5,-17.5].
            RegisterBlockedFootprint(zones, grid, FarmSceneSpatialContract.AnimalBuildings);

            // Barn_01 v7: centro (-13,-19), footprint 7x4 => x in [-16.5,-9.5], y in [-21,-17].
            // AnimalBuildings above covers the cooperative and barn as one catalog footprint.

            // CheesePress v7: centro (-28,-19), footprint ~2x2 => x in [-29,-27], y in [-20,-18].
            RegisterBlockedFootprint(zones, grid, FarmSceneSpatialContract.CraftingYard);

            // WineBarrel v7: centro (-8,-19), footprint ~2x2 => x in [-9,-7], y in [-20,-18].
            // CraftingYard above covers both processing stations.

            // Acude/Nascente v7: centro (22,18), footprint ~4x3 => x in [20,24], y in [16.5,19.5].
            // Fica na faixa da montanha; registro garante nao-aravel.
            RegisterBlockedFootprint(zones, grid, FarmSceneSpatialContract.River);

            // Rio v7 — BORDA LESTE, rota (22,18)→(21,8)→(20,-2)→(19,-8)→foz lago (19,-6).
            // NUNCA cruza o miolo x[-18,16]. Todos os segmentos em x > 18.
            // Seg_N v7: x in [20.6,22.4], y in [8,18].
            // River is registered once from the spatial contract above.
            // Seg_C_Upper v7: x in [20.1,21.9], y in [4,8].
            // River is registered once from the spatial contract above.
            // Seg_C_Lower v7 (abaixo da ponte): x in [19.1,20.9], y in [-2,2].
            // River is registered once from the spatial contract above.
            // Seg_Lower2 v7: x in [18.6,20.4], y in [-8,-2].
            // River is registered once from the spatial contract above.
            // Seg_Delta v7 (foz): x in [18.1,19.9], y in [-8,-6].
            // River is registered once from the spatial contract above.

            // Lago organico SE v7: bounding box conservadora x in [5,31], y in [-20,-6].
            RegisterBlockedFootprint(zones, grid, FarmSceneSpatialContract.Lake);

            // Ponte v7: centro (21,3), footprint ~3x2 => x in [19.5,22.5], y in [2,4].
            // Corredor sem colisor sob a ponte (vao aberto — passagem real).
            var bridge = GetFootprintBounds(FarmSceneSpatialContract.Bridge);
            zones.UnregisterBlockedRect(WorldToTileX(grid, bridge.min.x), WorldToTileY(grid, bridge.min.y),
                Mathf.CeilToInt(bridge.size.x), Mathf.CeilToInt(bridge.size.y));

            // 3 areas de expansao reservadas v7 (nas BORDAS — fora do miolo):
            // Exp_North: centro (-2,16.5), footprint 8x4 => x in [-6,2], y in [14.5,18.5].
            RegisterBlockedWorldRect(zones, grid, -6f, 14.5f, 8f, 4f);
            // Exp_NE: centro (16,16.5), footprint 7x4 => x in [12.5,19.5], y in [14.5,18.5].
            RegisterBlockedWorldRect(zones, grid, 12.5f, 14.5f, 7f, 4f);
            // Exp_West: centro (-30,-10), footprint 5x8 => x in [-32.5,-27.5], y in [-14,-6].
            RegisterBlockedWorldRect(zones, grid, -32.5f, -14f, 5f, 8f);

            // The exception contains only whole tiles between the same bases used by the creator.
            var greenhouseInterior = FarmSettlementPhysicsContract.GreenhouseInteriorBounds;
            RegisterGreenhouseWorldRect(zones, grid, greenhouseInterior.xMin, greenhouseInterior.yMin,
                greenhouseInterior.width, greenhouseInterior.height);

            Debug.Log("[FarmSceneRuntimeBootstrap] FarmTileGrid configurado: " +
                      $"bounds {WidthTiles}x{HeightTiles}, worldOrigin ({WorldOriginX},{WorldOriginY}), " +
                      "zonas nao-araveis v7 registradas (construcoes borda+agua borda+montanha+3 areas expansao), " +
                      "miolo x[-18,16] y[-15,16] totalmente aravel, interior da estufa aravel.");
        }

        // ── Helpers de conversao world -> tile ──────────────────────────────────────────────────

        private static int WorldToTileX(FarmTileGrid grid, float worldX)
        {
            return grid.WorldToTile(worldX, WorldOriginY).x;
        }

        private static int WorldToTileY(FarmTileGrid grid, float worldY)
        {
            return grid.WorldToTile(WorldOriginX, worldY).y;
        }

        private static void RegisterBlockedWorldRect(FarmNonArableZones zones, FarmTileGrid grid,
            float worldX, float worldY, float worldWidth, float worldHeight)
        {
            int tileX = WorldToTileX(grid, worldX);
            int tileY = WorldToTileY(grid, worldY);
            var tileMin = grid.TileToWorldCenter(tileX, tileY) - Vector2.one * (grid.TileSizeUnits * 0.5f);
            int tileW = Mathf.CeilToInt((worldX + worldWidth - tileMin.x) / grid.TileSizeUnits);
            int tileH = Mathf.CeilToInt((worldY + worldHeight - tileMin.y) / grid.TileSizeUnits);
            zones.RegisterBlockedRect(tileX, tileY, tileW, tileH);
        }

        private static void RegisterBlockedSolidRect(FarmNonArableZones zones, FarmTileGrid grid, FarmSolidRect body)
        {
            var bounds = body.Bounds;
            var min = grid.WorldToTile(bounds.xMin, bounds.yMin);
            var max = grid.WorldToTile(bounds.xMax - 0.0001f, bounds.yMax - 0.0001f);
            zones.RegisterBlockedRect(min.x, min.y, max.x - min.x + 1, max.y - min.y + 1);
        }

        private static void RegisterBlockedPolygon(FarmNonArableZones zones, FarmTileGrid grid, string footprintId)
        {
            if (!FarmSceneSpatialContract.TryGet(footprintId, out var footprint)) return;
            for (var y = FarmLevel1LayoutContract.GridOriginTileY; y < FarmLevel1LayoutContract.GridOriginTileY + HeightTiles; y++)
            for (var x = FarmLevel1LayoutContract.GridOriginTileX; x < FarmLevel1LayoutContract.GridOriginTileX + WidthTiles; x++)
                if (FarmSceneNavigationRaster.Contains(footprint, grid.TileToWorldCenter(x, y)))
                    zones.RegisterBlockedTile(x, y);
        }

        private static void RegisterBlockedFootprint(FarmNonArableZones zones, FarmTileGrid grid, string footprintId)
        {
            var bounds = GetFootprintBounds(footprintId);
            RegisterBlockedWorldRect(zones, grid, bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y);
        }

        private static Bounds GetFootprintBounds(string footprintId)
        {
            if (!FarmSceneSpatialContract.TryGet(footprintId, out var footprint))
            {
                throw new System.InvalidOperationException("Farm spatial footprint is missing: " + footprintId);
            }

            return footprint.Bounds;
        }

        private static void RegisterGreenhouseWorldRect(FarmNonArableZones zones, FarmTileGrid grid,
            float worldX, float worldY, float worldWidth, float worldHeight)
        {
            int tileX = WorldToTileX(grid, worldX);
            int tileY = WorldToTileY(grid, worldY);
            var tileMin = grid.TileToWorldCenter(tileX, tileY) - Vector2.one * (grid.TileSizeUnits * 0.5f);
            // Unlike a blocking mask, the greenhouse exception must not spill outside its walls.
            if (tileMin.x < worldX) { tileX++; tileMin.x += grid.TileSizeUnits; }
            if (tileMin.y < worldY) { tileY++; tileMin.y += grid.TileSizeUnits; }
            int tileW = Mathf.Max(0, Mathf.FloorToInt((worldX + worldWidth - tileMin.x) / grid.TileSizeUnits));
            int tileH = Mathf.Max(0, Mathf.FloorToInt((worldY + worldHeight - tileMin.y) / grid.TileSizeUnits));
            zones.RegisterGreenhouseRect(tileX, tileY, tileW, tileH);
        }

    }
}
