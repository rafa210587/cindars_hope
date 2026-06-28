using System.Collections.Generic;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Generation;
using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Enemy;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// Responsável por materializar inimigos de um CaveGeneratedLevel a partir de um
    /// CaveEnemySpawnPlan. Lida com wiring de profiles de movimento, vulnerabilidade,
    /// tamanho, elite affixes e pack coordinator. Sem MonoBehaviour.
    /// </summary>
    internal sealed class CaveEnemyMaterializer
    {
        private EnemyDatabaseSO _enemyDatabase;
        private readonly GameObject _enemyPrefab;
        private readonly EnemySpawnProfileSO[] _spawnProfiles;
        private readonly EnemySpawnPackSO[] _spawnPacks;
        private readonly EnemyFactionLockSO[] _factionLocks;
        private EnemyMovementProfileDatabaseSO _movementProfileDatabase;
        private EnemyActionSetDatabaseSO _actionSetDatabase;
        private EnemyActionDatabaseSO _actionDatabase;
        private EnemyTelegraphProfileDatabaseSO _telegraphDatabase;
        private EnemyVulnerabilityProfileDatabaseSO _vulnerabilityProfileDatabase;
        private EnemySizeProfileDatabaseSO _sizeProfileDatabase;
        private readonly CaveEcosystemBalanceSO _ecosystemBalance;
        private readonly CaveRunManager _caveRunManager;
        private readonly Transform _playerTransform;
        private readonly int _maxEnemiesPerLevel;
        private readonly CaveEnemySpawnPlanner _spawnPlanner = new CaveEnemySpawnPlanner();

        /// <summary>
        /// Inicializa o materializer de inimigos com todas as dependências de databases e configuração.
        /// </summary>
        internal CaveEnemyMaterializer(
            EnemyDatabaseSO enemyDatabase,
            GameObject enemyPrefab,
            EnemySpawnProfileSO[] spawnProfiles,
            EnemySpawnPackSO[] spawnPacks,
            EnemyFactionLockSO[] factionLocks,
            EnemyMovementProfileDatabaseSO movementProfileDatabase,
            EnemyActionSetDatabaseSO actionSetDatabase,
            EnemyActionDatabaseSO actionDatabase,
            EnemyTelegraphProfileDatabaseSO telegraphDatabase,
            EnemyVulnerabilityProfileDatabaseSO vulnerabilityProfileDatabase,
            EnemySizeProfileDatabaseSO sizeProfileDatabase,
            CaveEcosystemBalanceSO ecosystemBalance,
            CaveRunManager caveRunManager,
            Transform playerTransform,
            int maxEnemiesPerLevel)
        {
            _enemyDatabase = enemyDatabase;
            _enemyPrefab = enemyPrefab;
            _spawnProfiles = spawnProfiles;
            _spawnPacks = spawnPacks;
            _factionLocks = factionLocks;
            _movementProfileDatabase = movementProfileDatabase;
            _actionSetDatabase = actionSetDatabase;
            _actionDatabase = actionDatabase;
            _telegraphDatabase = telegraphDatabase;
            _vulnerabilityProfileDatabase = vulnerabilityProfileDatabase;
            _sizeProfileDatabase = sizeProfileDatabase;
            _ecosystemBalance = ecosystemBalance;
            _caveRunManager = caveRunManager;
            _playerTransform = playerTransform;
            _maxEnemiesPerLevel = maxEnemiesPerLevel;
        }

        /// <summary>
        /// Atualiza as databases de combate após RebindCombatDatabases no adapter.
        /// Chamado sempre que EnsureCombatDatabasesBound re-bind as databases.
        /// </summary>
        internal void UpdateDatabases(
            EnemyDatabaseSO enemyDatabase,
            EnemyMovementProfileDatabaseSO movementProfileDatabase,
            EnemyActionSetDatabaseSO actionSetDatabase,
            EnemyActionDatabaseSO actionDatabase,
            EnemyTelegraphProfileDatabaseSO telegraphDatabase,
            EnemyVulnerabilityProfileDatabaseSO vulnerabilityProfileDatabase,
            EnemySizeProfileDatabaseSO sizeProfileDatabase)
        {
            if (enemyDatabase != null) _enemyDatabase = enemyDatabase;
            if (movementProfileDatabase != null) _movementProfileDatabase = movementProfileDatabase;
            if (actionSetDatabase != null) _actionSetDatabase = actionSetDatabase;
            if (actionDatabase != null) _actionDatabase = actionDatabase;
            if (telegraphDatabase != null) _telegraphDatabase = telegraphDatabase;
            if (vulnerabilityProfileDatabase != null) _vulnerabilityProfileDatabase = vulnerabilityProfileDatabase;
            if (sizeProfileDatabase != null) _sizeProfileDatabase = sizeProfileDatabase;
        }

        /// <summary>
        /// Materializa os inimigos do nível. Se houver snapshotEnemySpawnPlan válido, reutiliza-o;
        /// caso contrário, gera um novo plano via CaveEnemySpawnPlanner.
        /// Preenche lastEnemySpawnPlan e packCoordinator.
        /// </summary>
        internal void MaterializeEnemies(
            CaveGeneratedLevel level,
            Transform generatedRuntimeRoot,
            List<GameObject> materializedObjects,
            CaveRuntimeMaterializationResult result,
            CaveEnemySpawnPlan snapshotEnemySpawnPlan,
            IReadOnlyList<EnemyHpRecord> snapshotEnemyHpRecords,
            out CaveEnemySpawnPlan lastEnemySpawnPlan,
            out EnemyPackCoordinator packCoordinator)
        {
            lastEnemySpawnPlan = null;
            packCoordinator = null;

            if (generatedRuntimeRoot == null)
            {
                Debug.LogError("CaveEnemyMaterializer: Cannot materialize enemies without generated runtime root.");
                return;
            }

            if (_caveRunManager == null)
            {
                Debug.LogError("CaveEnemyMaterializer: Cannot materialize enemies because CaveRunManager is not assigned.");
                return;
            }

            if (_enemyDatabase == null)
            {
                LogEnemySpawnWiringWarning(level, snapshotEnemySpawnPlan, "EnemyDatabaseSO not assigned");
                return;
            }

            if ((_spawnProfiles == null || _spawnProfiles.Length == 0)
                && (snapshotEnemySpawnPlan == null || !snapshotEnemySpawnPlan.IsValid))
            {
                LogEnemySpawnWiringWarning(level, snapshotEnemySpawnPlan, "Enemy spawn profiles not assigned");
                return;
            }

            lastEnemySpawnPlan = snapshotEnemySpawnPlan != null && snapshotEnemySpawnPlan.IsValid
                ? snapshotEnemySpawnPlan
                : _spawnPlanner.CreatePlan(
                    level,
                    _caveRunManager,
                    _spawnProfiles,
                    _spawnPacks,
                    _factionLocks,
                    _maxEnemiesPerLevel);

            if (lastEnemySpawnPlan == null || !lastEnemySpawnPlan.IsValid)
            {
                Debug.LogWarning($"CaveEnemyMaterializer: Enemy spawn plan empty for level {level.CaveLevel}.");
                LogEnemySpawnPlanWarnings(lastEnemySpawnPlan);
                return;
            }

            LogEnemySpawnPlanWarnings(lastEnemySpawnPlan);

            var enemyParent = new GameObject("GeneratedEnemies");
            enemyParent.transform.SetParent(generatedRuntimeRoot);
            enemyParent.transform.localPosition = Vector3.zero;
            materializedObjects.Add(enemyParent);

            var coordinatorGO = new GameObject("EnemyPackCoordinator");
            coordinatorGO.transform.SetParent(enemyParent.transform);
            coordinatorGO.transform.localPosition = Vector3.zero;
            packCoordinator = coordinatorGO.AddComponent<EnemyPackCoordinator>();
            materializedObjects.Add(coordinatorGO);

            Dictionary<string, int> savedHpByInstance = null;
            if (snapshotEnemyHpRecords != null && snapshotEnemyHpRecords.Count > 0)
            {
                savedHpByInstance = new Dictionary<string, int>();
                foreach (var record in snapshotEnemyHpRecords)
                {
                    if (record != null && !string.IsNullOrWhiteSpace(record.EnemyInstanceId))
                    {
                        savedHpByInstance[record.EnemyInstanceId] = record.CurrentHp;
                    }
                }
            }

            foreach (var entry in lastEnemySpawnPlan.Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.EnemyId))
                {
                    continue;
                }

                if (!_enemyDatabase.TryGetById(entry.EnemyId, out var enemyData) || enemyData == null)
                {
                    Debug.LogError($"CaveEnemyMaterializer: EnemyDataSO '{entry.EnemyId}' not found in EnemyDatabaseSO. SpawnProfileId={entry.SpawnProfileId}, InstanceId={entry.EnemyInstanceId}.");
                    continue;
                }

                var savedHp = int.MinValue;
                var hasSavedHp = savedHpByInstance != null && savedHpByInstance.TryGetValue(entry.EnemyInstanceId, out savedHp);
                if (hasSavedHp && savedHp <= 0)
                {
                    continue;
                }

                var enemyObject = CreateEnemyRuntimeObject(entry, enemyData, enemyParent.transform, packCoordinator, level.CaveLevel);

                if (hasSavedHp)
                {
                    var enemyHealth = enemyObject.GetComponent<CindarsHope.Combat.EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.RestoreHp(savedHp);
                    }
                }
                materializedObjects.Add(enemyObject);
                result.CreatedEnemies++;

                GameEventBus.Publish(new EnemySpawnedEvent(
                    entry.EnemyId,
                    entry.WorldPosition,
                    entry.EnemyInstanceId,
                    level.CaveLevel));
                GameEventBus.Publish(new EnemySeenEvent(
                    entry.EnemyId,
                    entry.WorldPosition,
                    entry.EnemyInstanceId,
                    level.CaveLevel));
            }

            CombatLog.Log(
                $"CaveEnemyMaterializer: Materialized {result.CreatedEnemies} enemies for level {level.CaveLevel}. Seed={lastEnemySpawnPlan.LevelSeed}. LayoutHash={lastEnemySpawnPlan.LayoutHash}.",
                null);
        }

        /// <summary>
        /// Cria o GameObject de runtime de um inimigo, instanciando o prefab ou criando proceduralmente.
        /// Chama ConfigureEnemyRuntimeObject internamente.
        /// </summary>
        internal GameObject CreateEnemyRuntimeObject(
            CaveEnemySpawnPlanEntry entry,
            EnemyDataSO enemyData,
            Transform parent,
            EnemyPackCoordinator packCoordinator,
            int caveLevel = 0)
        {
            GameObject enemyObject;
            if (_enemyPrefab != null)
            {
                enemyObject = Object.Instantiate(_enemyPrefab, entry.WorldPosition, Quaternion.identity, parent);
            }
            else
            {
                enemyObject = new GameObject(entry.EnemyInstanceId);
                enemyObject.transform.SetParent(parent);
                enemyObject.transform.position = entry.WorldPosition;
            }

            enemyObject.name = entry.EnemyInstanceId;
            ConfigureEnemyRuntimeObject(enemyObject, enemyData, entry, packCoordinator, caveLevel);
            return enemyObject;
        }

        private void ConfigureEnemyRuntimeObject(
            GameObject enemyObject,
            EnemyDataSO enemyData,
            CaveEnemySpawnPlanEntry entry,
            EnemyPackCoordinator packCoordinator,
            int caveLevel = 0)
        {
            EnemyMovementProfileSO movementProfile = null;
            if (!string.IsNullOrEmpty(enemyData.MovementProfileId))
            {
                if (_movementProfileDatabase == null)
                    Debug.LogError($"CombatLog: ProfileResolveFailed. EnemyId={enemyData.enemyId}, ProfileType=MovementProfile, ProfileId={enemyData.MovementProfileId}, Reason=DatabaseNotAssigned.");
                else if (!_movementProfileDatabase.TryGetById(enemyData.MovementProfileId, out movementProfile) || movementProfile == null)
                    Debug.LogError($"CombatLog: ProfileResolveFailed. EnemyId={enemyData.enemyId}, ProfileType=MovementProfile, ProfileId={enemyData.MovementProfileId}, Reason=IdNotFoundInDatabase '{_movementProfileDatabase.name}'.");
            }

            EnemyVulnerabilityProfileSO vulnerabilityProfile = null;
            if (!string.IsNullOrEmpty(enemyData.VulnerabilityProfileId))
            {
                if (_vulnerabilityProfileDatabase == null)
                    Debug.LogError($"CombatLog: ProfileResolveFailed. EnemyId={enemyData.enemyId}, ProfileType=VulnerabilityProfile, ProfileId={enemyData.VulnerabilityProfileId}, Reason=DatabaseNotAssigned.");
                else if (!_vulnerabilityProfileDatabase.TryGetById(enemyData.VulnerabilityProfileId, out vulnerabilityProfile) || vulnerabilityProfile == null)
                    Debug.LogError($"CombatLog: ProfileResolveFailed. EnemyId={enemyData.enemyId}, ProfileType=VulnerabilityProfile, ProfileId={enemyData.VulnerabilityProfileId}, Reason=IdNotFoundInDatabase '{_vulnerabilityProfileDatabase.name}'.");
            }

            EnemySizeProfileSO sizeProfile = null;
            if (!string.IsNullOrEmpty(enemyData.SizeProfileId))
            {
                if (_sizeProfileDatabase == null)
                    Debug.LogError($"CombatLog: ProfileResolveFailed. EnemyId={enemyData.enemyId}, ProfileType=SizeProfile, ProfileId={enemyData.SizeProfileId}, Reason=DatabaseNotAssigned.");
                else if (!_sizeProfileDatabase.TryGetById(enemyData.SizeProfileId, out sizeProfile) || sizeProfile == null)
                    Debug.LogError($"CombatLog: ProfileResolveFailed. EnemyId={enemyData.enemyId}, ProfileType=SizeProfile, ProfileId={enemyData.SizeProfileId}, Reason=IdNotFoundInDatabase '{_sizeProfileDatabase.name}'.");
            }

            var spriteRenderer = enemyObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = enemyObject.AddComponent<SpriteRenderer>();
            }

            // Sprite: Icon do SO; se vazio, autoload em runtime por id de
            // Assets/_Game/Resources/EnemySprites/<slug>.png (enemyId 'enemy_<slug>' -> '<slug>').
            // Independe de assignment manual no asset e de qual instancia de SO a caverna resolve.
            var resolvedSprite = enemyData.Icon;
            if (resolvedSprite == null && !string.IsNullOrEmpty(enemyData.enemyId))
            {
                var slug = enemyData.enemyId.StartsWith("enemy_")
                    ? enemyData.enemyId.Substring("enemy_".Length)
                    : enemyData.enemyId;
                resolvedSprite = UnityEngine.Resources.Load<Sprite>("EnemySprites/" + slug);
            }

            spriteRenderer.sprite = resolvedSprite != null ? resolvedSprite : CaveTileMaterializer.GetBuiltinSprite();
            // Com sprite REAL: branco (mostra as cores verdadeiras; elite recebe leve tint quente).
            // Sem sprite (placeholder): mantem o tint vermelho/laranja de antes.
            spriteRenderer.color = resolvedSprite != null
                ? (enemyData.IsElite || entry.IsElite ? new Color(1f, 0.78f, 0.6f) : Color.white)
                : (enemyData.IsElite || entry.IsElite ? new Color(1f, 0.55f, 0.25f) : new Color(0.85f, 0.23f, 0.23f));
            spriteRenderer.sortingOrder = 3;

            float visualScale = Mathf.Max(0.1f,
                EnemyScaleResolver.ResolveVisualScale(
                    enemyData.BestiarySize, enemyData.IsMiniBoss, enemyData.IsBoss));
            enemyObject.transform.localScale = new Vector3(visualScale, visualScale, 1f);

            // Juice de apresentacao: squash + flash. Bob de posicao OFF porque a IA move
            // o inimigo por transform.position (bob brigaria com o movimento).
            var spriteJuice = enemyObject.GetComponent<CindarsHope.Visual.SpriteJuice>();
            if (spriteJuice == null) spriteJuice = enemyObject.AddComponent<CindarsHope.Visual.SpriteJuice>();
            spriteJuice.SetBobPosition(false);
            Debug.Log($"[ScaleLog] EnemyId={enemyData.enemyId} SizeClass={enemyData.BestiarySize} " +
                      $"IsMiniBoss={enemyData.IsMiniBoss} IsBoss={enemyData.IsBoss} " +
                      $"PlayerRef={EnemyScaleResolver.PlayerReferenceScale} " +
                      $"VisualScale={visualScale:F3}", enemyObject);

            var collider = enemyObject.GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = enemyObject.AddComponent<CircleCollider2D>();
            }

            collider.radius = sizeProfile != null
                ? Mathf.Max(0.1f, sizeProfile.ColliderRadius)
                : ResolveColliderRadius(entry.SizeClass);

            var rigidbody = enemyObject.GetComponent<Rigidbody2D>();
            if (rigidbody == null)
            {
                rigidbody = enemyObject.AddComponent<Rigidbody2D>();
            }

            rigidbody.gravityScale = 0f;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

            var enemyHealth = enemyObject.GetComponent<CindarsHope.Combat.EnemyHealth>();
            if (enemyHealth == null)
            {
                enemyHealth = enemyObject.AddComponent<CindarsHope.Combat.EnemyHealth>();
            }
            var hpMult = _ecosystemBalance != null ? _ecosystemBalance.EnemyHpBaseMultiplier : 1f;
            enemyHealth.ConfigureWithScaling(enemyData, caveLevel, hpMult);

            enemyHealth.ConfigureLootContext(
                entry != null ? entry.EnemyInstanceId : string.Empty,
                _caveRunManager != null ? _caveRunManager.CaveRunSeed : string.Empty);

            EnemyVulnerabilityProfileSO matrixProfile = vulnerabilityProfile;
            if (!string.IsNullOrEmpty(enemyData.VulnerabilityMatrixProfileId) && _vulnerabilityProfileDatabase != null)
            {
                if (_vulnerabilityProfileDatabase.TryGetById(enemyData.VulnerabilityMatrixProfileId, out var resolvedMatrix) && resolvedMatrix != null)
                {
                    matrixProfile = resolvedMatrix;
                }
                else
                {
                    Debug.LogError($"CombatLog: ProfileResolveFailed. EnemyId={enemyData.enemyId}, ProfileType=VulnerabilityMatrixProfile, ProfileId={enemyData.VulnerabilityMatrixProfileId}, Reason=IdNotFoundInDatabase.");
                }
            }
            enemyHealth.ConfigureVulnerabilityMatrix(matrixProfile);

            if (enemyObject.GetComponent<EnemyVulnerabilityState>() == null)
            {
                enemyObject.AddComponent<EnemyVulnerabilityState>();
            }

            var postureState = enemyObject.GetComponent<CindarsHope.Combat.EnemyPostureState>();
            if (postureState == null)
            {
                postureState = enemyObject.AddComponent<CindarsHope.Combat.EnemyPostureState>();
            }
            postureState.Configure(enemyData.baseDifficulty);

            if (enemyObject.GetComponent<EnemyTelegraphController>() == null)
            {
                enemyObject.AddComponent<EnemyTelegraphController>();
            }

            if (enemyObject.GetComponent<KnockbackController>() == null)
            {
                enemyObject.AddComponent<KnockbackController>();
            }

            var hitFlash = enemyObject.GetComponent<HitFlashController>();
            if (hitFlash == null)
            {
                hitFlash = enemyObject.AddComponent<HitFlashController>();
            }

            if (enemyObject.GetComponent<DamagePopupAnchor>() == null)
            {
                enemyObject.AddComponent<DamagePopupAnchor>();
            }

            CindarsHope.Enemy.Speech.CreatureSpeechBubbleDisplayer.EnsureExists();
            var chatter = enemyObject.GetComponent<CindarsHope.Enemy.Speech.CreatureChatterController>();
            if (chatter == null)
            {
                chatter = enemyObject.AddComponent<CindarsHope.Enemy.Speech.CreatureChatterController>();
            }
            chatter.Configure(
                enemyData.enemyId,
                enemyData.CaveBand,
                entry != null ? entry.EnemyInstanceId : enemyObject.name,
                enemyObject.GetComponent<DamagePopupAnchor>());

            var brain = enemyObject.GetComponent<EnemyBrain>();
            if (brain == null)
            {
                brain = enemyObject.AddComponent<EnemyBrain>();
            }

            var packId = entry?.PackId;
            if (packCoordinator != null && !string.IsNullOrWhiteSpace(packId))
            {
                packCoordinator.Register(packId, brain, entry.WorldPosition);
            }

            bool hasFullDatabases = _actionSetDatabase != null && _actionDatabase != null && _telegraphDatabase != null;
            if (hasFullDatabases)
            {
                brain.ConfigureRuntime(
                    enemyData,
                    movementProfile,
                    _actionSetDatabase,
                    _actionDatabase,
                    _telegraphDatabase,
                    vulnerabilityProfile,
                    packCoordinator,
                    packId);
            }
            else
            {
                brain.Configure(enemyData, movementProfile);
            }

            var eliteAffix = entry?.EliteAffix ?? CindarsHope.Enemy.EliteAffix.None;
            if (eliteAffix != CindarsHope.Enemy.EliteAffix.None)
            {
                brain.ConfigureElite(eliteAffix);
                if (entry != null && string.IsNullOrEmpty(entry.EliteDisplayName))
                {
                    entry.EliteDisplayName = CindarsHope.Enemy.EliteAffixRules.BuildEliteDisplayName(eliteAffix, enemyData.DisplayName);
                }

                CombatLog.Log($"CombatLog: EliteSpawned. EnemyId={enemyData.enemyId}, Affix={eliteAffix}, " +
                          $"Name={entry?.EliteDisplayName}, InstanceId={entry?.EnemyInstanceId}, CaveLevel={caveLevel}.", enemyObject);
            }

            bool hasMovementProfileId = !string.IsNullOrEmpty(enemyData.MovementProfileId);
            bool useLegacyChase = movementProfile == null && !hasMovementProfileId;
            if (movementProfile == null && hasMovementProfileId)
            {
                Debug.LogError($"CombatLog: LegacyChaseFallbackSuppressed. EnemyId={enemyData.enemyId}, " +
                               $"MovementProfileId={enemyData.MovementProfileId}. Profile failed to resolve - see ProfileResolveFailed log. " +
                               $"EnemyBrain remains in control to keep the regression visible.");
            }
            var chaseController = enemyObject.GetComponent<EnemyChaseController>();
            if (useLegacyChase)
            {
                if (chaseController == null)
                    chaseController = enemyObject.AddComponent<EnemyChaseController>();
                chaseController.ConfigureFromData(enemyData);
                if (_playerTransform != null)
                    chaseController.RebindTarget(_playerTransform);
            }
            else if (chaseController != null)
            {
                chaseController.enabled = false;
            }

            var triggerChild = new GameObject("ContactDamageTrigger");
            triggerChild.transform.SetParent(enemyObject.transform);
            triggerChild.transform.localPosition = Vector3.zero;

            var triggerCollider = triggerChild.AddComponent<CircleCollider2D>();
            triggerCollider.radius = Mathf.Max(collider.radius, 0.5f);
            triggerCollider.isTrigger = true;

            var contactDamage = triggerChild.AddComponent<EnemyContactDamage>();
            contactDamage.Configure(enemyData, triggerCollider);

            bool actionSetResolved = brain.HasResolvedActionSet;
            int actionsCount       = brain.ResolvedActionCount;
            bool movementResolved  = movementProfile != null;
            bool vulnResolved      = vulnerabilityProfile != null;
            bool sizeResolved      = sizeProfile != null;
            float colliderRadius   = sizeProfile != null
                ? Mathf.Max(0.1f, sizeProfile.ColliderRadius)
                : ResolveColliderRadius(entry.SizeClass);

            CombatLog.Log(
                $"CombatLog: EnemyRuntimeConfigured. " +
                $"Name={enemyData.DisplayName}, EnemyId={enemyData.enemyId}, " +
                $"InstanceId={entry.EnemyInstanceId}, CaveLevel={caveLevel}, " +
                $"EnemyDataLevel={enemyData.CaveBand}, Faction={enemyData.FactionId}, " +
                $"MovementProfileId={enemyData.MovementProfileId}, MovementProfileResolved={movementResolved}, " +
                $"MovementType={brain.MovementType}, " +
                $"ActionSetId={enemyData.ActionSetId}, ActionSetResolved={actionSetResolved}, ActionsCount={actionsCount}, " +
                $"VulnerabilityProfileId={enemyData.VulnerabilityProfileId}, VulnerabilityResolved={vulnResolved}, " +
                $"SizeProfileId={enemyData.SizeProfileId}, SizeProfileResolved={sizeResolved}, " +
                $"SizeClass={entry.SizeClass}, VisualScale={visualScale:F2}, ColliderRadius={colliderRadius:F2}, " +
                $"HasEnemyBrain=True, HasLegacyChase={useLegacyChase}",
                enemyObject);

            if (!string.IsNullOrEmpty(enemyData.MovementProfileId) && !movementResolved)
                Debug.LogError($"CombatLog: EnemyRuntimeConfigured MISSING MovementProfile '{enemyData.MovementProfileId}' for {enemyData.enemyId}. _movementProfileDatabase assigned={_movementProfileDatabase != null}.", enemyObject);
            if (!string.IsNullOrEmpty(enemyData.ActionSetId) && !actionSetResolved)
                Debug.LogError($"CombatLog: EnemyRuntimeConfigured MISSING ActionSet '{enemyData.ActionSetId}' for {enemyData.enemyId}. _actionSetDatabase assigned={_actionSetDatabase != null}, _actionDatabase assigned={_actionDatabase != null}.", enemyObject);
            if (!string.IsNullOrEmpty(enemyData.SizeProfileId) && !sizeResolved)
                Debug.LogError($"CombatLog: EnemyRuntimeConfigured MISSING SizeProfile '{enemyData.SizeProfileId}' for {enemyData.enemyId}. _sizeProfileDatabase assigned={_sizeProfileDatabase != null}.", enemyObject);
        }

        /// <summary>
        /// Resolve o raio do collider a partir da classe de tamanho do inimigo quando não há SizeProfile.
        /// </summary>
        internal static float ResolveColliderRadius(string sizeClass)
        {
            return sizeClass switch
            {
                "Tiny" => 0.25f,
                "Small" => 0.35f,
                "Large" => 0.65f,
                "Huge" => 0.95f,
                "Boss" => 1.2f,
                _ => 0.45f
            };
        }

        private void LogEnemySpawnWiringWarning(CaveGeneratedLevel level, CaveEnemySpawnPlan snapshotPlan, string cause)
        {
            var profileCount = CountAssigned(_spawnProfiles);
            var packCount = CountAssigned(_spawnPacks);
            var lockCount = CountAssigned(_factionLocks);
            var hasSnapshotPlan = snapshotPlan != null && snapshotPlan.IsValid;
            var snapshotEntries = snapshotPlan?.Entries?.Count ?? 0;
            var lvl = level != null ? level.CaveLevel.ToString() : "unknown";
            var walkableTiles = level != null ? level.WalkableTiles.Count.ToString() : "unknown";

            Debug.LogWarning(
                "CaveEnemyMaterializer: Enemy materialization skipped. " +
                $"Cause='{cause}'. Level={lvl}, WalkableTiles={walkableTiles}, " +
                $"EnemyDatabaseAssigned={(_enemyDatabase != null)}, EnemyPrefabAssigned={(_enemyPrefab != null)}, " +
                $"SpawnProfiles={profileCount}, SpawnPacks={packCount}, FactionLocks={lockCount}, " +
                $"SnapshotPlanValid={hasSnapshotPlan}, SnapshotEntries={snapshotEntries}. " +
                "Expected assets: Assets/_Game/Data/Combat/EnemyDatabase.asset, " +
                "Assets/_Game/Data/EnemySpawn/Profiles, Assets/_Game/Data/EnemySpawn/Packs, " +
                "Assets/_Game/Data/EnemySpawn/FactionLocks. " +
                "Run CindarsHope/SPEC 13/Generate And Wire SPEC 13G Assets.");
        }

        private static int CountAssigned<T>(IEnumerable<T> values) where T : Object
        {
            var count = 0;
            if (values == null) return count;
            foreach (var value in values)
            {
                if (value != null) count++;
            }
            return count;
        }

        private static void LogEnemySpawnPlanWarnings(CaveEnemySpawnPlan plan)
        {
            if (plan?.Warnings == null || plan.Warnings.Count == 0) return;
            foreach (var warning in plan.Warnings)
            {
                if (!string.IsNullOrWhiteSpace(warning))
                {
                    Debug.LogWarning($"CaveEnemyMaterializer enemy spawn warning: {warning}");
                }
            }
        }
    }
}
