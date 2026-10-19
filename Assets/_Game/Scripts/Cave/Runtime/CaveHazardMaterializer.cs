using System;
using System.Collections.Generic;
using CindarsHope.Cave.Art;
using CindarsHope.Cave.Generation;
using CindarsHope.Combat;
using CindarsHope.DebugTools;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// Responsável por materializar tiles de hazard e a sala de tesouro (baú + guardiões realocados)
    /// de um CaveGeneratedLevel. Roda após inimigos para poder mover guardiões já materializados.
    /// </summary>
    internal sealed class CaveHazardMaterializer
    {
        private readonly SpriteRenderer _hazardTilePrefab;
        private readonly InventoryManager _inventoryManager;
        private readonly CaveRunManager _caveRunManager;
        // spec_cave_biome_art_profiles_runtime (CV01): resolver opcional; null = comportamento atual.
        private readonly CaveBiomeArtResolver _biomeArtResolver;

        /// <summary>
        /// Inicializa o materializer de hazards e tesouro.
        /// </summary>
        internal CaveHazardMaterializer(
            SpriteRenderer hazardTilePrefab,
            InventoryManager inventoryManager,
            CaveRunManager caveRunManager,
            CaveBiomeArtResolver biomeArtResolver = null)
        {
            _hazardTilePrefab = hazardTilePrefab;
            _inventoryManager = inventoryManager;
            _caveRunManager = caveRunManager;
            _biomeArtResolver = biomeArtResolver;
        }

        /// <summary>
        /// Materializa os tiles de hazard do plano, adicionando colisão trigger e CaveHazardTile.
        /// </summary>
        internal void MaterializeHazards(
            CaveGeneratedLevel level,
            Transform generatedRuntimeRoot,
            CaveHazardPlan plan,
            List<GameObject> materializedObjects)
        {
            if (plan == null || plan.Hazards.Count == 0)
            {
                return;
            }

            var hazardParent = new GameObject("GeneratedHazards");
            hazardParent.transform.SetParent(generatedRuntimeRoot);
            hazardParent.transform.localPosition = Vector3.zero;
            materializedObjects.Add(hazardParent);

            // spec_cave_biome_art_profiles_runtime (CV01): banda derivada do nível (mesma fonte que
            // traps/inimigos); resolver ausente ou sem profile = fallback integral (Sprite fica null).
            var bandId = CaveBiomeArtDebug.ResolveBandForArt(Runtime.CaveBandScaling.BandForLevel(level.CaveLevel));

            foreach (var hazard in plan.Hazards)
            {
                var worldPos = CaveTileMaterializer.GridToWorld(hazard.GridPosition, level);
                SpriteRenderer spriteRenderer;
                GameObject hazardGO;

                if (_hazardTilePrefab != null)
                {
                    spriteRenderer = UnityEngine.Object.Instantiate(_hazardTilePrefab, worldPos, Quaternion.identity, hazardParent.transform);
                    hazardGO = spriteRenderer.gameObject;
                }
                else
                {
                    hazardGO = new GameObject(hazard.HazardId);
                    hazardGO.transform.SetParent(hazardParent.transform);
                    hazardGO.transform.position = worldPos;
                    spriteRenderer = hazardGO.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = CaveTileMaterializer.GetBuiltinSprite();
                }

                hazardGO.name = hazard.HazardId;
                spriteRenderer.sortingOrder = 0;
                spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
                spriteRenderer.sortingLayerName = CaveWorldSortingLayers.World;

                // spec_codex_13: layer de gameplay do hazard.
                CindarsHope.Core.Physics.GameplayLayerNames.TryAssignRuntimeLayer(
                    hazardGO, CindarsHope.Core.Physics.GameplayLayerNames.Hazard);

                var trigger = hazardGO.GetComponent<BoxCollider2D>();
                if (trigger == null)
                {
                    trigger = hazardGO.AddComponent<BoxCollider2D>();
                }
                trigger.size = Vector2.one;
                trigger.isTrigger = true;

                // spec_cave_biome_art_profiles_runtime (CV01): fallback-first — sprite do bioma só
                // sobrescreve se o resolver encontrar um; caso contrário CaveHazardTile aplica o
                // placeholder de cor EXATAMENTE como hoje.
                var hasCustomSprite = false;
                if (_biomeArtResolver != null && _biomeArtResolver.TryGetHazardSprite(bandId, hazard.Kind, out var hazardSprite))
                {
                    spriteRenderer.sprite = hazardSprite;
                    spriteRenderer.color = Color.white;
                    hasCustomSprite = true;
                }

                var hazardTile = hazardGO.GetComponent<CaveHazardTile>();
                if (hazardTile == null)
                {
                    hazardTile = hazardGO.AddComponent<CaveHazardTile>();
                }
                hazardTile.Configure(hazard.HazardId, hazard.Kind, spriteRenderer, hasCustomSprite);
                TemporaryRevealTargetBehaviour.Attach(hazardGO, hazard.HazardId,
                    CindarsHope.Foundation.TemporaryRevealKind.Hazard, isActive: () => hazardTile.isActiveAndEnabled);

                materializedObjects.Add(hazardGO);
            }

            CombatLog.Log($"CaveHazardMaterializer: materialized {plan.Hazards.Count} hazard(s) for level {level.CaveLevel}.", null);
        }

        /// <summary>
        /// Materializa a sala de tesouro: realoca guardiões existentes e cria o baú interativo.
        /// </summary>
        internal void MaterializeTreasureRoom(
            CaveGeneratedLevel level,
            Transform generatedRuntimeRoot,
            CaveHazardPlan plan,
            CaveEnemySpawnPlan lastEnemySpawnPlan,
            HashSet<string> openedChestIds,
            List<GameObject> materializedObjects,
            Action<string> registerOpenedChest)
        {
            if (plan == null || !plan.HasTreasureRoom)
            {
                return;
            }

            var treasure = plan.TreasureRoom;

            RelocateGuardians(treasure, level, lastEnemySpawnPlan, materializedObjects);

            var treasureParent = new GameObject("GeneratedTreasure");
            treasureParent.transform.SetParent(generatedRuntimeRoot);
            treasureParent.transform.localPosition = Vector3.zero;
            materializedObjects.Add(treasureParent);

            var worldPos = CaveTileMaterializer.GridToWorld(treasure.ChestGridPosition, level);
            var chestGO = new GameObject(treasure.ChestId);
            chestGO.transform.SetParent(treasureParent.transform);
            chestGO.transform.position = worldPos;

            var spriteRenderer = chestGO.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CaveTileMaterializer.GetBuiltinSprite();
            spriteRenderer.sortingOrder = 0;
            spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
            spriteRenderer.sortingLayerName = CaveWorldSortingLayers.World;

            var collider = chestGO.AddComponent<CircleCollider2D>();
            collider.radius = 0.45f;
            collider.isTrigger = true;

            // spec_cave_biome_art_profiles_runtime (CV01): sprites opcionais do bioma; ausentes = null,
            // TreasureChestInteractable mantém o placeholder de cor atual (fallback-first).
            var chestBandId = CaveBiomeArtDebug.ResolveBandForArt(Runtime.CaveBandScaling.BandForLevel(level.CaveLevel));
            Sprite closedSprite = null;
            Sprite openSprite = null;
            if (_biomeArtResolver != null)
            {
                _biomeArtResolver.TryGetChestSprite(chestBandId, CaveChestVisualState.Closed, out closedSprite);
                _biomeArtResolver.TryGetChestSprite(chestBandId, CaveChestVisualState.Open, out openSprite);
            }

            var alreadyOpened = openedChestIds.Contains(treasure.ChestId);
            var chest = chestGO.AddComponent<TreasureChestInteractable>();
            chest.Configure(
                treasure.ChestId,
                level.CaveLevel,
                treasure.LootSeed,
                alreadyOpened,
                _inventoryManager,
                spriteRenderer,
                registerOpenedChest,
                closedSprite,
                openSprite);
            TemporaryRevealTargetBehaviour.Attach(chestGO, treasure.ChestId,
                CindarsHope.Foundation.TemporaryRevealKind.Interactable,
                isExhausted: () => chest.IsOpened);

            materializedObjects.Add(chestGO);
            CombatLog.Log(
                $"CaveHazardMaterializer: materialized treasure chest '{treasure.ChestId}' (opened={alreadyOpened}) for level {level.CaveLevel}.",
                null);
        }

        private void RelocateGuardians(
            CaveTreasureRoomPlacement treasure,
            CaveGeneratedLevel level,
            CaveEnemySpawnPlan lastEnemySpawnPlan,
            List<GameObject> materializedObjects)
        {
            if (treasure.GuardianGridPositions == null || treasure.GuardianGridPositions.Count == 0)
            {
                return;
            }

            if (lastEnemySpawnPlan == null || lastEnemySpawnPlan.Entries == null || lastEnemySpawnPlan.Entries.Count == 0)
            {
                return;
            }

            var guardianEntries = new System.Collections.Generic.List<CaveEnemySpawnPlanEntry>(lastEnemySpawnPlan.Entries);
            guardianEntries.Sort((a, b) => a.SpawnIndex.CompareTo(b.SpawnIndex));

            var count = Mathf.Min(treasure.GuardianGridPositions.Count, guardianEntries.Count);
            for (var i = 0; i < count; i++)
            {
                var entry = guardianEntries[i];
                var targetGrid = treasure.GuardianGridPositions[i];
                var targetWorld = CaveTileMaterializer.GridToWorld(targetGrid, level);

                entry.GridPosition = targetGrid;
                entry.WorldPosition = targetWorld;

                foreach (var obj in materializedObjects)
                {
                    if (obj != null && obj.name == entry.EnemyInstanceId)
                    {
                        obj.transform.position = targetWorld;
                        break;
                    }
                }
            }
        }
    }
}
