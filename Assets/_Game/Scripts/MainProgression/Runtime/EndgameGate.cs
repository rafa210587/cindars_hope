using CindarsHope.MainProgression;

namespace CindarsHope.MainProgression.Runtime
{
    /// <summary>
    /// fable_43 — pure-C# gate logic for the 100 -> 101 transition and the Ithryndor branch. It
    /// CONSUMES the existing <see cref="FinalChoiceService.CanUnlockLevel101"/> rule (Life fragment
    /// integrated + Level100GateState == Entered) and never reimplements it. No Unity reference.
    /// </summary>
    public enum IthryndorBranch
    {
        None = 0,
        AllyNoFight = 1,        // Protect: awakes as ally, no fight
        CeremonialPartial = 2,  // Seal: partial ceremonial fight (1 phase, player not expected to die)
        FullFourPhase = 3       // Use: full 4-phase boss fight (F05 AI)
    }

    public static class EndgameGate
    {
        public const string Level101BlockedMessage =
            "O nivel 101 permanece selado: derrote o Carcereiro Antigo e integre o Fragmento da Vida na Fonte.";

        /// <summary>
        /// True if the player may ENTER level 101. Delegates the unlock rule to the existing service,
        /// then requires the access state to actually be Unlocked (the gate is applied separately when
        /// the Elder falls). A null progression is never allowed.
        /// </summary>
        public static bool CanEnterLevel101(FinalChoiceService service, MainProgressionSection progression)
        {
            if (service == null || progression == null) return false;
            return progression.Level101AccessState == Level101AccessStatus.Unlocked;
        }

        /// <summary>
        /// Applies the gate transition after the Draconic Elder is defeated: marks the level-100 gate
        /// Entered and, if the existing rule allows (Life fragment integrated), unlocks level-101
        /// access. Idempotent — re-running it never regresses state. Returns true if access is unlocked
        /// after the call.
        /// </summary>
        public static bool TryUnlockLevel101OnElderDefeated(
            FinalChoiceService service, MainProgressionSection progression)
        {
            if (service == null || progression == null) return false;

            if (progression.Level100GateState != Level100GateStatus.Entered)
                progression.Level100GateState = Level100GateStatus.Entered;

            if (progression.Level101AccessState == Level101AccessStatus.Unlocked
                || progression.Level101AccessState == Level101AccessStatus.Resolved)
                return true;

            if (service.CanUnlockLevel101(progression))
            {
                progression.Level101AccessState = Level101AccessStatus.Unlocked;
                return true;
            }

            return false;
        }

        /// <summary>How Ithryndor responds to the resolved final choice (catalog mapping).</summary>
        public static IthryndorBranch BranchFor(FinalChoiceType choice)
        {
            switch (choice)
            {
                case FinalChoiceType.Protect: return IthryndorBranch.AllyNoFight;
                case FinalChoiceType.Seal: return IthryndorBranch.CeremonialPartial;
                case FinalChoiceType.Use: return IthryndorBranch.FullFourPhase;
                default: return IthryndorBranch.None;
            }
        }

        /// <summary>How Ithryndor responds, derived from a resolved ending id.</summary>
        public static IthryndorBranch BranchForEnding(string endingId)
        {
            switch (endingId)
            {
                case "ending_protect": return IthryndorBranch.AllyNoFight;
                case "ending_seal": return IthryndorBranch.CeremonialPartial;
                case "ending_use": return IthryndorBranch.FullFourPhase;
                default: return IthryndorBranch.None;
            }
        }
    }
}
