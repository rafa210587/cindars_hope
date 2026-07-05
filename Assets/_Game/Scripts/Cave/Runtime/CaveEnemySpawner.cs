using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Generation;
using CindarsHope.Core.Data;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CindarsHope.Cave.Runtime
{
    public sealed class CaveEnemySpawner : MonoBehaviour
    {
        [SerializeField] private DataRegistrySO<EnemyDataSO> _enemyDatabase;
        [SerializeField] private CaveRunManager _caveRunManager;
        [SerializeField] private EnemyDataSO _fallbackEnemyData;
        [SerializeField] private CaveEcosystemBalanceSO _ecosystemBalance;

        private List<GameObject> _spawnedEnemies = new List<GameObject>();
        private List<string> _spawnedEnemyIds = new List<string>();
        private GameObject _generatedEnemiesRoot;
        private Transform _playerTarget;

        public List<string> LastSpawnedEnemyIds => new List<string>(_spawnedEnemyIds);

        public void SpawnEnemiesForLevel(CaveGeneratedLevel generatedLevel, GameObject generatedRuntimeRoot, Transform playerTarget = null)
        {
            if (generatedLevel == null)
            {
                Debug.LogError("CaveEnemySpawner: Cannot spawn enemies for null CaveGeneratedLevel.");
                return;
            }

            CleanupPreviousSpawns();
            _spawnedEnemyIds.Clear();

            List<EnemyDataSO> availableEnemies;
            if (_enemyDatabase != null && _enemyDatabase.All.Count > 0)
            {
                availableEnemies = new List<EnemyDataSO>(_enemyDatabase.All);
            }
            else if (_fallbackEnemyData != null)
            {
                availableEnemies = new List<EnemyDataSO> { _fallbackEnemyData };
                Debug.LogWarning("CaveEnemySpawner: Using fallback enemy data (database empty).", this);
            }
            else
            {
                Debug.LogWarning("CaveEnemySpawner: No enemies in database and no fallback. Skipping enemy spawning.", this);
                return;
            }

            // Create GeneratedEnemies parent
            _generatedEnemiesRoot = new GameObject("GeneratedEnemies");
            _generatedEnemiesRoot.transform.SetParent(generatedRuntimeRoot.transform);
            _generatedEnemiesRoot.transform.localPosition = Vector3.zero;

            _playerTarget = playerTarget;

            // Deterministic enemy selection using world + run + level seeds.
            // spec_codex_09 / cave-stable-run: string.GetHashCode() nao e garantido estavel entre
            // processos/runtimes; usa o FNV-1a determinístico ja existente no projeto.
            var seedString = _caveRunManager != null
                ? $"{_caveRunManager.CaveWorldSeed}_{_caveRunManager.CaveRunSeed}_{generatedLevel.CaveLevel}_enemies"
                : $"{generatedLevel.CaveLevel}_enemies";
            var deterministicRandom = new System.Random(CaveLayoutStableHash.Compute(seedString));

            var sortedSpawnPoints = generatedLevel.EnemySpawnPoints
                .OrderBy(sp => Vector2.Distance(sp.Position, generatedLevel.Entrance))
                .ToList();

            foreach (var spawnPoint in sortedSpawnPoints)
            {
                var selectedEnemy = availableEnemies[deterministicRandom.Next(0, availableEnemies.Count)];
                SpawnEnemyAtPoint(selectedEnemy, spawnPoint.Position, generatedLevel);
            }

            Debug.Log($"CaveEnemySpawner: Spawned {_spawnedEnemies.Count} enemies for level {generatedLevel.CaveLevel}.", this);
        }

        private void SpawnEnemyAtPoint(EnemyDataSO enemyData, Vector2Int gridPosition, CaveGeneratedLevel generatedLevel)
        {
            if (enemyData == null)
            {
                return;
            }

            var offsetX = generatedLevel.Width * 0.5f;
            var offsetY = generatedLevel.Height * 0.5f;
            var spawnPos = new Vector3(gridPosition.x - offsetX, gridPosition.y - offsetY, 0);
            var enemyGO = new GameObject($"Enemy_{enemyData.DisplayName}");
            enemyGO.transform.position = spawnPos;
            enemyGO.transform.parent = _generatedEnemiesRoot.transform;

            var spriteRenderer = enemyGO.AddComponent<SpriteRenderer>();
            if (enemyData.Icon != null)
            {
                spriteRenderer.sprite = enemyData.Icon;
                spriteRenderer.color = Color.white;
            }
            else
            {
                spriteRenderer.sprite = GetBuiltinSprite();
                spriteRenderer.color = new Color(0.85f, 0.23f, 0.23f);
            }
            spriteRenderer.sortingOrder = 0;
            spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
            spriteRenderer.sortingLayerName = CaveWorldSortingLayers.World;

            // Data-driven scale via EnemyScaleResolver (player-relative), identical ao caminho do
            // CaveRuntimeMaterializer. VisualScale hardcoded era ignorado para size class; agora
            // Tiny/Small/Medium/Large/Huge/Boss aparecem em tamanhos distintos tambem neste spawner legado.
            var visualScale = Mathf.Max(0.1f,
                EnemyScaleResolver.ResolveVisualScale(enemyData.BestiarySize, false, false));
            enemyGO.transform.localScale = new Vector3(visualScale, visualScale, 1f);

            var collider = enemyGO.AddComponent<CircleCollider2D>();
            collider.radius = EnemyScaleResolver.ColliderRadiusFor(enemyData.BestiarySize);

            var rigidbody = enemyGO.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

            var enemyHealth = enemyGO.AddComponent<EnemyHealth>();
            var hpMult = _ecosystemBalance != null ? _ecosystemBalance.EnemyHpBaseMultiplier : 1f;
            enemyHealth.ConfigureWithScaling(enemyData, generatedLevel.CaveLevel, hpMult);

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
            triggerCollider.radius = Mathf.Max(collider.radius, 0.5f * visualScale);
            triggerCollider.isTrigger = true;

            var contactDamage = triggerChild.AddComponent<EnemyContactDamage>();
            contactDamage.Configure(enemyData, triggerCollider);

            _spawnedEnemies.Add(enemyGO);
            _spawnedEnemyIds.Add(enemyData.enemyId);

            Debug.Log(
                $"CaveEnemySpawner: Spawned {enemyData.DisplayName} at grid ({gridPosition.x}, {gridPosition.y}) world ({spawnPos.x}, {spawnPos.y}).",
                this);
        }

        private Sprite GetBuiltinSprite()
        {
#if UNITY_EDITOR
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
#else
            return null;
#endif
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
