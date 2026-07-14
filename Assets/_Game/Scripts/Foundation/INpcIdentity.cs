namespace CindarsHope.Foundation
{
    /// <summary>
    /// arch: quebra da direcao Quests->NPC (2026-07-14) — porta pura (string apenas) que permite a
    /// QuestGiverInteractable resolver o id do NPC dono do componente sem nomear CindarsHope.NPC.
    /// </summary>
    public interface INpcIdentity
    {
        string NpcId { get; }
    }
}
