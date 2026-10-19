using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Quests.Runtime;
using CindarsHope.World.Calendar;

namespace CindarsHope.Quests.CaveContracts
{
    /// <summary>
    /// fable_51 — runtime orchestrator for Zrix's cave contracts (the 8 cc_*).
    ///
    /// REUSE, not a second system: this offers the 6 depth milestones once and rotates the 2
    /// weekly contracts deterministically, all as fable_34 <c>QuestInstance</c>s registered into the
    /// EXISTING <see cref="QuestService"/> flow (single accept/progress/turn-in/save). It owns no
    /// quest state, no second registry, and no second reward formula.
    ///
    /// Determinism: weekly targets come from <see cref="CaveContractCatalog"/> (StableHash over
    /// worldSeed|week) — ADR-0005 generalized; never touches CaveRunSeed / snapshots. Milestone
    /// completion rides QuestService.OnCaveLevelEntered; rematch rides OnEnemyKilled; no-hit is
    /// driven by <see cref="NoHitFloorTracker"/> on a clean clear.
    ///
    /// Pure-testable surface (EditMode): <see cref="RefreshWeek"/>, <see cref="OfferMilestones"/>,
    /// <see cref="OnLevelEntered"/>, <see cref="OnLevelCleared"/>, <see cref="OnPlayerDamaged"/>.
    /// The GameEventBus subscriptions (<see cref="Subscribe"/>/<see cref="Unsubscribe"/>) are a thin
    /// adapter over that surface.
    /// </summary>
    public sealed class CaveContractService
    {
        private readonly QuestService _questService;
        private readonly Func<string> _worldSeedProvider;
        private readonly Func<int> _playerLevelProvider;
        private readonly Func<(int Min, int Max)> _playerBandProvider;

        private readonly NoHitFloorTracker _noHit = new NoHitFloorTracker();

        // Gate bosses already defeated this save — eligibility set for weekly rematch.
        // Stable ids only; populated from CaveBossDefeatedEvent (and seedable for tests).
        private readonly HashSet<string> _defeatedGateBosses = new HashSet<string>(StringComparer.Ordinal);

        private int _currentWeek = -1;
        private bool _milestonesOffered;
        private bool _subscribed;

        public CaveContractService(
            QuestService questService,
            Func<string> worldSeedProvider = null,
            Func<int> playerLevelProvider = null,
            Func<(int Min, int Max)> playerBandProvider = null)
        {
            _questService = questService;
            _worldSeedProvider = worldSeedProvider ?? (() => "world");
            _playerLevelProvider = playerLevelProvider ?? (() => 1);
            _playerBandProvider = playerBandProvider ?? (() => (1, 5));
        }

        /// <summary>Canonical week index from an absolute day (matches GameDate.DaysPerWeek / VeskaWeeklyRotationService).</summary>
        public static int WeekForDay(int day)
        {
            int d = day < 1 ? 1 : day;
            return (d - 1) / GameDate.DaysPerWeek;
        }

        public int CurrentWeek => _currentWeek;
        public NoHitFloorTracker NoHitTracker => _noHit;
        public IReadOnlyCollection<string> DefeatedGateBosses => _defeatedGateBosses;

        // ─── Test/runtime seam: gate eligibility ────────────────────────────────────────────────

        /// <summary>Records a defeated gate boss so it becomes eligible for weekly rematch.</summary>
        public void RegisterDefeatedGateBoss(string bossGateId)
        {
            if (!string.IsNullOrEmpty(bossGateId)) _defeatedGateBosses.Add(bossGateId);
        }

        // ─── Milestones (1×) ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Offers the 6 depth milestones once (registers them so the board can list/accept them).
        /// Idempotent: a completed milestone is not re-offered (QuestService skips re-register of a
        /// known quest, and a completed quest never re-rewards).
        /// </summary>
        public void OfferMilestones()
        {
            if (_questService == null || _milestonesOffered) return;
            foreach (var depth in CaveContractCatalog.MilestoneDepths)
            {
                _questService.RegisterDynamicInstance(CaveContractCatalog.BuildMilestoneInstance(depth));
            }
            _milestonesOffered = true;
        }

        // ─── Weekly rotation ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Regenerates the 2 weekly contracts for <paramref name="week"/> when the week changes.
        /// Already-accepted weeklies are NOT revoked (board rule F34). Publishes
        /// CaveContractsRefreshedEvent. Returns the offered instances (boss rematch may be null
        /// when no gate has been defeated yet — that contract is simply unavailable).
        /// </summary>
        public List<QuestInstance> RefreshWeek(int week)
        {
            var offered = new List<QuestInstance>();
            if (_questService == null) return offered;
            if (week == _currentWeek) return offered;
            _currentWeek = week;

            string seed = _worldSeedProvider();
            int level = _playerLevelProvider();
            var defeated = new List<string>(_defeatedGateBosses);

            var rematch = CaveContractCatalog.BuildBossRematchInstance(seed, week, defeated, level);
            if (rematch != null)
            {
                _questService.RegisterDynamicInstance(rematch);
                offered.Add(rematch);
            }

            var band = _playerBandProvider();
            var noHit = CaveContractCatalog.BuildNoHitInstance(seed, week, band.Min, band.Max, level);
            _questService.RegisterDynamicInstance(noHit);
            offered.Add(noHit);

            GameEventBus.Publish(new CaveContractsRefreshedEvent(week));
            return offered;
        }

