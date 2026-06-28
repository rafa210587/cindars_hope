using System.Collections.Generic;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Generation;
using CindarsHope.Combat;
using CindarsHope.Core.Data;
using CindarsHope.Enemy;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    public sealed class CaveBossSpawner : MonoBehaviour
    {
        [SerializeField] private CaveBossGateRegistrySO _bossGateRegistry;
        [SerializeField] private CaveRunManager _caveRunManager;
        [SerializeField] private DataRegistrySO<EnemyDataSO> _enemyDatabase;
        [SerializeField] private EnemyDataSO _fallbackEnemyData;
        [SerializeField] private GameScaleConfigSO _scaleConfig;
        [SerializeField] private CaveEcosystemBalanceSO _ecosystemBalance;

        // fable_05: optional boss-phase wiring. When a profile exists for the boss, the spawner adds an
        // EnemyBrain + BossBrainController; bosses without a profile keep the simple behaviour below.
        [SerializeField] private BossPhaseProfileRegistrySO _bossPhaseProfileRegistry;
        [SerializeField] private CombatRuntimeDatabasesRegistrySO _combatDatabases;

        private GameObject _spawnedBoss;
        private Transform _playerTarget;
        private readonly System.Collections.Generic.List<GameObject> _spawnedAdds = new System.Collections.Generic.List<GameObject>();
        private int _currentBossCaveLevel;

        // fable_44: true enquanto um boss real está spawnado neste nível. O CaveLevelRuntimeController
        // lê esta propriedade após SpawnBossForLevel para decidir se inicia a boss fight (gate de save).
        // false quando não há gate, o boss já foi derrotado, faltam dados, ou após CleanupBoss().
        public bool HasLiveBoss => _spawnedBoss != null;

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

            var bossScale = GetBossScale(bossEnemyData);
            _spawnedBoss.transform.localScale = Vector3.one * bossScale;

            var collider = _spawnedBoss.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f * bossScale;

            var rigidbody = _spawnedBoss.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

            _currentBossCaveLevel = generatedLevel.CaveLevel;
            var enemyHealth = _spawnedBoss.AddComponent<EnemyHealth>();
            var hpMult = _ecosystemBalance != null ? _ecosystemBalance.EnemyHpBaseMultiplier : 1f;
            enemyHealth.ConfigureWithScaling(bossEnemyData, generatedLevel.CaveLevel, hpMult);

            _spawnedBoss.AddComponent<KnockbackController>();
            _spawnedBoss.AddComponent<HitFlashController>();

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
            triggerCollider.radius = 0.5f * bossScale;
            triggerCollider.isTrigger = true;

            var contactDamage = triggerChild.AddComponent<EnemyContactDamage>();
            contactDamage.Configure(bossEnemyData, triggerCollider);

            // Do not assign a custom Unity tag here. Tags must be predeclared in ProjectSettings/TagManager.asset;
            // assigning an undeclared tag throws/logs errors in Play Mode. Boss identity is tracked by CaveBossDeathReporter.

            var bossDeathReporter = _spawnedBoss.AddComponent<CaveBossDeathReporter>();
            bossDeathReporter.Configure(
                _caveRunManager,
                bossGate.Id,
                bossEnemyData.enemyId,
                generatedLevel.CaveLevel,
                bossGate.CheckpointUnlockedOnDefeat,
                spawnPos);

            // fable_05: attach data-driven phases when a profile exists for this boss. No profile =>
            // the boss keeps the EnemyChaseController-only behaviour wired above (anti-regression).
            TryAttachBossPhaseController(bossEnemyData, generatedLevel, generatedRuntimeRoot, bossGridPos);

            var strategy = "Unknown";
            var distToExit = Vector2Int.Distance(bossGridPos, generatedLevel.Exit);
            if (distToExit <= 1.5f) strategy = "Adjacent";
            else if (distToExit <= 2.5f) strategy = "Diagonal";
            else if (distToExit <= 3f) strategy = "Radius";
            else strategy = "Fallback";

            Debug.Log(
                $"CaveBossSpawner: Boss spawn resolved near ForwardExit. GateGrid={generatedLevel.Exit}, BossGrid={bossGridPos}, DistanceToGate={distanceToGate}, Strategy={strategy}.",
                this);

            Debug.Log(
                $"CaveBossSpawner: Spawned boss {bossEnemyData.DisplayName} (enemyId={bossEnemyData.enemyId}, gate={bossGate.Id}, hp={bossEnemyData.maxHp}) at level {generatedLevel.CaveLevel} world ({spawnPos.x}, {spawnPos.y}).",
                this);
        }

        private Vector2Int ResolveBossSpawnNearGate(CaveGeneratedLevel level, Transform playerTarget)
        {
            var exit = level.Exit;
            var adjacentTiles = new[]
            {
                exit + Vector2Int.up,
                exit + Vector2Int.down,
                exit + Vector2Int.left,
                exit + Vector2Int.right
            };

            foreach (var tile in adjacentTiles)
            {
                if (IsValidBossSpawnTile(tile, level, playerTarget, exit))
                {
                    return tile;
                }
            }

            var diagonalTiles = new[]
            {
                exit + new Vector2Int(1, 1),
                exit + new Vector2Int(1, -1),
                exit + new Vector2Int(-1, 1),
                exit + new Vector2Int(-1, -1)
            };

            foreach (var tile in diagonalTiles)
            {
                if (IsValidBossSpawnTile(tile, level, playerTarget, exit))
                {
                    return tile;
                }
            }

            var candidatesInRadius = new List<(Vector2Int tile, float distance)>();
            foreach (var tile in level.WalkableTiles)
            {
                var dist = Vector2Int.Distance(tile, exit);
                if (dist > 0f && dist <= 3f && IsValidBossSpawnTile(tile, level, playerTarget, exit))
                {
                    candidatesInRadius.Add((tile, dist));
                }
            }

            if (candidatesInRadius.Count > 0)
            {
                candidatesInRadius.Sort((a, b) => a.distance.CompareTo(b.distance));
                return candidatesInRadius[0].tile;
            }

            return FindEnemySpawnPointClosestToExit(level);
        }

        private bool IsValidBossSpawnTile(Vector2Int tile, CaveGeneratedLevel level, Transform playerTarget, Vector2Int exit)
        {
            if (!level.WalkableTiles.Contains(tile))
            {
                return false;
            }

            if (tile == exit || tile == level.Entrance)
            {
                return false;
            }

            if (tile.x < 0 || tile.x >= level.Width || tile.y < 0 || tile.y >= level.Height)
            {
                return false;
            }

            if (playerTarget != null)
            {
                var world = GridToWorld(tile, level);
                if (Vector3.Distance(world, playerTarget.position) < 2f)
                {
                    return false;
                }
            }

            return true;
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
                Debug.LogWarning($"CaveBossSpawner: BossEnemyId '{bossEnemyId}' not found. Using fallback enemy data '{_fallbackEnemyData.DisplayName}' (enemyId={_fallbackEnemyData.enemyId}). Boss death will still be tracked by gate id.", this);
                return _fallbackEnemyData;
            }

            return null;
        }

        private float GetBossScale(EnemyDataSO bossEnemyData)
        {
            var dataScale = bossEnemyData != null ? bossEnemyData.VisualScale : 1f;
            if (_scaleConfig == null)
            {
                // No config: honour a per-boss override, else the historical default.
                return EnemyScaleResolver.ResolveBossScale(dataScale, 2.5f, 2f, 2.5f);
            }

            return EnemyScaleResolver.ResolveBossScale(
                dataScale, _scaleConfig.BossScale, _scaleConfig.BossMinScale, _scaleConfig.BossMaxScale);
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

        // ─── fable_05: boss phase wiring ──────────────────────────────────────

        private void TryAttachBossPhaseController(
            EnemyDataSO bossEnemyData,
            CaveGeneratedLevel generatedLevel,
            GameObject generatedRuntimeRoot,
            Vector2Int bossGridPos)
        {
            if (_bossPhaseProfileRegistry == null || _spawnedBoss == null)
            {
                return;
            }

            var profile = _bossPhaseProfileRegistry.GetProfileForBoss(bossEnemyData.enemyId);
            if (profile == null || !profile.HasPhases)
            {
                return; // no profile for this boss -> simple behaviour (anti-regression)
            }

            var databases = ResolveCombatDatabases();
            if (databases == null)
            {
                Debug.LogWarning($"CaveBossSpawner: Boss '{bossEnemyData.enemyId}' has a phase profile but no CombatRuntimeDatabasesRegistry to wire the brain. Phases skipped.", this);
                return;
            }

            // Add the brain stack so action-set swaps work. The boss already has EnemyHealth/Rigidbody2D;
            // EnemyBrain drives phase action sets while EnemyChaseController (already attached) handles
            // base pursuit. The vulnerability state lets transitions open windows (CA-2).
            if (_spawnedBoss.GetComponent<EnemyVulnerabilityState>() == null)
            {
                _spawnedBoss.AddComponent<EnemyVulnerabilityState>();
            }

            if (_spawnedBoss.GetComponent<EnemyTelegraphController>() == null)
            {
                _spawnedBoss.AddComponent<EnemyTelegraphController>();
            }

            var brain = _spawnedBoss.GetComponent<EnemyBrain>();
            if (brain == null)
            {
                brain = _spawnedBoss.AddComponent<EnemyBrain>();
            }

            // Hand pursuit to the brain: disable the simple chase controller so the two do not fight
            // over the Rigidbody2D velocity. EnemyChaseController stays for non-profile bosses.
            var chase = _spawnedBoss.GetComponent<EnemyChaseController>();
            if (chase != null)
            {
                chase.enabled = false;
            }

            EnemyMovementProfileSO movementProfile = null;
            if (databases.MovementProfileDatabase != null && !string.IsNullOrWhiteSpace(bossEnemyData.MovementProfileId))
            {
                databases.MovementProfileDatabase.TryGetById(bossEnemyData.MovementProfileId, out movementProfile);
            }

            EnemyVulnerabilityProfileSO vulnerabilityProfile = null;
            if (databases.VulnerabilityProfileDatabase != null && !string.IsNullOrWhiteSpace(bossEnemyData.VulnerabilityProfileId))
            {
                databases.VulnerabilityProfileDatabase.TryGetById(bossEnemyData.VulnerabilityProfileId, out vulnerabilityProfile);
            }

            brain.ConfigureRuntime(
                bossEnemyData,
                movementProfile,
                databases.ActionSetDatabase,
                databases.ActionDatabase,
                databases.TelegraphDatabase,
                vulnerabilityProfile);

            var controller = _spawnedBoss.GetComponent<BossBrainController>();
            if (controller == null)
            {
                controller = _spawnedBoss.AddComponent<BossBrainController>();
            }

            var worldSeed = _caveRunManager != null ? _caveRunManager.CaveWorldSeed : string.Empty;
            var runSeed = _caveRunManager != null ? _caveRunManager.CaveRunSeed : string.Empty;

            controller.Configure(
                profile,
                worldSeed,
                runSeed,
                generatedLevel.CaveLevel,
                bossEnemyData.enemyId,
                spawnAddCallback: (addEnemyId, world) => SpawnAdd(addEnemyId, world, generatedRuntimeRoot),
                walkableTilesProvider: () => new System.Collections.Generic.List<Vector2Int>(generatedLevel.WalkableTiles),
                bossTileProvider: () => bossGridPos,
                gridToWorld: tile => GridToWorld(tile, generatedLevel),
                movementProfileDatabase: databases.MovementProfileDatabase);

            Debug.Log($"CaveBossSpawner: Boss phase controller attached. BossId={bossEnemyData.enemyId}, ProfileId={profile.ProfileId}, Phases={profile.Phases.Length}.", this);
        }

        private CombatRuntimeDatabasesRegistrySO ResolveCombatDatabases()
        {
            if (_combatDatabases != null)
            {
                return _combatDatabases;
            }

            // SPEC 14A-FIX10 pattern: the registry is resource-loadable so missing inspector wiring is
            // not a single point of failure.
            _combatDatabases = UnityEngine.Resources.Load<CombatRuntimeDatabasesRegistrySO>("CombatRuntimeDatabasesRegistry");
            return _combatDatabases;
        }

        // Boss-owned add spawn path (CA-3). Reuses the same minimal enemy construction the boss uses for
        // itself; deterministic positions are resolved by BossPhaseLogic before this is called.
        private void SpawnAdd(string addEnemyId, Vector3 world, GameObject generatedRuntimeRoot)
        {
            var addData = GetBossEnemyData(addEnemyId);
            if (addData == null)
            {
                Debug.LogWarning($"CaveBossSpawner: Boss add enemy data not found for '{addEnemyId}'. Skipping add.", this);
                return;
            }

            var add = new GameObject($"BossAdd_{addData.DisplayName}");
            add.transform.position = world;
            add.transform.parent = generatedRuntimeRoot != null ? generatedRuntimeRoot.transform : _spawnedBoss?.transform;

            var spriteRenderer = add.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = addData.Icon != null ? addData.Icon : GetBuiltinSprite();
            spriteRenderer.color = addData.Icon != null ? Color.white : new Color(0.85f, 0.23f, 0.23f);
            spriteRenderer.sortingOrder = 3;

            var addScale = Mathf.Max(0.1f, addData.VisualScale);
            add.transform.localScale = new Vector3(addScale, addScale, 1f);

            var collider = add.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f * addScale;

            var rigidbody = add.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

            var enemyHealth = add.AddComponent<EnemyHealth>();
            var hpMult = _ecosystemBalance != null ? _ecosystemBalance.EnemyHpBaseMultiplier : 1f;
            enemyHealth.ConfigureWithScaling(addData, _currentBossCaveLevel, hpMult);

            add.AddComponent<KnockbackController>();
            add.AddComponent<HitFlashController>();

            var chaseController = add.AddComponent<EnemyChaseController>();
            chaseController.ConfigureFromData(addData);
            if (_playerTarget != null)
            {
                chaseController.RebindTarget(_playerTarget);
            }

            var triggerChild = new GameObject("ContactDamageTrigger");
            triggerChild.transform.SetParent(add.transform);
            triggerChild.transform.localPosition = Vector3.zero;

            var triggerCollider = triggerChild.AddComponent<CircleCollider2D>();
            triggerCollider.radius = 0.5f * addScale;
            triggerCollider.isTrigger = true;

            var contactDamage = triggerChild.AddComponent<EnemyContactDamage>();
            contactDamage.Configure(addData, triggerCollider);

            _spawnedAdds.Add(add);
        }

        public void CleanupBoss()
        {
            if (_spawnedBoss != null)
            {
                Destroy(_spawnedBoss);
                _spawnedBoss = null;
            }

            foreach (var add in _spawnedAdds)
            {
                if (add != null)
                {
                    Destroy(add);
                }
            }
            _spawnedAdds.Clear();
        }

        private void OnDestroy()
        {
            CleanupBoss();
        }
    }
}