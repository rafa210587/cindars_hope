using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Cave.Generation;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    public sealed class CaveEnemySpawner : MonoBehaviour
    {
        [SerializeField] private DataRegistrySO<EnemyDataSO> _enemyDatabase;
        [SerializeField] private CaveRunManager _caveRunManager;

        private List<GameObject> _spawnedEnemies = new List<GameObject>();
        private GameObject _generatedEnemiesRoot;
        private Transform _playerTarget;

        public void SpawnEnemiesForLevel(CaveGeneratedLevel generatedLevel, GameObject generatedRuntimeRoot, Transform playerTarget = null)
        {
            if (generatedLevel == null)
            {
                Debug.LogError("CaveEnemySpawner: Cannot spawn enemies for null CaveGeneratedLevel.");
                return;
            }

            CleanupPreviousSpawns();

            if (_enemyDatabase == null || _enemyDatabase.All.Count == 0)
            {
                Debug.LogWarning("CaveEnemySpawner: No enemies in database. Skipping enemy spawning.", this);
                return;
            }

            // Create GeneratedEnemies parent
            _generatedEnemiesRoot = new GameObject("GeneratedEnemies");
            _generatedEnemiesRoot.transform.SetParent(generatedRuntimeRoot.transform);
            _generatedEnemiesRoot.transform.localPosition = Vector3.zero;

            _playerTarget = playerTarget;

            // Deterministic enemy selection using world + run + level seeds
            var seedString = _caveRunManager != null
                ? $"{_caveRunManager.CaveWorldSeed}_{_caveRunManager.CaveRunSeed}_{generatedLevel.CaveLevel}_enemies"
                : $"{generatedLevel.CaveLevel}_enemies";
            var deterministicRandom = new System.Random(seedString.GetHashCode());
            var availableEnemies = new List<EnemyDataSO>(_enemyDatabase.All);

            foreach (var spawnPoint in generatedLevel.EnemySpawnPoints)
            {
                var selectedEnemy = availableEnemies[deterministicRandom.Next(0, availableEnemies.Count)];
                SpawnEnemyAtPoint(selectedEnemy, spawnPoint.Position);
            }

            Debug.Log($"CaveEnemySpawner: Spawned {_spawnedEnemies.Count} enemies for level {generatedLevel.CaveLevel}.", this);
        }

        private void SpawnEnemyAtPoint(EnemyDataSO enemyData, Vector2Int worldPosition)
        {
            if (enemyData == null)
            {
                return;
            }

            var spawnPos = new Vector3(worldPosition.x, worldPosition.y, 0);
            var enemyGO = new GameObject($"Enemy_{enemyData.DisplayName}");
            enemyGO.transform.position = spawnPos;
            enemyGO.transform.parent = _generatedEnemiesRoot.transform;

            var spriteRenderer = enemyGO.AddComponent<SpriteRenderer>();
            if (enemyData.Icon != null)
            {
                spriteRenderer.sprite = enemyData.Icon;
            }
            spriteRenderer.sortingOrder = 1;

            var collider = enemyGO.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f;

            var rigidbody = enemyGO.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

            var enemyHealth = enemyGO.AddComponent<EnemyHealth>();
            enemyHealth.Configure(enemyData);

            var knockback = enemyGO.AddComponent<KnockbackController>();
            var hitFlash = enemyGO.AddComponent<HitFlashController>();

            var chaseController = enemyGO.AddComponent<EnemyChaseController>();
            chaseController.ConfigureFromData(enemyData);
            if (_playerTarget != null)
            {
                chaseController.RebindTarget(_playerTarget);
            }

            // Create trigger child for contact damage
            var triggerChild = new GameObject("ContactDamageTrigger");
            triggerChild.transform.SetParent(enemyGO.transform);
            triggerChild.transform.localPosition = Vector3.zero;

            var triggerCollider = triggerChild.AddComponent<CircleCollider2D>();
            triggerCollider.radius = 0.5f;
            triggerCollider.isTrigger = true;

            var contactDamage = triggerChild.AddComponent<EnemyContactDamage>();
            contactDamage.Configure(enemyData, triggerCollider);

            _spawnedEnemies.Add(enemyGO);

            Debug.Log($"CaveEnemySpawner: Spawned {enemyData.DisplayName} at ({worldPosition.x}, {worldPosition.y}).", this);
        }

        public void CleanupSpawns()
        {
            CleanupPreviousSpawns();
        }

        private void CleanupPreviousSpawns()
        {
            foreach (var enemy in _spawnedEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            _spawnedEnemies.Clear();
        }

        private void OnDestroy()
        {
            CleanupPreviousSpawns();
        }
    }
}
