using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Cave.Art;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Ecosystem;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Resources;
using CindarsHope.Core.Data;
using CindarsHope.DebugTools;
using CindarsHope.Equipment;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// Responsável por materializar elementos ambientais de um CaveGeneratedLevel:
    /// decor (bloqueante/não-bloqueante), tiles de água e nós mineráveis.
    /// Geração fresh chama o planner determinístico; revisita restaura do snapshot (stable-run / ADR-0005).
    /// </summary>
    internal sealed class CaveEnvironmentElementMaterializer
    {
        private readonly CaveEnvironmentElementDatabaseSO _environmentElementDatabase;
        private readonly CaveEcosystemBalanceSO _ecosystemBalance;
        private readonly SpriteRenderer _decorElementPrefab;
        private readonly SpriteRenderer _waterTilePrefab;
        private readonly ResourceNode _resourceNodePrefab;
        private readonly ResourceNodeDatabaseSO _resourceNodeDatabase;
        private readonly InventoryManager _inventoryManager;
        private readonly EquipmentManager _equipmentManager;
        private readonly CaveRunManager _caveRunManager;
        private readonly CaveBiomeArtResolver _biomeArtResolver;

        /// <summary>
        /// Inicializa o materializer de elementos ambientais com todas as dependências.
        /// </summary>
        internal CaveEnvironmentElementMaterializer(
            CaveEnvironmentElementDatabaseSO environmentElementDatabase,
            CaveEcosystemBalanceSO ecosystemBalance,
            SpriteRenderer decorElementPrefab,
            SpriteRenderer waterTilePrefab,
            ResourceNode resourceNodePrefab,
            ResourceNodeDatabaseSO resourceNodeDatabase,
            InventoryManager inventoryManager,
            EquipmentManager equipmentManager,
            CaveRunManager caveRunManager,
            CaveBiomeArtResolver biomeArtResolver)
        {
            _environmentElementDatabase = environmentElementDatabase;
            _ecosystemBalance = ecosystemBalance;
            _decorElementPrefab = decorElementPrefab;
            _waterTilePrefab = waterTilePrefab;
            _resourceNodePrefab = resourceNodePrefab;
            _resourceNodeDatabase = resourceNodeDatabase;
            _inventoryManager = inventoryManager;
            _equipmentManager = equipmentManager;
            _caveRunManager = caveRunManager;
            _biomeArtResolver = biomeArtResolver;
        }

        /// <summary>
        /// Materializa os elementos ambientais do nível. Restaura do snapshot na revisita;
        /// planeja deterministicamente na geração fresh. Preenche lastEnvironmentElements,
        /// lastHasWater e lastResourceNodeSnapshots.
        /// </summary>
        internal void MaterializeEnvironmentElements(
            CaveGeneratedLevel level,
            Transform generatedRuntimeRoot,
            Vector2Int lastPlayerSpawnGrid,
            IReadOnlyList<SerializedEnvironmentElement> snapshotEnvironmentElements,
            List<GameObject> materializedObjects,
            List<SerializedEnvironmentElement> lastEnvironmentElements,
            out bool lastHasWater,
            List<CaveResourceNodeSnapshotEntry> lastResourceNodeSnapshots,
            CaveRuntimeMaterializationResult result)
        {
            lastHasWater = false;

            var elements = BuildOrRestoreEnvironmentElements(level, lastPlayerSpawnGrid, snapshotEnvironmentElements, ref lastHasWater);
            if (elements == null || elements.Count == 0)
            {
                return;
            }

            var parent = new GameObject("GeneratedEnvironmentElements");
            parent.transform.SetParent(generatedRuntimeRoot);
            parent.transform.localPosition = Vector3.zero;
            materializedObjects.Add(parent);

            foreach (var element in elements)
            {
                if (element == null || string.IsNullOrWhiteSpace(element.ElementId))
                {
                    continue;
                }

                var gridPos = new Vector2Int(element.GridX, element.GridY);
                switch ((CaveEnvironmentElementKind)element.Kind)
                {
                    case CaveEnvironmentElementKind.WaterTile:
                        lastHasWater = true;
                        MaterializeWaterTile(level, parent.transform, element, gridPos, materializedObjects);
                        break;
                    case CaveEnvironmentElementKind.MineableNode:
                        MaterializeMineableElement(level, parent.transform, element, gridPos, materializedObjects, lastResourceNodeSnapshots, result);
                        break;
                    case CaveEnvironmentElementKind.DecorBlocking:
                        MaterializeDecorElement(level, parent.transform, element, gridPos, blocking: true, materializedObjects);
                        break;
                    default:
                        MaterializeDecorElement(level, parent.transform, element, gridPos, blocking: false, materializedObjects);
                        break;
                }

                lastEnvironmentElements.Add(element);
            }

            CombatLog.Log(
                $"CaveEnvironmentElementMaterializer: materialized {lastEnvironmentElements.Count} environment element(s) for level {level.CaveLevel} (hasWater={lastHasWater}).",
                null);
        }

        private List<SerializedEnvironmentElement> BuildOrRestoreEnvironmentElements(
            CaveGeneratedLevel level,
            Vector2Int lastPlayerSpawnGrid,
            IReadOnlyList<SerializedEnvironmentElement> snapshotEnvironmentElements,
            ref bool lastHasWater)
        {
            if (snapshotEnvironmentElements != null && snapshotEnvironmentElements.Count > 0)
            {
                var restored = new List<SerializedEnvironmentElement>(snapshotEnvironmentElements.Count);
                foreach (var element in snapshotEnvironmentElements)
                {
                    if (element != null && !string.IsNullOrWhiteSpace(element.ElementId))
                    {
                        restored.Add(new SerializedEnvironmentElement
                        {
                            ElementId = element.ElementId,
                            Kind = element.Kind,
                            GridX = element.GridX,
                            GridY = element.GridY,
                            IsMineable = element.IsMineable,
                            MineNodeDataId = element.MineNodeDataId ?? string.Empty,
                            IsDepleted = element.IsDepleted
                        });
                    }
                }
                return restored;
            }

            var profile = ResolveEnvironmentProfile(level);
            if (profile == null)
            {
                Debug.LogWarning(
                    "CaveEnvironmentElementMaterializer.MaterializeEnvironmentElements: no CaveEnvironmentElementProfileSO resolved. " +
                    $"AffectedLevel={level.CaveLevel}, BiomeId='{level.BiomeId}'. " +
                    "Environment elements skipped (DEFERRED_UNITY: profiles/database assets — slice 6).");
                return null;
            }

            var worldSeed = _caveRunManager != null ? _caveRunManager.CaveWorldSeed : string.Empty;
            var runSeed = _caveRunManager != null ? _caveRunManager.CaveRunSeed : string.Empty;
            var plan = CaveEnvironmentElementPlanner.Build(
                level,
                profile,
                _ecosystemBalance,
                worldSeed,
                runSeed,
                level.CaveLevel,
                lastPlayerSpawnGrid);

            var result2 = new List<SerializedEnvironmentElement>(plan.Placements.Count);
            foreach (var placement in plan.Placements)
            {
                result2.Add(new SerializedEnvironmentElement
                {
                    ElementId = placement.ElementId,
                    Kind = (int)placement.Kind,
                    GridX = placement.GridPosition.x,
                    GridY = placement.GridPosition.y,
                    IsMineable = placement.IsMineable,
                    MineNodeDataId = placement.MineNodeDataId ?? string.Empty,
                    IsDepleted = false
                });
            }

            if (plan.HasWater)
            {
                lastHasWater = true;
            }

            return result2;
        }

        private CaveEnvironmentElementProfileSO ResolveEnvironmentProfile(CaveGeneratedLevel level)
        {
            if (_environmentElementDatabase == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(level.BiomeId)
                && _environmentElementDatabase.TryGetByBiome(level.BiomeId, out var byBiome)
                && byBiome != null)
            {
                return byBiome;
            }

            var band = Mathf.Clamp(CaveBandScaling.BandForLevel(level.CaveLevel) - 1, 0, CaveEcosystemBalanceSO.BandCount - 1);
            return _environmentElementDatabase.TryGetByBand(band, out var byBand) ? byBand : null;
        }

        private void MaterializeDecorElement(
            CaveGeneratedLevel level,
            Transform parent,
            SerializedEnvironmentElement element,
            Vector2Int gridPos,
            bool blocking,
            List<GameObject> materializedObjects)
        {
            var worldPos = CaveTileMaterializer.GridToWorld(gridPos, level);
            SpriteRenderer spriteRenderer;
            GameObject elementGO;

            // spec_cave_decor_placement_runtime (CV02): tenta o sprite real do pool do bioma ANTES
            // do fallback prefab/builtin. Pick determinístico por (banda, Kind, hash estável da
            // posição) — mesmo hash usado pelo CaveTileMaterializer/CaveBiomeArtResolver (FNV-1a via
            // CaveLayoutStableHash), nunca Random/GetHashCode (cave-stable-run). Falha (resolver
            // nulo, sem profile, pool vazio) cai no comportamento atual, byte-for-byte.
            if (TryResolveDecorSprite(level, gridPos, blocking, out var resolvedSprite))
            {
                elementGO = new GameObject(element.ElementId);
                elementGO.transform.SetParent(parent);
                elementGO.transform.position = worldPos;
                spriteRenderer = elementGO.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = resolvedSprite;
            }
            else if (_decorElementPrefab != null)
            {
                spriteRenderer = Object.Instantiate(_decorElementPrefab, worldPos, Quaternion.identity, parent);
                elementGO = spriteRenderer.gameObject;
            }
            else
            {
                LogElementPrefabMissing(level, element, nameof(_decorElementPrefab));
                elementGO = new GameObject(element.ElementId);
                elementGO.transform.SetParent(parent);
                elementGO.transform.position = worldPos;
                spriteRenderer = elementGO.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = CaveTileMaterializer.GetBuiltinSprite();
                spriteRenderer.color = blocking ? new Color(0.45f, 0.4f, 0.35f) : new Color(0.55f, 0.55f, 0.5f, 0.85f);
            }

            elementGO.name = element.ElementId;

            // spec_cave_decor_composition_runtime (CV03), critério anti-regressão: decor de teto
            // (CeilingHang) ocupa uma célula de PAREDE (WallTile) — precisa renderizar ACIMA da parede
            // (sortingOrder maior) e NUNCA recebe collider, mesmo que o Kind seja DecorBlocking (guard
            // defensivo; teto nunca deveria bloquear passagem).
            var isCeilingHang = CaveDecorContextClassifier.Classify(gridPos, level) == CaveDecorPlacementContext.CeilingHang;
            spriteRenderer.sortingOrder = isCeilingHang ? 1 : 0;
            spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
            spriteRenderer.sortingLayerName = CaveWorldSortingLayers.World;

            if (blocking && !isCeilingHang)
            {
                var collider = elementGO.GetComponent<BoxCollider2D>();
                if (collider == null)
                {
                    collider = elementGO.AddComponent<BoxCollider2D>();
                }
                collider.size = Vector2.one;
            }

            materializedObjects.Add(elementGO);
        }

        /// <summary>spec_cave_decor_composition_runtime (CV03): resolve o sprite real de decor do pool
        /// do bioma POR CONTEXTO (teto/wall-hug/chão) para a posição do elemento. O contexto é derivável
        /// da posição (não persistido no save) — recomputado aqui via CaveDecorContextClassifier a
        /// partir do CaveGeneratedLevel atual, tanto em geração fresh quanto em revisita (o level é
        /// sempre reconstruído/restaurado antes da materialização). Retorna false (sem mutar nada) se o
        /// resolver não foi injetado, se não houver profile para a banda do nível, ou se o pool do
        /// contexto estiver vazio — o chamador cai no fallback atual.</summary>
        private bool TryResolveDecorSprite(CaveGeneratedLevel level, Vector2Int gridPos, bool blocking, out Sprite sprite)
        {
            sprite = null;
            if (_biomeArtResolver == null)
            {
                return false;
            }

            // Fix pós-Play-Mode 2026-07-04: o decor (código fable_78, banda 0-indexed do ecossistema)
            // consultava BandForLevel-1, mas o CaveBiomeArtResolver (CV01) é keyed pelo BandId do profile
            // = BandForLevel (1-indexed), o mesmo que o CaveTileMaterializer usa para o chão. O -1 fazia o
            // decor pedir uma banda inexistente → pool vazio → sprite genérico. Alinhar com o chão (e
            // respeitar o toggle de banda forçada de debug, para o decor casar com o terreno).
            var band = CaveBiomeArtDebug.ResolveBandForArt(CaveBandScaling.BandForLevel(level.CaveLevel));
            var worldSeed = _caveRunManager != null ? _caveRunManager.CaveWorldSeed : string.Empty;
            var runSeed = _caveRunManager != null ? _caveRunManager.CaveRunSeed : string.Empty;
            var stableHash = CaveBiomeArtResolver.ComputeCellHash(worldSeed, runSeed, level.CaveLevel, gridPos.x, gridPos.y);

            var context = CaveDecorContextClassifier.Classify(gridPos, level) ?? CaveDecorPlacementContext.FloorCluster;

            return _biomeArtResolver.TryGetDecorSprite(band, context, blocking, stableHash, out sprite);
        }

        private void MaterializeWaterTile(
            CaveGeneratedLevel level,
            Transform parent,
            SerializedEnvironmentElement element,
            Vector2Int gridPos,
            List<GameObject> materializedObjects)
        {
            var worldPos = CaveTileMaterializer.GridToWorld(gridPos, level);
            SpriteRenderer spriteRenderer;
            GameObject waterGO;

            if (_waterTilePrefab != null)
            {
                spriteRenderer = Object.Instantiate(_waterTilePrefab, worldPos, Quaternion.identity, parent);
                waterGO = spriteRenderer.gameObject;
            }
            else
            {
                LogElementPrefabMissing(level, element, nameof(_waterTilePrefab));
                waterGO = new GameObject(element.ElementId);
                waterGO.transform.SetParent(parent);
                waterGO.transform.position = worldPos;
                spriteRenderer = waterGO.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = CaveTileMaterializer.GetBuiltinSprite();
                spriteRenderer.color = new Color(0.2f, 0.45f, 0.8f, 0.7f);
            }

            waterGO.name = element.ElementId;
            spriteRenderer.sortingOrder = 0;
            spriteRenderer.sortingLayerName = CaveWorldSortingLayers.Ground;
            materializedObjects.Add(waterGO);
        }

        private void MaterializeMineableElement(
            CaveGeneratedLevel level,
            Transform parent,
            SerializedEnvironmentElement element,
            Vector2Int gridPos,
            List<GameObject> materializedObjects,
            List<CaveResourceNodeSnapshotEntry> lastResourceNodeSnapshots,
            CaveRuntimeMaterializationResult result)
        {
            if (element.IsDepleted)
            {
                return;
            }

            var nodeData = FindResourceNodeData(element.MineNodeDataId);
            if (nodeData == null)
            {
                Debug.LogWarning(
                    "CaveEnvironmentElementMaterializer.MaterializeEnvironmentElements: mineable element data not found. " +
                    $"ElementId='{element.ElementId}', MineNodeDataId='{element.MineNodeDataId}', AffectedLevel={level.CaveLevel}. " +
                    "Mineable element skipped (DEFERRED_UNITY: ore ResourceNodeDataSO assets — slice 6).");
                return;
            }

            var worldPos = CaveTileMaterializer.GridToWorld(gridPos, level);
            ResourceNode resourceNode;
            if (_resourceNodePrefab != null)
            {
                resourceNode = Object.Instantiate(_resourceNodePrefab, worldPos, Quaternion.identity, parent);
            }
            else
            {
                var nodeGO = new GameObject(element.ElementId);
                nodeGO.transform.SetParent(parent);
                nodeGO.transform.position = worldPos;
                resourceNode = nodeGO.AddComponent<ResourceNode>();
            }

            ConfigureResourceNodeWithData(resourceNode, nodeData, element.ElementId, gridPos);

            lastResourceNodeSnapshots.Add(new CaveResourceNodeSnapshotEntry
            {
                NodeInstanceId = element.ElementId,
                ResourceNodeId = nodeData.Id,
                GridPosition = gridPos,
                IsDepleted = _caveRunManager != null && _caveRunManager.IsNodeDepleted(element.ElementId)
            });

            materializedObjects.Add(resourceNode.gameObject);
            result.CreatedResourceNodes++;
        }

        private void ConfigureResourceNodeWithData(
            ResourceNode nodeInstance,
            ResourceNodeDataSO nodeData,
            string nodeInstanceId,
            Vector2Int spawnPosition)
        {
            if (nodeInstance == null || nodeData == null) return;

            var spriteRenderer = nodeInstance.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = nodeInstance.gameObject.AddComponent<SpriteRenderer>();
            }

            nodeInstance.gameObject.name = $"{nodeInstanceId}_{nodeData.Id}";
            spriteRenderer.sprite = CaveTileMaterializer.GetBuiltinSprite();
            spriteRenderer.color = new Color(0.8f, 0.6f, 0.4f);
            spriteRenderer.sortingOrder = 0;
            spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
            spriteRenderer.sortingLayerName = CaveWorldSortingLayers.World;

            var collider = nodeInstance.GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = nodeInstance.gameObject.AddComponent<CircleCollider2D>();
                collider.radius = 0.4f;
                collider.isTrigger = true;
            }

            nodeInstance.Configure(
                nodeInstanceId,
                nodeData,
                _inventoryManager,
                _equipmentManager,
                _caveRunManager,
                spriteRenderer);
        }

        private ResourceNodeDataSO FindResourceNodeData(string nodeDataId)
        {
            if (_resourceNodeDatabase == null || string.IsNullOrWhiteSpace(nodeDataId)) return null;
            foreach (var node in _resourceNodeDatabase.All)
            {
                if (node != null && node.Id == nodeDataId) return node;
            }
            return null;
        }

        private void LogElementPrefabMissing(
            CaveGeneratedLevel level,
            SerializedEnvironmentElement element,
            string fieldName)
        {
            CombatLog.Log(
                "CaveEnvironmentElementMaterializer.MaterializeEnvironmentElements: element prefab not assigned, using procedural placeholder. " +
                $"Field='{fieldName}', ElementId='{element.ElementId}', AffectedLevel={level.CaveLevel}. " +
                "DEFERRED_UNITY: element prefabs (slice 6).",
                null);
        }
    }
}
