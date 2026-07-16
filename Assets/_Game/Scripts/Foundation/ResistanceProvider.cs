namespace CindarsHope.Foundation
{
    /// <summary>
    /// arch: quebra do par mutuo Combat|Player (2026-07-16) — hook neutro para a resistencia por
    /// tipo de dano (F18), no molde de RepairEfficiencyProvider. O Player (PlayerVitalsApplier)
    /// registra a Source (resistencia derivada das passivas/equipamento); o Combat
    /// (PlayerDamageReceiver/PlayerStatusReceiver) so le, sem nomear CindarsHope.Player. Sem Source
    /// registrada, a resistencia e tratada como zero — mesmo fallback do campo estatico anterior.
    /// </summary>
    public static class ResistanceProvider
    {
        /// <summary>tipo de dano -&gt; resistencia atual do player nesse eixo.</summary>
        public static System.Func<DamageType, int> Source;
    }
}
