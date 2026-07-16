namespace CindarsHope.Foundation
{
    /// <summary>
    /// arch: quebra do par mutuo Combat|Player (2026-07-16) — hook neutro para o estado "em combate"
    /// (fable_69 CombatStateTracker). O Combat registra a Source no static ctor do tracker; o Player
    /// (PlayerSprintController) so le, sem nomear CindarsHope.Combat. Sem Source registrada, o player
    /// e tratado como fora de combate — mesmo fallback do acesso direto anterior.
    /// </summary>
    public static class CombatStateProvider
    {
        /// <summary>True enquanto o player estiver dentro da janela de combate (F69).</summary>
        public static System.Func<bool> IsInCombat;
    }
}
