using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.NPC.Friendship;
using CindarsHope.Quests.Runtime;
using CindarsHope.Quests.Save;

namespace CindarsHope.Quests.FestivalQuests
{
    /// <summary>
    /// fable_53 — runtime orchestrator for the 8 festival quests (fq_*).
    ///
    /// REUSE, not a second system: it offers the one fq_* of the active festival as a fable_34
    /// <see cref="QuestInstance"/> through the EXISTING <see cref="QuestService"/> (single accept /
    /// progress / turn-in / save) and the single reward scaling point. It owns no quest registry, no
    /// second reward formula, and no second festival calendar — the festival id and timing come from
    /// F37 (<c>FestivalStartedEvent</c> / <c>WorldEventService.IsFestivalActive</c>).
    ///
    /// Lifecycle (CA-1/CA-2): on the festival start it offers + accepts the day's fq_*; when the day
    /// turns and the festival is no longer active, any still-active fq_* expires WITHOUT punishment
    /// (state Expired, leaves the active log) and reopens next year as a new per-year instance. The
    /// trophy/title flag is granted only on the first ever completion (idempotent across years).
    ///
    /// Pure-testable surface (EditMode): <see cref="OfferForFestival"/>, <see cref="ExpireForDay"/>,
    /// <see cref="RegisterEcho"/>, <see cref="RegisterGift"/>, the per-kind progress hooks, and the
    /// trackers. The GameEventBus subscriptions are a thin adapter over that surface.
    /// </summary>
    public sealed class FestivalQuestService
    {
        private readonly QuestService _questService;
        private readonly Func<int> _playerLevelProvider;

        private readonly NightEchoTracker _echoTracker = new NightEchoTracker(3);
        private readonly GiftCountTracker _giftTracker = new GiftCountTracker(5);

        // The fq_* currently offered/active this festival day (concrete per-year instance id), or null.
        private string _activeQuestId;
        private string _activeTemplateId;
        private string _activeFestivalId;
        private int _activeYear;
        private bool _subscribed;

        public FestivalQuestService(QuestService questService, Func<int> playerLevelProvider = null)
        {
            _questService = questService;
            _playerLevelProvider = playerLevelProvider ?? (() => 1);
        }

        public NightEchoTracker EchoTracker => _echoTracker;
        public GiftCountTracker GiftTracker => _giftTracker;
        public string ActiveQuestId => _activeQuestId;

        // ─── Offer (CA-1) ───────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Offers (and accepts) the festival quest for <paramref name="festivalId"/> on
        /// <paramref name="absoluteDay"/>. Returns the concrete per-year quest id, or null when the
        /// festival has no fq_* or the quest already completed this year (no re-offer same year).
        /// Trophy reward is attached only when not yet granted in any prior year.
        /// </summary>
        public string OfferForFestival(string festivalId, int absoluteDay)
        {
            if (_questService == null) return null;
            var def = FestivalQuestCatalog.FindByFestival(festivalId);
            if (def == null) return null;

            int year = FestivalQuestCatalog.YearForDay(absoluteDay);
            string questId = FestivalQuestCatalog.InstanceId(def.QuestId, year);

            // Already completed this year? Do not re-offer the same year (idempotent annual key).
            var existing = _questService.GetQuestState(questId);
            if (existing != null &&
                (QuestStateStatus)existing.State == QuestStateStatus.Completed)
            {
                return null;
            }

            bool trophyGranted = !string.IsNullOrEmpty(def.TrophyFlagId) && IsTrophyAlreadyGranted(def.TrophyFlagId);
            var instance = FestivalQuestCatalog.BuildInstance(def, year, _playerLevelProvider(), trophyGranted);
            if (instance == null) return null;

            // Reset the per-night trackers for the new festival night.
            _echoTracker.ResetNight();
            _giftTracker.ResetNight();
            _localCounts.Remove(questId);

            if (existing == null)
            {
                if (!_questService.AcceptDynamicInstance(instance)) return null;
            }

            _activeQuestId = instance.QuestId;
            _activeTemplateId = def.QuestId;
            _activeFestivalId = def.FestivalId;
            _activeYear = year;

            GameEventBus.Publish(new FestivalQuestOfferedEvent(instance.QuestId, def.QuestId, def.FestivalId, year));
            return instance.QuestId;
        }

