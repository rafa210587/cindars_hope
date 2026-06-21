namespace CindarsHope.UI.Title
{
    /// <summary>
    /// fable_56: superficie de estado em memoria que o fluxo New Game / Sair-para-o-titulo
    /// limpa. Cada manager/service com estado runtime que um jogo novo NAO pode herdar
    /// (inventario, ouro, tempo, quests, flags) registra uma implementacao no
    /// <see cref="NewGameStateResetService"/>. Mantem o reset em um ponto unico e testavel
    /// sem depender de UnityEngine.
    /// </summary>
    public interface IResettableGameState
    {
        /// <summary>Identificador legivel da superficie (para log/diagnostico/teste).</summary>
        string ResetSurfaceId { get; }

        /// <summary>Limpa o estado runtime para um jogo novo. Deve ser idempotente.</summary>
        void ResetToNewGame();
    }
}
