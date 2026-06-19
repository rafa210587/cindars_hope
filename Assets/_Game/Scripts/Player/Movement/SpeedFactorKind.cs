namespace CindarsHope.Player.Movement
{
    /// <summary>
    /// fable_47 — fatores nomeados que compõem a velocidade do player.
    /// Cada sistema escreve APENAS o seu fator (SetFactor/ClearFactor); o produto é a
    /// velocidade efetiva. Substitui o padrão frágil "guardar valor anterior e restaurar"
    /// que disputava o mutável PlayerController.SpeedMultiplier.
    ///
    /// IMPORTANTE: a fome (GetHungerMoveSpeedModifier) NÃO é um fator aqui — permanece no
    /// caminho próprio do PlayerController (preservado pela F18/spec).
    /// </summary>
    public enum SpeedFactorKind
    {
        /// <summary>Empurrão/knockback resolvido por PlayerMovementDisplacementResolver (0 enquanto desloca).</summary>
        Displacement = 0,

        /// <summary>Dash/Dodge (0 durante a animação de deslocamento).</summary>
        Dash = 1,

        /// <summary>Block segurado (×0.45 da velocidade base enquanto bloqueia).</summary>
        Block = 2,

        /// <summary>Fadiga/Exhausted (×0.85 quando muito cansado — F16).</summary>
        Exhausted = 3,

        /// <summary>Status de movimento (Chill/Slow/ColdStress/Root/Stun — menor fator ativo). Sujeito ao floor de status (F01).</summary>
        Status = 4,

        /// <summary>MoveSpeed derivado permanente da F18 (equipamento/passivas).</summary>
        DerivedMoveSpeed = 5,

        /// <summary>Compat: escrita via setter público legado de SpeedMultiplier (warning DEV).</summary>
        Legacy = 6
    }
}
