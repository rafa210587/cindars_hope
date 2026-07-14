using System;
using System.Collections.Generic;
using CindarsHope.Cave.Art;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Traps;
using CindarsHope.Combat;
using CindarsHope.DebugTools;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// Responsável por materializar armadilhas determinísticas (CA-1..CA-5) de um CaveGeneratedLevel.
    /// Estado restaurado do snapshot (revisita não rearma Triggered/Disarmed — ADR-0005).
    /// SpawnTrapEnemyById é recebido como delegate do adapter para evitar dependência cíclica.
    /// </summary>
    internal sealed class CaveTrapMaterializer
    {
        private readonly SpriteRenderer _trapTilePrefab;
        private readonly CaveRunManager _caveRunManager;
        private readonly Transform _playerTransform;
        // spec_cave_biome_art_profiles_runtime (CV01): resolver opcional; null = comportamento atual.
        private readonly CaveBiomeArtResolver _biomeArtResolver;

        /// <summary>
        /// Inicializa o materializer de armadilhas.
        /// </summary>
        internal CaveTrapMaterializer(
            SpriteRenderer trapTilePrefab,
            CaveRunManager caveRunManager,
            Transform playerTransform,
            CaveBiomeArtResolver biomeArtResolver = null)
        {
            _trapTilePrefab = trapTilePrefab;
            _caveRunManager = caveRunManager;
            _playerTransform = playerTransform;
            _biomeArtResolver = biomeArtResolver;
        }

        /// <summary>
        /// Materializa as armadilhas do nível. Estado é lido de trapStates (snapshot + mutações desta sessão).
        /// spawnTrapEnemyById é o delegate do adapter que chama o enemy materializer para spawnar 1 inimigo.
        /// registerTrapStateCallback persiste mudanças de estado no adapter.
        /// </summary>
        internal void MaterializeTraps(
            CaveGeneratedLevel level,
            Transform generatedRuntimeRoot,
            string worldSeed,
            string runSeed,
            Vector2Int lastPlayerSpawnGrid,
            Dictionary<string, CaveTrapSnapshotEntry> trapStates,
            IReadOnlyList<CaveTrapSnapshotEntry> snapshotTrapStates,
            List<GameObject> materializedObjects,
            out CaveTrapPlan lastTrapPlan,
            Func<string, bool> spawnTrapEnemyById,
            Action<string, TrapState> registerTrapStateCallback)
        {
            lastTrapPlan = CaveTrapPlanner.BuildPlan(level, worldSeed, runSeed, lastPlayerSpawnGrid);
            if (lastTrapPlan == null || lastTrapPlan.Traps.Count == 0)
            {
                return;
            }

            var trapParent = new GameObject("GeneratedTraps");
            trapParent.transform.SetParent(generatedRuntimeRoot);
            trapParent.transform.localPosition = Vector3.zero;
            materializedObjects.Add(trapParent);

            var detectionRuntime = trapParent.AddComponent<TrapDetectionRuntime>();
            detectionRuntime.Configure(_playerTransform);

            var runSeedSafe = runSeed ?? string.Empty;
            var caveLevel = level.CaveLevel;

            foreach (var trap in lastTrapPlan.Traps)
            {
                if (trap == null)
                {
                    continue;
                }

                var initialState = ResolveInitialTrapState(trap, trapStates);

                if (initialState == TrapState.Triggered || initialState == TrapState.Disarmed)
                {
                    RecordTrapState(trap, initialState, trapStates);
                    continue;
                }

                var worldPos = CaveTileMaterializer.GridToWorld(trap.Cell, level);
                var def = TrapDefinition.Get(trap.TrapId);
                var isFalseChest = def != null && def.Category == TrapEffectCategory.SpawnEnemy;

                SpriteRenderer spriteRenderer;
                GameObject trapGO;
                if (_trapTilePrefab != null)
                {
                    spriteRenderer = UnityEngine.Object.Instantiate(_trapTilePrefab, worldPos, Quaternion.identity, trapParent.transform);
                    trapGO = spriteRenderer.gameObject;
                }
                else
                {
                    trapGO = new GameObject(trap.TrapInstanceId);
                    trapGO.transform.SetParent(trapParent.transform);
                    trapGO.transform.position = worldPos;
                    spriteRenderer = trapGO.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = CaveTileMaterializer.GetBuiltinSprite();
                }

                trapGO.name = trap.TrapInstanceId;
                spriteRenderer.sortingOrder = 0;
                spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
                spriteRenderer.sortingLayerName = CaveWorldSortingLayers.World;

                var trigger = trapGO.GetComponent<BoxCollider2D>();
                if (trigger == null)
                {
                    trigger = trapGO.AddComponent<BoxCollider2D>();
                }
                trigger.size = Vector2.one;
                trigger.isTrigger = true;

                RecordTrapState(trap, initialState, trapStates);

                // spec_cave_biome_art_profiles_runtime (CV01): trapBandId é a banda de GAMEPLAY do
                // trap (dano por tier, nunca afetada pelo toggle dev); artBandId é só para lookup de
                // sprite e respeita CaveBiomeArtDebug.ForcedBandId (dev-only, arte apenas).
                var trapBandId = trap.Band > 0 ? Mathf.Clamp(trap.Band, 1, 7) : Runtime.CaveBandScaling.BandForLevel(caveLevel);
                var artBandId = CaveBiomeArtDebug.ResolveBandForArt(trapBandId);

                if (isFalseChest)
                {
                    Sprite falseChestClosed = null;
                    Sprite falseChestRevealed = null;
                    if (_biomeArtResolver != null)
                    {
                        _biomeArtResolver.TryGetChestSprite(artBandId, CaveChestVisualState.Closed, out falseChestClosed);
                        _biomeArtResolver.TryGetChestSprite(artBandId, CaveChestVisualState.FalseChestRevealed, out falseChestRevealed);
                    }

                    var falseChest = trapGO.AddComponent<FalseChestTrap>();
                    falseChest.Configure(
                        trap,
                        caveLevel,
                        initialState,
                        spriteRenderer,
                        spawnTrapEnemyById,
                        registerTrapStateCallback,
                        falseChestClosed,
                        falseChestRevealed);
                    detectionRuntime.Register(falseChest);
                }
                else
                {
                    // trapId do profile é a chave canônica ("trap_spike_floor"), não o enum.ToString().
                    Sprite trapSprite = null;
                    var trapKey = def != null ? def.TrapKey : null;
                    _biomeArtResolver?.TryGetTrapSprite(artBandId, trapKey, out trapSprite);

                    var behaviour = trapGO.AddComponent<TrapBehaviour>();
                    behaviour.Configure(
                        trap,
                        runSeedSafe,
                        caveLevel,
                        initialState,
                        spriteRenderer,
                        registerTrapStateCallback,
                        trapSprite);
                    detectionRuntime.Register(behaviour);
                }

                materializedObjects.Add(trapGO);
            }

            CombatLog.Log(
                $"CaveTrapMaterializer: materialized {lastTrapPlan.Traps.Count} trap(s) for level {caveLevel}.",
                null);
        }

        private static TrapState ResolveInitialTrapState(
            CaveTrapPlacement trap,
            Dictionary<string, CaveTrapSnapshotEntry> trapStates)
        {
            if (trap == null || string.IsNullOrWhiteSpace(trap.TrapInstanceId))
            {
                return TrapState.Armed;
            }

            if (trapStates.TryGetValue(trap.TrapInstanceId, out var entry) && entry != null)
            {
                var state = (TrapState)entry.State;
                return state == TrapState.Telegraphing ? TrapState.Armed : state;
            }

            return TrapState.Armed;
        }

        private static void RecordTrapState(
            CaveTrapPlacement trap,
            TrapState state,
            Dictionary<string, CaveTrapSnapshotEntry> trapStates)
        {
            if (trap == null || string.IsNullOrWhiteSpace(trap.TrapInstanceId))
            {
                return;
            }

            var def = TrapDefinition.Get(trap.TrapId);
            var trapKey = def != null ? def.TrapKey : trap.TrapId.ToString().ToLowerInvariant();
            trapStates[trap.TrapInstanceId] = new CaveTrapSnapshotEntry
            {
                TrapInstanceId = trap.TrapInstanceId,
                TrapKey = trapKey,
                Cell = trap.Cell,
                State = (int)state
            };
        }
    }
}