        // ─── Expire (CA-1) ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Ends the festival window for the new <paramref name="absoluteDay"/>: if a fq_* is still
        /// active and its festival is no longer active today, it expires without punishment (state
        /// Expired) and leaves the active log. <paramref name="festivalActiveToday"/> is the F37 truth
        /// (WorldEventService.IsFestivalActive of the previously active festival id). Returns the
        /// expired quest id, or null when nothing expired.
        /// </summary>
        public string ExpireForDay(int absoluteDay, bool festivalActiveToday)
        {
            if (_questService == null || string.IsNullOrEmpty(_activeQuestId)) return null;
            if (festivalActiveToday) return null; // festival still running — keep the quest

            var record = _questService.GetQuestState(_activeQuestId);
            string expiredId = _activeQuestId;
            string expiredTemplate = _activeTemplateId;

            // Only an unfinished quest "expires"; a completed one simply stays completed (trophy kept).
            if (record != null &&
                (QuestStateStatus)record.State != QuestStateStatus.Completed)
            {
                record.State = (int)QuestStateStatus.Expired;
                GameEventBus.Publish(new FestivalQuestExpiredEvent(expiredId, expiredTemplate));
            }

            _activeQuestId = null;
            _activeTemplateId = null;
            _activeFestivalId = null;
            _activeYear = 0;
            _echoTracker.ResetNight();
            _giftTracker.ResetNight();
            _localCounts.Remove(expiredId);
            return expiredId;
        }

        // ─── Trackers (CA-3) ──────────────────────────────────────────────────────────────────────

        /// <summary>fq_luas — an echo collected under <paramref name="moonId"/>. Completes when the 3
        /// distinct moons are gathered the same night (the active quest must be fq_luas).</summary>
        public void RegisterEcho(string moonId)
        {
            if (_activeTemplateId != FestivalQuestCatalog.MoonsId) return;
            bool progressed = _echoTracker.CollectEcho(moonId);
            if (progressed && _echoTracker.IsComplete)
            {
                CompleteActiveObjective();
            }
        }

        /// <summary>fq_anonovo — a gift accepted by <paramref name="npcId"/> before midnight. Completes
        /// when 5 distinct NPCs are gifted in time (the active quest must be fq_anonovo).</summary>
        public void RegisterGift(string npcId)
        {
            if (_activeTemplateId != FestivalQuestCatalog.NewYearId) return;
            bool progressed = _giftTracker.RegisterGift(npcId);
            if (progressed && _giftTracker.IsComplete)
            {
                CompleteActiveObjective();
            }
        }

        /// <summary>Midnight on the new-year festival closes the gift window (deadline is real).</summary>
        public void CloseGiftWindowAtMidnight()
        {
            _giftTracker.CloseAtMidnight();
        }

        // ─── Per-kind progress hooks (reuse existing events) ───────────────────────────────────────

        /// <summary>fq_plantio — a Thandra seed was planted (PlantCrop). Progresses count toward N.</summary>
        public void OnSeedPlanted(string seedId)
        {
            if (_activeTemplateId != FestivalQuestCatalog.PlantingId) return;
            ProgressActiveCount(seedId);
        }

        /// <summary>fq_caravana — an escort encounter was cleared / fq_torneio — an arena duel won
        /// (DefeatEnemy). Progresses count toward N.</summary>
        public void OnEncounterCleared(string targetId)
        {
            if (_activeTemplateId != FestivalQuestCatalog.CaravanId &&
                _activeTemplateId != FestivalQuestCatalog.TournamentId)
            {
                return;
            }
            ProgressActiveCount(targetId);
        }

        /// <summary>fq_colheita — a Gold-quality crop delivered (DeliverItem). Completes the objective.</summary>
        public void OnGoldCropDelivered(string itemId)
        {
            if (_activeTemplateId != FestivalQuestCatalog.HarvestId) return;
            if (MatchesActiveTarget(itemId)) CompleteActiveObjective();
        }

        /// <summary>fq_veus — the special night-market item was bought (BuyItem). Completes the objective.</summary>
        public void OnSpecialItemBought(string itemId)
        {
            if (_activeTemplateId != FestivalQuestCatalog.VeilsId) return;
            if (MatchesActiveTarget(itemId)) CompleteActiveObjective();
        }

        /// <summary>fq_vigilia — the Fountain vigil interaction completed at night (InteractWithObject;
        /// no combat). Completes the objective.</summary>
        public void OnFountainVigil(string fountainId)
        {
            if (_activeTemplateId != FestivalQuestCatalog.VigilId) return;
            if (MatchesActiveTarget(fountainId)) CompleteActiveObjective();
        }

