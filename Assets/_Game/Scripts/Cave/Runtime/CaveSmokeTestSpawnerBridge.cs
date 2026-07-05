using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// WAVE_INTEGRATION_17 — CaveSmokeTestSpawnerBridge
    ///
    /// Thin MonoBehaviour for smoke-testing the cave combat + loot loop without
    /// the full procedural CaveGeneratedLevel pipeline. Places a single enemy
    /// using existing combat components (EnemyHealth, EnemyChaseController,
    /// EnemyContactDamage, KnockbackController) and the EnemyDropSpawner for loot.
    ///
    /// Status: CODE_READY_HUMAN_UNITY_ACTION_REQUIRED
    /// Human wiring: place this on a GameObject in CaveScene, assign _smokeEnemyData,
    /// set _spawnOffset (e.g. 5 units right of player spawn), and add an EnemyDropSpawner
    /// to the same scene wired to InventoryManager.
    /// </summary>
    [DisallowMultipleComponent]
    public class CaveSmokeTestSpawnerBridge : MonoBehaviour
    {
        [Header("Smoke Enemy")]
        [SerializeField] private EnemyDataSO _smokeEnemyData;
        [SerializeField] private Vector2 _spawnOffset = new Vector2(5f, 0f);
        [SerializeField] private bool _spawnOnStart = true;

        private GameObject _spawnedEnemy;
        private bool _hasSpawned;

        private void Start()
        {
            if (_spawnOnStart)
            {
                SpawnSmokeEnemy();
            }
        }

        /// <summary>
        /// Spawns a standalone smoke-test enemy at transform.position + _spawnOffset.
        /// Reuses the same component setup as CaveEnemySpawner to stay consistent
        /// with the full cave runtime. Safe to call from Play Mode or editor tools.
        /// </summary>
        public void SpawnSmokeEnemy()
        {
            if (_hasSpawned)
            {
                Debug.Log("[WAVE17] CaveSmokeTestSpawnerBridge: smoke enemy already spawned; skipping duplicate.", this);
                return;
            }

            if (_smokeEnemyData == null)
            {
                Debug.LogWarning("[WAVE17] CaveSmokeTestSpawnerBridge: _smokeEnemyData is null. Assign an EnemyDataSO in the inspector.", this);
                return;
            }

            var spawnPos = (Vector2)transform.position + _spawnOffset;
            _spawnedEnemy = CreateSmokeEnemyAt(spawnPos);
            _hasSpawned = true;

            Debug.Log($"[WAVE17] CaveSmokeTestSpawnerBridge: spawned smoke enemy '{_smokeEnemyData.DisplayName}' " +
                      $"(id={_smokeEnemyData.enemyId}) at {spawnPos}.", this);

            GameEventBus.Publish(new EnemySpawnedEvent(
                _smokeEnemyData.enemyId,
                spawnPos,
                $"{_smokeEnemyData.enemyId}_smoke_01",
                caveLevel: 1));
        }

        private GameObject CreateSmokeEnemyAt(Vector2 position)
        {
            var enemyGO = new GameObject($"SmokeEnemy_{_smokeEnemyData.DisplayName}");
            enemyGO.transform.position = position;
            enemyGO.transform.parent = transform;

            // Visual — red placeholder square
            var spriteRenderer = enemyGO.AddComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(0.85f, 0.23f, 0.23f);
            spriteRenderer.sortingOrder = 0;
            spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
            spriteRenderer.sortingLayerName = CaveWorldSortingLayers.World;

            // Physics
            var col = enemyGO.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;

            var rb = enemyGO.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Combat components (existing runtime — REUSE_EXISTING)
            var enemyHealth = enemyGO.AddComponent<EnemyHealth>();
            enemyHealth.Configure(_smokeEnemyData);

            enemyGO.AddComponent<KnockbackController>();
            enemyGO.AddComponent<HitFlashController>();

            var chase = enemyGO.AddComponent<EnemyChaseController>();
            chase.ConfigureFromData(_smokeEnemyData);

            // Try to bind player target from GameBootstrap
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap != null && bootstrap.PlayerManager != null)
            {
                var playerTransform = bootstrap.PlayerManager.transform;
                chase.RebindTarget(playerTransform);
            }

            // Contact damage trigger child
            var triggerChild = new GameObject("ContactDamageTrigger");
            triggerChild.transform.SetParent(enemyGO.transform);
            triggerChild.transform.localPosition = Vector3.zero;

            var triggerCol = triggerChild.AddComponent<CircleCollider2D>();
            triggerCol.radius = 0.5f;
            triggerCol.isTrigger = true;

            var contactDmg = triggerChild.AddComponent<EnemyContactDamage>();
            contactDmg.Configure(_smokeEnemyData, triggerCol);

            return enemyGO;
        }

        private void OnDestroy()
        {
            if (_spawnedEnemy != null)
            {
                Destroy(_spawnedEnemy);
            }
        }
    }
}
