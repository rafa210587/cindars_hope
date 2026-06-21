namespace CindarsHope.Audio
{
    /// <summary>
    /// fable_58 EMENDA V3 (decisão 4.9) — estado da trilha de música.
    /// save-safe (ids estáveis) embora NÃO seja persistido no v1.
    ///
    /// Prioridade determinística (resolvida em <see cref="MusicStateResolver"/>):
    /// Boss > Combate > Festival > Calmo.
    /// </summary>
    public enum MusicState
    {
        /// <summary>Default — mundo/fazenda/cidade fora de combate.</summary>
        Calmo = 0,

        /// <summary>Ao menos um inimigo hostil engajado com o jogador.</summary>
        Combate = 1,

        /// <summary>Luta de boss/miniboss ativa (vence Combate enquanto durar).</summary>
        Boss = 2,

        /// <summary>Evento de festival ativo (vence o ambiente; Boss/Combate sobrepõem).</summary>
        Festival = 3,

        /// <summary>Ambiente da FAZENDA (pastoral). Estado base quando na FarmScene fora de combate.</summary>
        Fazenda = 4,

        /// <summary>Ambiente da CIDADE (animado). Estado base quando na TownScene fora de combate.</summary>
        Cidade = 5,

        /// <summary>Ambiente da CAVERNA (misterioso). Estado base quando na CaveScene fora de combate.</summary>
        Caverna = 6
    }
}
