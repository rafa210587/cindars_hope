using System;
using System.Collections.Generic;
using CindarsHope.Cave.Art;
using CindarsHope.Cave.Data;
using CindarsHope.Cave.Ecosystem;
using CindarsHope.Cave.Generation;
using CindarsHope.Cave.Resources;
using CindarsHope.Cave.Traps;
using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.DebugTools;
using CindarsHope.Equipment;
using CindarsHope.Enemy;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using CindarsHope.SceneManagement;
using UnityEngine;
using Unity.Profiling;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CindarsHope.Cave.Runtime
{
    public sealed class CaveRuntimeMaterializer : MonoBehaviour
    {
        private static readonly ProfilerMarker MaterializeMarker =
            new ProfilerMarker("CindarsHope.Cave.Materialize");
        private static readonly ProfilerMarker CleanupMarker =
            new ProfilerMarker("CindarsHope.Cave.Cleanup");
        [SerializeField] private SpriteRenderer _floorTilePrefab;
        [SerializeField] private SpriteRenderer _wallTilePrefab;
        [SerializeField] private ScenePortal _entrancePrefab;
        [SerializeField] private CaveExitPortal _exitPortalPrefab;
        [SerializeField] private ResourceNode _resourceNodePrefab;
        [SerializeField] private CaveRunManager _caveRunManager;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private EquipmentManager _equipmentManager;
        [SerializeField] private EnemyDatabaseSO _enemyDatabase;
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private EnemySpawnProfileSO[] _enemySpawnProfiles = new EnemySpawnProfileSO[0];
        [SerializeField] private EnemySpawnPackSO[] _enemySpawnPacks = new EnemySpawnPackSO[0];
        [SerializeField] private EnemyFactionLockSO[] _enemyFactionLocks = new EnemyFactionLockSO[0];
        [SerializeField] private EnemyMovementProfileDatabaseSO _movementProfileDatabase;
        [SerializeField] private EnemyActionSetDatabaseSO _actionSetDatabase;
        [SerializeField] private EnemyActionDatabaseSO _actionDatabase;
        [SerializeField] private EnemyTelegraphProfileDatabaseSO _telegraphDatabase;
        [SerializeField] private EnemyVulnerabilityProfileDatabaseSO _vulnerabilityProfileDatabase;
        [SerializeField] private EnemySizeProfileDatabaseSO _sizeProfileDatabase;
        [SerializeField] private ResourceNodeDatabaseSO _resourceNodeDatabase;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private CaveLevelRuntimeController _levelController;
        [SerializeField, Range(0f, 1f)] private float _resourceSpawnChance = 0.28f;
        [SerializeField] private int _minResourceNodes = 1;
        [SerializeField] private int _maxResourceNodes = 4;
        [SerializeField] private int _maxEnemiesPerLevel = 24;
        // fable_09: prefab opcional do tile de hazard (fallback procedural quando ausente).
        [SerializeField] private SpriteRenderer _hazardTilePrefab;
        // fable_60: prefab opcional do tile de armadilha (fallback procedural quando ausente).
        [SerializeField] private SpriteRenderer _trapTilePrefab;
        // fable_78: dados/balance do ecossistema + prefabs opcionais de elementos ambientais.
        [SerializeField] private CaveEnvironmentElementDatabaseSO _environmentElementDatabase;
        [SerializeField] private CaveEcosystemBalanceSO _ecosystemBalance;
        [SerializeField] private SpriteRenderer _decorElementPrefab;
        [SerializeField] private SpriteRenderer _waterTilePrefab;
        // spec_cave_biome_art_profiles_runtime (CV01): 8 profiles de arte por bioma (opcional; array
        // vazio/profiles vazios = fallback integral aos placeholders atuais nos materializers).
        [SerializeField] private CaveBiomeArtProfileSO[] _biomeArtProfiles = new CaveBiomeArtProfileSO[0];

        private GameObject _generatedRuntimeRoot;
        private CaveExitPortal _backExitPortal;
        private CaveExitPortal _forwardExitPortal;
        // fable_04: pack coordinator lives under the generated root, so it is destroyed/recreated
        // with each materialization (no stale aggro across levels; no scene search).
        private EnemyPackCoordinator _packCoordinator;
        private List<GameObject> _materializedObjects = new List<GameObject>();
        private CaveEnemySpawnPlan _lastEnemySpawnPlan;
        private CaveEnemySpawnPlan _snapshotEnemySpawnPlan;
        private IReadOnlyList<CaveResourceNodeSnapshotEntry> _snapshotResourceNodeStates;
        private IReadOnlyList<EnemyHpRecord> _snapshotEnemyHpRecords;
        private readonly List<CaveResourceNodeSnapshotEntry> _lastResourceNodeSnapshots = new List<CaveResourceNodeSnapshotEntry>();
        // fable_09: baús abertos conhecidos do snapshot (entrada) + os abertos nesta sessão de nível.
        private IReadOnlyList<string> _snapshotOpenedChestIds;
        private readonly HashSet<string> _openedChestIds = new HashSet<string>();
        private Vector2Int _lastPlayerSpawnGrid;
        private CaveHazardPlan _lastHazardPlan;
        // fable_60: plano determinístico de armadilhas + estado por instância.
        private CaveTrapPlan _lastTrapPlan;
        private IReadOnlyList<CaveTrapSnapshotEntry> _snapshotTrapStates;
        private readonly Dictionary<string, CaveTrapSnapshotEntry> _trapStates = new Dictionary<string, CaveTrapSnapshotEntry>();
        // fable_78: elementos ambientais materializados nesta sessão + presença de água.
        private IReadOnlyList<SerializedEnvironmentElement> _snapshotEnvironmentElements;
        private readonly List<SerializedEnvironmentElement> _lastEnvironmentElements = new List<SerializedEnvironmentElement>();
        private bool _lastHasWater;
        // Bugfix 2026-07-10 (mesmo padrão de ResolveEnvironmentElementDatabase/ResolveBiomeArtProfiles):
        // CaveScene recriada serializa _ecosystemBalance como fileID: 0 (asset vive em Assets/_Game/Data,
        // fora de Resources) -> conflito inter-monstro, wounded, threat budget e o
        // FloorClusterDensityMultiplier da CV03 caem em defaults hardcoded, em silêncio. Resolvido 1x por
        // sessão em EnsureCollaborators() e cacheado aqui; serialized field continua sendo a fonte de
        // verdade quando preenchido, Resources é só o fallback.
        private CaveEcosystemBalanceSO _resolvedEcosystemBalance;
        private bool _ecosystemBalanceResolveAttempted;

        // Colaboradores — instanciados lazy via EnsureCollaborators().
        private CaveTileMaterializer _tileMaterializer;
        private CaveExitMaterializer _exitMaterializer;
        private CaveResourceNodeMaterializer _resourceNodeMaterializer;
        private CaveEnemyMaterializer _enemyMaterializer;
        private CaveHazardMaterializer _hazardMaterializer;
        private CaveTrapMaterializer _trapMaterializer;
        private CaveEnvironmentElementMaterializer _environmentElementMaterializer;
        // spec_cave_biome_art_profiles_runtime (CV01): resolver puro banda->arte; reconstruído se o
        // array de profiles mudar (raro; array é estável por sessão de Editor/Play).
        private CaveBiomeArtResolver _biomeArtResolver;

        public CaveExitPortal BackExitPortal => _backExitPortal;
        public CaveExitPortal ForwardExitPortal => _forwardExitPortal;
        public GameObject GeneratedRuntimeRoot => _generatedRuntimeRoot;

        private CaveRuntimeMaterializationResult _lastMaterializationResult;

        public CaveRuntimeMaterializationResult LastMaterializationResult => _lastMaterializationResult;
        public CaveEnemySpawnPlan LastEnemySpawnPlan => _lastEnemySpawnPlan;
        public IReadOnlyList<CaveResourceNodeSnapshotEntry> LastResourceNodeSnapshots => _lastResourceNodeSnapshots;
        // fable_09: baús abertos (snapshot de entrada + abertos nesta sessão) para persistência.
        public IReadOnlyCollection<string> OpenedChestIds => _openedChestIds;
        public CaveHazardPlan LastHazardPlan => _lastHazardPlan;
        // fable_60: plano de armadilhas materializado e estado por instância (para o snapshot).
        public CaveTrapPlan LastTrapPlan => _lastTrapPlan;
        public IReadOnlyCollection<CaveTrapSnapshotEntry> TrapStates => _trapStates.Values;
        // fable_78: elementos ambientais materializados + presença de água (para o snapshot stable-run).
        public IReadOnlyList<SerializedEnvironmentElement> LastEnvironmentElements => _lastEnvironmentElements;
        public bool LastHasWater => _lastHasWater;
        // fable_78 (SLICE 4): balance do ecossistema (para o controller rolar o conflito por entrada).
        // Bugfix 2026-07-10: garante resolução (serialized field ou Resources fallback) mesmo se
        // chamado antes de qualquer Materialize() nesta sessão (mesmo padrão de BiomeArtResolver).
        public CaveEcosystemBalanceSO EcosystemBalance
        {
            get
            {
                EnsureCollaborators();
                return _resolvedEcosystemBalance;
            }
        }

        public void Materialize(CaveGeneratedLevel generatedLevel, CaveSpawnAnchor spawnAnchor = CaveSpawnAnchor.Entrance)
        {
            MaterializeInternal(generatedLevel, spawnAnchor, null, null);
        }

        public void MaterializeFromSnapshot(VisitedLevelSnapshot snapshot, CaveGeneratedLevel generatedLevel, CaveSpawnAnchor spawnAnchor = CaveSpawnAnchor.Entrance)
        {
            _snapshotEnemyHpRecords = snapshot?.EnemyHpRecords;
            _snapshotOpenedChestIds = snapshot?.OpenedChestIds; // fable_09: revisita mostra baú aberto
            _snapshotTrapStates = snapshot?.TrapStates;          // fable_60: revisita preserva estado das armadilhas
            _snapshotEnvironmentElements = snapshot?.EnvironmentElements; // fable_78: revisita restaura elementos
            MaterializeInternal(
                generatedLevel,
                spawnAnchor,
                snapshot?.EnemySpawnPlan,
                snapshot?.ResourceNodeStates);
        }

        // fable_78 (SLICE 4): aplica um plano de conflito inter-monstro às instâncias JÁ materializadas.
        public void ApplyConflict(CaveEcosystemConflictPlan conflictPlan, int caveLevel)
        {
            EnsureCollaborators();
            if (conflictPlan == null || !conflictPlan.ConflictActive || _resolvedEcosystemBalance == null)
            {
                return;
            }

            var factionA = conflictPlan.FactionAEnemyId;
            var factionB = conflictPlan.FactionBEnemyId;
            if (string.IsNullOrEmpty(factionA) || string.IsNullOrEmpty(factionB) || factionA == factionB)
            {
                return;
            }

            var sideA = new List<CindarsHope.Combat.EnemyHealth>();
            var sideB = new List<CindarsHope.Combat.EnemyHealth>();
            foreach (var obj in _materializedObjects)
            {
                if (obj == null)
                {
                    continue;
                }

                var health = obj.GetComponent<CindarsHope.Combat.EnemyHealth>();
                if (health == null || health.IsDead)
                {
                    continue;
                }

                if (health.EnemyId == factionA)
                {
                    sideA.Add(health);
                }
                else if (health.EnemyId == factionB)
                {
                    sideB.Add(health);
                }
            }

            if (sideA.Count == 0 || sideB.Count == 0)
            {
                return;
            }

            WireConflictSide(sideA, factionA, factionB, sideB, caveLevel);
            WireConflictSide(sideB, factionB, factionA, sideA, caveLevel);

            CombatLog.Log(
                $"CaveRuntimeMaterializer: inter-monster conflict wired for level {caveLevel}. FactionA='{factionA}' ({sideA.Count}), FactionB='{factionB}' ({sideB.Count}).",
                this);
        }

        private void WireConflictSide(
            List<CindarsHope.Combat.EnemyHealth> side,
            string ownEnemyId,
            string rivalEnemyId,
            List<CindarsHope.Combat.EnemyHealth> rivals,
            int caveLevel)
        {
            foreach (var health in side)
            {
                if (health == null)
                {
                    continue;
                }

                var combatant = health.GetComponent<CaveConflictCombatant>();
                if (combatant == null)
                {
                    combatant = health.gameObject.AddComponent<CaveConflictCombatant>();
                }

                combatant.Configure(ownEnemyId, rivalEnemyId, caveLevel);
                foreach (var rival in rivals)
                {
                    combatant.AddRival(rival);
                }

                var brain = health.GetComponent<EnemyBrain>();
                if (brain != null)
                {
                    brain.ConfigureConflict(combatant, _resolvedEcosystemBalance);
                }
            }
        }

        // F13: HP corrente por instância dos inimigos materializados (mortos inclusos, HP 0).
        public List<EnemyHpRecord> CollectEnemyHpRecords()
        {
            var records = new List<EnemyHpRecord>();
            foreach (var materializedObject in _materializedObjects)
            {
                if (materializedObject == null)
                {
                    continue;
                }

                var health = materializedObject.GetComponent<CindarsHope.Combat.EnemyHealth>();
                if (health == null)
                {
                    continue;
                }

                records.Add(new EnemyHpRecord
                {
                    EnemyInstanceId = materializedObject.name,
                    CurrentHp = health.CurrentHp
                });
            }

            return records;
        }

        // Garante que todos os colaboradores estão instanciados antes da materialização.
        // Colaboradores stateless (Tile/Exit) são singletons de sessão.
        // Colaboradores com dependências são recriados se nulos.
        // Fix pós-Play-Mode 2026-07-04: guard one-shot por instância — o wiring-error/status de
        // resolução do biome art só precisa aparecer 1x por materializer (mesmo padrão de
        // AudioManager._missingClipLogged, skill observability-and-logging).
        private bool _biomeArtWiringLogged;

        private void EnsureCollaborators()
        {
            // Bugfix 2026-07-10: resolvido antes dos demais colaboradores (que o recebem no
            // construtor) — mesmo motivo do biome art profile ser resolvido primeiro. Só tenta 1x por
            // sessão (mesmo se retornar null) para não repetir carregamento de asset a cada materialização.
            if (!_ecosystemBalanceResolveAttempted)
            {
                _ecosystemBalanceResolveAttempted = true;
                _resolvedEcosystemBalance = ResolveEcosystemBalance();
            }
            // spec_cave_biome_art_profiles_runtime (CV01): resolvido primeiro — hazard/trap
            // materializers o recebem no construtor.
            _biomeArtResolver ??= new CaveBiomeArtResolver(ResolveBiomeArtProfiles());
            _tileMaterializer ??= new CaveTileMaterializer();
            _exitMaterializer ??= new CaveExitMaterializer();
            _resourceNodeMaterializer ??= new CaveResourceNodeMaterializer(
                _resourceNodeDatabase,
                _resourceNodePrefab,
                _inventoryManager,
                _equipmentManager,
                _caveRunManager,
                _resourceSpawnChance,
                _minResourceNodes,
                _maxResourceNodes);
            _enemyMaterializer ??= new CaveEnemyMaterializer(
                _enemyDatabase,
                _enemyPrefab,
                _enemySpawnProfiles,
                _enemySpawnPacks,
                _enemyFactionLocks,
                _movementProfileDatabase,
                _actionSetDatabase,
                _actionDatabase,
                _telegraphDatabase,
                _vulnerabilityProfileDatabase,
                _sizeProfileDatabase,
                _resolvedEcosystemBalance,
                _caveRunManager,
                _playerTransform,
                _maxEnemiesPerLevel);
            _hazardMaterializer ??= new CaveHazardMaterializer(
                _hazardTilePrefab,
                _inventoryManager,
                _caveRunManager,
                _biomeArtResolver);
            _trapMaterializer ??= new CaveTrapMaterializer(
                _trapTilePrefab,
                _caveRunManager,
                _playerTransform,
                _biomeArtResolver);
            _environmentElementMaterializer ??= new CaveEnvironmentElementMaterializer(
                ResolveEnvironmentElementDatabase(),
                _resolvedEcosystemBalance,
                _decorElementPrefab,
                _waterTilePrefab,
                _resourceNodePrefab,
                _resourceNodeDatabase,
                _inventoryManager,
                _equipmentManager,
                _caveRunManager,
                _biomeArtResolver);
        }

        // Fix pós-Play-Mode 2026-07-04: cenas de CaveScene criadas ANTES da spec CV01 têm
        // _biomeArtProfiles serializado vazio (nunca preenchido no Editor) — sem isto, o resolver
        // fica sempre vazio e nenhum tile/sprite de bioma aparece, em silêncio. Precedente idêntico:
        // EnsureCombatDatabasesBound() / Resources.Load<CombatRuntimeDatabasesRegistrySO> (linha
        // ~545). Serialized field continua sendo a fonte de verdade quando preenchido; Resources é
        // só o fallback.
        private IReadOnlyList<CaveBiomeArtProfileSO> ResolveBiomeArtProfiles()
        {
            if (_biomeArtProfiles != null && _biomeArtProfiles.Length > 0)
            {
                LogBiomeArtWiringStatusOnce("serialized field", _biomeArtProfiles.Length);
                return _biomeArtProfiles;
            }

            var registry = UnityEngine.Resources.Load<CaveBiomeArtProfileRegistrySO>("CaveBiomeArtProfileRegistry");
            if (registry != null && registry.Profiles != null && registry.Profiles.Count > 0)
            {
                LogBiomeArtWiringStatusOnce("Resources/CaveBiomeArtProfileRegistry", registry.Profiles.Count);
                return registry.Profiles;
            }

            LogBiomeArtWiringStatusOnce(registry == null ? "none (registry asset not found)" : "none (registry empty)", 0);
            return System.Array.Empty<CaveBiomeArtProfileSO>();
        }

        private void LogBiomeArtWiringStatusOnce(string source, int count)
        {
            if (_biomeArtWiringLogged)
            {
                return;
            }

            _biomeArtWiringLogged = true;

            if (count > 0)
            {
                CombatLog.Log(
                    $"[Cave] CaveRuntimeMaterializer: biome art profiles resolved from {source} ({count} profile(s)).",
                    this);
                return;
            }

            Debug.LogWarning(
                $"[Cave][Wiring] CaveRuntimeMaterializer: 0 CaveBiomeArtProfileSO resolved (source='{source}'). " +
                $"Scene='{gameObject.scene.name}', go='{name}', field='_biomeArtProfiles'. " +
                "Fallback ativo: chao/parede/hazard/trap/baus/saidas usam APENAS os placeholders " +
                "proceduais atuais (sem arte de bioma). Corrija rodando CindarsHope/Inicializar Projeto " +
                "(gera os 8 CaveBiomeArtProfileSO + o registry em Resources) ou wireie manualmente o " +
                "array _biomeArtProfiles no Inspector desta CaveScene.",
                this);
        }

        // Bugfix 2026-07-04 (mesmo padrão de ResolveBiomeArtProfiles/EnsureCombatDatabasesBound):
        // guard one-shot por instância para o wiring-status do database de elementos ambientais.
        private bool _environmentElementDatabaseWiringLogged;

        // Bugfix 2026-07-04: CaveScene serializa _environmentElementDatabase como fileID: 0 (nunca
        // foi wireado no Editor). Sem database, CaveEnvironmentElementPlanner nao recebe perfil por
        // banda -> zero elementos ambientais materializados (decor real E placeholder), em silencio.
        // Mesmo precedente de ResolveBiomeArtProfiles (linha ~327): serialized field continua sendo a
        // fonte de verdade quando preenchido; Resources e so o fallback.
        private CaveEnvironmentElementDatabaseSO ResolveEnvironmentElementDatabase()
        {
            if (_environmentElementDatabase != null)
            {
                LogEnvironmentElementDatabaseWiringStatusOnce("serialized field");
                return _environmentElementDatabase;
            }

            var resolved = UnityEngine.Resources.Load<CaveEnvironmentElementDatabaseSO>("CaveEnvironmentElementDatabase");
            if (resolved != null)
            {
                LogEnvironmentElementDatabaseWiringStatusOnce("Resources/CaveEnvironmentElementDatabase");
                return resolved;
            }

            LogEnvironmentElementDatabaseWiringStatusOnce(null);
            return null;
        }

        private void LogEnvironmentElementDatabaseWiringStatusOnce(string source)
        {
            if (_environmentElementDatabaseWiringLogged)
            {
                return;
            }

            _environmentElementDatabaseWiringLogged = true;

            if (source != null)
            {
                CombatLog.Log(
                    $"[Cave] CaveRuntimeMaterializer: environment element database resolved from {source}.",
                    this);
                return;
            }

            Debug.LogWarning(
                "[Cave][Wiring] CaveRuntimeMaterializer: CaveEnvironmentElementDatabaseSO nao resolvido " +
                "(nem serialized field, nem Resources/CaveEnvironmentElementDatabase). " +
                $"Scene='{gameObject.scene.name}', go='{name}', field='_environmentElementDatabase'. " +
                "Fallback ativo: zero elementos ambientais (decor/agua/no minerio de fable_78) sao " +
                "materializados nesta cave. Corrija rodando CindarsHope/Inicializar Projeto (gera os 7 " +
                "CaveEnvironmentElementProfileSO + o database em Resources) ou wireie manualmente o " +
                "campo _environmentElementDatabase no Inspector desta CaveScene.",
                this);
        }

        // Bugfix 2026-07-10 (mesmo padrão de ResolveEnvironmentElementDatabase/ResolveBiomeArtProfiles):
        // guard one-shot por instância para o wiring-status do CaveEcosystemBalanceSO.
        private bool _ecosystemBalanceWiringLogged;

        // Bugfix 2026-07-10/12: CaveScene recriada pode serializar _ecosystemBalance como fileID: 0.
        // A fonte correta é o campo serializado preenchido pelo gerador de cena. Para cenas antigas ainda
        // sem wiring, cria uma instância default em memória em vez de adicionar novo carregamento direto neste
        // materializer, mantendo o ratchet arquitetural sem aumentar debt de runtime loading.
        private CaveEcosystemBalanceSO ResolveEcosystemBalance()
        {
            if (_ecosystemBalance != null)
            {
                LogEcosystemBalanceWiringStatusOnce("serialized field");
                return _ecosystemBalance;
            }

            var fallback = ScriptableObject.CreateInstance<CaveEcosystemBalanceSO>();
            fallback.name = "CaveEcosystemBalance_RuntimeDefaultFallback";
            LogEcosystemBalanceWiringStatusOnce(null);
            return fallback;
        }

        private void LogEcosystemBalanceWiringStatusOnce(string source)
        {
            if (_ecosystemBalanceWiringLogged)
            {
                return;
            }

            _ecosystemBalanceWiringLogged = true;

            if (source != null)
            {
                CombatLog.Log(
                    $"[Cave] CaveRuntimeMaterializer: ecosystem balance resolved from {source}.",
                    this);
                return;
            }

            Debug.LogWarning(
                "[Cave][Wiring] CaveRuntimeMaterializer: CaveEcosystemBalanceSO nao resolvido " +
                "(serialized field ausente). " +
                $"Scene='{gameObject.scene.name}', go='{name}', field='_ecosystemBalance'. " +
                "Fallback ativo: instancia default em memoria de CaveEcosystemBalanceSO. Corrija rodando " +
                "CindarsHope/Inicializar Projeto para preencher o campo _ecosystemBalance no Inspector " +
                "desta CaveScene.",
                this);
        }

        /// <summary>spec_cave_biome_art_profiles_runtime (CV01): resolver puro banda->arte, exposto
        /// para o CaveLevelRuntimeController (evento) e para os materializers (fallback-first).</summary>
        public CaveBiomeArtResolver BiomeArtResolver
        {
            get
            {
                EnsureCollaborators();
                return _biomeArtResolver;
            }
        }

        /// <summary>BiomeId de ARTE (não gameplay) do profile resolvido para a banda, ou string vazia
        /// se não houver profile carregado para essa banda (fallback null-safe).</summary>
        public string ResolveArtBiomeIdForBand(int bandId)
        {
            EnsureCollaborators();
            return _biomeArtResolver.TryGetProfile(bandId, out var profile) ? profile.BiomeId : string.Empty;
        }

        private void MaterializeInternal(
            CaveGeneratedLevel generatedLevel,
            CaveSpawnAnchor spawnAnchor,
            CaveEnemySpawnPlan enemySpawnPlanOverride,
            IReadOnlyList<CaveResourceNodeSnapshotEntry> resourceNodeStateOverride)
        {
            using var profilerScope = MaterializeMarker.Auto();
            if (generatedLevel == null)
            {
                Debug.LogError("CaveRuntimeMaterializer: Cannot materialize null CaveGeneratedLevel.");
                return;
            }

            if (_caveRunManager == null)
            {
                _caveRunManager = GetComponent<CaveRunManager>();
            }

            if (_levelController == null)
            {
                _levelController = GetComponent<CaveLevelRuntimeController>();
            }

            CleanupPreviousMaterialization();
            _snapshotEnemySpawnPlan = enemySpawnPlanOverride;
            _snapshotResourceNodeStates = resourceNodeStateOverride;
            _lastResourceNodeSnapshots.Clear();
            // fable_78: estado de elementos ambientais começa zerado nesta materialização.
            _lastEnvironmentElements.Clear();
            _lastHasWater = false;

            // fable_09: o conjunto de baús abertos começa do snapshot (revisita) e cresce nesta sessão.
            _openedChestIds.Clear();
            if (_snapshotOpenedChestIds != null)
            {
                foreach (var chestId in _snapshotOpenedChestIds)
                {
                    if (!string.IsNullOrWhiteSpace(chestId))
                    {
                        _openedChestIds.Add(chestId);
                    }
                }
            }

            // fable_60: o estado das armadilhas começa do snapshot (revisita) e cresce nesta sessão.
            _trapStates.Clear();
            if (_snapshotTrapStates != null)
            {
                foreach (var trap in _snapshotTrapStates)
                {
                    if (trap != null && !string.IsNullOrWhiteSpace(trap.TrapInstanceId))
                    {
                        _trapStates[trap.TrapInstanceId] = new CaveTrapSnapshotEntry
                        {
                            TrapInstanceId = trap.TrapInstanceId,
                            TrapKey = trap.TrapKey,
                            Cell = trap.Cell,
                            State = trap.State
                        };
                    }
                }
            }

            _lastMaterializationResult = new CaveRuntimeMaterializationResult();

            EnsureCollaborators();

            // Create root hierarchy
            _generatedRuntimeRoot = new GameObject("CaveGeneratedRuntime");
            _generatedRuntimeRoot.transform.position = Vector3.zero;

            // ORDEM SAGRADA — cave-stable-run / ADR-0005. Não altere a sequência.
            var worldSeedForArt = _caveRunManager != null ? _caveRunManager.CaveWorldSeed : string.Empty;
            var runSeedForArt = _caveRunManager != null ? _caveRunManager.CaveRunSeed : string.Empty;
            _tileMaterializer.MaterializeFloor(generatedLevel, _generatedRuntimeRoot.transform, _floorTilePrefab, _materializedObjects, _lastMaterializationResult, _biomeArtResolver, worldSeedForArt, runSeedForArt, _resolvedEcosystemBalance);
            _tileMaterializer.MaterializeWalls(generatedLevel, _generatedRuntimeRoot.transform, _wallTilePrefab, _materializedObjects, _lastMaterializationResult, _biomeArtResolver, worldSeedForArt, runSeedForArt, _resolvedEcosystemBalance);

            // spec_cave_visual_polish_runtime (CV04), T005: mood de luz FAKE (vinheta + feixe perto da
            // entrada) — estático, sem custo por-frame, puramente de apresentação (não influencia
            // layout/spawn/loot/snapshot). Roda logo após terreno para já aparecer sob decor/inimigos.
            var bandIdForVignette = CaveBiomeArtDebug.ResolveBandForArt(CaveBandScaling.BandForLevel(generatedLevel.CaveLevel));
            CaveVignetteController.ApplyVignetteAndLightShaft(generatedLevel, _generatedRuntimeRoot.transform, _biomeArtResolver, bandIdForVignette, _materializedObjects);

            _exitMaterializer.Materialize(
                generatedLevel,
                _generatedRuntimeRoot.transform,
                _exitPortalPrefab,
                _caveRunManager,
                _levelController,
                _materializedObjects,
                _lastMaterializationResult,
                out _backExitPortal,
                out _forwardExitPortal,
                _biomeArtResolver);

            _resourceNodeMaterializer.MaterializeResourceNodes(
                generatedLevel,
                _generatedRuntimeRoot.transform,
                _materializedObjects,
                _lastMaterializationResult,
                _lastResourceNodeSnapshots,
                _snapshotResourceNodeStates);

            // SPEC 14A-FIX10: self-heal combat database wiring + emit explicit status log.
            EnsureCombatDatabasesBound();
            LogDatabasesWiringStatus(generatedLevel);
            _enemyMaterializer?.UpdateDatabases(
                _enemyDatabase,
                _movementProfileDatabase,
                _actionSetDatabase,
                _actionDatabase,
                _telegraphDatabase,
                _vulnerabilityProfileDatabase,
                _sizeProfileDatabase);

            _enemyMaterializer.MaterializeEnemies(
                generatedLevel,
                _generatedRuntimeRoot.transform,
                _materializedObjects,
                _lastMaterializationResult,
                _snapshotEnemySpawnPlan,
                _snapshotEnemyHpRecords,
                out _lastEnemySpawnPlan,
                out _packCoordinator);

            // fable_09: spawn grid do player — usado para manter hazards longe do ponto de chegada.
            var anchorGrid = ResolveAnchorPosition(spawnAnchor, generatedLevel);
            _lastPlayerSpawnGrid = ResolvePlayerSpawnGrid(anchorGrid, spawnAnchor, generatedLevel);

            if (_playerTransform != null)
            {
                _playerTransform.position = CaveTileMaterializer.GridToWorld(_lastPlayerSpawnGrid, generatedLevel);

                CombatLog.Log(
                    $"CaveRuntimeMaterializer: Player spawned at anchor {spawnAnchor}. AnchorGrid: {anchorGrid}, ResolvedGrid: {_lastPlayerSpawnGrid}, WorldPos: {_playerTransform.position}",
                    this);

                RepositionCamera();
            }

            // fable_09: hazards + sala de tesouro DETERMINÍSTICOS (após inimigos, para realocar guardiões).
            MaterializeHazardsAndTreasure(generatedLevel);

            // fable_78: elementos ambientais por bioma. Roda após o player spawn estar resolvido.
            MaterializeEnvironmentElements(generatedLevel);

            CombatLog.Log(
                $"CaveRuntimeMaterializer: Materialized level {generatedLevel.CaveLevel}. Floor: {_lastMaterializationResult.CreatedFloorTiles}, Walls: {_lastMaterializationResult.CreatedWallTiles}, Resources: {_lastMaterializationResult.CreatedResourceNodes}, Enemies: {_lastMaterializationResult.CreatedEnemies}. BackExit: {_lastMaterializationResult.BackExitPosition}, ForwardExit: {_lastMaterializationResult.ForwardExitPosition}. SpawnAnchor: {spawnAnchor}",
                this);

            GameEventBus.Publish(new CaveRuntimeMaterializationCompleteEvent(generatedLevel));
            _snapshotEnemySpawnPlan = null;
            _snapshotResourceNodeStates = null;
            _snapshotEnemyHpRecords = null;
            _snapshotOpenedChestIds = null;
            _snapshotTrapStates = null;
            _snapshotEnvironmentElements = null;
        }

        // Mantido para retrocompatibilidade com callers externos (ex.: CaveLevelRuntimeController).
        private static Vector3 GridToWorld(Vector2Int gridPosition, CaveGeneratedLevel level)
        {
            return CaveTileMaterializer.GridToWorld(gridPosition, level);
        }

        private void RepositionCamera()
        {
            var mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            var cameraFollow = mainCamera.GetComponent<CindarsHope.Camera.CameraFollow2D>();
            if (cameraFollow != null)
            {
                cameraFollow.RebindTarget(_playerTransform);
                cameraFollow.SnapToTarget();
            }
            else
            {
                mainCamera.transform.position = new Vector3(
                    _playerTransform.position.x,
                    _playerTransform.position.y,
                    mainCamera.transform.position.z);
            }
        }

        // SPEC 14A-FIX10: explicit rebind so installers can wire combat databases at runtime.
        public void RebindCombatDatabases(CindarsHope.Core.Data.CombatRuntimeDatabasesRegistrySO registry)
        {
            if (registry == null)
            {
                Debug.LogError("CaveRuntimeMaterializer.RebindCombatDatabases: registry is null.", this);
                return;
            }

            if (registry.EnemyDatabase != null)                  _enemyDatabase                 = registry.EnemyDatabase;
            if (registry.MovementProfileDatabase != null)        _movementProfileDatabase       = registry.MovementProfileDatabase;
            if (registry.ActionSetDatabase != null)              _actionSetDatabase             = registry.ActionSetDatabase;
            if (registry.ActionDatabase != null)                 _actionDatabase                = registry.ActionDatabase;
            if (registry.TelegraphDatabase != null)              _telegraphDatabase             = registry.TelegraphDatabase;
            if (registry.VulnerabilityProfileDatabase != null)   _vulnerabilityProfileDatabase  = registry.VulnerabilityProfileDatabase;
            if (registry.SizeProfileDatabase != null)            _sizeProfileDatabase           = registry.SizeProfileDatabase;
        }

        private void EnsureCombatDatabasesBound()
        {
            bool anyMissing = _enemyDatabase == null
                              || _movementProfileDatabase == null
                              || _actionSetDatabase == null
                              || _actionDatabase == null
                              || _telegraphDatabase == null
                              || _vulnerabilityProfileDatabase == null
                              || _sizeProfileDatabase == null;

            if (!anyMissing) return;

            var registry = UnityEngine.Resources.Load<CindarsHope.Core.Data.CombatRuntimeDatabasesRegistrySO>("CombatRuntimeDatabasesRegistry");
            if (registry == null)
            {
                Debug.LogError("CaveRuntimeMaterializer.EnsureCombatDatabasesBound: registry asset not found at Resources/CombatRuntimeDatabasesRegistry. Inspector wiring is the only path left.", this);
                return;
            }

            RebindCombatDatabases(registry);
            CombatLog.Log("CaveRuntimeMaterializer: combat databases re-bound from Resources/CombatRuntimeDatabasesRegistry.", this);
        }

        private void LogDatabasesWiringStatus(CaveGeneratedLevel generatedLevel)
        {
            CombatLog.Log(
                "CombatLog: EnemyDatabasesWiringStatus. " +
                $"CaveLevel={generatedLevel?.CaveLevel}, " +
                $"EnemyDatabaseAssigned={_enemyDatabase != null}, " +
                $"MovementProfileDatabaseAssigned={_movementProfileDatabase != null}, " +
                $"ActionSetDatabaseAssigned={_actionSetDatabase != null}, " +
                $"ActionDatabaseAssigned={_actionDatabase != null}, " +
                $"TelegraphDatabaseAssigned={_telegraphDatabase != null}, " +
                $"VulnerabilityProfileDatabaseAssigned={_vulnerabilityProfileDatabase != null}, " +
                $"SizeProfileDatabaseAssigned={_sizeProfileDatabase != null}, " +
                $"EnemyPrefabAssigned={_enemyPrefab != null}",
                this);
        }

        // fable_09: hazards + sala de tesouro DETERMINÍSTICOS. Roda após inimigos.
        private void MaterializeHazardsAndTreasure(CaveGeneratedLevel generatedLevel)
        {
            var worldSeed = _caveRunManager != null ? _caveRunManager.CaveWorldSeed : string.Empty;
            var runSeed = _caveRunManager != null ? _caveRunManager.CaveRunSeed : string.Empty;

            _lastHazardPlan = CaveHazardPlanner.BuildPlan(generatedLevel, worldSeed, runSeed, _lastPlayerSpawnGrid);

            _hazardMaterializer.MaterializeHazards(generatedLevel, _generatedRuntimeRoot.transform, _lastHazardPlan, _materializedObjects);
            _hazardMaterializer.MaterializeTreasureRoom(
                generatedLevel,
                _generatedRuntimeRoot.transform,
                _lastHazardPlan,
                _lastEnemySpawnPlan,
                _openedChestIds,
                _materializedObjects,
                RegisterOpenedChest);

            // fable_60: armadilhas determinísticas por bioma/tier.
            _trapMaterializer.MaterializeTraps(
                generatedLevel,
                _generatedRuntimeRoot.transform,
                worldSeed,
                runSeed,
                _lastPlayerSpawnGrid,
                _trapStates,
                _snapshotTrapStates,
                _materializedObjects,
                out _lastTrapPlan,
                SpawnTrapEnemyByIdViaEnemy,
                RegisterTrapStateFromAdapter);
        }

        // fable_78: elementos ambientais por bioma. Roda após player spawn.
        private void MaterializeEnvironmentElements(CaveGeneratedLevel generatedLevel)
        {
            _environmentElementMaterializer.MaterializeEnvironmentElements(
                generatedLevel,
                _generatedRuntimeRoot.transform,
                _lastPlayerSpawnGrid,
                _snapshotEnvironmentElements,
                _materializedObjects,
                _lastEnvironmentElements,
                out _lastHasWater,
                _lastResourceNodeSnapshots,
                _lastMaterializationResult);
        }

        // fable_60: spawn de inimigo por ID para o baú falso (Hoardmaw), via enemy materializer.
        // Mantido no adapter pois precisa de acesso ao _generatedRuntimeRoot e ao enemy materializer.
        private bool SpawnTrapEnemyByIdViaEnemy(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId) || _generatedRuntimeRoot == null)
            {
                return false;
            }

            EnsureCombatDatabasesBound();
            if (_enemyDatabase == null || !_enemyDatabase.TryGetById(enemyId, out var enemyData) || enemyData == null)
            {
                Debug.LogWarning(
                    $"CaveRuntimeMaterializer: false-chest enemy '{enemyId}' not found in EnemyDatabaseSO. Spawn skipped.",
                    this);
                return false;
            }

            var spawnParent = _generatedRuntimeRoot.transform;
            var instanceId = $"falsechest_{enemyId}_{_trapStates.Count}";
            Vector2 worldPosition = _playerTransform != null
                ? (Vector2)_playerTransform.position
                : (_generatedRuntimeRoot != null ? (Vector2)_generatedRuntimeRoot.transform.position : Vector2.zero);

            var entry = new CaveEnemySpawnPlanEntry
            {
                EnemyId = enemyId,
                EnemyInstanceId = instanceId,
                WorldPosition = worldPosition,
                SizeClass = enemyData.SizeProfileId
            };

            _enemyMaterializer?.UpdateDatabases(
                _enemyDatabase, _movementProfileDatabase, _actionSetDatabase,
                _actionDatabase, _telegraphDatabase, _vulnerabilityProfileDatabase, _sizeProfileDatabase);

            var enemyObject = _enemyMaterializer?.CreateEnemyRuntimeObject(entry, enemyData, spawnParent, _packCoordinator, 0);
            if (enemyObject == null) return false;

            _materializedObjects.Add(enemyObject);

            GameEventBus.Publish(new EnemySpawnedEvent(enemyId, worldPosition, instanceId, 0));
            GameEventBus.Publish(new EnemySeenEvent(enemyId, worldPosition, instanceId, 0));
            return true;
        }

        // fable_60: callback de persistência de estado das armadilhas. Só avança o estado.
        private void RegisterTrapStateFromAdapter(string trapInstanceId, TrapState state)
        {
            if (string.IsNullOrWhiteSpace(trapInstanceId))
            {
                return;
            }

            if (_trapStates.TryGetValue(trapInstanceId, out var entry) && entry != null)
            {
                if ((int)state > entry.State)
                {
                    entry.State = (int)state;
                }

                return;
            }

            _trapStates[trapInstanceId] = new CaveTrapSnapshotEntry
            {
                TrapInstanceId = trapInstanceId,
                TrapKey = string.Empty,
                State = (int)state
            };
        }

        private void RegisterOpenedChest(string chestId)
        {
            if (!string.IsNullOrWhiteSpace(chestId))
            {
                _openedChestIds.Add(chestId);
            }
        }

        private Vector2Int ResolveAnchorPosition(CaveSpawnAnchor anchor, CaveGeneratedLevel generatedLevel)
        {
            return anchor switch
            {
                CaveSpawnAnchor.Entrance => generatedLevel.Entrance,
                CaveSpawnAnchor.ForwardExit => generatedLevel.Exit,
                CaveSpawnAnchor.BackExit => generatedLevel.Entrance,
                _ => generatedLevel.Entrance
            };
        }

        private Vector2Int ResolvePlayerSpawnGrid(
            Vector2Int anchorGridPos,
            CaveSpawnAnchor anchor,
            CaveGeneratedLevel generatedLevel)
        {
            var safeSpawn = FindSafeAdjacentWalkableTile(anchorGridPos, generatedLevel);

            if (safeSpawn.HasValue)
            {
                return safeSpawn.Value;
            }

            if (generatedLevel.WalkableTiles.Count > 0)
            {
                Debug.LogWarning(
                    $"CaveRuntimeMaterializer: No safe adjacent spawn found near anchor {anchor} at {anchorGridPos}. Using first walkable tile as fallback.",
                    this);

                foreach (var tile in generatedLevel.WalkableTiles)
                {
                    return tile;
                }
            }

            Debug.LogError(
                $"CaveRuntimeMaterializer: No walkable tiles available in level {generatedLevel.CaveLevel}. Using anchor position as last resort.",
                this);

            return anchorGridPos;
        }

        private Vector2Int? FindSafeAdjacentWalkableTile(Vector2Int centerPos, CaveGeneratedLevel generatedLevel)
        {
            var directions = new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right,
                new Vector2Int(1, 1),
                new Vector2Int(1, -1),
                new Vector2Int(-1, 1),
                new Vector2Int(-1, -1),
                new Vector2Int(2, 0),
                new Vector2Int(-2, 0),
                new Vector2Int(0, 2),
                new Vector2Int(0, -2)
            };

            foreach (var direction in directions)
            {
                var candidate = centerPos + direction;
                if (generatedLevel.WalkableTiles.Contains(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        public void CleanupMaterialization()
        {
            CleanupPreviousMaterialization();
        }

        private void CleanupPreviousMaterialization()
        {
            using var profilerScope = CleanupMarker.Auto();
            foreach (var obj in _materializedObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            _materializedObjects.Clear();
            // fable_04: coordinator GO was just destroyed; drop the reference.
            _packCoordinator = null;
        }

        private void OnDestroy()
        {
            CleanupPreviousMaterialization();
        }
    }
}
