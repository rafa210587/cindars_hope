using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Fonte;
using CindarsHope.MainProgression;
using UnityEngine;

namespace CindarsHope.Quests.Runtime
{
    /// <summary>
    /// fable_10 — bridges the Act 1 main questline to the WAVE 10 Fonte/MainProgression state.
    ///
    /// When the final Act 1 quest (mq_act1_05_fragmento_da_agua) completes, this bridge grants
    /// the Water fragment to the Fonte de Anya via the EXISTING WAVE 10 API
    /// (<see cref="FonteRuntimeService.IntegrateFragment"/>), which integrates the fragment and
    /// re-runs the unlock pass — unlocking Limited Living Water (Agua Viva). No parallel
    /// progression system is created (fonte_rules Rule 2: integration entry point is fable_10).
    ///
    /// Idempotency (CA-2): granting is guarded three ways so a reload + re-turn-in never
    /// duplicates the fragment:
    ///  1. a local <see cref="_waterFragmentGranted"/> flag for the live session;
    ///  2. <see cref="FonteRuntimeService"/> already reports the Water fragment integrated
    ///     (survives save/load — FonteSaveData persists IntegratedFragments);
    ///  3. <see cref="FonteFunctionUnlockService.TryIntegrateFragment"/> itself returns success
    ///     without side effects when the fragment is already integrated.
    ///
    /// Communication is event-only (subscribes to QuestCompletedEvent on the GameEventBus);
    /// no GameObject.Find / FindObjectOfType. The bridge is a pure C# object owned by
    /// QuestRuntimeBootstrap; <see cref="Unsubscribe"/> is called on teardown.
    /// </summary>
    public sealed class MainProgressionQuestBridge
    {
        /// <summary>The quest whose completion grants the Water fragment to the Fonte.</summary>
        public const string FragmentGrantQuestId = QuestMainAct1Ids.Quest05FragmentoDaAgua;

        private bool _subscribed;
        private bool _waterFragmentGranted;

        public bool WaterFragmentGranted => _waterFragmentGranted;

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
            if (evt.QuestId != FragmentGrantQuestId) return;
            TryGrantWaterFragment();
        }

        /// <summary>
        /// Grants the Water fragment to the Fonte exactly once. Idempotent and safe to call
        /// when the Fonte host is not present yet (returns false, leaving the session flag unset
        /// so a later completion/retry can still grant). Returns true only on the first real grant.
        /// </summary>
        public bool TryGrantWaterFragment()
        {
            if (_waterFragmentGranted) return false;

            var fonte = FonteRuntimeService.Instance;
            if (fonte == null)
            {
                Debug.LogWarning(
                    "[MainProgressionQuestBridge] mq_act1_05 completed but FonteRuntimeService is absent; " +
                    "Water fragment grant deferred (Fonte host not initialized).");
                return false;
            }

            // Already integrated (e.g. restored from a save where Act 1 was finished): mark the
            // session flag and report no new grant — never re-integrate.
            if (fonte.Progression != null && fonte.Progression.IsFragmentIntegrated(MainFragmentType.Water))
            {
                _waterFragmentGranted = true;
                return false;
            }

            bool integrated = fonte.IntegrateFragment(MainFragmentType.Water);
            if (integrated)
            {
                _waterFragmentGranted = true;
                Debug.Log(
                    "[MainProgressionQuestBridge] Act 1 complete: Water fragment integrated into the Fonte; " +
                    "Agua Viva unlocked.");
            }
            return integrated;
        }
    }
}
