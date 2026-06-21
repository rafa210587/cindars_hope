using System.Collections.Generic;

namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_43 — Endgame (Act 5) event contracts. Gameplay communication for the level-101
    /// final sequence flows through the GameEventBus only (unity-architecture Rule 2). All three
    /// events are immutable readonly structs carrying simple types / stable string ids — no Unity
    /// references — so they are safe to log, persist-adjacent and consume from pure-C# controllers.
    /// </summary>

    /// <summary>
    /// Published by the Archivist of Silence encounter (the "information fight") each time the boss
    /// changes phase. It asks the HUD to HIDE a set of existing widgets (boss HP bar, minimap, the
    /// pre-existing floating damage numbers — never created here, only suppressed) and RESTORE them
    /// otherwise.
    ///
    /// EMENDA V3: fable_71 ("combat feel pass") is now the CANONICAL owner of this contract; F43 stays
    /// a CONSUMER. The struct lives here (Core/Events) so both F43 and the fable_71 publisher
    /// (<c>CindarsHope.Combat.Feel.HudSuppressionBroadcaster</c>) share ONE definition — there is no
    /// second HudSuppressionChangedEvent anywhere in the codebase. The signature is unchanged
    /// (int Phase, IReadOnlyList&lt;string&gt; HiddenWidgets) so F43 keeps compiling. Phase 0 = empty
    /// <see cref="HiddenWidgets"/> = HUD fully visible = the guaranteed restoration state, which MUST
    /// be published on victory / death / exit (and on the broadcaster's enable/disable).
    /// </summary>
    public readonly struct HudSuppressionChangedEvent
    {
        /// <summary>Encounter phase index. Phase 0 means "restore everything" (nothing hidden).</summary>
        public readonly int Phase;

        /// <summary>Stable widget ids to hide this phase (empty = all visible). Never null.</summary>
        public readonly IReadOnlyList<string> HiddenWidgets;

        public HudSuppressionChangedEvent(int phase, IReadOnlyList<string> hiddenWidgets)
        {
            Phase = phase < 0 ? 0 : phase;
            HiddenWidgets = hiddenWidgets ?? System.Array.Empty<string>();
        }

        /// <summary>Canonical "restore all HUD" event (phase 0, nothing hidden).</summary>
        public static HudSuppressionChangedEvent RestoreAll() =>
            new HudSuppressionChangedEvent(0, System.Array.Empty<string>());
    }

    /// <summary>
    /// Published by <c>FinalChoiceRuntimeAdapter</c> after the final choice is applied exactly once
    /// (Protect / Seal / Use). Carries the resolved ending id (matches
    /// <c>EndingEffectProfile.EndingId</c>) for HUD/lore/quest-log feedback. Idempotent: it fires
    /// only on the first real application, never on the AlreadyApplied path.
    /// </summary>
    public readonly struct FinalChoiceResolvedEvent
    {
        /// <summary>Stable ending id: ending_protect | ending_seal | ending_use.</summary>
        public readonly string EndingId;

        public FinalChoiceResolvedEvent(string endingId)
        {
            EndingId = endingId ?? string.Empty;
        }
    }

    /// <summary>
    /// Published by <c>EndgameSequenceController</c> when the active level-101 encounter changes
    /// (Begin / AdvanceEncounter). Carries the boss id and its 0-based index in the canonical order
    /// (Vel-Karaum 0, Cindrathel 1, Archivist 2, Ithryndor 3). A negative index means the sequence
    /// finished (no active encounter).
    /// </summary>
    public readonly struct EndgameEncounterChangedEvent
    {
        /// <summary>Stable boss id (empty when the sequence is complete).</summary>
        public readonly string BossId;

        /// <summary>0-based encounter index, or -1 when the sequence is complete.</summary>
        public readonly int Index;

        public EndgameEncounterChangedEvent(string bossId, int index)
        {
            BossId = bossId ?? string.Empty;
            Index = index;
        }
    }
}