        // ─── Progress hooks (pure) ──────────────────────────────────────────────────────────────

        /// <summary>
        /// On entering a cave level: milestone progress is handled by QuestService.OnCaveLevelEntered
        /// (ReachCaveDepth). Here we (re)start the no-hit tracker for the level.
        /// </summary>
        public void OnLevelEntered(int caveLevel)
        {
            _noHit.EnterLevel(caveLevel);
        }

        /// <summary>
        /// On clearing a cave level: if the no-hit weekly is active for this exact level and no
        /// damage was taken, complete it through the existing quest flow (single objective).
        /// </summary>
        public void OnLevelCleared(int caveLevel)
        {
            bool clean = _noHit.EvaluateClear(caveLevel);
            if (!clean || _questService == null) return;

            string questId = CaveContractCatalog.WeeklyInstanceId(CaveContractCatalog.NoHitFloorId, _currentWeek);
            var state = _questService.GetQuestState(questId);
            if (state == null) return; // not accepted — nothing to complete

            // The no-hit target level must match the cleared level.
            if (state.InstanceTargetId != caveLevel.ToString()) return;

            _questService.MarkObjectiveComplete(questId, "obj_" + questId);
        }

        /// <summary>On player damage: dirties the current level's no-hit flag.</summary>
        public void OnPlayerDamaged()
        {
            _noHit.MarkDamaged();
        }

        /// <summary>On a gate boss defeated: record eligibility (and resolve a clean clear path).</summary>
        public void OnBossDefeated(string bossGateId)
        {
            RegisterDefeatedGateBoss(bossGateId);
        }

        // ─── GameEventBus adapter (thin) ────────────────────────────────────────────────────────

        public void Subscribe()
        {
            if (_subscribed) return;
            GameEventBus.Subscribe<DayStartedEvent>(HandleDayStarted);
            GameEventBus.Subscribe<CaveLevelEnteredEvent>(HandleLevelEntered);
            GameEventBus.Subscribe<CaveExitedEvent>(HandleCaveExited);
            GameEventBus.Subscribe<PlayerDamagedEvent>(HandlePlayerDamaged);
            GameEventBus.Subscribe<CaveBossDefeatedEvent>(HandleBossDefeated);
            _subscribed = true;
        }

        public void Unsubscribe()
        {
            if (!_subscribed) return;
            GameEventBus.Unsubscribe<DayStartedEvent>(HandleDayStarted);
            GameEventBus.Unsubscribe<CaveLevelEnteredEvent>(HandleLevelEntered);
            GameEventBus.Unsubscribe<CaveExitedEvent>(HandleCaveExited);
            GameEventBus.Unsubscribe<PlayerDamagedEvent>(HandlePlayerDamaged);
            GameEventBus.Unsubscribe<CaveBossDefeatedEvent>(HandleBossDefeated);
            _subscribed = false;
        }

        private int _lastEnteredLevel;

        private void HandleDayStarted(DayStartedEvent evt)
        {
            OfferMilestones();
            RefreshWeek(WeekForDay(evt.DayNumber));
        }

        private void HandleLevelEntered(CaveLevelEnteredEvent evt)
        {
            // Entering a deeper level means the previous one was cleared (no separate clear event).
            if (_lastEnteredLevel > 0 && evt.CaveLevel > _lastEnteredLevel)
            {
                OnLevelCleared(_lastEnteredLevel);
            }
            _lastEnteredLevel = evt.CaveLevel;
            OnLevelEntered(evt.CaveLevel);
        }

        private void HandleCaveExited(CaveExitedEvent evt)
        {
            // Leaving the cave entirely ends no-hit tracking without crediting a clear.
            _lastEnteredLevel = 0;
            _noHit.Reset();
        }

        private void HandlePlayerDamaged(PlayerDamagedEvent evt)
        {
            OnPlayerDamaged();
        }

        private void HandleBossDefeated(CaveBossDefeatedEvent evt)
        {
            OnBossDefeated(evt.BossGateId);
            // Defeating a gate boss also clears that level cleanly for no-hit evaluation purposes
            // (the level the boss guards) if it matches the tracked level.
            OnLevelCleared(evt.CaveLevel);
        }
    }
}
