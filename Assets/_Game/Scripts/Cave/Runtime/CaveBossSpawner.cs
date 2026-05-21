using CindarsHope.Combat;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Generation;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    public sealed class CaveBossSpawner : MonoBehaviour
    {
        [SerializeField] private CaveBossGateRegistrySO _bossGateRegistry;
        [SerializeField] private CaveRunManager _caveRunManager;
        [SerializeField] private DataRegistrySO<EnemyDataSO> _enemyDatabase;
        [SerializeField] private EnemyDataSO _fallbackEnemyData;

        private GameObject _spawnedBoss;
        private Transform _playerTarget;

        public void SpawnBossForLevel(CaveGeneratedLevel generatedLevel, GameObject generatedRuntimeRoot, Transform playerTarget = null)
        {
            if (generatedLevel == null || _bossGateRegistry == null)
            {
                return;
            }

            CleanupBoss();

            var bossGate = _bossGateRegistry.GetGateByLevel(generatedLevel.CaveLevel);
            if (bossGate == null)
            {
                Debug.Log($"CaveBossSpawner: No boss gate for level {generatedLevel.CaveLevel}. Skipping boss spawn.", this);
                return;
            }

            if (_caveRunManager != null && _caveRunManager.IsBossDefeated(bossGate.Id))
            {
                Debug.Log($"CaveBossSpawner: Boss gate '{bossGate.Id}' already defeated. Skipping boss spawn.", this);
                return;
            }

            Debug.Log($"CaveBossSpawner: Boss gate found for level {generatedLevel.CaveLevel}: {bossGate.Id}.", this);

            var bossEnemyData = GetBossEnemyData(bossGate.BossEnemyId);
            if (bossEnemyData == null)
            {
                Debug.LogWarning($"CaveBossSpawner: Boss enemy data not found for '{bossGate.BossEnemyId}'.", this);
                return;
            }

            _playerTarget = playerTarget;

            var bossGridPos = ResolveBossSpawnNearGate(generatedLevel, playerTarget);
            var spawnPos = GridToWorld(bossGridPos, generatedLevel);
            var distanceToGate = Vector2Int.Distance(bossGridPos, generatedLevel.Exit);

            _spawnedBoss = new GameObject($"Boss_{bossEnemyData.DisplayName}");
            _spawnedBoss.transform.position = spawnPos;
            _spawnedBoss.transform.parent = generatedRuntimeRoot.transform;

            var spriteRenderer = _spawnedBoss.AddComponent<SpriteRenderer>();
            if (bossEnemyData.Icon != null)
            {
                spriteRenderer.sprite = bossEnemyData.Icon;
                spriteRenderer.color = GetBossColor();
            }
            else
            {
                spriteRenderer.sprite = GetBuiltinSprite();
                spriteRenderer.color = GetBossColor();
            }
            spriteRenderer.sortingOrder = 3;

            _spawnedBoss.transform.localScale = Vector3.one * 1.2f;

            var collider = _spawnedBoss.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f;

            var rigidbody = _spawnedBoss.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

            var enemyHealth = _spawnedBoss.AddComponent<EnemyHealth>();
            enemyHealth.Configure(bossEnemyData);

            var knockback = _spawnedBoss.AddComponent<KnockbackController>();
            var hitFlash = _spawnedBoss.AddComponent<HitFlashController>();

            var chaseController = _spawnedBoss.AddComponent<EnemyChaseController>();
            chaseController.ConfigureFromData(bossEnemyData);
            if (_playerTarget != null)
            {
                chaseController.RebindTarget(_playerTarget);
            }

            var triggerChild = new GameObject("ContactDamageTrigger");
            triggerChild.transform.SetParent(_spawnedBoss.transform);
            triggerChild.transform.localPosition = Vector3.zero;

            var triggerCollider = triggerChild.AddComponent<CircleCollider2D>();
            triggerCollider.radius = 0.5f;
            triggerCollider.isTrigger = true;

            var contactDamage = triggerChild.AddComponent<EnemyContactDamage>();
            contactDamage.Configure(bossEnemyData, triggerCollider);

            // Tag boss for identification
            _spawnedBoss.tag = "BossEnemy";

            // Add death reporter to track boss kill and unlock gate
            var bossDeathReporter = _spawnedBoss.AddComponent<CaveBossDeathReporter>();
            bossDeathReporter.Configure(
                _caveRunManager,
                bossGate.Id,
                bossEnemyData.enemyId,
                generatedLevel.CaveLevel,
                bossGate.CheckpointUnlockedOnDefeat,
                spawnPos);

            Debug.Log(
                $"CaveBossSpawner: Boss spawn resolved near ForwardExit. GateGrid={generatedLevel.Exit}, BossGrid={bossGridPos}, DistanceToGate={distanceToGate}.",
                this);

            Debug.Log(
                $"CaveBossSpawner: Spawned boss {bossEnemyData.DisplayName} (gate={bossGate.Id}) at level {generatedLevel.CaveLevel} world ({spawnPos.x}, {spawnPos.y}).",
                this);
        }

        private Vector2Int ResolveBossSpawnNearGate(CaveGeneratedLevel level, Transform playerTarget)
        {
            var exit = level.Exit;
            var best = default(Vector2Int);
            var hasBest = false;
            var bestScore = float.MaxValue;

            foreach (var tile in level.WalkableTiles)
            {
                if (tile == level.Exit || tile == level.Entrance)
                {
                    continue;
                }

                var distanceToExit = Vector2Int.Distance(tile, exit);
                if (distanceToExit < 2f || distanceToExit > 6f)
                {
                    continue;
                }

                if (playerTarget != null)
                {
                    var world = GridToWorld(tile, level);
                    if (Vector3.Distance(world, playerTarget.position) < 3f)
                    {
                        continue;
                    }
                }

                var score = Mathf.Abs(distanceToExit - 3f);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = tile;
                    hasBest = true;
                }
            }

            if (hasBest)
            {
                return best;
            }

            return FindEnemySpawnPointClosestToExit(level);
        }

        private Vector2Int FindEnemySpawnPointClosestToExit(CaveGeneratedLevel level)
        {
            if (level.EnemySpawnPoints.Count == 0)
            {
                return level.Exit;
            }

            var exit = level.Exit;
            var bestPoint = level.EnemySpawnPoints[0].Position;
            var bestDist = Vector2Int.Distance(bestPoint, exit);

            for (int i = 1; i < level.EnemySpawnPoints.Count; i++)
            {
                var point = level.EnemySpawnPoints[i].Position;
                var dist = Vector2Int.Distance(point, exit);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestPoint = point;
                }
            }

            return bestPoint;
        }

        private Vector3 GridToWorld(Vector2Int gridPos, CaveGeneratedLevel level)
        {
            var offsetX = level.Width * 0.5f;
            var offsetY = level.Height * 0.5f;
            return new Vector3(gridPos.x - offsetX, gridPos.y - offsetY, 0);
        }

        private EnemyDataSO GetBossEnemyData(string bossEnemyId)
        {
            if (_enemyDatabase != null && _enemyDatabase.All.Count > 0)
            {
                foreach (var enemy in _enemyDatabase.All)
                {
                    if (enemy != null && enemy.enemyId == bossEnemyId)
                    {
                        return enemy;
                    }
                }

                foreach (var enemy in _enemyDatabase.All)
                {
                    if (enemy != null && enemy.name == bossEnemyId)
                    {
                        return enemy;
                    }
                }
            }

            if (_fallbackEnemyData != null)
            {
                Debug.LogWarning($"CaveBossSpawner: BossEnemyId '{bossEnemyId}' not found. Using fallback enemy data. Boss death may not be uniquely trackable.", this);
                return _fallbackEnemyData;
            }

            return null;
        }

        private Color GetBossColor()
        {
            return new Color(1.0f, 0.5f, 0.0f, 1.0f);
        }

        private Sprite GetBuiltinSprite()
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
#else
            return null;
#endif
        }

        public void CleanupBoss()
        {
            if (_spawnedBoss != null)
            {
                Destroy(_spawnedBoss);
                _spawnedBoss = null;
            }
        }

        private void OnDestroy()
        {
            CleanupBoss();
        }
    }
}
