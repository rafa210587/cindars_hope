namespace CindarsHope.Foundation
{
    /// <summary>
    /// arch: quebra do par mutuo Combat|Player (2026-07-16) — hook neutro para "acao bloqueada"
    /// (Stun ativo via PlayerStatusReceiver, F01). O Combat registra a Source no static ctor do
    /// receiver; o Player (PlayerSprintController) so le, sem nomear CindarsHope.Combat. Sem Source
    /// registrada, nenhuma acao e tratada como bloqueada — mesmo fallback do acesso direto anterior.
    /// </summary>
    public static class ActionBlockProvider
    {
        /// <summary>True enquanto o player estiver com uma acao bloqueada (ex.: Stun).</summary>
        public static System.Func<bool> IsActionBlocked;
    }
}
