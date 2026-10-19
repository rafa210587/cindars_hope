namespace CindarsHope.Foundation
{
    /// <summary>
    /// Emitted after an active skill attempt resolves. Unlike the pre-execution offensive commit
    /// signal, Committed means its executor succeeded. Empty actionId/rank 0 mean unresolved input.
    /// </summary>
    public readonly struct SkillActionResolvedEvent
    {
        public const string Committed = "Committed";
        public const string Refused = "Refused";

        public string ActionId { get; }
        public int Rank { get; }
        public string VariantId { get; }
        public string Outcome { get; }
        public string FailureKey { get; }

        public SkillActionResolvedEvent(string actionId, int rank, string variantId,
            string outcome, string failureKey = "")
        {
            ActionId = actionId ?? string.Empty;
            Rank = rank;
            VariantId = variantId ?? string.Empty;
            Outcome = outcome ?? string.Empty;
            FailureKey = failureKey ?? string.Empty;
        }
    }
}
