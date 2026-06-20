namespace CindarsHope.Quests
{
    /// <summary>
    /// fable_34 — the single point of "cave secret" quest offering (CA-3).
    ///
    /// Consumed by the wandering merchant (deterministic 15% chance, by seed) and by peaceful-monster
    /// interactables (F33 peaceful flag). Both go through <see cref="ShouldOfferByChance"/> +
    /// the runtime entry point so the rule lives in ONE place and is EditMode-testable.
    ///
    /// EMENDA 2026-06-12-B: the concrete scq_* content is authored by fable_52; this spec only
    /// provides the API and the deterministic gate.
    /// </summary>
    public static class SecretQuestOffer
    {
        /// <summary>Catalog chance that a wandering merchant offers a secret instead of trading.</summary>
        public const int MerchantOfferChancePercent = 15;

        public const string ChanceSalt = "fable_34_secret_offer_v1";

        /// <summary>
        /// Deterministic offer decision: same (seed, contextId) => same answer, on every machine.
        /// No Unity Random / timestamp / GUID (rng-and-determinism / ADR-0005 generalized).
        /// </summary>
        public static bool ShouldOfferByChance(string seed, string contextId, int chancePercent)
        {
            if (chancePercent <= 0) return false;
            if (chancePercent >= 100) return true;
            int hash = QuestStableHash.Compute($"{ChanceSalt}|{seed}|{contextId}");
            int bucket = (int)(((uint)hash) % 100u);
            return bucket < chancePercent;
        }

        /// <summary>Wandering-merchant convenience overload using the canonical 15% chance.</summary>
        public static bool ShouldMerchantOffer(string seed, string contextId)
            => ShouldOfferByChance(seed, contextId, MerchantOfferChancePercent);
    }
}
