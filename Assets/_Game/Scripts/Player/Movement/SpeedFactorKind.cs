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
        Legacy = 6,

        /// <summary>
        /// fable_69 — mobilidade em combate. Fora de combate: ausente (neutro 1.0). Em combate
        /// sem sprint: penalidade canônica (~0.9 → 3.4-3.8 tiles/s). Em combate com sprint
        /// segurado e stamina: 1.0 (restaura a faixa de fora-de-combate, 3.8-4.2). Único dono
        /// deste fator é o PlayerSprintController; ninguém escreve direto no SpeedMultiplier.
        /// </summary>
        CombatMobility = 7,

        /// <summary>Sinal de Retirada; aplicado somente enquanto o movimento aponta para longe da ameaça.</summary>
        RetreatSignal = 8,

        /// <summary>Penalidade de terreno; separada de Chill/Slow/Root/Stun.</summary>
        Terrain = 9,

        /// <summary>Bônus direcional temporário após disparo ranged.</summary>
        Kiting = 10
    }
}
