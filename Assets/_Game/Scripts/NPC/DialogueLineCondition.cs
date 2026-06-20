using System;
using CindarsHope.World.Calendar;
using CindarsHope.World.Weather;

namespace CindarsHope.NPC
{
    /// <summary>
    /// fable_28 — band of day used to vary lines morning/afternoon/night.
    /// Derived from the in-game hour at a single point (DialogueConditionContext), never read
    /// directly by the selector. Kept tiny and pure so EditMode tests can build it synthetically.
    /// </summary>
    public enum DialogueTimeBand
    {
        Manha = 0,
        Tarde = 1,
        Noite = 2
    }

    /// <summary>
    /// fable_28 — additive, optional set of world conditions gating a single dialogue line inside
    /// a node's <see cref="DialogueNode.ConditionalLines"/> pool. Every field is optional: a null
    /// (or default) field means "no constraint on that axis" so the line is always eligible there.
    ///
    /// Naming note (Phase 0 system-reuse audit): a richer text-key-based
    /// <c>CindarsHope.Dialogue.DialogueCondition</c> already exists, but it operates on the
    /// unwired <c>DialogueSetDefinition</c>/<c>DialogueContext</c> model — NOT on the live
    /// <see cref="TownNpcDialogueLibrary"/>/<see cref="DialogueNode"/> runtime consumed by
    /// <c>NpcController</c>. To gate the live nodes without a name collision in the same namespace
    /// and without bending the unrelated text-key model, this is a small line-level condition that
    /// reads the real world services through <see cref="DialogueConditionContext"/>. F35 (side-quest
    /// chains) reuses THIS condition type — no parallel gating mechanism is created there.
    ///
    /// Pure data + a pure <see cref="IsMet"/> predicate. No Unity reference, no GameObject.Find,
    /// no GameEventBus (a condition check is a synchronous read, not gameplay communication).
    /// </summary>
    [Serializable]
    public sealed class DialogueLineCondition
    {
        /// <summary>Optional season gate. Null = any season.</summary>
        public Season? Season;

        /// <summary>Optional weather gate. Null = any weather.</summary>
        public WeatherType? Weather;

        /// <summary>Optional minimum friendship level (0-5). Null = no friendship requirement.</summary>
        public int? MinFriendship;

        /// <summary>Optional quest/story flag that must be set. Null/empty = no required flag.</summary>
        public string RequiredFlag;

        /// <summary>Optional quest/story flag that must NOT be set. Null/empty = no forbidden flag.</summary>
        public string ForbiddenFlag;

        /// <summary>Optional time-of-day band gate. Null = any time band.</summary>
        public DialogueTimeBand? TimeBand;

        /// <summary>When true, the line is only eligible on a festival day. Default false = any day.</summary>
        public bool RequiresFestivalDay;

        /// <summary>
        /// Number of constrained axes. Used by the selector as the specificity score: the eligible
        /// line that constrains the most axes wins (the most specific line beats a generic one).
        /// </summary>
        public int Specificity
        {
            get
            {
                int n = 0;
                if (Season.HasValue) n++;
                if (Weather.HasValue) n++;
                if (MinFriendship.HasValue) n++;
                if (!string.IsNullOrEmpty(RequiredFlag)) n++;
                if (!string.IsNullOrEmpty(ForbiddenFlag)) n++;
                if (TimeBand.HasValue) n++;
                if (RequiresFestivalDay) n++;
                return n;
            }
        }

        /// <summary>
        /// True when every constrained axis is satisfied by <paramref name="ctx"/>. A null context
        /// is treated as "no world info": only an unconstrained condition (Specificity 0) is met,
        /// so the fallback line is always selectable even without services.
        /// </summary>
        public bool IsMet(DialogueConditionContext ctx)
        {
            if (ctx == null)
            {
                return Specificity == 0;
            }

            if (Season.HasValue && ctx.Season != Season.Value) return false;
            if (Weather.HasValue && ctx.Weather != Weather.Value) return false;
            if (MinFriendship.HasValue && ctx.FriendshipLevel < MinFriendship.Value) return false;
            if (!string.IsNullOrEmpty(RequiredFlag) && !ctx.IsFlagSet(RequiredFlag)) return false;
            if (!string.IsNullOrEmpty(ForbiddenFlag) && ctx.IsFlagSet(ForbiddenFlag)) return false;
            if (TimeBand.HasValue && ctx.TimeBand != TimeBand.Value) return false;
            if (RequiresFestivalDay && !ctx.IsFestivalDay) return false;
            return true;
        }
    }
}
