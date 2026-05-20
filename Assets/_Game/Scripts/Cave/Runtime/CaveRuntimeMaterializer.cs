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

        private List<GameObject> _materializedObjects = new List<GameObject>();

        public void Materialize(CaveGeneratedLevel generatedLevel)
        {
            if (generatedLevel == null)
            {
                Debug.LogError("CaveRuntimeMaterializer: Cannot materialize null CaveGeneratedLevel.");
                return;
            }

            CleanupPreviousMaterialization();

            MaterializeFloor(generatedLevel);
            MaterializeWalls(generatedLevel);
            MaterializeEntranceAndExit(generatedLevel);
            MaterializeResourceNodes(generatedLevel);

            Debug.Log(
                $"CaveRuntimeMaterializer: Materialized level {generatedLevel.CaveLevel}. Floor tiles: {generatedLevel.WalkableTiles.Count}, Walls: {generatedLevel.WallTiles.Count}, Resources: {generatedLevel.ResourceSpawnPoints.Count}, Enemies: {generatedLevel.EnemySpawnPoints.Count}.",
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

            foreach (var tilePos in generatedLevel.WalkableTiles)
            {
                var worldPos = new Vector3(tilePos.x, tilePos.y, 0);
                var floorTile = Instantiate(_floorTilePrefab, worldPos, Quaternion.identity, transform);
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

            foreach (var tilePos in generatedLevel.WallTiles)
            {
                var worldPos = new Vector3(tilePos.x, tilePos.y, 0);
                var wallTile = Instantiate(_wallTilePrefab, worldPos, Quaternion.identity, transform);
                wallTile.gameObject.name = $"WallTile_{tilePos.x}_{tilePos.y}";
                wallTile.sortingOrder = 0;

                var collider = wallTile.gameObject.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one;

                _materializedObjects.Add(wallTile.gameObject);
            }
        }

        private void MaterializeEntranceAndExit(CaveGeneratedLevel generatedLevel)
        {
            var entranceWorldPos = new Vector3(generatedLevel.Entrance.x, generatedLevel.Entrance.y, 0);

            if (_entrancePrefab != null)
            {
                var entrancePortal = Instantiate(_entrancePrefab, entranceWorldPos, Quaternion.identity, transform);
                entrancePortal.gameObject.name = "CaveEntrance";

                var entranceCollider = entrancePortal.GetComponent<BoxCollider2D>();
                if (entranceCollider == null)
                {
                    entranceCollider = entrancePortal.gameObject.AddComponent<BoxCollider2D>();
                }
                entranceCollider.size = Vector2.one;
                entranceCollider.isTrigger = true;

                _materializedObjects.Add(entrancePortal.gameObject);
            }
            else
            {
                Debug.LogWarning("CaveRuntimeMaterializer: Entrance portal prefab not assigned. Skipping entrance.", this);
            }

            var exitWorldPos = new Vector3(generatedLevel.Exit.x, generatedLevel.Exit.y, 0);

            if (_exitPortalPrefab != null)
            {
                var exitPortal = Instantiate(_exitPortalPrefab, exitWorldPos, Quaternion.identity, transform);
                exitPortal.gameObject.name = "CaveExit";

                var exitCollider = exitPortal.GetComponent<BoxCollider2D>();
                if (exitCollider == null)
                {
                    exitCollider = exitPortal.gameObject.AddComponent<BoxCollider2D>();
                }
                exitCollider.size = Vector2.one;
                exitCollider.isTrigger = true;

                _materializedObjects.Add(exitPortal.gameObject);
            }
            else
            {
                Debug.LogWarning("CaveRuntimeMaterializer: Exit portal prefab not assigned. Skipping exit.", this);
            }

            Debug.Log($"CaveRuntimeMaterializer: Entrance at ({generatedLevel.Entrance.x}, {generatedLevel.Entrance.y}), Exit at ({generatedLevel.Exit.x}, {generatedLevel.Exit.y}).", this);
        }

        private void MaterializeResourceNodes(CaveGeneratedLevel generatedLevel)
        {
            if (_resourceNodePrefab == null)
            {
                Debug.LogWarning("CaveRuntimeMaterializer: ResourceNode prefab not assigned. Skipping resource node materialization.", this);
                return;
            }

            foreach (var spawnPoint in generatedLevel.ResourceSpawnPoints)
            {
                var worldPos = new Vector3(spawnPoint.Position.x, spawnPoint.Position.y, 0);
                var resourceNode = Instantiate(_resourceNodePrefab, worldPos, Quaternion.identity, transform);
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

            var nodeList = new List<ResourceNodeDataSO>(allNodes);
            return nodeList[Random.Range(0, nodeList.Count)];
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
