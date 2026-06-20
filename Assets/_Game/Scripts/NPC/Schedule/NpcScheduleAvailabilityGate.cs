using CindarsHope.Core.Events;
using CindarsHope.Core;

namespace CindarsHope.NPC.Schedule
{
    /// <summary>
    /// fable_11 (CA-4) — single point that decides whether a town NPC is currently interactable per
    /// its schedule, and produces the honest "not available" prompt/feedback.
    ///
    /// Fail-open by design: if the <see cref="NpcScheduleService"/> is absent or has no profile for
    /// the NPC, the NPC is treated as available — this preserves the pre-fable_11 behavior (dialogue/
    /// shop intact) until anchors and profiles are wired by the generator (anti-regression rule).
    /// </summary>
    public static class NpcScheduleAvailabilityGate
    {
        /// <summary>True when the NPC is currently unavailable for shop/dialogue due to its schedule.</summary>
        public static bool IsUnavailable(NpcDataSO npcData)
        {
            if (npcData == null || string.IsNullOrEmpty(npcData.NpcId))
            {
                return false;
            }

            var service = NpcScheduleService.Instance;
            if (service == null)
            {
                return false; // fail-open: no schedule runtime → behave as before
            }

            // Only gate NPCs the service actually knows (has a runtime state / profile for).
            if (!service.TryGetRuntimeState(npcData.NpcId, out var state) || state == null)
            {
                return false;
            }

            return !state.IsAvailable;
        }

        /// <summary>The interaction prompt shown when the NPC is unavailable.</summary>
        public static string UnavailablePrompt(NpcDataSO npcData)
        {
            var name = npcData != null && !string.IsNullOrEmpty(npcData.DisplayName)
                ? npcData.DisplayName
                : "NPC";
            return $"{name} nao esta disponivel agora.";
        }

        /// <summary>Publish the unavailability reason as player feedback (toast).</summary>
        public static void PublishUnavailableFeedback(NpcDataSO npcData)
        {
            GameEventBus.Publish(new PlayerActionFeedbackEvent(UnavailablePrompt(npcData)));
        }
    }
}
