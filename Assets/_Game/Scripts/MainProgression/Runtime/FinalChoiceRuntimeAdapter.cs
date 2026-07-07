using CindarsHope.Core;
using CindarsHope.Core.Events;

namespace CindarsHope.MainProgression.Runtime
{
    /// <summary>
    /// fable_43 — thin runtime adapter for the final choice. It assembles the strong-confirmation
    /// <see cref="FinalChoiceRequest"/> (token included), delegates the actual decision to the
    /// EXISTING <see cref="FinalChoiceService"/> (rules never reimplemented), and on the FIRST real
    /// application publishes <see cref="FinalChoiceResolvedEvent"/> for HUD / lore / quest-log
    /// feedback. The AlreadyApplied path publishes nothing (idempotency, CA-4).
    ///
    /// Pure C# (no Unity reference, no GameObject.Find): the Fonte interactable owns one instance and
    /// passes the live MainProgressionSection + FonteAnyaSection. The strong-confirmation UI (modal)
    /// is upstream of this adapter — it only fires once the player has confirmed and supplied the
    /// token.
    /// </summary>
    public sealed class FinalChoiceRuntimeAdapter
    {
        public const string ConfirmationToken = "FINAL_CHOICE_CONFIRMED";

        private readonly FinalChoiceService _service;

        public FinalChoiceRuntimeAdapter(FinalChoiceService service = null)
        {
            _service = service ?? new FinalChoiceService();
        }

        /// <summary>
        /// Builds a preview request (no token requirement bypass — token still supplied) to fetch the
        /// ending id and warning text without mutating any state. Returns the service result.
        /// </summary>
        public FinalChoiceResult Preview(
            MainProgressionSection progression, IFinalChoiceFonteStateSink fonte, FinalChoiceType choice, int day)
        {
            return _service.EvaluateFinalChoice(progression, fonte, BuildRequest(choice, day, previewOnly: true));
        }

        /// <summary>
        /// Applies the final choice exactly once. On the first successful application it publishes
        /// <see cref="FinalChoiceResolvedEvent"/>; a repeat call returns AlreadyApplied and publishes
        /// nothing. Returns the service result so the caller can show the failure reason on refusal.
        /// </summary>
        public FinalChoiceResult Apply(
            MainProgressionSection progression, IFinalChoiceFonteStateSink fonte, FinalChoiceType choice, int day)
        {
            var result = _service.EvaluateFinalChoice(progression, fonte, BuildRequest(choice, day, previewOnly: false));

            if (result.Success && result.ChoiceApplied && !result.AlreadyApplied)
            {
                GameEventBus.Publish(new FinalChoiceResolvedEvent(result.EndingEffectProfileId));
            }
            return result;
        }

        /// <summary>The Ithryndor branch implied by a resolved ending id (catalog mapping).</summary>
        public IthryndorBranch BranchForEnding(string endingId) => EndgameGate.BranchForEnding(endingId);

        private static FinalChoiceRequest BuildRequest(FinalChoiceType choice, int day, bool previewOnly) =>
            new FinalChoiceRequest
            {
                ChoiceType = choice,
                ActorId = "player",
                Day = day,
                RequiredConfirmationToken = ConfirmationToken,
                PreviewOnly = previewOnly
            };
    }
}
