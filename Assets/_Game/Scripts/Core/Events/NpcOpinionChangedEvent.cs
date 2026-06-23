namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_26 — publicado quando a opiniao (-100..+100) de um NPC sobre o jogador muda
    /// (via FriendshipService.AdjustOpinion/SetOpinion). Consumido pelo painel de retrato
    /// (NpcInteractionPortraitHud) para atualizar feicao e barra de afinidade ao vivo.
    ///
    /// Imutavel; apenas tipos simples + id estavel (sem refs Unity).
    /// </summary>
    public readonly struct NpcOpinionChangedEvent
    {
        /// <summary>Id estavel do NPC cuja opiniao mudou.</summary>
        public readonly string NpcId;

        /// <summary>Opiniao atual (-100..+100; 0 = neutro).</summary>
        public readonly int Opinion;

        /// <summary>Opiniao anterior, antes desta mudanca.</summary>
        public readonly int PreviousOpinion;

        public NpcOpinionChangedEvent(string npcId, int current, int previous)
        {
            NpcId = npcId;
            Opinion = current;
            PreviousOpinion = previous;
        }
    }
}
