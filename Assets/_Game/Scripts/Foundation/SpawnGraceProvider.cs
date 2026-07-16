namespace CindarsHope.Foundation
{
    /// <summary>
    /// arch: quebra do par mutuo Combat|Player (2026-07-16) — hook reverso para a graca de spawn
    /// (invulnerabilidade curta ao (re)entrar numa cena/reviver). O Combat (PlayerDamageReceiver, dono
    /// da janela de graca) registra a Action; o Player (PlayerDeathController) so invoca, sem nomear
    /// CindarsHope.Combat. Sem Action registrada, o pedido de graca vira no-op silencioso.
    /// </summary>
    public static class SpawnGraceProvider
    {
        /// <summary>segundos de graca -&gt; concede invulnerabilidade a partir de agora.</summary>
        public static System.Action<float> GrantSpawnGrace;
    }
}
