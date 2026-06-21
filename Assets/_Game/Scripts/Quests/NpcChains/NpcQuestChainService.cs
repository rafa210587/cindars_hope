using System;
using System.Collections.Generic;
using CindarsHope.Quests.Flags;
using CindarsHope.Quests.Runtime;

namespace CindarsHope.Quests.NpcChains
{
    /// <summary>
    /// fable_35 — runtime orchestrator for the 12 NPC side-quest chains (sq_&lt;npc&gt;_&lt;n&gt;).
    ///
    /// REUSE, not a second system (spec §138):
    /// - offers a chain step as a fable_34 <see cref="QuestInstance"/> through the EXISTING
    ///   <see cref="QuestService"/> (single accept / progress / turn-in / save) and the single reward
    ///   scaling point — no second registry, no second reward formula;
    /// - the composite eligibility gate (previous step done + min friendship + optional act flag) is
    ///   evaluated by reading the EXISTING QuestFlagService (flags) and an injected friendship probe
    ///   (FriendshipService.IsAtLeast) — no new gate API;
    /// - the +8 amizade per completed quest is granted by the EXISTING FriendshipService turn-in hook
    ///   (OnQuestGiverInteracted → RegisterQuestCompleted) — this service never touches friendship state;
    /// - objective completion rides the EXISTING typed event hooks: a dynamic fable_34 instance carries a
    ///   generic count objective, so this service maps the chain target to the matching gameplay event and
    ///   completes the single objective via <see cref="QuestService.MarkObjectiveComplete"/> (same pattern
    ///   as <c>FestivalQuestService</c>). There is still ONE source of quest state (QuestService).
    ///
    /// Pure-testable surface (EditMode): <see cref="IsEligible"/>, <see cref="Offer"/>,
    /// <see cref="GetOfferableStep"/>, and the per-kind progress hooks. No Unity dependency.
    /// </summary>
    public sealed class NpcQuestChainService
    {
        private readonly QuestService _questService;
        private readonly QuestFlagService _flagService;
        private readonly Func<string, int, bool> _friendshipAtLeast; // (npcId, level) => bool

        // Per-quest count progress for count-based chain objectives (Collect/Defeat/Harvest), cleared on
        // offer. Collect rides inventory auto-progress when the target is a real item; the service-owned
        // counter covers Defeat (and any kind not auto-progressed by the generic CollectItem default).
        private readonly Dictionary<string, int> _counts = new Dictionary<string, int>();

        public NpcQuestChainService(QuestService questService, QuestFlagService flagService,
            Func<string, int, bool> friendshipAtLeast = null)
        {
            _questService = questService;
            _flagService = flagService;
            _friendshipAtLeast = friendshipAtLeast ?? ((npc, lvl) => true);
        }

