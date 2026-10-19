namespace CindarsHope.NPC.Friendship
{
    /// <summary>
    /// fable_26 — níveis canônicos de amizade por NPC (0-5), derivados dos pontos.
    /// Thresholds fixos (10/30/60/100/150). Sem decaimento (decisão de simplicidade).
    ///
    /// Os nomes seguem o roster/SOCIAL (Desconhecido → Confidente). O valor inteiro do enum
    /// é o número de nível exposto no diálogo ("Amizade: nível N") e consumido por
    /// IsAtLeast(npcId, level) em F25/F28/F35.
    /// </summary>
    public enum FriendshipLevel
    {
        Unknown = 0,    // Desconhecido (0 pts)
        Known = 1,      // Conhecido    (10 pts)
        Cordial = 2,    // Cordial      (30 pts)
        Friend = 3,     // Amigo        (60 pts)
        Close = 4,      // Próximo      (100 pts)
        Confidant = 5   // Confidente   (150 pts)
    }
}