        // ─── GameEventBus adapter (thin) ───────────────────────────────────────────────────────────

        public void Subscribe()
        {
            if (_subscribed) return;
            GameEventBus.Subscribe<FestivalStartedEvent>(HandleFestivalStarted);
            GameEventBus.Subscribe<DayStartedEvent>(HandleDayStarted);
            GameEventBus.Subscribe<NpcGiftReactionEvent>(HandleGiftReaction);
            _subscribed = true;
        }

        public void Unsubscribe()
        {
            if (!_subscribed) return;
            GameEventBus.Unsubscribe<FestivalStartedEvent>(HandleFestivalStarted);
            GameEventBus.Unsubscribe<DayStartedEvent>(HandleDayStarted);
            GameEventBus.Unsubscribe<NpcGiftReactionEvent>(HandleGiftReaction);
            _subscribed = false;
        }

        private void HandleFestivalStarted(FestivalStartedEvent evt)
        {
            OfferForFestival(evt.FestivalId, evt.DayNumber);
        }

        private void HandleDayStarted(DayStartedEvent evt)
        {
            // A new day arrives: if today is NOT the active festival (festivals are single-day in F37),
            // any still-active fq_* expires without punishment. FestivalStartedEvent (if any) fires
            // separately to offer today's quest.
            bool stillActive = _activeFestivalId != null &&
                               FestivalQuestCatalog.YearForDay(evt.DayNumber) == _activeYear &&
                               IsFestivalDay(_activeFestivalId, evt.DayNumber);
            ExpireForDay(evt.DayNumber, stillActive);
        }

        private void HandleGiftReaction(NpcGiftReactionEvent evt)
        {
            RegisterGift(evt.NpcId);
        }

        /// <summary>Pure calendar check: is <paramref name="festivalId"/> the festival on this day?
        /// (F37 festivals are single-day, addressed by Season/DayInSeason — derived, no second calendar.)</summary>
        private static bool IsFestivalDay(string festivalId, int absoluteDay)
        {
            var def = CindarsHope.World.Events.WorldEventResolver.ResolveFestival(absoluteDay);
            return def != null && def.FestivalId == festivalId;
        }

        // ─── Internals ──────────────────────────────────────────────────────────────────────────

        private bool MatchesActiveTarget(string targetId)
        {
            if (string.IsNullOrEmpty(_activeQuestId)) return false;
            var record = _questService.GetQuestState(_activeQuestId);
            if (record == null) return false;
            return targetId == "any" || record.InstanceTargetId == "any" || record.InstanceTargetId == targetId;
        }

        private void CompleteActiveObjective()
        {
            if (string.IsNullOrEmpty(_activeQuestId)) return;
            _questService.MarkObjectiveComplete(_activeQuestId, "obj_" + _activeQuestId);
        }

        // A dynamic fable_34 instance registers a single CollectItem objective (the generic default in
        // QuestService.ObjectiveTypeForSource) which is NOT auto-progressed by the typed event hooks for
        // festival targets. So this service owns the count: it increments a festival-local counter
        // (same-night, cleared on offer/expire) and completes the single objective via the EXISTING
        // QuestService.MarkObjectiveComplete entry point once the required amount is reached. There is
        // still a single source of quest state (QuestService); only the per-kind matching lives here.
        private readonly Dictionary<string, int> _localCounts = new Dictionary<string, int>();

        private void ProgressActiveCount(string targetId)
        {
            if (!MatchesActiveTarget(targetId)) return;

            var record = _questService.GetQuestState(_activeQuestId);
            if (record == null) return;

            int required = record.InstanceQuantity < 1 ? 1 : record.InstanceQuantity;
            int current = _localCounts.TryGetValue(_activeQuestId, out var c) ? c : 0;
            current++;
            _localCounts[_activeQuestId] = current;

            if (current >= required)
            {
                CompleteActiveObjective();
            }
        }

        // Trophy is one-time across all years: a prior completed fq_* of the same template carries the
        // trophy flag in its persisted GrantedFlagIds (survives save). Scan completed records for it.
        private bool IsTrophyAlreadyGranted(string trophyFlagId)
        {
            if (string.IsNullOrEmpty(trophyFlagId)) return false;
            foreach (var record in _questService.GetCompletedQuests())
            {
                if (record.GrantedFlagIds != null && record.GrantedFlagIds.Contains(trophyFlagId))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
