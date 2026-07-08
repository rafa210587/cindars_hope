using CindarsHope.Cave.Art;
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
        // spec_cave_biome_art_profiles_runtime (CV01): banda publicada na última entrada de nível
        // (sentinela NoPreviousBand garante que a primeira entrada da run conta como mudança).
        private int _lastPublishedBandId = CaveBiomeChangeDecision.NoPreviousBand;
        private string _lastPublishedBiomeId = string.Empty;

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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Poll de debug (Shift+R = regenerar run). Gateado fora de builds de shipping para nao
            // alocar SceneManager.GetActiveScene().name por frame em produção (achado de eficiencia).
            if (SceneManager.GetActiveScene().name != "CaveScene")
            {
                return;
            }

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
            {
                RegenerateCurrentRunDebug();
            }
#endif
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
                // fable_78 (SLICE 4): roll de conflito inter-monstro POR ENTRADA, após a captura do
                // snapshot (que carrega EntryCount/HasHadConflict). Marca rivais e publica o toast.
                ApplyInterMonsterConflict();
            }

            GameEventBus.Publish(new CaveLevelEnteredEvent(
                _runManager.CurrentCaveLevel,
                _defaultBiomeId,
                _runManager.CaveRunSeed));

            PublishBiomeChangedIfNeeded(_runManager.CurrentCaveLevel);
        }

        // spec_cave_biome_art_profiles_runtime (CV01): publica CaveBiomeChangedEvent SOMENTE quando a
        // banda difere da última publicada (a primeira entrada da run sempre conta como mudança —
        // critério 14.4). BandId vem de CaveBandScaling (puro, já fonte de verdade em todo o resto do
        // código); BiomeId vem do CaveBiomeArtProfileSO da banda quando o materializer expõe um
        // resolver com profile carregado, senão string vazia (não é dado de gameplay, só de arte).
        private void PublishBiomeChangedIfNeeded(int caveLevel)
        {
            // ResolveBandForArt: toggle dev T011 (default OFF) afeta só esta apresentação, nunca
            // layout/spawn/loot/snapshot — CaveBandScaling.BandForLevel puro segue sendo a fonte de
            // verdade de gameplay em todo o resto do código.
            var bandId = CaveBiomeArtDebug.ResolveBandForArt(CaveBandScaling.BandForLevel(caveLevel));
            if (!CaveBiomeChangeDecision.HasBandChanged(_lastPublishedBandId, bandId))
            {
                return;
            }

            var biomeId = _materializer != null ? _materializer.ResolveArtBiomeIdForBand(bandId) : string.Empty;
            GameEventBus.Publish(new CaveBiomeChangedEvent(_lastPublishedBiomeId, biomeId, bandId, caveLevel));

            _lastPublishedBandId = bandId;
            _lastPublishedBiomeId = biomeId;
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
                _materializer != null ? _materializer.TrapStates : null, // fable_60
                _materializer != null ? _materializer.LastEnvironmentElements : null, // fable_78
                _materializer != null && _materializer.LastHasWater, // fable_78
                ResolveConflictStateForCapture()); // fable_78

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

        // fable_78: resolve o estado de conflito a ser persistido na captura do snapshot. Preserva o
        // estado persistido do nível corrente (EntryCount/HasHadConflict são acumulativos por run e NÃO
        // podem ser zerados a cada recaptura). A rolagem de conflito por entrada (5%->0,5%) e a marcação
        // de ConflictActive/Faction* da visita são da slice 4 (runtime de conflito); aqui só preservamos
        // o estado existente para que a persistência aditiva já o carregue intacto.
        private CaveConflictSnapshot ResolveConflictStateForCapture()
        {
            if (CurrentGeneratedLevel != null
                && _runManager != null
                && _runManager.State.VisitedLevelSnapshots.TryGetValue(CurrentGeneratedLevel.CaveLevel, out var existing)
                && existing?.ConflictState != null)
            {
                return existing.ConflictState;
            }

            return null;
        }

        // fable_78: regrava os elementos ambientais (e a presença de água) no snapshot do nível atual
        // antes de sair/salvar. Mesmo padrão de RefreshCurrentSnapshotTrapStates — o estado depletado de
        // mineráveis é mutável (fora do LayoutHash); a depleção real é idempotente via CaveLootSnapshotService.
        public void RefreshCurrentSnapshotEnvironmentElements()
        {
            if (CurrentGeneratedLevel == null || _materializer == null || _runManager == null)
            {
                return;
            }

            if (_runManager.State.VisitedLevelSnapshots.TryGetValue(CurrentGeneratedLevel.CaveLevel, out var snapshot)
                && snapshot != null)
            {
                snapshot.SetEnvironmentElements(_materializer.LastEnvironmentElements);
                if (_materializer.LastHasWater)
                {
                    snapshot.HasWater = true;
                }
            }
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
                // fable_78 (SLICE 4): re-roll de conflito POR ENTRADA também na revisita (comportamento por
                // visita, não composição — ADR-0018). O snapshot já existe com EntryCount/HasHadConflict.
                ApplyInterMonsterConflict();
            }

            GameEventBus.Publish(new CaveLevelEnteredEvent(
                snapshot.CaveLevel,
                snapshot.BiomeId,
                _runManager.CaveRunSeed));

            PublishBiomeChangedIfNeeded(snapshot.CaveLevel);
        }

        // fable_78 (SLICE 4): rola o conflito inter-monstro desta ENTRADA, marca os rivais nas instâncias
        // materializadas e publica o feedback. Determinístico por (worldSeed, runSeed, caveLevel, entryIndex)
        // — entryIndex = EntryCount persistido (re-roll por visita; queda 5%->0,5% após o primeiro conflito).
        // Conflito é comportamento por visita (ADR-0018): não toca composição/posições/IDs (estáveis).
        private void ApplyInterMonsterConflict()
        {
            if (_materializer == null || CurrentGeneratedLevel == null || _runManager == null)
            {
                return;
            }

            var balance = _materializer.EcosystemBalance;
            if (balance == null)
            {
                return; // balance não ligado (DEFERRED_UNITY: asset CaveEcosystemBalance — slice 6)
            }

            var plan = _materializer.LastEnemySpawnPlan;
            if (plan == null || plan.Entries == null || plan.Entries.Count == 0)
            {
                return;
            }

            // Estado persistido do nível (EntryCount/HasHadConflict). Get-or-create p/ back-compat de save.
            if (!_runManager.State.VisitedLevelSnapshots.TryGetValue(CurrentGeneratedLevel.CaveLevel, out var snapshot)
                || snapshot == null)
            {
                return;
            }

            var conflictState = snapshot.GetOrCreateConflictState();

            var presentEnemyIds = new System.Collections.Generic.List<string>(plan.Entries.Count);
            foreach (var entry in plan.Entries)
            {
                if (entry != null && !string.IsNullOrWhiteSpace(entry.EnemyId))
                {
                    presentEnemyIds.Add(entry.EnemyId);
                }
            }

            var conflictPlan = Cave.Ecosystem.CaveEcosystemConflictPlanner.Decide(
                _runManager.CaveWorldSeed,
                _runManager.CaveRunSeed,
                CurrentGeneratedLevel.CaveLevel,
                conflictState.EntryCount, // entryIndex = entradas anteriores (re-roll por visita)
                conflictState.HasHadConflict,
                presentEnemyIds,
                balance);

            if (conflictPlan.ConflictActive)
            {
                _materializer.ApplyConflict(conflictPlan, CurrentGeneratedLevel.CaveLevel);
                GameEventBus.Publish(new CaveEcosystemConflictStartedEvent(
                    CurrentGeneratedLevel.CaveLevel,
                    conflictPlan.FactionAEnemyId,
                    conflictPlan.FactionBEnemyId));
            }

            // Persiste a entrada (incrementa EntryCount; trava HasHadConflict; grava Active/Faction* da visita).
            snapshot.RecordConflictEntry(
                conflictPlan.ConflictActive,
                conflictPlan.FactionAEnemyId,
                conflictPlan.FactionBEnemyId);
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
            // Le do registro estatico ResourceNode.ActiveInstances (auto-registrado via OnEnable/OnDisable)
            // em vez de FindObjectsByType a cada DayStartedEvent (rule unity-architecture #1).
            var allNodes = ResourceNode.ActiveInstances;
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
