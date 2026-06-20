namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_57 — disparado quando o jogador presenteia um NPC NO DIA do aniversário dele
    /// (o presente vale x2 pontos de amizade). Um toast escuta para sinalizar "Hoje é
    /// aniversário de X!". Comunicação só via GameEventBus; nenhum estado salvo (o aniversário
    /// é re-derivável da tabela + calendário, e o cap diário de presente já persiste via F26).
    /// </summary>
    public readonly struct NpcBirthdayGiftEvent
    {
        /// <summary>Id canônico do NPC aniversariante (ex.: "npc_pip").</summary>
        public readonly string NpcId;

        /// <summary>Nome de exibição do NPC (para o toast); pode ser igual ao id em fallback.</summary>
        public readonly string DisplayName;

        public NpcBirthdayGiftEvent(string npcId, string displayName)
        {
            NpcId = npcId ?? string.Empty;
            DisplayName = string.IsNullOrEmpty(displayName) ? (npcId ?? string.Empty) : displayName;
        }
    }
}
