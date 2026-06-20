namespace CindarsHope.NPC.Gifting
{
    /// <summary>
    /// fable_72 — resultado do ato de dar presente. Distingue os caminhos da spec (CA-2/CA-3/CA-4)
    /// para que o caller decida feedback, consumo e publicação de evento.
    /// </summary>
    public enum GiftGivingOutcome
    {
        /// <summary>Aceito: delta aplicado via F26, item consumido, evento publicado.</summary>
        Accepted = 0,

        /// <summary>Item sem ItemTag.Giftable: recusa silenciosa (0 ganho/perda, NÃO consome).</summary>
        RefusedNotGiftable = 1,

        /// <summary>Limite diário do NPC atingido: recusa amigável (0 ganho/perda, NÃO consome).</summary>
        RefusedDailyLimit = 2,

        /// <summary>Pré-condição inválida (npcId/itemId vazio, item ausente do inventário, sem F26).</summary>
        Failed = 3
    }

    /// <summary>
    /// fable_72 — resultado detalhado do processamento de um presente: outcome + dados para o evento.
    /// Estrutura transiente (não persistida).
    /// </summary>
    public readonly struct GiftGivingResult
    {
        public readonly GiftGivingOutcome Outcome;
        public readonly Friendship.GiftTaste Taste;
        public readonly int AppliedDelta;
        public readonly bool ItemConsumed;
        public readonly bool ReactionPublished;

        public GiftGivingResult(
            GiftGivingOutcome outcome,
            Friendship.GiftTaste taste,
            int appliedDelta,
            bool itemConsumed,
            bool reactionPublished)
        {
            Outcome = outcome;
            Taste = taste;
            AppliedDelta = appliedDelta;
            ItemConsumed = itemConsumed;
            ReactionPublished = reactionPublished;
        }

        public bool Accepted => Outcome == GiftGivingOutcome.Accepted;
    }
}
