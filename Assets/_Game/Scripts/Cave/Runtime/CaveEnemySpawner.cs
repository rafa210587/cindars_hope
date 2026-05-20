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

        private List<GameObject> _spawnedEnemies = new List<GameObject>();

        public void SpawnEnemiesForLevel(CaveGeneratedLevel generatedLevel)
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

            foreach (var spawnPoint in generatedLevel.EnemySpawnPoints)
            {
                var availableEnemies = new List<EnemyDataSO>(_enemyDatabase.All);
                var selectedEnemy = availableEnemies[Random.Range(0, availableEnemies.Count)];
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
            enemyGO.transform.parent = transform;

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
