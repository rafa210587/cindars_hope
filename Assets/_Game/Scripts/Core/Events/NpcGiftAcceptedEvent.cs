namespace CindarsHope.Core.Events
{
    /// <summary>
    /// arch: quebra da direcao Quests->NPC (2026-07-14) — versao minima (so o id do NPC, sem
    /// GiftTaste) do fato "presente aceito", publicada ao lado do existente NpcGiftReactionEvent
    /// (CindarsHope.NPC.Events, mantido intacto para UI/dialogo). Existe para que
    /// FestivalQuestService (Quests/FestivalQuests) conte presentes distintos por noite sem
    /// referenciar CindarsHope.NPC.Friendship (GiftTaste), unico motivo pelo qual o evento completo
    /// nao pode viver em Core.Events sem criar um par mutuo Core|NPC novo.
    /// </summary>
    public readonly struct NpcGiftAcceptedEvent
    {
        public readonly string NpcId;

        public NpcGiftAcceptedEvent(string npcId)
        {
            NpcId = npcId;
        }
    }
}
