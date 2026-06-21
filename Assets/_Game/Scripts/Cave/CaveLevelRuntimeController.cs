using CindarsHope.Cave.Data;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Resources;
using CindarsHope.Cave.Runtime;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Cave
{
    [DisallowMultipleComponent]
    public sealed class CaveLevelRuntimeController : MonoBehaviour
    {
        [SerializeField] private CaveRunManager _runManager;
        [SerializeField] private CaveRuntimeMaterializer _materializer;
        [SerializeField] private CaveEnemySpawner _enemySpawner;
        [SerializeField] private CaveBossSpawner _bossSpawner;
        [SerializeField] private CaveGenerationConfigSO _generationConfig;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private CaveSnapshotCacheManager _snapshotCacheManager;
        [SerializeField] private string _defaultBiomeId = "biome_cave_earth";
        [SerializeField] private bool _logGeneratedLayout = true;
        [SerializeField] private bool _materializeAfterGeneration = true;

        private readonly CaveProceduralGenerator _generator = new CaveProceduralGenerator();
        private readonly CaveEnemySpawnPlanService _spawnPlanService = new CaveEnemySpawnPlanService();
        private readonly CaveSnapshotService _snapshotService = new CaveSnapshotService();
        // fable_44: estado puro do gate de save em boss fight (set no spawn; clear em derrota/morte/saída).
        private readonly Cave.Runtime.CaveBossFightSaveGate _bossFightSaveGate = new Cave.Runtime.CaveBossFightSaveGate();
        private CaveSpawnAnchor _currentSpawnAnchor = CaveSpawnAnchor.Entrance;
        private CaveLevelEnemyPlan _currentEnemyPlan;

        public CaveGeneratedLevel CurrentGeneratedLevel { get; private set; }
        public CaveSpawnAnchor CurrentSpawnAnchor => _currentSpawnAnchor;
        public CaveLevelEnemyPlan CurrentEnemyPlan => _currentEnemyPlan;
        public int RoomCount => CurrentGeneratedLevel != null ? CurrentGeneratedLevel.Rooms.Count : 0;
        public int EnemyPointCount => CurrentGeneratedLevel != null ? CurrentGeneratedLevel.EnemySpawnPoints.Count : 0;
        public int ResourcePointCount => CurrentGeneratedLevel != null ? CurrentGeneratedLevel.ResourceSpawnPoints.Count : 0;
        public CaveRunManager RunManager => _runManager;
        public CaveRuntimeMaterializer Materializer => _materializer;

        // fable_44: lido pelo SaveManager (via canal existente do bootstrap — sem GameObject.Find) para
        // recusar o save manual durante uma boss fight ativa. True = boss fight em andamento neste nível.
        public bool IsBossFightActive => _bossFightSaveGate.IsBossFightActive;

        public void SetSpawnAnchorForNextGeneration(CaveSpawnAnchor anchor)
        {
            _currentSpawnAnchor = anchor;
            Debug.Log($"CaveLevelRuntimeController: spawn anchor set to {anchor} for next generation.", this);
        }

        public void RegisterEnemySpawnPlan(CaveLevelEnemyPlan plan)
        {
            _currentEnemyPlan = plan;
            if (plan != null)
            {
                CindarsHope.Combat.CombatLog.Log($"CaveLevelRuntimeController: enemy spawn plan registered for level {CurrentGeneratedLevel?.CaveLevel} with {plan.EnemyPlans.Count} entries.", this);
            }
        }

        private void Awake()
        {
            if (_runManager == null)
            {
                _runManager = GetComponent<CaveRunManager>();
            }

            if (_materializer == null)
            {
                _materializer = GetComponent<CaveRuntimeMaterializer>();
            }

            if (_enemySpawner == null)
            {
                _enemySpawner = GetComponent<CaveEnemySpawner>();
            }
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<CaveRuntimeMaterializationCompleteEvent>(OnMaterializationComplete);
            GameEventBus.Subscribe<DayStartedEvent>(OnDayStarted);
            GameEventBus.Subscribe<SceneTransitionStartedEvent>(OnSceneTransitionStarted);
            // fable_44: clear do gate de save em boss fight (derrota do boss / morte do player).
            GameEventBus.Subscribe<CaveBossDefeatedEvent>(OnBossDefeated);
            GameEventBus.Subscribe<CavePlayerDefeatedEvent>(OnPlayerDefeated);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<CaveRuntimeMaterializationCompleteEvent>(OnMaterializationComplete);
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            GameEventBus.Unsubscribe<SceneTransitionStartedEvent>(OnSceneTransitionStarted);
            GameEventBus.Unsubscribe<CaveBossDefeatedEvent>(OnBossDefeated);
            GameEventBus.Unsubscribe<CavePlayerDefeatedEvent>(OnPlayerDefeated);
        }

        private void OnSceneTransitionStarted(SceneTransitionStartedEvent evt)
        {
            // fable_44: sair do nível ou da caverna sempre encerra a boss fight (flag nunca fica órfão).
            _bossFightSaveGate.EndBossFight();
            DetermineSpawnAnchorFromTransition(evt.SourceSceneName, evt.TargetSceneName, evt.TargetSpawnId);
        }

        // fable_44: derrota do boss libera o save manual.
        private void OnBossDefeated(CaveBossDefeatedEvent _)
        {
            _bossFightSaveGate.EndBossFight();
        }

        // fable_44: morte do player (KO/defeat na caverna) encerra a boss fight e libera o save.
        private void OnPlayerDefeated(CavePlayerDefeatedEvent _)
        {
            _bossFightSaveGate.EndBossFight();
        }

        private void DetermineSpawnAnchorFromTransition(string sourceScene, string targetScene, string spawnId)
        {
            if (sourceScene == "FarmScene" && targetScene == "CaveScene")
            {
                _currentSpawnAnchor = CaveSpawnAnchor.Entrance;
                Debug.Log($"CaveLevelRuntimeController: spawn anchor set to Entrance (Farm -> Cave)", this);
            }
            else if (sourceScene == "CaveScene" && targetScene == "FarmScene")
            {
                _currentSpawnAnchor = CaveSpawnAnchor.BackExit;
                Debug.Log($"CaveLevelRuntimeController: spawn anchor set to BackExit (Cave -> Farm)", this);
            }
            else if (sourceScene == "CaveScene" && targetScene == "CaveScene")
            {
                if (spawnId == "cave_forward_exit")
                {
                    _currentSpawnAnchor = CaveSpawnAnchor.Entrance;
                    Debug.Log($"CaveLevelRuntimeController: spawn anchor set to Entrance (ForwardExit -> next level)", this);
                }
                else if (spawnId == "cave_back_exit")
                {
                    _currentSpawnAnchor = CaveSpawnAnchor.ForwardExit;
                    Debug.Log($"CaveLevelRuntimeController: spawn anchor set to ForwardExit (BackExit -> prev level)", this);
                }
            }
        }

        private void OnMaterializationComplete(CaveRuntimeMaterializationCompleteEvent e)
        {
            if (_materializer == null || _materializer.GeneratedRuntimeRoot == null)
            {
                return;
            }

            if (_bossSpawner != null)
            {
                _bossSpawner.SpawnBossForLevel(e.GeneratedLevel, _materializer.GeneratedRuntimeRoot, _playerTransform);

                // fable_44: a boss fight fica ativa (save manual bloqueado) somente se um boss REAL
                // spawnou neste nível. Sem gate, ou boss já derrotado → HasLiveBoss=false → save liberado.
                // Materialização ocorre em geração nova E em restore de snapshot; recomputar aqui mantém
                // o flag coerente em ambos os caminhos (nível sem boss reabre com save liberado).
                if (_bossSpawner.HasLiveBoss)
                {
                    _bossFightSaveGate.BeginBossFight(e.GeneratedLevel != null ? e.GeneratedLevel.CaveLevel : _runManager.CurrentCaveLevel);
                }
                else
                {
                    _bossFightSaveGate.EndBossFight();
                }
            }
            else
            {
                _bossFightSaveGate.EndBossFight();
            }
        }

        private void Start()
        {
            GenerateCurrentLevel();
        }

        private void Update()
        {
            if (SceneManager.GetActiveScene().name != "CaveScene")
            {
                return;
            }

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
            {
                RegenerateCurrentRunDebug();
            }
        }

        public void GenerateCurrentLevel()
        {
            EnsureRuntimeReferences();
            _runManager.InitializeIfNeeded();

            var caveLevel = _runManager.CurrentCaveLevel;
            VisitedLevelSnapshot visitedSnapshot = null;

            if (_snapshotCacheManager != null && _snapshotCacheManager.TryGetSnapshotFromCache(caveLevel, out var cachedSnapshot))
            {
                visitedSnapshot = cachedSnapshot;
                CindarsHope.Combat.CombatLog.Log($"CaveLevelRuntimeController: Using cached snapshot for level {caveLevel}.", this);
            }
            else if (_snapshotService.TryGetSnapshot(_runManager.State, _runManager.CaveRunSeed, caveLevel, out var stateSnapshot))
            {
                visitedSnapshot = stateSnapshot;
            }

            if (visitedSnapshot != null && visitedSnapshot.IsValid())
            {
                if (ValidateSnapshotIntegrity(visitedSnapshot))
                {
                    RestoreFromSnapshot(visitedSnapshot);
                    if (_snapshotCacheManager != null)
                    {
                        _snapshotCacheManager.CacheSnapshot(caveLevel, visitedSnapshot);
                    }
                    return;
                }
                else
                {
                    Debug.LogWarning($"CaveLevelRuntimeController: Snapshot failed integrity check for level {caveLevel}. Regenerating level.", this);
                }
            }

            // fable_09: perfil de layout por banda (estático em código) parametriza a geração
            // (tamanho/salas/corredor) de forma determinística por seed. Sem perfil, o gerador
            // mantém o comportamento anterior (rollback).
            var layoutProfile = CaveBiomeLayoutProfile.ForLevel(_runManager.CurrentCaveLevel);

            CurrentGeneratedLevel = _generator.Generate(
                _generationConfig,
                _runManager.CurrentCaveLevel,
                _runManager.CaveWorldSeed,
                _runManager.CaveRunSeed,
                _defaultBiomeId,
                layoutProfile);

            CurrentGeneratedLevel.ComputeLayoutHash();

            CindarsHope.Combat.CombatLog.Log(
                $"CaveLevelRuntimeController: Cave level generated.\n" +
                $"  Level: {_runManager.CurrentCaveLevel}\n" +
                $"  SpawnAnchor: {_currentSpawnAnchor}\n" +
                $"  WorldSeed: {_runManager.CaveWorldSeed}\n" +
                $"  RunSeed: {_runManager.CaveRunSeed}\n" +
                $"  LayoutHash: {CurrentGeneratedLevel.LayoutHash}\n" +
                $"  Rooms: {RoomCount}\n" +
                $"  EnemyPoints: {EnemyPointCount}\n" +
                $"  ResourcePoints: {ResourcePointCount}\n" +
                $"  UsedSnapshot: false\n" +
                $"  GeneratedNewSnapshot: true",
                this);

            if (_logGeneratedLayout)
            {
                CindarsHope.Combat.CombatLog.Log(CaveGenerationDebugPrinter.ToAscii(CurrentGeneratedLevel), this);
            }

            if (_materializeAfterGeneration && _materializer != null)
            {
                _materializer.Materialize(CurrentGeneratedLevel, _currentSpawnAnchor);
                RegisterEnemySpawnPlan(_spawnPlanService.CreatePlanFromCaveEnemySpawnPlan(_materializer.LastEnemySpawnPlan));
                RepositionCamera();
                CaptureSnapshot();
            }

            GameEventBus.Publish(new CaveLevelEnteredEvent(
                _runManager.CurrentCaveLevel,
                _defaultBiomeId,
                _runManager.CaveRunSeed));
        }

        public void CaptureSnapshot()
        {
            if (CurrentGeneratedLevel == null)
            {
                return;
            }

            var snapshot = _snapshotService.CaptureSnapshot(
                CurrentGeneratedLevel,
                _runManager.CaveWorldSeed,
                _runManager.CaveRunSeed,
                _materializer != null ? _materializer.LastEnemySpawnPlan : null,
                _materializer != null ? _materializer.LastResourceNodeSnapshots : null,
                null,
                _runManager.State.DepletedNodeIds,
                _materializer != null ? _materializer.CollectEnemyHpRecords() : null,
                _materializer != null ? _materializer.OpenedChestIds : null, // fable_09
                _materializer != null ? _materializer.TrapStates : null); // fable_60

            if (snapshot == null)
            {
                Debug.LogWarning("CaveLevelRuntimeController: Snapshot capture returned null.", this);
                return;
            }

            _runManager.State.VisitedLevelSnapshots[CurrentGeneratedLevel.CaveLevel] = snapshot;
            CindarsHope.Combat.CombatLog.Log(
                $"CaveLevelRuntimeController: snapshot captured for level {CurrentGeneratedLevel.CaveLevel}.\n" +
                $"  LayoutHash: {snapshot.LayoutHash}\n" +
                $"  Dimensions: {snapshot.Width}x{snapshot.Height}\n" +
                $"  WalkableTiles: {snapshot.WalkableTilesList.Count}\n" +
                $"  WallTiles: {snapshot.WallTilesList.Count}\n" +
                $"  EnemySpawnPoints: {snapshot.EnemySpawnPointsList.Count}\n" +
                $"  ResourceSpawnPoints: {snapshot.ResourceSpawnPointsList.Count}",
                this);
        }

        // F13: regrava o HP corrente dos inimigos no snapshot do nível atual antes de sair
        // do nível ou salvar o jogo (snapshot é capturado na entrada; HP muda durante o nível).
        public void RefreshCurrentSnapshotEnemyHp()
        {
            if (CurrentGeneratedLevel == null || _materializer == null || _runManager == null)
            {
                return;
            }

            if (_runManager.State.VisitedLevelSnapshots.TryGetValue(CurrentGeneratedLevel.CaveLevel, out var snapshot)
                && snapshot != null)
            {
                snapshot.SetEnemyHpRecords(_materializer.CollectEnemyHpRecords());
            }
        }

        // fable_09: regrava os baús abertos no snapshot do nível atual antes de sair/salvar
        // (snapshot é capturado na entrada; o jogador pode abrir baús durante o nível). Mesmo
        // padrão de RefreshCurrentSnapshotEnemyHp — estado mutável fora do LayoutHash.
        public void RefreshCurrentSnapshotOpenedChests()
        {
            if (CurrentGeneratedLevel == null || _materializer == null || _runManager == null)
            {
                return;
            }

            if (_runManager.State.VisitedLevelSnapshots.TryGetValue(CurrentGeneratedLevel.CaveLevel, out var snapshot)
                && snapshot != null)
            {
                foreach (var chestId in _materializer.OpenedChestIds)
                {
                    snapshot.MarkChestOpened(chestId);
                }
            }
        }

        // fable_60: regrava o estado das armadilhas no snapshot do nível atual antes de sair/salvar
        // (snapshot é capturado na entrada; o jogador pode disparar/desarmar armadilhas durante o
        // nível). Mesmo padrão de RefreshCurrentSnapshotOpenedChests — estado mutável fora do LayoutHash.
        public void RefreshCurrentSnapshotTrapStates()
        {
            if (CurrentGeneratedLevel == null || _materializer == null || _runManager == null)
            {
                return;
            }

            if (_runManager.State.VisitedLevelSnapshots.TryGetValue(CurrentGeneratedLevel.CaveLevel, out var snapshot)
                && snapshot != null)
            {
                foreach (var trap in _materializer.TrapStates)
                {
                    if (trap != null)
                    {
                        snapshot.SetTrapState(trap.TrapInstanceId, trap.TrapKey, trap.Cell, trap.State);
                    }
                }
            }
        }

        public void RestoreFromSnapshot(VisitedLevelSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.IsValid())
            {
                Debug.LogWarning("CaveLevelRuntimeController: attempted to restore from invalid snapshot.", this);
                return;
            }

            CurrentGeneratedLevel = _snapshotService.RestoreGeneratedLevel(snapshot);
            if (CurrentGeneratedLevel == null)
            {
                Debug.LogWarning("CaveLevelRuntimeController: Snapshot could not restore CaveGeneratedLevel.", this);
                return;
            }

            _currentEnemyPlan = snapshot.RestoreEnemySpawnPlan();

            ValidateLayoutHashFromSnapshot(snapshot, CurrentGeneratedLevel);

            CindarsHope.Combat.CombatLog.Log(
                $"CaveLevelRuntimeController: Cave level restored from snapshot.\n" +
                $"  Level: {snapshot.CaveLevel}\n" +
                $"  SpawnAnchor: {_currentSpawnAnchor}\n" +
                $"  RunSeed: {_runManager.CaveRunSeed}\n" +
                $"  LayoutHash: {snapshot.LayoutHash}\n" +
                $"  Dimensions: {snapshot.Width}x{snapshot.Height}\n" +
                $"  WalkableTiles: {snapshot.WalkableTilesList.Count}\n" +
                $"  WallTiles: {snapshot.WallTilesList.Count}\n" +
                $"  EnemySpawnPoints: {snapshot.EnemySpawnPointsList.Count}\n" +
                $"  ResourceSpawnPoints: {snapshot.ResourceSpawnPointsList.Count}\n" +
                $"  UsedSnapshot: true\n" +
                $"  GeneratedNewSnapshot: false",
                this);

            if (_logGeneratedLayout)
            {
                CindarsHope.Combat.CombatLog.Log(CaveGenerationDebugPrinter.ToAscii(CurrentGeneratedLevel), this);
            }

            if (_materializeAfterGeneration && _materializer != null)
            {
                _materializer.MaterializeFromSnapshot(snapshot, CurrentGeneratedLevel, _currentSpawnAnchor);
                RegisterEnemySpawnPlan(_spawnPlanService.CreatePlanFromCaveEnemySpawnPlan(_materializer.LastEnemySpawnPlan));
                RepositionCamera();
            }

            GameEventBus.Publish(new CaveLevelEnteredEvent(
                snapshot.CaveLevel,
                snapshot.BiomeId,
                _runManager.CaveRunSeed));
        }

        public void RegenerateCurrentRunDebug()
        {
            EnsureRuntimeReferences();
            CleanupBeforeRegeneration();
            var oldRunSeed = _runManager.CaveRunSeed;
            _runManager.GenerateNewRunSeed("debug_regeneration");
            var newRunSeed = _runManager.CaveRunSeed;
            GenerateCurrentLevel();
            Debug.Log($"Cave regenerated via debug (Shift+R). RunSeed: {oldRunSeed} -> {newRunSeed}.", this);
        }

        private void CleanupBeforeRegeneration()
        {
            if (_materializer != null)
            {
                _materializer.CleanupMaterialization();
            }

            if (_enemySpawner != null)
            {
                _enemySpawner.CleanupSpawns();
            }
        }

        private void RepositionCamera()
        {
            if (_playerTransform == null)
            {
                return;
            }

            var mainCamera = UnityEngine.Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(
                    _playerTransform.position.x,
                    _playerTransform.position.y,
                    mainCamera.transform.position.z);
            }
        }

        private void OnDayStarted(DayStartedEvent _)
        {
            RefreshDailyResourceNodes();
        }

        private void RefreshDailyResourceNodes()
        {
            var allNodes = FindObjectsByType<ResourceNode>();
            var refreshedCount = 0;
            foreach (var node in allNodes)
            {
                if (node.IsDepleted)
                {
                    node.RefreshForNewDay();
                    if (!node.IsDepleted)
                    {
                        refreshedCount++;
                    }
                }
            }

            if (refreshedCount > 0)
            {
                CindarsHope.Combat.CombatLog.Log($"CaveLevelRuntimeController: refreshed {refreshedCount} resource nodes for new day.", this);
            }
        }

        private void EnsureRuntimeReferences()
        {
            if (_runManager == null)
            {
                _runManager = GetComponent<CaveRunManager>();
            }

            if (_runManager == null)
            {
                _runManager = gameObject.AddComponent<CaveRunManager>();
            }

            if (_generationConfig == null)
            {
                _generationConfig = ScriptableObject.CreateInstance<CaveGenerationConfigSO>();
                _generationConfig.Id = "runtime_default_cave_generation";
            }
        }

        private bool ValidateSnapshotIntegrity(VisitedLevelSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.IsValid())
            {
                return false;
            }

            if (snapshot.CaveLevel <= 0)
            {
                Debug.LogWarning("CaveLevelRuntimeController: Snapshot has invalid cave level.", this);
                return false;
            }

            if (string.IsNullOrWhiteSpace(snapshot.LayoutHash))
            {
                Debug.LogWarning("CaveLevelRuntimeController: Snapshot missing layout hash.", this);
                return false;
            }

            if (snapshot.Width <= 0 || snapshot.Height <= 0)
            {
                Debug.LogWarning("CaveLevelRuntimeController: Snapshot has invalid dimensions.", this);
                return false;
            }

            if (snapshot.WalkableTilesList.Count == 0)
            {
                Debug.LogWarning("CaveLevelRuntimeController: Snapshot has no walkable tiles.", this);
                return false;
            }

            CindarsHope.Combat.CombatLog.Log($"CaveLevelRuntimeController: Snapshot integrity check passed for level {snapshot.CaveLevel}.", this);
            return true;
        }

        private void ValidateLayoutHashFromSnapshot(VisitedLevelSnapshot snapshot, CaveGeneratedLevel reconstructed)
        {
            if (snapshot == null || reconstructed == null)
            {
                return;
            }

            var replayHash = _snapshotService.CalculateLayoutHash(snapshot);

            if (snapshot.LayoutHash != replayHash)
            {
                Debug.LogWarning(
                    $"CaveLevelRuntimeController: Layout hash mismatch for level {snapshot.CaveLevel}.\n" +
                    $"  Snapshot hash: {snapshot.LayoutHash}\n" +
                    $"  Recomputed hash: {replayHash}\n" +
                    $"  This may indicate a corruption or version mismatch.",
                    this);
            }
            else
            {
                CindarsHope.Combat.CombatLog.Log($"CaveLevelRuntimeController: Layout hash validated for level {snapshot.CaveLevel}.", this);
            }
        }
    }
}
