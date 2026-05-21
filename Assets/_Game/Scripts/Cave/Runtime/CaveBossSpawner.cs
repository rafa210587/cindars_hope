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
                return;
            }

            var bossEnemyData = GetBossEnemyData(bossGate.BossEnemyId);
            if (bossEnemyData == null)
            {
                return;
            }

            _playerTarget = playerTarget;

            if (generatedLevel.EnemySpawnPoints.Count == 0)
            {
                Debug.LogWarning($"CaveBossSpawner: No spawn points available for boss at level {generatedLevel.CaveLevel}.", this);
                return;
            }

            var bossSpawnPoint = generatedLevel.EnemySpawnPoints[0];
            var offsetX = generatedLevel.Width * 0.5f;
            var offsetY = generatedLevel.Height * 0.5f;
            var spawnPos = new Vector3(bossSpawnPoint.Position.x - offsetX, bossSpawnPoint.Position.y - offsetY, 0);

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

            Debug.Log(
                $"CaveBossSpawner: Spawned boss {bossEnemyData.DisplayName} (gate={bossGate.Id}) at level {generatedLevel.CaveLevel} world ({spawnPos.x}, {spawnPos.y}).",
                this);
        }

        private EnemyDataSO GetBossEnemyData(string bossEnemyId)
        {
            if (_enemyDatabase != null && _enemyDatabase.All.Count > 0)
            {
                foreach (var enemy in _enemyDatabase.All)
                {
                    if (enemy.Id == bossEnemyId)
                    {
                        return enemy;
                    }
                }
            }

            if (_fallbackEnemyData != null)
            {
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
