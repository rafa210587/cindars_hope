namespace CindarsHope.NPC.Friendship
{
    /// <summary>
    /// fable_26 — as EXATAMENTE 4 fontes de ganho de amizade (não inflar). Cada fonte tem o
    /// seu próprio cap diário aplicado dentro do FriendshipService:
    ///   DailyConversation — primeira conversa do dia (+1, 1×/dia/NPC);
    ///   PersonalQuest     — side quest do NPC concluída (+8, idempotente por questId);
    ///   Gift              — presente (delta variável pelo gosto do NPC; cap DailyGiftLimit/dia/NPC);
    ///   ShopPurchase      — compra na loja do NPC (+1, 1×/dia/NPC).
    /// </summary>
    public enum FriendshipSource
    {
        DailyConversation = 0,
        PersonalQuest = 1,
        Gift = 2,
        ShopPurchase = 3
    }

    /// <summary>
    /// fable_26 (emenda 2026-06-13-V3 §2) — cinco níveis de reação a presente, do mais
    /// negativo ao mais positivo. A precedência de classificação é hated > disliked >
    /// loved(id) > liked(tag) > neutral(default).
    /// </summary>
    public enum GiftTaste
    {
        Hated = 0,      // -6  (REDUZ amizade)
        Disliked = 1,   // -2
        Neutral = 2,    // +2  (default: qualquer Giftable não classificado)
        Liked = 3,      // +6
        Loved = 4       // +12 (item raro/pessoal por LovedItemId)
    }
}
