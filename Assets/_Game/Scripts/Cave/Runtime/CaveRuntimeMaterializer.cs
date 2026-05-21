using System.Collections.Generic;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Resources;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
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
        [SerializeField] private ResourceNodeDatabaseSO _resourceNodeDatabase;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private CaveLevelRuntimeController _levelController;
        [SerializeField, Range(0f, 1f)] private float _resourceSpawnChance = 0.28f;
        [SerializeField] private int _minResourceNodes = 1;
        [SerializeField] private int _maxResourceNodes = 4;

        private GameObject _generatedRuntimeRoot;
        private CaveExitPortal _backExitPortal;
        private CaveExitPortal _forwardExitPortal;
        private List<GameObject> _materializedObjects = new List<GameObject>();

        public CaveExitPortal BackExitPortal => _backExitPortal;
        public CaveExitPortal ForwardExitPortal => _forwardExitPortal;
        public GameObject GeneratedRuntimeRoot => _generatedRuntimeRoot;

        private CaveRuntimeMaterializationResult _lastMaterializationResult;

        public CaveRuntimeMaterializationResult LastMaterializationResult => _lastMaterializationResult;

        public void Materialize(CaveGeneratedLevel generatedLevel, CaveSpawnAnchor spawnAnchor = CaveSpawnAnchor.Entrance)
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

            _lastMaterializationResult = new CaveRuntimeMaterializationResult();

            // Create root hierarchy
            _generatedRuntimeRoot = new GameObject("CaveGeneratedRuntime");
            _generatedRuntimeRoot.transform.position = Vector3.zero;

            MaterializeFloor(generatedLevel);
            MaterializeWalls(generatedLevel);
            MaterializeEntranceAndExit(generatedLevel);
            MaterializeResourceNodes(generatedLevel);

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

            var spriteRenderer = nodeInstance.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = nodeInstance.gameObject.AddComponent<SpriteRenderer>();
            }

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
