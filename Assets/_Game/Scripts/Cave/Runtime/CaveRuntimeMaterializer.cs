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

        private GameObject _generatedRuntimeRoot;
        private CaveExitPortal _backExitPortal;
        private CaveExitPortal _forwardExitPortal;
        private List<GameObject> _materializedObjects = new List<GameObject>();

        public CaveExitPortal BackExitPortal => _backExitPortal;
        public CaveExitPortal ForwardExitPortal => _forwardExitPortal;
        public GameObject GeneratedRuntimeRoot => _generatedRuntimeRoot;

        public void Materialize(CaveGeneratedLevel generatedLevel)
        {
            if (generatedLevel == null)
            {
                Debug.LogError("CaveRuntimeMaterializer: Cannot materialize null CaveGeneratedLevel.");
                return;
            }

            CleanupPreviousMaterialization();

            // Create root hierarchy
            _generatedRuntimeRoot = new GameObject("CaveGeneratedRuntime");
            _generatedRuntimeRoot.transform.position = Vector3.zero;

            MaterializeFloor(generatedLevel);
            MaterializeWalls(generatedLevel);
            MaterializeEntranceAndExit(generatedLevel);
            MaterializeResourceNodes(generatedLevel);

            // Spawn player at entrance
            if (_playerTransform != null)
            {
                _playerTransform.position = new Vector3(generatedLevel.Entrance.x, generatedLevel.Entrance.y, 0);
            }

            Debug.Log(
                $"CaveRuntimeMaterializer: Materialized level {generatedLevel.CaveLevel}. Floor: {generatedLevel.WalkableTiles.Count}, Walls: {generatedLevel.WallTiles.Count}, Resources: {generatedLevel.ResourceSpawnPoints.Count}, Enemies: {generatedLevel.EnemySpawnPoints.Count}. BackExit: ({generatedLevel.Entrance.x},{generatedLevel.Entrance.y}), ForwardExit: ({generatedLevel.Exit.x},{generatedLevel.Exit.y})",
                this);

            GameEventBus.Publish(new CaveRuntimeMaterializationCompleteEvent(generatedLevel));
        }

        private void MaterializeFloor(CaveGeneratedLevel generatedLevel)
        {
            if (_floorTilePrefab == null)
            {
                Debug.LogWarning("CaveRuntimeMaterializer: Floor tile prefab not assigned. Skipping floor materialization.", this);
                return;
            }

            var floorParent = new GameObject("GeneratedFloor");
            floorParent.transform.SetParent(_generatedRuntimeRoot.transform);
            floorParent.transform.localPosition = Vector3.zero;

            foreach (var tilePos in generatedLevel.WalkableTiles)
            {
                var worldPos = new Vector3(tilePos.x, tilePos.y, 0);
                var floorTile = Instantiate(_floorTilePrefab, worldPos, Quaternion.identity, floorParent.transform);
                floorTile.gameObject.name = $"FloorTile_{tilePos.x}_{tilePos.y}";
                floorTile.sortingOrder = 0;
                _materializedObjects.Add(floorTile.gameObject);
            }
        }

        private void MaterializeWalls(CaveGeneratedLevel generatedLevel)
        {
            if (_wallTilePrefab == null)
            {
                Debug.LogWarning("CaveRuntimeMaterializer: Wall tile prefab not assigned. Skipping wall materialization.", this);
                return;
            }

            var wallParent = new GameObject("GeneratedWalls");
            wallParent.transform.SetParent(_generatedRuntimeRoot.transform);
            wallParent.transform.localPosition = Vector3.zero;

            foreach (var tilePos in generatedLevel.WallTiles)
            {
                var worldPos = new Vector3(tilePos.x, tilePos.y, 0);
                var wallTile = Instantiate(_wallTilePrefab, worldPos, Quaternion.identity, wallParent.transform);
                wallTile.gameObject.name = $"WallTile_{tilePos.x}_{tilePos.y}";
                wallTile.sortingOrder = 0;

                var collider = wallTile.gameObject.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one;

                _materializedObjects.Add(wallTile.gameObject);
            }
        }

        private void MaterializeEntranceAndExit(CaveGeneratedLevel generatedLevel)
        {
            var portalsParent = new GameObject("GeneratedExits");
            portalsParent.transform.SetParent(_generatedRuntimeRoot.transform);
            portalsParent.transform.localPosition = Vector3.zero;

            // BackExit at entrance position
            var backExitPos = new Vector3(generatedLevel.Entrance.x, generatedLevel.Entrance.y, 0);
            if (_exitPortalPrefab != null)
            {
                _backExitPortal = Instantiate(_exitPortalPrefab, backExitPos, Quaternion.identity, portalsParent.transform);
                _backExitPortal.gameObject.name = "GeneratedBackExit";

                var collider = _backExitPortal.GetComponent<BoxCollider2D>();
                if (collider == null)
                {
                    collider = _backExitPortal.gameObject.AddComponent<BoxCollider2D>();
                }
                collider.size = Vector2.one;
                collider.isTrigger = true;

                _materializedObjects.Add(_backExitPortal.gameObject);
            }
            else
            {
                Debug.LogWarning("CaveRuntimeMaterializer: Exit portal prefab not assigned. BackExit skipped.", this);
            }

            // ForwardExit at exit position
            var forwardExitPos = new Vector3(generatedLevel.Exit.x, generatedLevel.Exit.y, 0);
            if (_exitPortalPrefab != null)
            {
                _forwardExitPortal = Instantiate(_exitPortalPrefab, forwardExitPos, Quaternion.identity, portalsParent.transform);
                _forwardExitPortal.gameObject.name = "GeneratedForwardExit";

                var collider = _forwardExitPortal.GetComponent<BoxCollider2D>();
                if (collider == null)
                {
                    collider = _forwardExitPortal.gameObject.AddComponent<BoxCollider2D>();
                }
                collider.size = Vector2.one;
                collider.isTrigger = true;

                _materializedObjects.Add(_forwardExitPortal.gameObject);
            }
            else
            {
                Debug.LogWarning("CaveRuntimeMaterializer: Exit portal prefab not assigned. ForwardExit skipped.", this);
            }

            Debug.Log($"CaveRuntimeMaterializer: BackExit at ({generatedLevel.Entrance.x}, {generatedLevel.Entrance.y}), ForwardExit at ({generatedLevel.Exit.x}, {generatedLevel.Exit.y}).", this);
        }

        private void MaterializeResourceNodes(CaveGeneratedLevel generatedLevel)
        {
            if (_resourceNodePrefab == null)
            {
                Debug.LogWarning("CaveRuntimeMaterializer: ResourceNode prefab not assigned. Skipping resource node materialization.", this);
                return;
            }

            var resourceNodesParent = new GameObject("GeneratedResourceNodes");
            resourceNodesParent.transform.SetParent(_generatedRuntimeRoot.transform);
            resourceNodesParent.transform.localPosition = Vector3.zero;

            foreach (var spawnPoint in generatedLevel.ResourceSpawnPoints)
            {
                var worldPos = new Vector3(spawnPoint.Position.x, spawnPoint.Position.y, 0);
                var resourceNode = Instantiate(_resourceNodePrefab, worldPos, Quaternion.identity, resourceNodesParent.transform);
                resourceNode.gameObject.name = $"ResourceNode_{spawnPoint.Position.x}_{spawnPoint.Position.y}";

                var nodeInstanceId = $"node_{generatedLevel.CaveLevel}_{spawnPoint.Position.x}_{spawnPoint.Position.y}_{generatedLevel.BiomeId}";

                SelectAndConfigureResourceNode(resourceNode, generatedLevel, nodeInstanceId);

                _materializedObjects.Add(resourceNode.gameObject);
            }
        }

        private void SelectAndConfigureResourceNode(
            ResourceNode nodeInstance,
            CaveGeneratedLevel generatedLevel,
            string nodeInstanceId)
        {
            ResourceNodeDataSO nodeData = SelectResourceNodeData(generatedLevel);

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

            nodeInstance.Configure(
                nodeInstanceId,
                nodeData,
                _inventoryManager,
                _equipmentManager,
                _caveRunManager,
                spriteRenderer);
        }

        private ResourceNodeDataSO SelectResourceNodeData(CaveGeneratedLevel generatedLevel)
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

            // Deterministic selection based on seeds
            var seedString = $"{_caveRunManager.CaveWorldSeed}_{_caveRunManager.CaveRunSeed}_{generatedLevel.CaveLevel}_resources";
            var deterministicRandom = new System.Random(seedString.GetHashCode());
            var nodeList = new List<ResourceNodeDataSO>(allNodes);
            return nodeList[deterministicRandom.Next(0, nodeList.Count)];
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
