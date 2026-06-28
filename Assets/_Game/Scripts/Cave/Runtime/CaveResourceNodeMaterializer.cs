using System.Collections.Generic;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Resources;
using CindarsHope.Core.Data;
using CindarsHope.Equipment;
using CindarsHope.Inventory;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// Responsável por materializar nós de recursos do CaveGeneratedLevel.
    /// Preserva o determinismo de seed: a string "resource_spawn" e a ordem de
    /// spawnRandom.NextDouble() nunca devem ser alteradas (cave-stable-run / ADR-0005).
    /// </summary>
    internal sealed class CaveResourceNodeMaterializer
    {
        private readonly ResourceNodeDatabaseSO _database;
        private readonly ResourceNode _resourceNodePrefab;
        private readonly InventoryManager _inventoryManager;
        private readonly EquipmentManager _equipmentManager;
        private readonly CaveRunManager _caveRunManager;
        private readonly float _resourceSpawnChance;
        private readonly int _minResourceNodes;
        private readonly int _maxResourceNodes;

        /// <summary>
        /// Inicializa o materializer com todas as dependências necessárias.
        /// </summary>
        internal CaveResourceNodeMaterializer(
            ResourceNodeDatabaseSO database,
            ResourceNode resourceNodePrefab,
            InventoryManager inventoryManager,
            EquipmentManager equipmentManager,
            CaveRunManager caveRunManager,
            float resourceSpawnChance,
            int minResourceNodes,
            int maxResourceNodes)
        {
            _database = database;
            _resourceNodePrefab = resourceNodePrefab;
            _inventoryManager = inventoryManager;
            _equipmentManager = equipmentManager;
            _caveRunManager = caveRunManager;
            _resourceSpawnChance = resourceSpawnChance;
            _minResourceNodes = minResourceNodes;
            _maxResourceNodes = maxResourceNodes;
        }

        /// <summary>
        /// Materializa os nós de recursos do nível. Se houver snapshot, restaura a partir dele;
        /// caso contrário, gera proceduralmente com seed determinístico.
        /// </summary>
        internal void MaterializeResourceNodes(
            CaveGeneratedLevel level,
            Transform parent,
            List<GameObject> materializedObjects,
            CaveRuntimeMaterializationResult result,
            List<CaveResourceNodeSnapshotEntry> lastResourceNodeSnapshots,
            IReadOnlyList<CaveResourceNodeSnapshotEntry> snapshotResourceNodeStates)
        {
            var resourceNodesParent = new GameObject("GeneratedResourceNodes");
            resourceNodesParent.transform.SetParent(parent);
            resourceNodesParent.transform.localPosition = Vector3.zero;

            result.ResourceCandidateCount = level.ResourceSpawnPoints.Count;

            if (snapshotResourceNodeStates != null && snapshotResourceNodeStates.Count > 0)
            {
                MaterializeResourceNodesFromSnapshot(resourceNodesParent.transform, level, materializedObjects, result, lastResourceNodeSnapshots, snapshotResourceNodeStates);
                return;
            }

            // CRÍTICO: a seed string "resource_spawn" é parte do contrato cave-stable-run.
            // Não altere esta string nem a ordem das chamadas de spawnRandom.NextDouble().
            var spawnSeedString = $"{_caveRunManager.CaveWorldSeed}_{_caveRunManager.CaveRunSeed}_{level.CaveLevel}_resource_spawn";
            var spawnRandom = new System.Random(spawnSeedString.GetHashCode());

            int createdCount = 0;
            for (int i = 0; i < level.ResourceSpawnPoints.Count; i++)
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
                var spawnPoint = level.ResourceSpawnPoints[i];
                var worldPos = CaveTileMaterializer.GridToWorld(spawnPoint.Position, level);
                ResourceNode resourceNode;

                if (_resourceNodePrefab != null)
                {
                    resourceNode = Object.Instantiate(_resourceNodePrefab, worldPos, Quaternion.identity, resourceNodesParent.transform);
                }
                else
                {
                    var nodeGO = new GameObject($"ResourceNode_{spawnPoint.Position.x}_{spawnPoint.Position.y}");
                    nodeGO.transform.SetParent(resourceNodesParent.transform);
                    nodeGO.transform.position = worldPos;
                    resourceNode = nodeGO.AddComponent<ResourceNode>();
                }

                resourceNode.gameObject.name = $"ResourceNode_{spawnPoint.Position.x}_{spawnPoint.Position.y}";

                var nodeInstanceId = $"node_{level.CaveLevel}_{spawnPoint.Position.x}_{spawnPoint.Position.y}_{level.BiomeId}";

                SelectAndConfigureResourceNode(resourceNode, level, nodeInstanceId, i, spawnPoint.Position);
                TrackResourceNodeSnapshot(nodeInstanceId, resourceNode, spawnPoint.Position, lastResourceNodeSnapshots);

                materializedObjects.Add(resourceNode.gameObject);
                result.CreatedResourceNodes++;
            }

            if (createdCount < _minResourceNodes && level.ResourceSpawnPoints.Count > 0)
            {
                var firstSpawnPoint = level.ResourceSpawnPoints[0];
                var worldPos = CaveTileMaterializer.GridToWorld(firstSpawnPoint.Position, level);
                ResourceNode resourceNode;

                if (_resourceNodePrefab != null)
                {
                    resourceNode = Object.Instantiate(_resourceNodePrefab, worldPos, Quaternion.identity, resourceNodesParent.transform);
                }
                else
                {
                    var nodeGO = new GameObject($"ResourceNode_{firstSpawnPoint.Position.x}_{firstSpawnPoint.Position.y}");
                    nodeGO.transform.SetParent(resourceNodesParent.transform);
                    nodeGO.transform.position = worldPos;
                    resourceNode = nodeGO.AddComponent<ResourceNode>();
                }

                resourceNode.gameObject.name = $"ResourceNode_{firstSpawnPoint.Position.x}_{firstSpawnPoint.Position.y}";

                var nodeInstanceId = $"node_{level.CaveLevel}_{firstSpawnPoint.Position.x}_{firstSpawnPoint.Position.y}_{level.BiomeId}";

                SelectAndConfigureResourceNode(resourceNode, level, nodeInstanceId, 0, firstSpawnPoint.Position);
                TrackResourceNodeSnapshot(nodeInstanceId, resourceNode, firstSpawnPoint.Position, lastResourceNodeSnapshots);

                materializedObjects.Add(resourceNode.gameObject);
                result.CreatedResourceNodes++;
            }
        }

        private void MaterializeResourceNodesFromSnapshot(
            Transform parent,
            CaveGeneratedLevel level,
            List<GameObject> materializedObjects,
            CaveRuntimeMaterializationResult result,
            List<CaveResourceNodeSnapshotEntry> lastResourceNodeSnapshots,
            IReadOnlyList<CaveResourceNodeSnapshotEntry> snapshotResourceNodeStates)
        {
            foreach (var snapshotEntry in snapshotResourceNodeStates)
            {
                if (snapshotEntry == null || snapshotEntry.IsDepleted)
                {
                    continue;
                }

                var nodeData = FindResourceNodeData(snapshotEntry.ResourceNodeId);
                if (nodeData == null)
                {
                    Debug.LogWarning($"CaveResourceNodeMaterializer: Snapshot resource node data '{snapshotEntry.ResourceNodeId}' not found. Node '{snapshotEntry.NodeInstanceId}' skipped.");
                    continue;
                }

                var worldPos = CaveTileMaterializer.GridToWorld(snapshotEntry.GridPosition, level);
                ResourceNode resourceNode;
                if (_resourceNodePrefab != null)
                {
                    resourceNode = Object.Instantiate(_resourceNodePrefab, worldPos, Quaternion.identity, parent);
                }
                else
                {
                    var nodeGO = new GameObject($"ResourceNode_{snapshotEntry.GridPosition.x}_{snapshotEntry.GridPosition.y}");
                    nodeGO.transform.SetParent(parent);
                    nodeGO.transform.position = worldPos;
                    resourceNode = nodeGO.AddComponent<ResourceNode>();
                }

                ConfigureResourceNodeWithData(resourceNode, nodeData, snapshotEntry.NodeInstanceId, snapshotEntry.GridPosition);

                lastResourceNodeSnapshots.Add(new CaveResourceNodeSnapshotEntry
                {
                    NodeInstanceId = snapshotEntry.NodeInstanceId,
                    ResourceNodeId = snapshotEntry.ResourceNodeId,
                    GridPosition = snapshotEntry.GridPosition,
                    IsDepleted = false
                });
                materializedObjects.Add(resourceNode.gameObject);
                result.CreatedResourceNodes++;
            }
        }

        private void SelectAndConfigureResourceNode(
            ResourceNode nodeInstance,
            CaveGeneratedLevel level,
            string nodeInstanceId,
            int spawnIndex,
            Vector2Int spawnPosition)
        {
            var nodeData = SelectResourceNodeData(level, spawnIndex, spawnPosition);

            if (nodeData == null)
            {
                Debug.LogWarning($"CaveResourceNodeMaterializer: No resource node data available for level {level.CaveLevel}. Destroying node instance.");
                Object.Destroy(nodeInstance.gameObject);
                return;
            }

            ConfigureResourceNodeWithData(nodeInstance, nodeData, nodeInstanceId, spawnPosition);
        }

        /// <summary>
        /// Configura um ResourceNode com os dados fornecidos, aplicando sprite, colisão e chamando Configure.
        /// </summary>
        internal void ConfigureResourceNodeWithData(
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

            spriteRenderer.sprite = CaveTileMaterializer.GetBuiltinSprite();
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

        private void TrackResourceNodeSnapshot(
            string nodeInstanceId,
            ResourceNode resourceNode,
            Vector2Int spawnPosition,
            List<CaveResourceNodeSnapshotEntry> lastResourceNodeSnapshots)
        {
            if (resourceNode == null || string.IsNullOrWhiteSpace(nodeInstanceId))
            {
                return;
            }

            var nodeDataId = ResolveResourceNodeDataId(resourceNode);
            lastResourceNodeSnapshots.Add(new CaveResourceNodeSnapshotEntry
            {
                NodeInstanceId = nodeInstanceId,
                ResourceNodeId = nodeDataId,
                GridPosition = spawnPosition,
                IsDepleted = _caveRunManager != null && _caveRunManager.IsNodeDepleted(nodeInstanceId)
            });
        }

        /// <summary>
        /// Localiza um ResourceNodeDataSO pelo ID no database.
        /// </summary>
        internal ResourceNodeDataSO FindResourceNodeData(string nodeDataId)
        {
            if (_database == null || string.IsNullOrWhiteSpace(nodeDataId))
            {
                return null;
            }

            foreach (var node in _database.All)
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
            if (resourceNode == null || _database == null)
            {
                return string.Empty;
            }

            foreach (var node in _database.All)
            {
                if (node != null && resourceNode.name.Contains(node.Id))
                {
                    return node.Id;
                }
            }

            foreach (var node in _database.All)
            {
                if (node != null)
                {
                    return node.Id;
                }
            }

            return string.Empty;
        }

        private ResourceNodeDataSO SelectResourceNodeData(CaveGeneratedLevel level, int spawnIndex, Vector2Int spawnPosition)
        {
            if (_database == null)
            {
                Debug.LogWarning("CaveResourceNodeMaterializer: ResourceNodeDatabase not assigned.");
                return null;
            }

            var allNodes = _database.All;
            if (allNodes.Count == 0)
            {
                Debug.LogWarning("CaveResourceNodeMaterializer: No resource nodes available in database.");
                return null;
            }

            var seedString = $"{_caveRunManager.CaveWorldSeed}_{_caveRunManager.CaveRunSeed}_{level.CaveLevel}_resources_{spawnIndex}_{spawnPosition.x}_{spawnPosition.y}";
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
    }
}
