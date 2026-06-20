namespace CindarsHope.Cave.Traps
{
    /// <summary>
    /// fable_60 — estado de uma armadilha materializada na caverna (CA-2/CA-5).
    ///
    /// Máquina de estados: Armed → Telegraphing (aviso visual durante telegraphSeconds) → Triggered
    /// (efeito aplicado, terminal) OU Armed/Detected → Disarmed (terminal, via desarme). Triggered e
    /// Disarmed NUNCA voltam a Armed dentro do mesmo CaveRunSeed (cave-stable-run / ADR-0005); o
    /// estado é persistido no VisitedLevelSnapshot.
    /// </summary>
    public enum TrapState
    {
        /// <summary>Armada e oculta: aguarda gatilho de proximidade/pisada.</summary>
        Armed = 0,

        /// <summary>Telegrafando: aviso visual/SFX em curso; ainda não causou efeito.</summary>
        Telegraphing = 1,

        /// <summary>Disparada: efeito (dano/status/spawn) já aplicado. Estado terminal na run.</summary>
        Triggered = 2,

        /// <summary>Desarmada: neutralizada por interação. Estado terminal na run.</summary>
        Disarmed = 3
    }
}
