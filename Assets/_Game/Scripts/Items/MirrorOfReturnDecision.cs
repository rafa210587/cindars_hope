namespace CindarsHope.Items
{
    /// <summary>
    /// fable_31 CA-3 — PURE guard for "Mirror of Return" under the cave stable-run contract (ADR-0005,
    /// cave_rules.md). The mirror teleports the player to the ENTRANCE of the CURRENT cave level. It must
    /// NEVER reroll layout/enemies/resources, which means it must never change <c>CaveRunSeed</c> nor the
    /// current <c>CaveLevel</c>. This struct verifies that invariant from a before/after reading so a test
    /// can prove the run seed is untouched, and the runtime path can assert before applying the teleport.
    /// </summary>
    public readonly struct MirrorOfReturnDecision
    {
        public bool StableRunPreserved { get; }
        public bool CanTeleport { get; }
        public string Reason { get; }

        private MirrorOfReturnDecision(bool stableRunPreserved, bool canTeleport, string reason)
        {
            StableRunPreserved = stableRunPreserved;
            CanTeleport = canTeleport;
            Reason = reason;
        }

        /// <summary>
        /// Evaluates a teleport from <paramref name="runSeedBefore"/>/<paramref name="levelBefore"/> to the
        /// readings observed after locating the entrance anchor. The teleport is only valid when BOTH the run
        /// seed and the cave level are unchanged (entrance of the SAME level, no regeneration).
        /// </summary>
        public static MirrorOfReturnDecision Evaluate(
            string runSeedBefore, int levelBefore, string runSeedAfter, int levelAfter)
        {
            if (string.IsNullOrWhiteSpace(runSeedBefore) || string.IsNullOrWhiteSpace(runSeedAfter))
            {
                return new MirrorOfReturnDecision(false, false, "no_active_cave_run");
            }

            var seedStable = string.Equals(runSeedBefore, runSeedAfter, System.StringComparison.Ordinal);
            var levelStable = levelBefore == levelAfter;
            var preserved = seedStable && levelStable;

            if (!preserved)
            {
                var reason = !seedStable ? "run_seed_changed" : "cave_level_changed";
                return new MirrorOfReturnDecision(false, false, reason);
            }

            return new MirrorOfReturnDecision(true, true, string.Empty);
        }
    }
}
