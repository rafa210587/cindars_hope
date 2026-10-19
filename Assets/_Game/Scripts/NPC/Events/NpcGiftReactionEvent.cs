using CindarsHope.NPC.Friendship;

namespace CindarsHope.NPC.Events
{
    /// <summary>
    /// fable_72 — feedback do ato de presentear um NPC. Publicado SOMENTE quando o presente é
    /// aceito (delta aplicado pela F26). Espelha o padrão dos demais eventos de NPC
    /// (readonly struct, tipos simples) em Core/Events/NpcInteractionEvents.cs.
    ///
    /// Reusa o enum de gosto da F26 (CindarsHope.NPC.Friendship.GiftTaste) — proibido criar um 2o
    /// enum de nivel de gosto (regra de nao duplicacao da spec). Consumido por UI/dialogo para a
    /// reacao do NPC; o evento de variacao de NIVEL de amizade continua sendo da F26
    /// (FriendshipLevelChangedEvent), nao reemitido aqui.
    /// </summary>
    public readonly struct NpcGiftReactionEvent
    {
        public readonly string NpcId;
        public readonly string ItemId;
        public readonly GiftTaste Taste;
        public readonly int AppliedDelta;

        public NpcGiftReactionEvent(string npcId, string itemId, GiftTaste taste, int appliedDelta)
        {
            NpcId = npcId;
            ItemId = itemId;
            Taste = taste;
            AppliedDelta = appliedDelta;
        }
    }
}
