using System.Collections.Generic;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Resources;
using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Enemy;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using CindarsHope.SceneManagement;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CindarsHope.Cave.Runtime
{
    public sealed class CaveRuntimeMaterializer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _floorTilePrefab;
        [SerializeField] private SpriteRenderer _wallTilePrefab;
        [SerializeField] private ScenePortal _entrancePrefab;
        [SerializeField] private CaveExitPortal _exitPortalPrefab;
        [SerializeField] private ResourceNode _resourceNodePrefab;
        [SerializeField] private CaveRunManager _caveRunManager;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private EquipmentManager _equipmentManager;
        [SerializeField] private EnemyDatabaseSO _enemyDatabase;
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private EnemySpawnProfileSO[] _enemySpawnProfiles = new EnemySpawnProfileSO[0];
        [SerializeField] private EnemySpawnPackSO[] _enemySpawnPacks = new EnemySpawnPackSO[0];
        [SerializeField] private EnemyFactionLockSO[] _enemyFactionLocks = new EnemyFactionLockSO[0];
        [SerializeField] private ResourceNodeDatabaseSO _resourceNodeDatabase;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private CaveLevelRuntimeController _levelController;
        [SerializeField, Range(0f, 1f)] private float _resourceSpawnChance = 0.28f;
        [SerializeField] private int _minResourceNodes = 1;
        [SerializeField] private int _maxResourceNodes = 4;
        [SerializeField] private int _maxEnemiesPerLevel = 20;

        private GameObject _generatedRuntimeRoot;
        private CaveExitPortal _backExitPortal;
        private CaveExitPortal _forwardExitPortal;
        private List<GameObject> _materializedObjects = new List<GameObject>();
        private readonly CaveEnemySpawnPlanner _enemySpawnPlanner = new CaveEnemySpawnPlanner();
        private CaveEnemySpawnPlan _lastEnemySpawnPlan;
        private CaveEnemySpawnPlan _snapshotEnemySpawnPlan;
        private IReadOnlyList<CaveResourceNodeSnapshotEntry> _snapshotResourceNodeStates;
        private readonly List<CaveResourceNodeSnapshotEntry> _lastResourceNodeSnapshots = new List<CaveResourceNodeSnapshotEntry>();

        public CaveExitPortal BackExitPortal => _backExitPortal;
        public CaveExitPortal ForwardExitPortal => _forwardExitPortal;
        public GameObject GeneratedRuntimeRoot => _generatedRuntimeRoot;

        private CaveRuntimeMaterializationResult _lastMaterializationResult;

        public CaveRuntimeMaterializationResult LastMaterializationResult => _lastMaterializationResult;
        public CaveEnemySpawnPlan LastEnemySpawnPlan => _lastEnemySpawnPlan;
        public IReadOnlyList<CaveResourceNodeSnapshotEntry> LastResourceNodeSnapshots => _lastResourceNodeSnapshots;

        public void Materialize(CaveGeneratedLevel generatedLevel, CaveSpawnAnchor spawnAnchor = CaveSpawnAnchor.Entrance)
        {
            MaterializeInternal(generatedLevel, spawnAnchor, null, null);
        }

        public void MaterializeFromSnapshot(VisitedLevelSnapshot snapshot, CaveGeneratedLevel generatedLevel, CaveSpawnAnchor spawnAnchor = CaveSpawnAnchor.Entrance)
        {
            MaterializeInternal(
                generatedLevel,
                spawnAnchor,
                snapshot?.EnemySpawnPlan,
                snapshot?.ResourceNodeStates);
        }

        private void MaterializeInternal(
            CaveGeneratedLevel generatedLevel,
            CaveSpawnAnchor spawnAnchor,
            CaveEnemySpawnPlan enemySpawnPlanOverride,
            IReadOnlyList<CaveResourceNodeSnapshotEntry> resourceNodeStateOverride)
        {
            if (generatedLevel == null)
            {
                Debug.LogError("CaveRuntimeMaterializer: Cannot materialize null CaveGeneratedLevel.");
                return;
            }

            if (_caveRunManager == null)
            {
                _caveRunManager = GetComponent<CaveRunManager>();
            }

            if (_levelController == null)
            {
                _levelController = GetComponent<CaveLevelRuntimeController>();
            }

            CleanupPreviousMaterialization();
            _snapshotEnemySpawnPlan = enemySpawnPlanOverride;
            _snapshotResourceNodeStates = resourceNodeStateOverride;
            _lastResourceNodeSnapshots.Clear();

            _lastMaterializationResult = new CaveRuntimeMaterializationResult();

            // Create root hierarchy
            _generatedRuntimeRoot = new GameObject("CaveGeneratedRuntime");
            _generatedRuntimeRoot.transform.position = Vector3.zero;

            MaterializeFloor(generatedLevel);
            MaterializeWalls(generatedLevel);
            MaterializeEntranceAndExit(generatedLevel);
            MaterializeResourceNodes(generatedLevel);
            MaterializeEnemies(generatedLevel);

            // Resolve safe spawn position based on anchor
            if (_playerTransform != null)
            {
                var anchorGridPos = ResolveAnchorPosition(spawnAnchor, generatedLevel);
                var safeSpawnGrid = ResolvePlayerSpawnGrid(anchorGridPos, spawnAnchor, generatedLevel);
                _playerTransform.position = GridToWorld(safeSpawnGrid, generatedLevel);

                Debug.Log(
                    $"CaveRuntimeMaterializer: Player spawned at anchor {spawnAnchor}. AnchorGrid: {anchorGridPos}, ResolvedGrid: {safeSpawnGrid}, WorldPos: {_playerTransform.position}",
                    this);

                RepositionCamera();
            }

            Debug.Log(
                $"CaveRuntimeMaterializer: Materialized level {generatedLevel.CaveLevel}. Floor: {_lastMaterializationResult.CreatedFloorTiles}, Walls: {_lastMaterializationResult.CreatedWallTiles}, Resources: {_lastMaterializationResult.CreatedResourceNodes}, Enemies: {_lastMaterializationResult.CreatedEnemies}. BackExit: {_lastMaterializationResult.BackExitPosition}, ForwardExit: {_lastMaterializationResult.ForwardExitPosition}. SpawnAnchor: {spawnAnchor}",
                this);

            GameEventBus.Publish(new CaveRuntimeMaterializationCompleteEvent(generatedLevel));
            _snapshotEnemySpawnPlan = null;
            _snapshotResourceNodeStates = null;
        }

        private static Vector3 GridToWorld(Vector2Int gridPosition, CaveGeneratedLevel level)
        {
            var offsetX = level.Width * 0.5f;
            var offsetY = level.Height * 0.5f;
            return new Vector3(gridPosition.x - offsetX, gridPosition.y - offsetY, 0f);
        }

        private void RepositionCamera()
        {
            var mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            var cameraFollow = mainCamera.GetComponent<CindarsHope.Camera.CameraFollow2D>();
            if (cameraFollow != null)
            {
                cameraFollow.RebindTarget(_playerTransform);
                cameraFollow.SnapToTarget();
            }
            else
            {
                mainCamera.transform.position = new Vector3(
                    _playerTransform.position.x,
                    _playerTransform.position.y,
                    mainCamera.transform.position.z);
            }
        }

        private void MaterializeFloor(CaveGeneratedLevel generatedLevel)
        {
            var floorParent = new GameObject("GeneratedFloor");
            floorParent.transform.SetParent(_generatedRuntimeRoot.transform);
            floorParent.transform.localPosition = Vector3.zero;

            foreach (var tilePos in generatedLevel.WalkableTiles)
            {
                var worldPos = GridToWorld(tilePos, generatedLevel);
                GameObject floorTile;

                if (_floorTilePrefab != null)
                {
                    var spriteRenderer = Instantiate(_floorTilePrefab, worldPos, Quaternion.identity, floorParent.transform);
                    floorTile = spriteRenderer.gameObject;
                    spriteRenderer.sortingOrder = 0;
                }
                else
                {
                    floorTile = new GameObject($"FloorTile_{tilePos.x}_{tilePos.y}");
                    floorTile.transform.SetParent(floorParent.transform);
                    floorTile.transform.position = worldPos;

                    var spriteRenderer = floorTile.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = GetBuiltinSprite();
                    spriteRenderer.color = new Color(0.4f, 0.35f, 0.3f);
                    spriteRenderer.sortingOrder = 0;
                }

                floorTile.name = $"FloorTile_{tilePos.x}_{tilePos.y}";
                _materializedObjects.Add(floorTile);
                _lastMaterializationResult.CreatedFloorTiles++;
            }
        }

        private Sprite GetBuiltinSprite()
        {
#if UNITY_EDITOR
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
#else
            return null;
#endif
        }

        private void MaterializeWalls(CaveGeneratedLevel generatedLevel)
        {
            var wallParent = new GameObject("GeneratedWalls");
            wallParent.transform.SetParent(_generatedRuntimeRoot.transform);
            wallParent.transform.localPosition = Vector3.zero;

            foreach (var tilePos in generatedLevel.WallTiles)
            {
                var worldPos = GridToWorld(tilePos, generatedLevel);
                GameObject wallTile;

                if (_wallTilePrefab != null)
                {
                    var spriteRenderer = Instantiate(_wallTilePrefab, worldPos, Quaternion.identity, wallParent.transform);
                    wallTile = spriteRenderer.gameObject;
                    spriteRenderer.sortingOrder = 1;
                }
                else
                {
                    wallTile = new GameObject($"WallTile_{tilePos.x}_{tilePos.y}");
                    wallTile.transform.SetParent(wallParent.transform);
                    wallTile.transform.position = worldPos;

                    var spriteRenderer = wallTile.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = GetBuiltinSprite();
                    spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f);
                    spriteRenderer.sortingOrder = 1;
                }

                wallTile.name = $"WallTile_{tilePos.x}_{tilePos.y}";

                var collider = wallTile.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one;

                _materializedObjects.Add(wallTile);
                _lastMaterializationResult.CreatedWallTiles++;
            }
        }

        private void MaterializeEntranceAndExit(CaveGeneratedLevel generatedLevel)
        {
            var portalsParent = new GameObject("GeneratedExits");
            portalsParent.transform.SetParent(_generatedRuntimeRoot.transform);
            portalsParent.transform.localPosition = Vector3.zero;

            // BackExit at entrance position
            var backExitPos = GridToWorld(generatedLevel.Entrance, generatedLevel);
            if (_exitPortalPrefab != null)
            {
                _backExitPortal = Instantiate(_exitPortalPrefab, backExitPos, Quaternion.identity, portalsParent.transform);
                _backExitPortal.gameObject.name = "GeneratedBackExit";
                _backExitPortal.InitializeBackExit(_caveRunManager, _levelController);
            }
            else
            {
                var backExitGO = new GameObject("GeneratedBackExit");
                backExitGO.transform.SetParent(portalsParent.transform);
                backExitGO.transform.position = backExitPos;

                var spriteRenderer = backExitGO.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = GetBuiltinSprite();
                spriteRenderer.color = new Color(0f, 1f, 1f, 0.7f);
                spriteRenderer.sortingOrder = 2;

                var collider = backExitGO.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one;
                collider.isTrigger = true;

                _backExitPortal = backExitGO.AddComponent<CaveExitPortal>();
                _backExitPortal.InitializeBackExit(_caveRunManager, _levelController);
            }

            if (_backExitPortal != null)
            {
                var collider = _backExitPortal.GetComponent<BoxCollider2D>();
                if (collider == null)
                {
                    collider = _backExitPortal.gameObject.AddComponent<BoxCollider2D>();
                    collider.size = Vector2.one;
                    collider.isTrigger = true;
                }
                _materializedObjects.Add(_backExitPortal.gameObject);
                _lastMaterializationResult.BackExitPosition = backExitPos;
            }

            // ForwardExit at exit position
            var forwardExitPos = GridToWorld(generatedLevel.Exit, generatedLevel);
            if (_exitPortalPrefab != null)
            {
                _forwardExitPortal = Instantiate(_exitPortalPrefab, forwardExitPos, Quaternion.identity, portalsParent.transform);
                _forwardExitPortal.gameObject.name = "GeneratedForwardExit";
                _forwardExitPortal.InitializeForwardExit(_caveRunManager, _levelController);
            }
            else
            {
                var forwardExitGO = new GameObject("GeneratedForwardExit");
                forwardExitGO.transform.SetParent(portalsParent.transform);
                forwardExitGO.transform.position = forwardExitPos;

                var spriteRenderer = forwardExitGO.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = GetBuiltinSprite();
                spriteRenderer.color = new Color(1f, 0f, 1f, 0.7f);
                spriteRenderer.sortingOrder = 2;

                var collider = forwardExitGO.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one;
                collider.isTrigger = true;

                _forwardExitPortal = forwardExitGO.AddComponent<CaveExitPortal>();
                _forwardExitPortal.InitializeForwardExit(_caveRunManager, _levelController);
            }

            if (_forwardExitPortal != null)
            {
                var collider = _forwardExitPortal.GetComponent<BoxCollider2D>();
                if (collider == null)
                {
                    collider = _forwardExitPortal.gameObject.AddComponent<BoxCollider2D>();
                    collider.size = Vector2.one;
                    collider.isTrigger = true;
                }
                _materializedObjects.Add(_forwardExitPortal.gameObject);
                _lastMaterializationResult.ForwardExitPosition = forwardExitPos;
            }

            Debug.Log($"CaveRuntimeMaterializer: BackExit at ({generatedLevel.Entrance.x}, {generatedLevel.Entrance.y}), ForwardExit at ({generatedLevel.Exit.x}, {generatedLevel.Exit.y}).", this);
        }

        private void MaterializeResourceNodes(CaveGeneratedLevel generatedLevel)
        {
            var resourceNodesParent = new GameObject("GeneratedResourceNodes");
            resourceNodesParent.transform.SetParent(_generatedRuntimeRoot.transform);
            resourceNodesParent.transform.localPosition = Vector3.zero;

            _lastMaterializationResult.ResourceCandidateCount = generatedLevel.ResourceSpawnPoints.Count;

            if (_snapshotResourceNodeStates != null && _snapshotResourceNodeStates.Count > 0)
            {
                MaterializeResourceNodesFromSnapshot(resourceNodesParent.transform, generatedLevel);
                return;
            }

            var spawnSeedString = $"{_caveRunManager.CaveWorldSeed}_{_caveRunManager.CaveRunSeed}_{generatedLevel.CaveLevel}_resource_spawn";
            var spawnRandom = new System.Random(spawnSeedString.GetHashCode());

            int createdCount = 0;
            for (int i = 0; i < generatedLevel.ResourceSpawnPoints.Count; i++)
            {
                if (createdCount >= _maxResourceNodes)
                {
                    break;
                }

                if (spawnRandom.NextDouble() > _resourceSpawnChance)
                {
                    continue;
                }

                createdCount++;
                var spawnPoint = generatedLevel.ResourceSpawnPoints[i];
                var worldPos = GridToWorld(spawnPoint.Position, generatedLevel);
                ResourceNode resourceNode;

                if (_resourceNodePrefab != null)
                {
                    resourceNode = Instantiate(_resourceNodePrefab, worldPos, Quaternion.identity, resourceNodesParent.transform);
                }
                else
                {
                    var nodeGO = new GameObject($"ResourceNode_{spawnPoint.Position.x}_{spawnPoint.Position.y}");
                    nodeGO.transform.SetParent(resourceNodesParent.transform);
                    nodeGO.transform.position = worldPos;

                    resourceNode = nodeGO.AddComponent<ResourceNode>();
                }

                resourceNode.gameObject.name = $"ResourceNode_{spawnPoint.Position.x}_{spawnPoint.Position.y}";

                var nodeInstanceId = $"node_{generatedLevel.CaveLevel}_{spawnPoint.Position.x}_{spawnPoint.Position.y}_{generatedLevel.BiomeId}";

                SelectAndConfigureResourceNode(resourceNode, generatedLevel, nodeInstanceId, i, spawnPoint.Position);
                TrackResourceNodeSnapshot(nodeInstanceId, resourceNode, spawnPoint.Position);

                _materializedObjects.Add(resourceNode.gameObject);
                _lastMaterializationResult.CreatedResourceNodes++;
            }

            if (createdCount < _minResourceNodes && generatedLevel.ResourceSpawnPoints.Count > 0)
            {
                var firstSpawnPoint = generatedLevel.ResourceSpawnPoints[0];
                var worldPos = GridToWorld(firstSpawnPoint.Position, generatedLevel);
                ResourceNode resourceNode;

                if (_resourceNodePrefab != null)
                {
                    resourceNode = Instantiate(_resourceNodePrefab, worldPos, Quaternion.identity, resourceNodesParent.transform);
                }
                else
                {
                    var nodeGO = new GameObject($"ResourceNode_{firstSpawnPoint.Position.x}_{firstSpawnPoint.Position.y}");
                    nodeGO.transform.SetParent(resourceNodesParent.transform);
                    nodeGO.transform.position = worldPos;

                    resourceNode = nodeGO.AddComponent<ResourceNode>();
                }

                resourceNode.gameObject.name = $"ResourceNode_{firstSpawnPoint.Position.x}_{firstSpawnPoint.Position.y}";

                var nodeInstanceId = $"node_{generatedLevel.CaveLevel}_{firstSpawnPoint.Position.x}_{firstSpawnPoint.Position.y}_{generatedLevel.BiomeId}";

                SelectAndConfigureResourceNode(resourceNode, generatedLevel, nodeInstanceId, 0, firstSpawnPoint.Position);
                TrackResourceNodeSnapshot(nodeInstanceId, resourceNode, firstSpawnPoint.Position);

                _materializedObjects.Add(resourceNode.gameObject);
                _lastMaterializationResult.CreatedResourceNodes++;
            }
        }

        private void MaterializeResourceNodesFromSnapshot(Transform parent, CaveGeneratedLevel generatedLevel)
        {
            foreach (var snapshotEntry in _snapshotResourceNodeStates)
            {
                if (snapshotEntry == null || snapshotEntry.IsDepleted)
                {
                    continue;
                }

                var nodeData = FindResourceNodeData(snapshotEntry.ResourceNodeId);
                if (nodeData == null)
                {
                    Debug.LogWarning($"CaveRuntimeMaterializer: Snapshot resource node data '{snapshotEntry.ResourceNodeId}' not found. Node '{snapshotEntry.NodeInstanceId}' skipped.", this);
                    continue;
                }

                var worldPos = GridToWorld(snapshotEntry.GridPosition, generatedLevel);
                ResourceNode resourceNode;
                if (_resourceNodePrefab != null)
                {
                    resourceNode = Instantiate(_resourceNodePrefab, worldPos, Quaternion.identity, parent);
                }
                else
                {
                    var nodeGO = new GameObject($"ResourceNode_{snapshotEntry.GridPosition.x}_{snapshotEntry.GridPosition.y}");
                    nodeGO.transform.SetParent(parent);
                    nodeGO.transform.position = worldPos;
                    resourceNode = nodeGO.AddComponent<ResourceNode>();
                }

                ConfigureResourceNodeWithData(
                    resourceNode,
                    nodeData,
                    snapshotEntry.NodeInstanceId,
                    snapshotEntry.GridPosition);

                _lastResourceNodeSnapshots.Add(new CaveResourceNodeSnapshotEntry
                {
                    NodeInstanceId = snapshotEntry.NodeInstanceId,
                    ResourceNodeId = snapshotEntry.ResourceNodeId,
                    GridPosition = snapshotEntry.GridPosition,
                    IsDepleted = false
                });
                _materializedObjects.Add(resourceNode.gameObject);
                _lastMaterializationResult.CreatedResourceNodes++;
            }
        }

        private void SelectAndConfigureResourceNode(
            ResourceNode nodeInstance,
            CaveGeneratedLevel generatedLevel,
            string nodeInstanceId,
            int spawnIndex,
            Vector2Int spawnPosition)
        {
            ResourceNodeDataSO nodeData = SelectResourceNodeData(generatedLevel, spawnIndex, spawnPosition);

            if (nodeData == null)
            {
                Debug.LogWarning($"CaveRuntimeMaterializer: No resource node data available for level {generatedLevel.CaveLevel}. Destroying node instance.", this);
                Destroy(nodeInstance.gameObject);
                return;
            }

            ConfigureResourceNodeWithData(nodeInstance, nodeData, nodeInstanceId, spawnPosition);
        }

        private void ConfigureResourceNodeWithData(
            ResourceNode nodeInstance,
            ResourceNodeDataSO nodeData,
            string nodeInstanceId,
            Vector2Int spawnPosition)
        {
            if (nodeInstance == null || nodeData == null)
            {
                return;
            }

            var spriteRenderer = nodeInstance.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = nodeInstance.gameObject.AddComponent<SpriteRenderer>();
            }

            nodeInstance.gameObject.name = $"{nodeInstanceId}_{nodeData.Id}";

            spriteRenderer.sprite = GetBuiltinSprite();
            spriteRenderer.color = new Color(0.8f, 0.6f, 0.4f);
            spriteRenderer.sortingOrder = 1;

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

        private void TrackResourceNodeSnapshot(string nodeInstanceId, ResourceNode resourceNode, Vector2Int spawnPosition)
        {
            if (resourceNode == null || string.IsNullOrWhiteSpace(nodeInstanceId))
            {
                return;
            }

            var nodeDataId = ResolveResourceNodeDataId(resourceNode);
            _lastResourceNodeSnapshots.Add(new CaveResourceNodeSnapshotEntry
            {
                NodeInstanceId = nodeInstanceId,
                ResourceNodeId = nodeDataId,
                GridPosition = spawnPosition,
                IsDepleted = _caveRunManager != null && _caveRunManager.IsNodeDepleted(nodeInstanceId)
            });
        }

        private ResourceNodeDataSO FindResourceNodeData(string nodeDataId)
        {
            if (_resourceNodeDatabase == null || string.IsNullOrWhiteSpace(nodeDataId))
            {
                return null;
            }

            foreach (var node in _resourceNodeDatabase.All)
            {
                if (node != null && node.Id == nodeDataId)
                {
                    return node;
                }
            }

            return null;
        }

        private string ResolveResourceNodeDataId(ResourceNode resourceNode)
        {
            if (resourceNode == null || _resourceNodeDatabase == null)
            {
                return string.Empty;
            }

            // ResourceNode does not expose its data asset yet, so resolve by stable instance id fallback.
            foreach (var node in _resourceNodeDatabase.All)
            {
                if (node != null && resourceNode.name.Contains(node.Id))
                {
                    return node.Id;
                }
            }

            foreach (var node in _resourceNodeDatabase.All)
            {
                if (node != null)
                {
                    return node.Id;
                }
            }

            return string.Empty;
        }

        private ResourceNodeDataSO SelectResourceNodeData(CaveGeneratedLevel generatedLevel, int spawnIndex, Vector2Int spawnPosition)
        {
            if (_resourceNodeDatabase == null)
            {
                Debug.LogWarning("CaveRuntimeMaterializer: ResourceNodeDatabase not assigned.", this);
                return null;
            }

            var allNodes = _resourceNodeDatabase.All;
            if (allNodes.Count == 0)
            {
                Debug.LogWarning($"CaveRuntimeMaterializer: No resource nodes available in database.", this);
                return null;
            }

            var seedString = $"{_caveRunManager.CaveWorldSeed}_{_caveRunManager.CaveRunSeed}_{generatedLevel.CaveLevel}_resources_{spawnIndex}_{spawnPosition.x}_{spawnPosition.y}";
            var deterministicRandom = new System.Random(seedString.GetHashCode());

            var roll = deterministicRandom.NextDouble();
            ResourceNodeDataSO selectedNode = null;

            foreach (var node in allNodes)
            {
                if (node == null) continue;

                if (node.Id.Contains("stone"))
                {
                    if (roll < 0.70f)
                    {
                        selectedNode = node;
                        break;
                    }
                    roll -= 0.70f;
                }
                else if (node.Id.Contains("copper"))
                {
                    if (roll < 0.20f)
                    {
                        selectedNode = node;
                        break;
                    }
                    roll -= 0.20f;
                }
                else
                {
                    if (roll < 0.10f)
                    {
                        selectedNode = node;
                        break;
                    }
                    roll -= 0.10f;
                }
            }

            if (selectedNode == null)
            {
                foreach (var node in allNodes)
                {
                    if (node != null)
                    {
                        selectedNode = node;
                        break;
                    }
                }
            }

            return selectedNode;
        }

        private void MaterializeEnemies(CaveGeneratedLevel generatedLevel)
        {
            _lastEnemySpawnPlan = null;

            if (_generatedRuntimeRoot == null)
            {
                Debug.LogError("CaveRuntimeMaterializer: Cannot materialize enemies without generated runtime root.", this);
                return;
            }

            if (_caveRunManager == null)
            {
                Debug.LogError("CaveRuntimeMaterializer: Cannot materialize enemies because CaveRunManager is not assigned.", this);
                return;
            }

            if (_enemyDatabase == null)
            {
                LogEnemySpawnWiringWarning(generatedLevel, "EnemyDatabaseSO not assigned");
                return;
            }

            if ((_enemySpawnProfiles == null || _enemySpawnProfiles.Length == 0)
                && (_snapshotEnemySpawnPlan == null || !_snapshotEnemySpawnPlan.IsValid))
            {
                LogEnemySpawnWiringWarning(generatedLevel, "Enemy spawn profiles not assigned");
                return;
            }

            _lastEnemySpawnPlan = _snapshotEnemySpawnPlan != null && _snapshotEnemySpawnPlan.IsValid
                ? _snapshotEnemySpawnPlan
                : _enemySpawnPlanner.CreatePlan(
                    generatedLevel,
                    _caveRunManager,
                    _enemySpawnProfiles,
                    _enemySpawnPacks,
                    _enemyFactionLocks,
                    _maxEnemiesPerLevel);

            if (_lastEnemySpawnPlan == null || !_lastEnemySpawnPlan.IsValid)
            {
                Debug.LogWarning($"CaveRuntimeMaterializer: Enemy spawn plan empty for level {generatedLevel.CaveLevel}.", this);
                LogEnemySpawnPlanWarnings(_lastEnemySpawnPlan);
                return;
            }

            LogEnemySpawnPlanWarnings(_lastEnemySpawnPlan);

            var enemyParent = new GameObject("GeneratedEnemies");
            enemyParent.transform.SetParent(_generatedRuntimeRoot.transform);
            enemyParent.transform.localPosition = Vector3.zero;
            _materializedObjects.Add(enemyParent);

            foreach (var entry in _lastEnemySpawnPlan.Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.EnemyId))
                {
                    continue;
                }

                if (!_enemyDatabase.TryGetById(entry.EnemyId, out var enemyData) || enemyData == null)
                {
                    Debug.LogError($"CaveRuntimeMaterializer: EnemyDataSO '{entry.EnemyId}' not found in EnemyDatabaseSO. SpawnProfileId={entry.SpawnProfileId}, InstanceId={entry.EnemyInstanceId}.", this);
                    continue;
                }

                var enemyObject = CreateEnemyRuntimeObject(entry, enemyData, enemyParent.transform);
                _materializedObjects.Add(enemyObject);
                _lastMaterializationResult.CreatedEnemies++;

                GameEventBus.Publish(new EnemySpawnedEvent(
                    entry.EnemyId,
                    entry.WorldPosition,
                    entry.EnemyInstanceId,
                    generatedLevel.CaveLevel));
                GameEventBus.Publish(new EnemySeenEvent(
                    entry.EnemyId,
                    entry.WorldPosition,
                    entry.EnemyInstanceId,
                    generatedLevel.CaveLevel));
            }

            Debug.Log(
                $"CaveRuntimeMaterializer: Materialized {_lastMaterializationResult.CreatedEnemies} enemies for level {generatedLevel.CaveLevel}. Seed={_lastEnemySpawnPlan.LevelSeed}. LayoutHash={_lastEnemySpawnPlan.LayoutHash}.",
                this);
        }

        private void LogEnemySpawnWiringWarning(CaveGeneratedLevel generatedLevel, string cause)
        {
            var profileCount = CountAssigned(_enemySpawnProfiles);
            var packCount = CountAssigned(_enemySpawnPacks);
            var lockCount = CountAssigned(_enemyFactionLocks);
            var hasSnapshotPlan = _snapshotEnemySpawnPlan != null && _snapshotEnemySpawnPlan.IsValid;
            var snapshotEntries = _snapshotEnemySpawnPlan?.Entries?.Count ?? 0;
            var level = generatedLevel != null ? generatedLevel.CaveLevel.ToString() : "unknown";
            var walkableTiles = generatedLevel != null ? generatedLevel.WalkableTiles.Count.ToString() : "unknown";

            Debug.LogWarning(
                "CaveRuntimeMaterializer: Enemy materialization skipped. " +
                $"Cause='{cause}'. Level={level}, WalkableTiles={walkableTiles}, " +
                $"EnemyDatabaseAssigned={(_enemyDatabase != null)}, EnemyPrefabAssigned={(_enemyPrefab != null)}, " +
                $"SpawnProfiles={profileCount}, SpawnPacks={packCount}, FactionLocks={lockCount}, " +
                $"SnapshotPlanValid={hasSnapshotPlan}, SnapshotEntries={snapshotEntries}. " +
                "Expected assets: Assets/_Game/Data/Combat/EnemyDatabase.asset, " +
                "Assets/_Game/Data/EnemySpawn/Profiles, Assets/_Game/Data/EnemySpawn/Packs, " +
                "Assets/_Game/Data/EnemySpawn/FactionLocks. " +
                "Run CindarsHope/SPEC 13/Generate And Wire SPEC 13G Assets.",
                this);
        }

        private static int CountAssigned<T>(IEnumerable<T> values)
            where T : Object
        {
            var count = 0;
            if (values == null)
            {
                return count;
            }

            foreach (var value in values)
            {
                if (value != null)
                {
                    count++;
                }
            }

            return count;
        }

        private void LogEnemySpawnPlanWarnings(CaveEnemySpawnPlan plan)
        {
            if (plan?.Warnings == null || plan.Warnings.Count == 0)
            {
                return;
            }

            foreach (var warning in plan.Warnings)
            {
                if (!string.IsNullOrWhiteSpace(warning))
                {
                    Debug.LogWarning($"CaveRuntimeMaterializer enemy spawn warning: {warning}", this);
                }
            }
        }

        private GameObject CreateEnemyRuntimeObject(
            CaveEnemySpawnPlanEntry entry,
            EnemyDataSO enemyData,
            Transform parent)
        {
            GameObject enemyObject;
            if (_enemyPrefab != null)
            {
                enemyObject = Instantiate(_enemyPrefab, entry.WorldPosition, Quaternion.identity, parent);
            }
            else
            {
                enemyObject = new GameObject(entry.EnemyInstanceId);
                enemyObject.transform.SetParent(parent);
                enemyObject.transform.position = entry.WorldPosition;
            }

            enemyObject.name = entry.EnemyInstanceId;
            ConfigureEnemyRuntimeObject(enemyObject, enemyData, entry);
            return enemyObject;
        }

        private void ConfigureEnemyRuntimeObject(
            GameObject enemyObject,
            EnemyDataSO enemyData,
            CaveEnemySpawnPlanEntry entry)
        {
            var spriteRenderer = enemyObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = enemyObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = enemyData.Icon != null ? enemyData.Icon : GetBuiltinSprite();
            spriteRenderer.color = enemyData.IsElite || entry.IsElite ? new Color(1f, 0.55f, 0.25f) : new Color(0.85f, 0.23f, 0.23f);
            spriteRenderer.sortingOrder = 3;

            var scale = Mathf.Max(0.1f, enemyData.VisualScale);
            enemyObject.transform.localScale = new Vector3(scale, scale, 1f);

            var collider = enemyObject.GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = enemyObject.AddComponent<CircleCollider2D>();
            }

            collider.radius = ResolveColliderRadius(entry.SizeClass);

            var rigidbody = enemyObject.GetComponent<Rigidbody2D>();
            if (rigidbody == null)
            {
                rigidbody = enemyObject.AddComponent<Rigidbody2D>();
            }

            rigidbody.gravityScale = 0f;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

            var enemyHealth = enemyObject.GetComponent<CindarsHope.Combat.EnemyHealth>();
            if (enemyHealth == null)
            {
                enemyHealth = enemyObject.AddComponent<CindarsHope.Combat.EnemyHealth>();
            }
            enemyHealth.Configure(enemyData);

            if (enemyObject.GetComponent<EnemyVulnerabilityState>() == null)
            {
                enemyObject.AddComponent<EnemyVulnerabilityState>();
            }

            if (enemyObject.GetComponent<EnemyTelegraphController>() == null)
            {
                enemyObject.AddComponent<EnemyTelegraphController>();
            }

            if (enemyObject.GetComponent<KnockbackController>() == null)
            {
                enemyObject.AddComponent<KnockbackController>();
            }

            var hitFlash = enemyObject.GetComponent<HitFlashController>();
            if (hitFlash == null)
            {
                hitFlash = enemyObject.AddComponent<HitFlashController>();
            }

            var brain = enemyObject.GetComponent<EnemyBrain>();
            if (brain == null)
            {
                brain = enemyObject.AddComponent<EnemyBrain>();
            }
            brain.Configure(enemyData);

            var chaseController = enemyObject.GetComponent<EnemyChaseController>();
            if (chaseController == null)
            {
                chaseController = enemyObject.AddComponent<EnemyChaseController>();
            }
            chaseController.ConfigureFromData(enemyData);
            if (_playerTransform != null)
            {
                chaseController.RebindTarget(_playerTransform);
            }

            var triggerChild = new GameObject("ContactDamageTrigger");
            triggerChild.transform.SetParent(enemyObject.transform);
            triggerChild.transform.localPosition = Vector3.zero;

            var triggerCollider = triggerChild.AddComponent<CircleCollider2D>();
            triggerCollider.radius = Mathf.Max(collider.radius, 0.5f);
            triggerCollider.isTrigger = true;

            var contactDamage = triggerChild.AddComponent<EnemyContactDamage>();
            contactDamage.Configure(enemyData, triggerCollider);
        }

        private static float ResolveColliderRadius(string sizeClass)
        {
            return sizeClass switch
            {
                "Tiny" => 0.25f,
                "Small" => 0.35f,
                "Large" => 0.65f,
                "Huge" => 0.95f,
                "Boss" => 1.2f,
                _ => 0.45f
            };
        }

        private Vector2Int ResolveAnchorPosition(CaveSpawnAnchor anchor, CaveGeneratedLevel generatedLevel)
        {
            return anchor switch
            {
                CaveSpawnAnchor.Entrance => generatedLevel.Entrance,
                CaveSpawnAnchor.ForwardExit => generatedLevel.Exit,
                CaveSpawnAnchor.BackExit => generatedLevel.Entrance,
                _ => generatedLevel.Entrance
            };
        }

        private Vector2Int ResolvePlayerSpawnGrid(
            Vector2Int anchorGridPos,
            CaveSpawnAnchor anchor,
            CaveGeneratedLevel generatedLevel)
        {
            var safeSpawn = FindSafeAdjacentWalkableTile(anchorGridPos, generatedLevel);

            if (safeSpawn.HasValue)
            {
                return safeSpawn.Value;
            }

            if (generatedLevel.WalkableTiles.Count > 0)
            {
                Debug.LogWarning(
                    $"CaveRuntimeMaterializer: No safe adjacent spawn found near anchor {anchor} at {anchorGridPos}. Using first walkable tile as fallback.",
                    this);

                foreach (var tile in generatedLevel.WalkableTiles)
                {
                    return tile;
                }
            }

            Debug.LogError(
                $"CaveRuntimeMaterializer: No walkable tiles available in level {generatedLevel.CaveLevel}. Using anchor position as last resort.",
                this);

            return anchorGridPos;
        }

        private Vector2Int? FindSafeAdjacentWalkableTile(Vector2Int centerPos, CaveGeneratedLevel generatedLevel)
        {
            var directions = new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right,
                new Vector2Int(1, 1),
                new Vector2Int(1, -1),
                new Vector2Int(-1, 1),
                new Vector2Int(-1, -1),
                new Vector2Int(2, 0),
                new Vector2Int(-2, 0),
                new Vector2Int(0, 2),
                new Vector2Int(0, -2)
            };

            foreach (var direction in directions)
            {
                var candidate = centerPos + direction;
                if (generatedLevel.WalkableTiles.Contains(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        public void CleanupMaterialization()
        {
            CleanupPreviousMaterialization();
        }

        private void CleanupPreviousMaterialization()
        {
            foreach (var obj in _materializedObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            _materializedObjects.Clear();
        }

        private void OnDestroy()
        {
            CleanupPreviousMaterialization();
        }
    }
}
