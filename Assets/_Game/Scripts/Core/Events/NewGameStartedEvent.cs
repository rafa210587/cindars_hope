namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_63 — publicado UMA vez quando o jogador inicia um New Game pela tela de titulo
    /// (TitleScreenController.StartNewGame, depois do reset de estado da F56).
    ///
    /// A intro narrativa (IntroSequenceController) escuta este evento para disparar a sequencia
    /// de abertura. A F56 continua dona do reset; a intro apenas ESCUTA (sem caminho duplicado).
    /// </summary>
    public readonly struct NewGameStartedEvent
    {
        public string InitialSceneName { get; }

        public NewGameStartedEvent(string initialSceneName)
        {
            InitialSceneName = initialSceneName;
        }
    }
}
