using System.Collections.Generic;
using CindarsHope.Quests.Runtime;

namespace CindarsHope.Cave.Runtime
{
    /// <summary>
    /// fable_36 — pure-C# narrative helpers for the main-quest acts inside the cave: (1) the
    /// flag-gated appearance of Nymirian (the Warden of Silence, conversable from Act 3) and (2) the
    /// SOFT depth advisory shown once per act when the player descends past the act's design gate.
    ///
    /// Both are pure decision logic (no UnityEngine refs, EditMode-testable). The concrete cave
    /// interactable placement and the toast presentation are thin adapters wired in the scene
    /// (DEFERRED to Play Mode per the spec's human-validation timing) — they only call into this.
    /// No GameObject.Find / FindObjectOfType: the caller injects the "is flag set" predicate and the
    /// current cave depth.
    /// </summary>
    public static class NymirianAppearance
    {
        /// <summary>The Nymirian roster id (stable). Conversable interactable, no schedule.</summary>
        public const string NpcId = QuestMainActsIds.NymirianId;

        /// <summary>The flag that makes Nymirian appear (set on mq_act3_04 turn-in).</summary>
        public const string AvailableFlag = QuestMainActsIds.FlagNymirianAvailable;

        /// <summary>Cave depth where Nymirian is placed (Act 3 "Warden of Silence" — level 60).</summary>
        public const int AppearanceDepth = 60;

        /// <summary>
        /// True when Nymirian should be present: the availability flag is set. The predicate reads
        /// the live QuestFlagService (injected) — fail-closed when the predicate is null.
        /// </summary>
        public static bool IsAvailable(System.Func<string, bool> flagIsSet)
            => flagIsSet != null && flagIsSet(AvailableFlag);

        /// <summary>The Warden's opening line (placeholder voice; localized text deferred to F73).</summary>
        public const string GreetingLine =
            "Sou Nymirian, o ultimo que lembra Vel-Karaum. Desca em silencio: a pedra escuta, e a esperanca ainda dorme aqui embaixo.";
    }

    /// <summary>
    /// fable_36 — the soft depth advisory: when the player descends BELOW the gate their current act
    /// has opened, show a single informative line ("as portas adiante permanecem seladas"). This is
    /// NARRATIVE only — it never blocks movement; the real depth gate stays the boss gates. The
    /// "shown once per act" guard lives in the caller's state (a flag / in-memory set keyed by act).
    /// </summary>
    public static class MainActDepthAdvisory
    {
        public const string AdvisoryLine =
            "As portas adiante permanecem seladas. So a Litania completa abre o que vem depois.";

        // The deepest cave level each act's questline opens (matches the act boss-gate objectives).
        // Act 0 (no act started) opens nothing beyond the Act 1 design depth.
        private static readonly Dictionary<int, int> s_actOpenDepth = new Dictionary<int, int>
        {
            { 0, 10 }, // before/at Act 1
            { 1, 10 },
            { 2, 30 },
            { 3, 75 },
            { 4, 90 }
        };

        /// <summary>The deepest level the given completed-act tier has opened (clamped to real content).</summary>
        public static int OpenDepthForAct(int highestCompletedAct)
        {
            if (highestCompletedAct < 0) highestCompletedAct = 0;
            if (highestCompletedAct > 4) highestCompletedAct = 4;
            return s_actOpenDepth.TryGetValue(highestCompletedAct, out var d) ? d : 10;
        }

        /// <summary>
        /// True when the advisory should be shown: the player's current depth is past what their
        /// highest completed act has opened, and the advisory has not yet been shown for that act.
        /// </summary>
        public static bool ShouldAdvise(int currentDepth, int highestCompletedAct, bool alreadyShownForThisAct)
        {
            if (alreadyShownForThisAct) return false;
            return currentDepth > OpenDepthForAct(highestCompletedAct);
        }
    }
}
