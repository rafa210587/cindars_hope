using CindarsHope.Fonte;

namespace CindarsHope.MainProgression
{
    // Pure C# service — no Unity/MonoBehaviour, no live scene state
    public class FinalChoiceService
    {
        private const string ConfirmationToken = "FINAL_CHOICE_CONFIRMED";

        // Evaluate final choice — does not apply unless PreviewOnly=false
        public FinalChoiceResult EvaluateFinalChoice(
            MainProgressionSection progression,
            FonteAnyaSection fonte,
            FinalChoiceRequest request)
        {
            if (progression == null) return FinalChoiceResult.Fail("PROGRESSION_NULL");
            if (fonte == null) return FinalChoiceResult.Fail("FONTE_NULL");
            if (request == null) return FinalChoiceResult.Fail("REQUEST_NULL");

            // Idempotency guard
            if (progression.FinalChoiceState == FinalChoiceStatus.Resolved)
                return new FinalChoiceResult { Success = true, AlreadyApplied = true };

            // Availability check
            if (progression.FinalChoiceState == FinalChoiceStatus.Unavailable)
                return FinalChoiceResult.Fail("FINAL_CHOICE_NOT_AVAILABLE");

            // Require Hope fragment
            if (!progression.IsFragmentIntegrated(MainFragmentType.Hope))
                return FinalChoiceResult.Fail("FINAL_CHOICE_REQUIRES_HOPE_FRAGMENT");

            // Require Level101 access
            if (progression.Level101AccessState != Level101AccessStatus.Unlocked)
                return FinalChoiceResult.Fail("FINAL_CHOICE_REQUIRES_LEVEL101_ACCESS");

            // Require strong confirmation token
            if (request.RequiredConfirmationToken != ConfirmationToken)
                return FinalChoiceResult.Fail("FINAL_CHOICE_REQUIRES_STRONG_CONFIRMATION");

            // Anya cannot be restored by any choice
            if (request.ChoiceType == FinalChoiceType.None)
                return FinalChoiceResult.Fail("FINAL_CHOICE_CANNOT_BE_NONE");

            if (request.PreviewOnly)
            {
                var profile = GetProfile(request.ChoiceType);
                return new FinalChoiceResult
                {
                    Success = true, ChoiceApplied = false,
                    EndingEffectProfileId = profile.EndingId,
                    DebugNotes = { "Preview only — no state changed" }
                };
            }

            // Apply choice
            var endingProfile = GetProfile(request.ChoiceType);
            progression.FinalChoiceState = FinalChoiceStatus.Resolved;
            progression.PostGameWorldState = endingProfile.EndingId;
            progression.CurrentAct = MainAct.PostGame;

            // Update Fonte final state via adapter
            var fonteEnum = endingProfile.FonteFinalState switch
            {
                "FinalizedProtected" => FonteState.FinalizedProtected,
                "FinalizedSealed"    => FonteState.FinalizedSealed,
                "FinalizedUsed"      => FonteState.FinalizedUsed,
                _ => FonteState.FinalizedProtected
            };
            fonte.FonteState = fonteEnum;
            fonte.FinalFonteState = endingProfile.FonteFinalState;

            return new FinalChoiceResult
            {
                Success = true, ChoiceApplied = true,
                EndingEffectProfileId = endingProfile.EndingId,
                MainProgressionUpdated = true, FonteStateUpdated = true,
                PostGameModifiersApplied = true
            };
        }

        // Evaluates if Level101 access should be unlocked based on current state
        public bool CanUnlockLevel101(MainProgressionSection progression)
        {
            return progression != null
                && progression.IsFragmentIntegrated(MainFragmentType.Life)
                && progression.Level100GateState == Level100GateStatus.Entered;
        }

        private EndingEffectProfile GetProfile(FinalChoiceType choice) => choice switch
        {
            FinalChoiceType.Protect => EndingEffectProfile.Protect(),
            FinalChoiceType.Seal    => EndingEffectProfile.Seal(),
            FinalChoiceType.Use     => EndingEffectProfile.Use(),
            _ => EndingEffectProfile.Protect()
        };
    }
}
