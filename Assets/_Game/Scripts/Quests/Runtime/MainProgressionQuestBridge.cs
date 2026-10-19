using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Fonte;
using CindarsHope.MainProgression;
using UnityEngine;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// fable_10 + fable_36 — bridges the main questline (Acts 1-4) to the WAVE 10 Fonte /
    /// MainProgression state and to the per-act skill-point grant.
    ///
    /// When the FINAL quest of an act completes (QuestCompletedEvent), this bridge — for that act —
    ///  1. integrates the act's fragment into the Fonte via the EXISTING WAVE 10 API
    ///     (<see cref="FonteRuntimeService.IntegrateFragment"/>), re-running the unlock pass
    ///     (Act 1 -> Agua Viva, Act 2 -> respec, Act 3 -> purification). Act 4's Hope fragment is
    ///     HINTED only (no integration): the integration + the irreversible final choice belong to
    ///     fable_43 (endgame). No parallel progression system is created (fonte_rules Rule 2).
    ///  2. grants +1 skill point via <see cref="QuestService.TryAwardActSkillPoint"/> — idempotent
    ///     across reloads through the persisted RewardedMainActIds (quest_rules / FABLE Q6.2b: 4 total).
    ///  3. publishes <see cref="ActCompletedEvent"/> (toast / lore-record feedback).
    ///
    /// Idempotency (CA-2): fragment grant is guarded three ways (local session set per fragment, the
    /// Fonte already reporting the fragment integrated — survives save/load — and the Fonte's own
    /// already-integrated short-circuit). The skill point is guarded by RewardedMainActIds. So a
    /// reload + re-turn-in never duplicates a fragment or a skill point.
    ///
    /// Communication is event-only (subscribes to QuestCompletedEvent on the GameEventBus);
    /// no GameObject.Find / FindObjectOfType. The bridge is a pure C# object owned by
    /// QuestRuntimeBootstrap; <see cref="Unsubscribe"/> is called on teardown. The QuestService used
    /// for the skill-point grant is injected (never searched).
    /// </summary>
    public sealed class MainProgressionQuestBridge
    {
        /// <summary>fable_10 — kept for back-compat: the Act 1 final quest (Water fragment grant).</summary>
        public const string FragmentGrantQuestId = QuestMainAct1Ids.Quest05FragmentoDaAgua;

        private bool _subscribed;
        private readonly HashSet<MainFragmentType> _fragmentsGranted = new HashSet<MainFragmentType>();
        private QuestService _questService;

        /// <summary>fable_10 — true once the Water fragment has been granted this session (back-compat).</summary>
        public bool WaterFragmentGranted => _fragmentsGranted.Contains(MainFragmentType.Water);

        /// <summary>fable_36 — true once the given act's fragment has been granted this session.</summary>
        public bool IsFragmentGranted(MainFragmentType fragment) => _fragmentsGranted.Contains(fragment);

        /// <summary>fable_36 — inject the QuestService used for the per-act +1 skill point grant.</summary>
        public void SetQuestService(QuestService questService) => _questService = questService;

        public void Subscribe()
        {
            if (_subscribed) return;
            GameEventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
            _subscribed = true;
        }

        public void Unsubscribe()
        {
            if (!_subscribed) return;
            GameEventBus.Unsubscribe<QuestCompletedEvent>(OnQuestCompleted);
            _subscribed = false;
        }

        private void OnQuestCompleted(QuestCompletedEvent evt)
        {
            var finale = QuestMainActsIds.FinaleForQuest(evt.QuestId);
            if (finale == null) return;
            CompleteAct(finale);
        }

        /// <summary>
        /// fable_36 — applies the milestone effects for a completed act: fragment integration (acts
        /// 1-3), the +1 skill point (idempotent), then publishes ActCompletedEvent. Safe to call when
        /// the Fonte host is absent (fragment grant is deferred; skill point still applies). Returns
        /// true if anything new was applied. Exposed for tests/diagnostics.
        /// </summary>
        public bool CompleteAct(QuestMainActsIds.ActFinale finale)
        {
            if (finale == null) return false;
            bool anyNew = false;

            if (finale.IntegratesFragment)
                anyNew |= TryGrantFragment(finale.Fragment);

            // +1 skill point per act, idempotent via persisted RewardedMainActIds.
            if (_questService != null && _questService.TryAwardActSkillPoint(finale.ActIdForSkillPoint))
                anyNew = true;

            GameEventBus.Publish(new ActCompletedEvent(finale.ActNumber, finale.ActDoneFlagId, finale.LoreRecordId));
            return anyNew;
        }

        /// <summary>fable_10 back-compat: grant the Water fragment exactly once.</summary>
        public bool TryGrantWaterFragment() => TryGrantFragment(MainFragmentType.Water);

        /// <summary>
        /// Integrates the given fragment into the Fonte exactly once. Idempotent and safe to call
        /// when the Fonte host is not present yet (returns false, leaving the session flag unset so a
        /// later completion/retry can still grant). Returns true only on the first real grant.
        /// </summary>
        public bool TryGrantFragment(MainFragmentType fragment)
        {
            if (_fragmentsGranted.Contains(fragment)) return false;

            var fonte = FonteRuntimeService.Instance;
            if (fonte == null)
            {
                Debug.LogWarning(
                    $"[MainProgressionQuestBridge] act finale completed but FonteRuntimeService is absent; " +
                    $"{fragment} fragment grant deferred (Fonte host not initialized).");
                return false;
            }

            // Already integrated (e.g. restored from a save where the act was finished): mark the
            // session flag and report no new grant — never re-integrate.
            if (fonte.Progression != null && fonte.Progression.IsFragmentIntegrated(fragment))
            {
                _fragmentsGranted.Add(fragment);
                return false;
            }

            bool integrated = fonte.IntegrateFragment(fragment);
            if (integrated)
            {
                _fragmentsGranted.Add(fragment);
                Debug.Log(
                    $"[MainProgressionQuestBridge] act finale complete: {fragment} fragment integrated into the Fonte.");
            }
            return integrated;
        }
    }
}
