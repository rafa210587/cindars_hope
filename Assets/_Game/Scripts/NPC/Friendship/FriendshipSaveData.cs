using System.Collections.Generic;

namespace CindarsHope.NPC.Friendship
{
    /// <summary>
    /// fable_26 — DTO de save da amizade (seção aditiva padrão WI-18). Apenas tipos simples
    /// (string/int) + IDs estáveis; sem referências Unity (ADR-0006). Compatível com JsonUtility.
    /// Ausente em save legado ⇒ lista vazia ⇒ todos os NPCs nível 0 (Unknown). Sem migration.
    /// </summary>
    [System.Serializable]
    public class FriendshipSaveData
    {
        public List<FriendshipEntrySaveData> Entries = new List<FriendshipEntrySaveData>();
    }

    /// <summary>
    /// Estado persistido de amizade por NPC: pontos + marcadores de dia dos caps.
    /// lastTalkDay/lastPurchaseDay/lastGiftDay = -1 significa "nunca" (default).
    /// </summary>
    [System.Serializable]
    public class FriendshipEntrySaveData
    {
        public string NpcId;
        public int Points;
        public int LastTalkDay = -1;
        public int LastGiftDay = -1;
        public int LastPurchaseDay = -1;
    }
}