        // ─── Eligibility (CA-2) ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// True when a chain step may be OFFERED right now (CA-2 composite gate):
        /// (1) it is not already accepted/completed; (2) its previous step is completed (done flag set);
        /// (3) friendship with the NPC is at least the chain progress minimum (steps 2+); (4) the cited
        /// act flag (if any) is set; (5) the step is not dormant.
        /// </summary>
        public bool IsEligible(string questId)
        {
            var step = NpcQuestChainCatalog.FindByQuestId(questId);
            if (step == null || step.Dormant) return false;

            // Not already in progress / done.
            var existing = _questService?.GetQuestState(questId);
            if (existing != null)
            {
                var status = (QuestStateStatus)existing.State;
                if (status == QuestStateStatus.Active ||
                    status == QuestStateStatus.ReadyToComplete ||
                    status == QuestStateStatus.Completed)
                {
                    return false;
                }
            }

            // Previous step must be completed (its done flag set). Step 1 has no predecessor.
            var prev = NpcQuestChainCatalog.PreviousStep(questId);
            if (prev != null && !IsFlagSet(NpcQuestChainCatalog.DoneFlag(prev.QuestId)))
            {
                return false;
            }

            // Friendship gate. fable_70: a per-step MinFriendshipOverride (from the v1.1 roster's varied
            // gate columns, e.g. Renko 1/2/4, Maelor 2/3/4) takes precedence and applies at ANY step index;
            // otherwise the fable_35 default applies (global ChainProgressFriendshipMin from step 2 on).
            int requiredFriendship = step.MinFriendshipOverride > 0
                ? step.MinFriendshipOverride
                : (step.Step >= 2 ? NpcQuestChainCatalog.ChainProgressFriendshipMin : 0);
            if (requiredFriendship > 0 && !_friendshipAtLeast(step.NpcId, requiredFriendship))
            {
                return false;
            }

            // Cross-act gate (read-only consumption of an F36 act flag), when the catalog cites one.
            if (!string.IsNullOrEmpty(step.RequiredActFlagId) && !IsFlagSet(step.RequiredActFlagId))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The single offerable step of an NPC's chain right now (the lowest-index eligible step), or null
        /// when the chain is complete or gated. Drives the "[!] Quest" dialogue offer marker.
        /// </summary>
        public NpcChainStepData GetOfferableStep(string npcId)
        {
            foreach (var step in NpcQuestChainCatalog.ForNpc(npcId))
            {
                if (IsEligible(step.QuestId)) return step;
            }
            return null;
        }

        /// <summary>True when the NPC has any offerable chain step right now (dialogue "[!]" predicate).</summary>
        public bool HasOffer(string npcId) => GetOfferableStep(npcId) != null;

        // ─── Offer / accept (CA-1) ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Offers (accepts) a chain step through the existing QuestService dynamic-instance flow. Returns
        /// the accepted quest id, or null when the step is not eligible / already running. Resets the
        /// service-owned count for the new step.
        /// </summary>
        public string Offer(string questId)
        {
            if (_questService == null) return null;
            if (!IsEligible(questId)) return null;

            var step = NpcQuestChainCatalog.FindByQuestId(questId);
            var instance = NpcQuestChainCatalog.BuildInstance(step);
            if (instance == null) return null;

            if (!_questService.AcceptDynamicInstance(instance)) return null;

            _counts.Remove(questId);
            return questId;
        }

        /// <summary>Offers the next eligible step of an NPC's chain, or null when none is offerable.</summary>
        public string OfferNextForNpc(string npcId)
        {
            var step = GetOfferableStep(npcId);
            return step == null ? null : Offer(step.QuestId);
        }

        // ─── Objective progress hooks (reuse existing gameplay events) ──────────────────────────────────

        /// <summary>An enemy of a band/boss was defeated — progresses any active Defeat chain step on it.</summary>
        public void OnEnemyDefeated(string enemyId) => ProgressActive(NpcChainObjectiveKind.Defeat, enemyId);

        /// <summary>A crop was harvested — progresses any active Harvest chain step ("any" or the crop id).</summary>
        public void OnCropHarvested(string cropId) => ProgressActive(NpcChainObjectiveKind.Harvest, cropId);

        /// <summary>A crop was planted — completes a Plant chain step on the target seed.</summary>
        public void OnCropPlanted(string seedId) => CompleteIfMatch(NpcChainObjectiveKind.Plant, seedId);

        /// <summary>An item was crafted — completes a Craft chain step (count) on the target item.</summary>
        public void OnItemCrafted(string itemId) => ProgressActive(NpcChainObjectiveKind.Craft, itemId);

        /// <summary>An NPC was talked to / accompanied — completes a Talk chain step on that target.</summary>
        public void OnNpcTalkedTo(string npcId) => CompleteIfMatch(NpcChainObjectiveKind.Talk, npcId);

        /// <summary>A location / cave level was reached or investigated — completes a Reach chain step.</summary>
        public void OnLocationReached(string locationId) => CompleteIfMatch(NpcChainObjectiveKind.Reach, locationId);

        /// <summary>An item was delivered — completes a Deliver chain step on that item/target.</summary>
        public void OnItemDelivered(string itemId) => CompleteIfMatch(NpcChainObjectiveKind.Deliver, itemId);

        // ─── Internals ──────────────────────────────────────────────────────────────────────────────────

        // Completes the single objective the moment a count target is reached (Defeat/Harvest/Craft).
        private void ProgressActive(NpcChainObjectiveKind kind, string targetId)
        {
            foreach (var step in EachActiveStepOfKind(kind, targetId))
            {
                int required = step.Quantity < 1 ? 1 : step.Quantity;
                int current = _counts.TryGetValue(step.QuestId, out var c) ? c : 0;
                current++;
                _counts[step.QuestId] = current;
                if (current >= required)
                {
                    Complete(step.QuestId);
                }
            }
        }

        // Completes the single objective immediately (Talk/Reach/Plant/Deliver — single-shot kinds).
        private void CompleteIfMatch(NpcChainObjectiveKind kind, string targetId)
        {
            foreach (var step in EachActiveStepOfKind(kind, targetId))
            {
                Complete(step.QuestId);
            }
        }

        private IEnumerable<NpcChainStepData> EachActiveStepOfKind(NpcChainObjectiveKind kind, string targetId)
        {
            if (_questService == null) yield break;
            foreach (var step in NpcQuestChainCatalog.AllSteps)
            {
                if (step.ObjectiveKind != kind) continue;
                if (!TargetMatches(step.TargetId, targetId)) continue;
                var record = _questService.GetQuestState(step.QuestId);
                if (record == null) continue;
                if ((QuestStateStatus)record.State != QuestStateStatus.Active) continue;
                yield return step;
            }
        }

        private static bool TargetMatches(string stepTarget, string eventTarget)
        {
            if (string.IsNullOrEmpty(stepTarget)) return false;
            return stepTarget == "any" || eventTarget == "any" || stepTarget == eventTarget;
        }

        private void Complete(string questId)
        {
            _questService.MarkObjectiveComplete(questId, "obj_" + questId);
        }

        private bool IsFlagSet(string flagId)
        {
            if (string.IsNullOrEmpty(flagId)) return true;
            return _flagService != null && _flagService.IsSet(flagId);
        }
    }
}
