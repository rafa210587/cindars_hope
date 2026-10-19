namespace CindarsHope.World.Fishing
{
    /// <summary>
    /// fable_50 — nota do minigame de timing da barra de pesca.
    /// Perfect melhora raridade/qualidade 1 passo; Miss não captura; Good = captura padrão.
    /// </summary>
    public enum FishingTimingGrade
    {
        Miss = 0,
        Good = 1,
        Perfect = 2
    }

    /// <summary>
    /// fable_50 — estado da máquina do minigame.
    /// </summary>
    public enum FishingMinigameState
    {
        Idle = 0,
        Casting = 1,
        Window = 2,
        Resolved = 3
    }

    /// <summary>
    /// fable_50 — máquina de estados PURA da barra de timing da pesca (sem MonoBehaviour, sem Unity,
    /// sem Random). Um cursor oscila numa barra normalizada [0..1]; o jogador confirma a captura
    /// (Submit) e a distância ao centro define Perfect/Good; estourar a janela (TickTimeout/Submit
    /// fora da janela) = Miss.
    ///
    /// Determinística: dado o mesmo tempo de submit, devolve sempre a mesma nota. Testável fora de
    /// cena (FishingV2Tests). O <see cref="FishingSpot"/> dirige Start/Tick/Submit a partir da
    /// coroutine existente; com a flag de minigame off, o spot ignora esta classe e usa o caminho v1.
    /// </summary>
    public sealed class FishingTimingMinigame
    {
        // Largura (em fração da barra, a partir do centro 0.5) de cada zona de nota.
        public const float PerfectHalfBand = 0.08f; // centro: |pos-0.5| <= 0.08 ⇒ Perfect
        public const float GoodHalfBand = 0.28f;    // dentro de 0.28 ⇒ Good; fora ⇒ Miss

        private readonly float _windowSeconds;
        private readonly float _oscillationsPerSecond;

        private FishingMinigameState _state = FishingMinigameState.Idle;
        private float _elapsedInWindow;

        public FishingTimingMinigame(float windowSeconds = 1.25f, float oscillationsPerSecond = 1.1f)
        {
            _windowSeconds = windowSeconds <= 0.05f ? 0.05f : windowSeconds;
            _oscillationsPerSecond = oscillationsPerSecond <= 0.05f ? 0.05f : oscillationsPerSecond;
        }

        public FishingMinigameState State => _state;
        public float WindowSeconds => _windowSeconds;

        /// <summary>Tempo decorrido dentro da janela (s) — usado pela view/feedback.</summary>
        public float ElapsedInWindow => _elapsedInWindow;

        /// <summary>Posição atual do cursor na barra normalizada [0..1] (triângulo simétrico).</summary>
        public float CursorPosition => CursorAt(_elapsedInWindow);

        /// <summary>Abre a janela do minigame (transição Casting/Idle → Window).</summary>
        public void Start()
        {
            _state = FishingMinigameState.Window;
            _elapsedInWindow = 0f;
        }

        /// <summary>
        /// Avança o tempo da janela. Retorna true enquanto a janela segue aberta; quando o tempo
        /// estoura, fecha em Miss (timeout) e retorna false. Idempotente após Resolved.
        /// </summary>
        public bool Tick(float deltaSeconds)
        {
            if (_state != FishingMinigameState.Window)
            {
                return _state == FishingMinigameState.Window;
            }

            if (deltaSeconds > 0f)
            {
                _elapsedInWindow += deltaSeconds;
            }

            if (_elapsedInWindow >= _windowSeconds)
            {
                _state = FishingMinigameState.Resolved;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Confirma a captura no tempo atual da janela. Fora da janela aberta = Miss. Dentro,
        /// a distância do cursor ao centro define Perfect/Good/Miss. Determinística.
        /// </summary>
        public FishingTimingGrade Submit()
        {
            if (_state != FishingMinigameState.Window)
            {
                _state = FishingMinigameState.Resolved;
                return FishingTimingGrade.Miss;
            }

            _state = FishingMinigameState.Resolved;
            return GradeForCursor(CursorAt(_elapsedInWindow));
        }

        /// <summary>Nota para uma posição de cursor [0..1] — pura, sem estado (testável direto).</summary>
        public static FishingTimingGrade GradeForCursor(float cursorPosition)
        {
            var distanceFromCenter = cursorPosition - 0.5f;
            if (distanceFromCenter < 0f)
            {
                distanceFromCenter = -distanceFromCenter;
            }

            if (distanceFromCenter <= PerfectHalfBand)
            {
                return FishingTimingGrade.Perfect;
            }

            if (distanceFromCenter <= GoodHalfBand)
            {
                return FishingTimingGrade.Good;
            }

            return FishingTimingGrade.Miss;
        }

        // Cursor triangular simétrico: 0→1→0 ao longo de cada ciclo de oscilação.
        private float CursorAt(float elapsed)
        {
            var phase = (elapsed * _oscillationsPerSecond) % 1f;
            if (phase < 0f)
            {
                phase += 1f;
            }

            return phase < 0.5f ? phase * 2f : (1f - phase) * 2f;
        }
    }
}
