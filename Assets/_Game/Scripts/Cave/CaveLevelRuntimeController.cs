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
        [SerializeField] private CaveGenerationConfigSO _generationConfig;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private string _defaultBiomeId = "biome_cave_earth";
        [SerializeField] private bool _logGeneratedLayout = true;
        [SerializeField] private bool _materializeAfterGeneration = true;

        private readonly CaveProceduralGenerator _generator = new CaveProceduralGenerator();
        private CaveSpawnAnchor _currentSpawnAnchor = CaveSpawnAnchor.Entrance;

        public CaveGeneratedLevel CurrentGeneratedLevel { get; private set; }
        public CaveSpawnAnchor CurrentSpawnAnchor => _currentSpawnAnchor;
        public int RoomCount => CurrentGeneratedLevel != null ? CurrentGeneratedLevel.Rooms.Count : 0;
        public int EnemyPointCount => CurrentGeneratedLevel != null ? CurrentGeneratedLevel.EnemySpawnPoints.Count : 0;
        public int ResourcePointCount => CurrentGeneratedLevel != null ? CurrentGeneratedLevel.ResourceSpawnPoints.Count : 0;
        public CaveRunManager RunManager => _runManager;
        public CaveRuntimeMaterializer Materializer => _materializer;

        public void SetSpawnAnchorForNextGeneration(CaveSpawnAnchor anchor)
        {
            _currentSpawnAnchor = anchor;
            Debug.Log($"CaveLevelRuntimeController: spawn anchor set to {anchor} for next generation.", this);
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
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<CaveRuntimeMaterializationCompleteEvent>(OnMaterializationComplete);
            GameEventBus.Unsubscribe<DayStartedEvent>(OnDayStarted);
            GameEventBus.Unsubscribe<SceneTransitionStartedEvent>(OnSceneTransitionStarted);
        }

        private void OnSceneTransitionStarted(SceneTransitionStartedEvent evt)
        {
            DetermineSpawnAnchorFromTransition(evt.SourceSceneName, evt.TargetSceneName, evt.TargetSpawnId);
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
            if (_enemySpawner != null && _materializer != null && _materializer.GeneratedRuntimeRoot != null)
            {
                _enemySpawner.SpawnEnemiesForLevel(e.GeneratedLevel, _materializer.GeneratedRuntimeRoot, _playerTransform);
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
            var visitedSnapshot = _runManager.State.VisitedLevelSnapshots.ContainsKey(caveLevel)
                ? _runManager.State.VisitedLevelSnapshots[caveLevel]
                : null;

            if (visitedSnapshot != null && visitedSnapshot.IsValid())
            {
                RestoreFromSnapshot(visitedSnapshot);
                return;
            }

            CurrentGeneratedLevel = _generator.Generate(
                _generationConfig,
                _runManager.CurrentCaveLevel,
                _runManager.CaveWorldSeed,
                _runManager.CaveRunSeed,
                _defaultBiomeId);

            CurrentGeneratedLevel.ComputeLayoutHash();

            Debug.Log(
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
                Debug.Log(CaveGenerationDebugPrinter.ToAscii(CurrentGeneratedLevel), this);
            }

            if (_materializeAfterGeneration && _materializer != null)
            {
                _materializer.Materialize(CurrentGeneratedLevel, _currentSpawnAnchor);
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

            var snapshot = new VisitedLevelSnapshot(
                CurrentGeneratedLevel.CaveLevel,
                CurrentGeneratedLevel.BiomeId,
                CurrentGeneratedLevel.LayoutHash);

            snapshot.SetEntranceAndExit(CurrentGeneratedLevel.Entrance, CurrentGeneratedLevel.Exit);
            snapshot.SetLayoutDimensions(CurrentGeneratedLevel.Width, CurrentGeneratedLevel.Height);

            foreach (var walkableTile in CurrentGeneratedLevel.WalkableTiles)
            {
                snapshot.AddWalkableTile(walkableTile);
            }

            foreach (var wallTile in CurrentGeneratedLevel.WallTiles)
            {
                snapshot.AddWallTile(wallTile);
            }

            foreach (var point in CurrentGeneratedLevel.EnemySpawnPoints)
            {
                snapshot.AddEnemySpawnPoint((int)point.PointType, point.Position);
                snapshot.AddEnemySpawn($"enemy_{point.Position.x}_{point.Position.y}", new Vector2(point.Position.x, point.Position.y), CurrentGeneratedLevel.CaveLevel);
            }

            foreach (var point in CurrentGeneratedLevel.ResourceSpawnPoints)
            {
                snapshot.AddResourceSpawnPoint((int)point.PointType, point.Position);
                snapshot.AddResourceNode($"node_{point.Position.x}_{point.Position.y}", new Vector2(point.Position.x, point.Position.y), point.PointType.ToString());
            }

            foreach (var depletedId in _runManager.State.DepletedNodeIds)
            {
                snapshot.MarkResourceNodeDepleted(depletedId);
            }

            _runManager.State.VisitedLevelSnapshots[CurrentGeneratedLevel.CaveLevel] = snapshot;
            Debug.Log(
                $"CaveLevelRuntimeController: snapshot captured for level {CurrentGeneratedLevel.CaveLevel}.\n" +
                $"  LayoutHash: {snapshot.LayoutHash}\n" +
                $"  Dimensions: {snapshot.Width}x{snapshot.Height}\n" +
                $"  WalkableTiles: {snapshot.WalkableTilesList.Count}\n" +
                $"  WallTiles: {snapshot.WallTilesList.Count}\n" +
                $"  EnemySpawnPoints: {snapshot.EnemySpawnPointsList.Count}\n" +
                $"  ResourceSpawnPoints: {snapshot.ResourceSpawnPointsList.Count}",
                this);
        }

        public void RestoreFromSnapshot(VisitedLevelSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.IsValid())
            {
                Debug.LogWarning("CaveLevelRuntimeController: attempted to restore from invalid snapshot.", this);
                return;
            }

            CurrentGeneratedLevel = new CaveGeneratedLevel
            {
                CaveLevel = snapshot.CaveLevel,
                BiomeId = snapshot.BiomeId,
                LayoutHash = snapshot.LayoutHash,
                Width = snapshot.Width,
                Height = snapshot.Height,
                Entrance = Vector2Int.FloorToInt(snapshot.EntrancePosition),
                Exit = Vector2Int.FloorToInt(snapshot.ExitPosition)
            };

            foreach (var walkableTile in snapshot.WalkableTilesList)
            {
                CurrentGeneratedLevel.WalkableTiles.Add(walkableTile);
            }

            foreach (var wallTile in snapshot.WallTilesList)
            {
                CurrentGeneratedLevel.WallTiles.Add(wallTile);
            }

            foreach (var serializedPoint in snapshot.EnemySpawnPointsList)
            {
                CurrentGeneratedLevel.EnemySpawnPoints.Add(
                    new CaveGenerationPoint((CaveGenerationPointType)serializedPoint.PointTypeValue, serializedPoint.Position));
            }

            foreach (var serializedPoint in snapshot.ResourceSpawnPointsList)
            {
                CurrentGeneratedLevel.ResourceSpawnPoints.Add(
                    new CaveGenerationPoint((CaveGenerationPointType)serializedPoint.PointTypeValue, serializedPoint.Position));
            }

            Debug.Log(
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
                Debug.Log(CaveGenerationDebugPrinter.ToAscii(CurrentGeneratedLevel), this);
            }

            if (_materializeAfterGeneration && _materializer != null)
            {
                _materializer.Materialize(CurrentGeneratedLevel, _currentSpawnAnchor);
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
                Debug.Log($"CaveLevelRuntimeController: refreshed {refreshedCount} resource nodes for new day.", this);
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
    }
}
