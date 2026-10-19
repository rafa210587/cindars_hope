namespace CindarsHope.Foundation
{
    /// <summary>
    /// Snapshot primitivo das resistências atuais e da redução de duração efetiva.
    /// A redução usa a fórmula canônica: min(50%, resistência × 2%).
    /// </summary>
    public readonly struct PlayerResistancesChangedEvent
    {
        public int ToxicResistance { get; }
        public int ColdResistance { get; }
        public int HeatResistance { get; }
        public float ToxicDurationReduction { get; }
        public float ColdDurationReduction { get; }
        public float HeatDurationReduction { get; }

        public PlayerResistancesChangedEvent(
            int toxicResistance,
            int coldResistance,
            int heatResistance,
            float toxicDurationReduction,
            float coldDurationReduction,
            float heatDurationReduction)
        {
            ToxicResistance = toxicResistance;
            ColdResistance = coldResistance;
            HeatResistance = heatResistance;
            ToxicDurationReduction = toxicDurationReduction;
            ColdDurationReduction = coldDurationReduction;
            HeatDurationReduction = heatDurationReduction;
        }
    }

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
