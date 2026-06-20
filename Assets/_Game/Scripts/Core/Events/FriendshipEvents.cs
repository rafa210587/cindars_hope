namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_26 — evento aditivo de mudança de nível de amizade. Publicado pelo FriendshipService
    /// na transição de nível (SUBIDA e DESCIDA — emenda 2026-06-13-V3 §2/§7, pois presente hated
    /// pode rebaixar). Consumido por HUD/toast ("X agora é seu amigo") e por F28/F35.
    ///
    /// Regra: sem refs Unity/MonoBehaviour/SO em event structs (event-bus-only rule). Só ids/ints.
    /// </summary>
    public readonly struct FriendshipLevelChangedEvent
    {
        public readonly string NpcId;
        public readonly int Level;
        public readonly int PreviousLevel;

        public FriendshipLevelChangedEvent(string npcId, int level, int previousLevel)
        {
            NpcId = npcId;
            Level = level;
            PreviousLevel = previousLevel;
        }

        /// <summary>True quando o nível subiu (para mensagens positivas de toast).</summary>
        public bool IsIncrease => Level > PreviousLevel;
    }
}
