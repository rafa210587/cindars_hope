using CindarsHope.Quests;

namespace CindarsHope.Farm.Visitors
{
    /// <summary>
    /// fable_52 — the occasional Goblin visitor on the FARM (decision Q1.1, FABLE_DECISOES §1):
    /// malvistas races never RESIDE in the city, but the world talks to the farm. A rare deterministic
    /// daily event; the goblin appears for one day with its own dialogue that references the
    /// scq_goblin_truce (mentioning the player's name once the truce is earned).
    ///
    /// This is the PURE decision + dialogue-variant layer (rng-and-determinism / ADR-0005 generalized):
    /// same worldSeed + same day ⇒ same visit decision, on every machine — no Unity Random / timestamp.
    /// The actual one-day NPC spawn in FarmScene is thin scene wiring (deferred to final validation);
    /// it consults <see cref="ShouldVisit"/> and <see cref="ResolveDialogueVariant"/>. The visitor is
    /// restricted to the farm and NEVER appears in the city (anti-regression).
    /// </summary>
    public static class GoblinFarmVisitor
    {
        /// <summary>Catalog rarity of the visit (~5% of days).</summary>
        public const int VisitChancePercent = 5;

        public const string DecisionSalt = "fable_52_goblin_farm_visitor_v1";

        /// <summary>The named pool entry, when the goblin visit is routed through the F37 random-event pool.</summary>
        public const string EventPoolId = "goblin_visitor";

        /// <summary>Dialogue pool id the spawned NPC reads (variant chosen by <see cref="ResolveDialogueVariant"/>).</summary>
        public const string DialoguePoolId = "dlg_goblin_farm_visitor";

        public enum DialogueVariant
        {
            /// <summary>Default greeting — the player has not earned the goblin truce.</summary>
            Default = 0,

            /// <summary>Post-truce — the band is grateful; the goblin mentions the player's name.</summary>
            PostTruce = 1,

            /// <summary>Advanced act — extra lore line gated by a main-act flag.</summary>
            AdvancedAct = 2
        }

        /// <summary>
        /// Deterministic visit decision for a given world seed + absolute day. Stable across reloads.
        /// </summary>
        public static bool ShouldVisit(string worldSeed, int absoluteDay)
        {
            if (absoluteDay < 1) return false;
            int hash = QuestStableHash.Compute($"{DecisionSalt}|{worldSeed}|{absoluteDay}");
            int bucket = (int)(((uint)hash) % 100u);
            return bucket < VisitChancePercent;
        }

        /// <summary>
        /// Picks the dialogue variant from the two world facts the visitor cares about: whether the
        /// truce was earned (warchief_crest_owned) and whether the player is in an advanced act
        /// (advancedActReached). Post-truce wins over advanced-act for the "mentions your name" beat.
        /// </summary>
        public static DialogueVariant ResolveDialogueVariant(bool truceEarned, bool advancedActReached)
        {
            if (truceEarned) return DialogueVariant.PostTruce;
            if (advancedActReached) return DialogueVariant.AdvancedAct;
            return DialogueVariant.Default;
        }
    }
}
