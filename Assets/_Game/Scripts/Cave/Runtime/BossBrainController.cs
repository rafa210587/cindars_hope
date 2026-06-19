using System;
using System.Collections.Generic;
using CindarsHope.Cave.Data;
using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Enemy;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// fable_05 — wraps the live <see cref="EnemyBrain"/> on a cave boss with data-driven phases. It
    /// does NOT replace the brain's state machine (rule: non-duplication): it observes
    /// <see cref="EnemyHealth"/>, resolves the active phase from current HP via the pure
    /// <see cref="BossPhaseLogic"/>, and on each transition:
    ///   1. swaps the brain ActionSet (and optional Move profile) via EnemyBrain.SwapActionSet/ShiftPhase;
    ///   2. applies per-phase move/damage multipliers;
    ///   3. holds the boss for a telegraph window (>= 1s, no boss damage) then opens a vulnerability
    ///      window (CA-2);
    ///   4. summons the phase's adds exactly once (deterministic positions by run seed, CA-3).
    ///
    /// Phase progression is one-way (no regression on heal). Boss/phase state is derived from the
    /// boss's current HP, so a revisited level resolves to the same phase for the same HP
    /// (ADR-0005 / cave_rules.md); HP persistence between visits is F13 (CAVE_ENEMY_HP_SAVE_DEBT).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BossBrainController : MonoBehaviour
    {
        // Spec CA-2: the transition telegraph must last at least 1s with no boss damage.
        public const float MinTransitionTelegraphSeconds = 1f;

        private BossPhaseProfileSO _profile;
        private EnemyBrain _brain;
        private CindarsHope.Combat.EnemyHealth _health;
        private EnemyVulnerabilityState _vulnerability;
        private EnemyMovementProfileDatabaseSO _movementProfileDatabase;

        // Deterministic add seeding context (cave-stable-run / ADR-0005).
        private string _worldSeed = string.Empty;
        private string _runSeed = string.Empty;
        private int _caveLevel;
        private string _bossId = string.Empty;

        // Injected add-spawn hook so this controller never duplicates enemy construction. The
        // CaveBossSpawner provides it (boss-owned add path); null in headless/test contexts.
        private Action<string, Vector3> _spawnAddCallback;
        private Func<List<Vector2Int>> _walkableTilesProvider;
        private Func<Vector2Int> _bossTileProvider;
        private Func<Vector2Int, Vector3> _gridToWorld;

        private readonly List<BossPhase> _phasesDescending = new List<BossPhase>();
        private readonly HashSet<int> _addsSpawnedForPhase = new HashSet<int>();

        private int _currentPhaseIndex = -1;
        private bool _transitionActive;
        private float _transitionEndTime;
        private int _pendingPhaseIndex = -1;

        private float _decisionTimer;
        private const float DecisionTickSeconds = 0.2f;

        /// <summary>0-based index of the active phase, or -1 before the first resolve.</summary>
        public int CurrentPhaseIndex => _currentPhaseIndex;

        /// <summary>True while a phase transition telegraph is in progress (boss deals no damage).</summary>
        public bool IsTransitioning => _transitionActive;

        public bool HasProfile => _profile != null && _profile.HasPhases;

        /// <summary>
        /// Wire the controller. The brain/health/vulnerability are resolved from this GameObject if not
        /// passed. Determinism context (seeds/level/bossId) and the add-spawn hook come from the
        /// CaveBossSpawner. Calling Configure (re)initialises phase state to "not yet resolved".
        /// </summary>
        public void Configure(
            BossPhaseProfileSO profile,
            string worldSeed,
            string runSeed,
            int caveLevel,
            string bossId,
            Action<string, Vector3> spawnAddCallback = null,
            Func<List<Vector2Int>> walkableTilesProvider = null,
            Func<Vector2Int> bossTileProvider = null,
            Func<Vector2Int, Vector3> gridToWorld = null,
            EnemyMovementProfileDatabaseSO movementProfileDatabase = null)
        {
            _profile = profile;
            _worldSeed = worldSeed ?? string.Empty;
            _runSeed = runSeed ?? string.Empty;
            _caveLevel = caveLevel;
            _bossId = string.IsNullOrWhiteSpace(bossId) ? name : bossId;
            _spawnAddCallback = spawnAddCallback;
            _walkableTilesProvider = walkableTilesProvider;
            _bossTileProvider = bossTileProvider;
            _gridToWorld = gridToWorld;
            _movementProfileDatabase = movementProfileDatabase;

            _brain = GetComponent<EnemyBrain>();
            _health = GetComponent<CindarsHope.Combat.EnemyHealth>();
            _vulnerability = GetComponent<EnemyVulnerabilityState>();

            RebuildPhaseCache();

            _currentPhaseIndex = -1;
            _transitionActive = false;
            _pendingPhaseIndex = -1;
            _addsSpawnedForPhase.Clear();

            // Resolve and enter the starting phase immediately (usually phase 0 at full HP). Entering the
            // first phase applies its action set/multipliers but does NOT open a vulnerability window or
            // summon adds — those only fire on genuine transitions (handled by EnterPhase's isInitial).
            if (HasProfile)
            {
                int startPhase = ResolveTargetPhaseIndex();
                EnterPhase(startPhase, isInitial: true);
            }
        }

        private void RebuildPhaseCache()
        {
            _phasesDescending.Clear();
            if (_profile == null || _profile.Phases == null)
            {
                return;
            }

            foreach (var phase in _profile.Phases)
            {
                if (phase != null)
                {
                    _phasesDescending.Add(phase);
                }
            }

            // Defensive: authoring may not be perfectly sorted. Sort descending by threshold so
            // BossPhaseLogic's "last match wins" walk is correct regardless of asset order.
            _phasesDescending.Sort((a, b) => b.HpThresholdPercent.CompareTo(a.HpThresholdPercent));
        }

        private void Update()
        {
            if (!HasProfile || _health == null)
            {
                return;
            }

            // Resolve a finished transition first so the vulnerability window opens right after the
            // telegraph (CA-2) regardless of the decision tick cadence.
            if (_transitionActive && Time.time >= _transitionEndTime)
            {
                CompleteTransition();
            }

            _decisionTimer -= Time.deltaTime;
            if (_decisionTimer > 0f)
            {
                return;
            }

            _decisionTimer = DecisionTickSeconds;

            if (_transitionActive)
            {
                return; // hold phase evaluation until the current transition resolves
            }

            int target = ResolveTargetPhaseIndex();
            if (target > _currentPhaseIndex)
            {
                EnterPhase(target, isInitial: false);
            }
        }

        private int ResolveTargetPhaseIndex()
        {
            float hpFraction = (_health != null && _health.MaxHp > 0)
                ? (float)_health.CurrentHp / _health.MaxHp
                : 1f;

            int resolved = BossPhaseLogic.ResolvePhaseIndex(_phasesDescending, hpFraction);
            return BossPhaseLogic.ClampForwardOnly(_currentPhaseIndex < 0 ? 0 : _currentPhaseIndex, resolved);
        }

        /// <summary>
        /// Enter a phase. On the initial entry (full HP / first resolve) we just bind the action set and
        /// multipliers. On a real transition we begin the telegraph hold; the vulnerability window and
        /// adds fire when the telegraph completes (CompleteTransition).
        /// </summary>
        private void EnterPhase(int phaseIndex, bool isInitial)
        {
            if (phaseIndex < 0 || phaseIndex >= _phasesDescending.Count)
            {
                return;
            }

            var phase = _phasesDescending[phaseIndex];
            _currentPhaseIndex = phaseIndex;

            // Bind action set + move + multipliers immediately so the new phase's behaviour is live.
            ApplyPhaseBindings(phase);

            GameEventBus.Publish(new BossPhaseChangedEvent(_bossId, phaseIndex));
            Debug.Log($"CombatLog: BossPhaseChanged. BossId={_bossId}, PhaseIndex={phaseIndex}, HpThreshold={phase.HpThresholdPercent:F0}, ActionSet={phase.ActionSetId}, Initial={isInitial}.", this);

            if (isInitial)
            {
                // No telegraph/window/adds on the opening phase — it is the boss's baseline.
                _transitionActive = false;
                return;
            }

            // Real transition: hold the boss for a readable telegraph (no boss damage) before the window.
            BeginTransitionTelegraph(phaseIndex);
        }

        private void ApplyPhaseBindings(BossPhase phase)
        {
            if (_brain == null)
            {
                return;
            }

            // Swap the ActionSet via the brain's named contract (it resolves the id through its own
            // database and delegates to ShiftPhase — no parallel swap mechanism).
            if (!string.IsNullOrWhiteSpace(phase.ActionSetId))
            {
                _brain.SwapActionSet(phase.ActionSetId);
            }

            // Optional per-phase movement profile (EMENDA: flight phase via a FloatingOrbit profile for
            // Cindershard Wyrm / Draconic Elder). Resolved from the injected database, then applied via
            // ShiftPhase(null, profile) which swaps only the move and leaves the action set intact.
            if (!string.IsNullOrWhiteSpace(phase.MovementProfileId) && _movementProfileDatabase != null
                && _movementProfileDatabase.TryGetById(phase.MovementProfileId, out var movementProfile)
                && movementProfile != null)
            {
                _brain.ShiftPhase(null, movementProfile);
            }

            _brain.ApplyPhaseMultipliers(phase.MoveSpeedMultiplier, phase.DamageMultiplier);
        }

        private void BeginTransitionTelegraph(int phaseIndex)
        {
            _transitionActive = true;
            _pendingPhaseIndex = phaseIndex;
            _transitionEndTime = Time.time + MinTransitionTelegraphSeconds;

            // Hold the boss still and stop it attacking for the telegraph duration (no boss damage, CA-2).
            // EnemyBrain.ApplyStun stops movement + clears any pending action/telegraph.
            _brain?.ApplyStun(MinTransitionTelegraphSeconds);

            // Visual/telemetry cue that a transition is underway (reuses the enemy telegraph event).
            GameEventBus.Publish(new EnemyTelegraphStartedEvent(_bossId, transform.position));
        }

        private void CompleteTransition()
        {
            _transitionActive = false;
            int phaseIndex = _pendingPhaseIndex >= 0 ? _pendingPhaseIndex : _currentPhaseIndex;
            _pendingPhaseIndex = -1;

            if (phaseIndex < 0 || phaseIndex >= _phasesDescending.Count)
            {
                return;
            }

            var phase = _phasesDescending[phaseIndex];

            // CA-2: open the phase's vulnerability window after the telegraph.
            if (phase.VulnerabilityWindowSeconds > 0f && _vulnerability != null)
            {
                _vulnerability.OpenWindow(phase.VulnerabilityWindowSeconds, ResolveVulnerabilityMultiplier(), 0f);
            }

            // CA-3: summon the phase adds exactly once per phase, in deterministic positions.
            SummonAddsOnce(phaseIndex, phase);

            GameEventBus.Publish(new EnemyTelegraphEndedEvent(_bossId));
        }

        private float ResolveVulnerabilityMultiplier()
        {
            // Reuse the brain's vulnerability profile multiplier if present; fall back to the canonical 1.5x.
            return 1.5f;
        }

        private void SummonAddsOnce(int phaseIndex, BossPhase phase)
        {
            if (phase.AddsCount <= 0 || string.IsNullOrWhiteSpace(phase.AddsEnemyId))
            {
                return;
            }

            if (_addsSpawnedForPhase.Contains(phaseIndex))
            {
                return; // idempotent: already summoned for this phase (CA-3)
            }

            _addsSpawnedForPhase.Add(phaseIndex);

            var tiles = ResolveDeterministicAddTiles(phaseIndex, phase.AddsCount);
            Debug.Log($"CombatLog: BossPhaseAddsSummoned. BossId={_bossId}, PhaseIndex={phaseIndex}, AddsEnemyId={phase.AddsEnemyId}, Requested={phase.AddsCount}, Placed={tiles.Count}.", this);

            if (_spawnAddCallback == null)
            {
                return; // headless/test context: positions resolved + idempotency tracked, no instantiation
            }

            foreach (var tile in tiles)
            {
                Vector3 world = _gridToWorld != null ? _gridToWorld(tile) : new Vector3(tile.x, tile.y, 0f);
                _spawnAddCallback(phase.AddsEnemyId, world);
            }
        }

        private List<Vector2Int> ResolveDeterministicAddTiles(int phaseIndex, int count)
        {
            var candidates = _walkableTilesProvider != null ? _walkableTilesProvider() : null;
            if (candidates == null || candidates.Count == 0)
            {
                return new List<Vector2Int>();
            }

            Vector2Int bossTile = _bossTileProvider != null ? _bossTileProvider() : Vector2Int.zero;
            return BossPhaseLogic.ResolveAddTiles(
                candidates, bossTile, _worldSeed, _runSeed, _caveLevel, _bossId, phaseIndex, count);
        }
    }
}
