using System;
using System.Collections.Generic;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Traps;
using CindarsHope.Combat;
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

        /// <summary>
        /// Inicializa o materializer de armadilhas.
        /// </summary>
        internal CaveTrapMaterializer(
            SpriteRenderer trapTilePrefab,
            CaveRunManager caveRunManager,
            Transform playerTransform)
        {
            _trapTilePrefab = trapTilePrefab;
            _caveRunManager = caveRunManager;
            _playerTransform = playerTransform;
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
                spriteRenderer.sortingOrder = 2;

                var trigger = trapGO.GetComponent<BoxCollider2D>();
                if (trigger == null)
                {
                    trigger = trapGO.AddComponent<BoxCollider2D>();
                }
                trigger.size = Vector2.one;
                trigger.isTrigger = true;

                RecordTrapState(trap, initialState, trapStates);

                if (isFalseChest)
                {
                    var falseChest = trapGO.AddComponent<FalseChestTrap>();
                    falseChest.Configure(
                        trap,
                        caveLevel,
                        initialState,
                        spriteRenderer,
                        spawnTrapEnemyById,
                        registerTrapStateCallback);
                    detectionRuntime.Register(falseChest);
                }
                else
                {
                    var behaviour = trapGO.AddComponent<TrapBehaviour>();
                    behaviour.Configure(
                        trap,
                        runSeedSafe,
                        caveLevel,
                        initialState,
                        spriteRenderer,
                        registerTrapStateCallback);
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
