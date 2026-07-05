using CindarsHope.Items;
using CindarsHope.NPC.Friendship;

namespace CindarsHope.NPC.Gifting
{
    /// <summary>
    /// fable_72 — abstração mínima do inventário para o fluxo de presente (consumo de 1 unidade).
    /// Permite testar o ato sem cena Unity. O adaptador real envolve o InventoryManager.
    /// </summary>
    public interface IGiftInventory
    {
        bool HasItem(string itemId, int amount);
        bool RemoveItem(string itemId, int amount);
    }

    /// <summary>
    /// fable_72 — abstração mínima da F26 para o fluxo de presente. Espelha
    /// FriendshipService.GiveGift (que delega ao FriendshipState e aplica clamp/cap/evento de nível).
    /// O adaptador real envolve o FriendshipService; o teste usa um fake sobre FriendshipState.
    /// </summary>
    public interface IGiftFriendship
    {
        FriendshipState.GiftResult GiveGift(string npcId, NpcGiftPreferences preferences, ItemDefinition item);
    }

    /// <summary>
    /// fable_72 — núcleo PURO (sem Unity) do ato de presentear. Orquestra a ordem da spec:
    ///   1) porteiro Giftable (item sem ItemTag.Giftable ⇒ RefusedNotGiftable, NÃO consome, sem evento);
    ///   2) checagem de inventário (item ausente ⇒ Failed, sem efeito);
    ///   3) F26.GiveGift (classifica por gosto/precedência + cap diário + delta + clamp em 0):
    ///        - recusado pelo cap ⇒ RefusedDailyLimit, NÃO consome, sem evento;
    ///        - aceito ⇒ consome 1 unidade, marca Accepted, expõe taste/delta para o evento.
    ///
    /// NÃO reimplementa amizade, classificação, deltas ou cap — tudo isso vive na F26
    /// (FriendshipService/FriendshipState/GiftTasteClassifier). Aqui é só o FLUXO + o consumo do item.
    /// A publicação do NpcGiftReactionEvent é responsabilidade do caller (MonoBehaviour), que usa
    /// ReactionPublished/Taste/AppliedDelta deste resultado — mantém o núcleo livre de GameEventBus
    /// para teste determinístico.
    /// </summary>
    public static class GiftGivingProcessor
    {
        public static GiftGivingResult Process(
            string npcId,
            string itemId,
            ItemDefinition item,
            NpcGiftPreferences preferences,
            IGiftFriendship friendship,
            IGiftInventory inventory)
        {
            // Pré-condições estruturais: ids válidos e dependências presentes.
            if (string.IsNullOrEmpty(npcId) || string.IsNullOrEmpty(itemId)
                || friendship == null || inventory == null)
            {
                return Refusal(GiftGivingOutcome.Failed, GiftTaste.Neutral);
            }

            // 1) Porteiro Giftable: recusa silenciosa, NÃO consome, sem evento (CA-2).
            if (item == null || !GiftTasteClassifier.IsGiftable(item))
            {
                return Refusal(GiftGivingOutcome.RefusedNotGiftable, GiftTaste.Neutral);
            }

            // 2) Precisa possuir o item para presentear (sem isso, falha sem efeito).
            if (!inventory.HasItem(itemId, 1))
            {
                return Refusal(GiftGivingOutcome.Failed, GiftTaste.Neutral);
            }

            // 3) F26 resolve gosto + cap diário + delta + clamp. Item ainda não foi consumido.
            FriendshipState.GiftResult giftResult = friendship.GiveGift(npcId, preferences, item);

            if (!giftResult.Accepted)
            {
                // Recusa amigável pelo cap diário: NÃO consome, sem evento (CA-3).
                return Refusal(GiftGivingOutcome.RefusedDailyLimit, giftResult.Taste);
            }

            // Aceito: consome exatamente 1 unidade. Se o consumo falhar (corrida), reporta Failed —
            // o delta já foi aplicado pela F26; isso é registrado como risco residual (sem rollback de
            // pontos nesta v1; o cap diário impede repetição no mesmo dia de qualquer forma).
            bool consumed = inventory.RemoveItem(itemId, 1);
            if (!consumed)
            {
                return new GiftGivingResult(
                    GiftGivingOutcome.Failed, giftResult.Taste, giftResult.Apply.PointsDelta, false, false);
            }

            // Sucesso completo: evento deve ser publicado pelo caller.
            return new GiftGivingResult(
                GiftGivingOutcome.Accepted,
                giftResult.Taste,
                giftResult.Apply.PointsDelta,
                true,
                true);
        }

        private static GiftGivingResult Refusal(GiftGivingOutcome outcome, GiftTaste taste)
        {
            return new GiftGivingResult(outcome, taste, 0, false, false);
        }
    }
}
