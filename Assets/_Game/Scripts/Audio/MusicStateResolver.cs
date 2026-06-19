namespace CindarsHope.Audio
{
    /// <summary>
    /// fable_58 EMENDA V3 — máquina de estado PURA da música.
    ///
    /// Mantém um conjunto de condições ativas (combate engajado, boss ativo,
    /// festival ativo) e resolve o estado dominante por prioridade determinística:
    ///   Boss &gt; Combate &gt; Festival &gt; Calmo.
    ///
    /// O retorno ao estado de menor prioridade ocorre automaticamente quando a
    /// condição dominante cessa (ex.: boss morto → se ainda há combate, Combate;
    /// senão Festival se ativo; senão Calmo).
    ///
    /// Classe pura (sem AudioSource / sem MonoBehaviour) — 100% testável em EditMode.
    /// A contagem de engajamento de combate é tolerante a underflow (clamp em 0)
    /// para nunca quebrar gameplay por eventos desbalanceados.
    /// </summary>
    public sealed class MusicStateResolver
    {
        private int _engagedEnemies;
        private bool _bossActive;
        private bool _festivalActive;

        /// <summary>Quantidade atual de inimigos engajados (nunca negativa).</summary>
        public int EngagedEnemies => _engagedEnemies;

        public bool BossActive => _bossActive;
        public bool FestivalActive => _festivalActive;

        /// <summary>Estado dominante atual segundo a prioridade canônica.</summary>
        public MusicState CurrentState => Resolve(_engagedEnemies, _bossActive, _festivalActive);

        /// <summary>
        /// Resolução pura de prioridade. Boss &gt; Combate &gt; Festival &gt; Calmo.
        /// </summary>
        public static MusicState Resolve(int engagedEnemies, bool bossActive, bool festivalActive)
        {
            if (bossActive)
            {
                return MusicState.Boss;
            }

            if (engagedEnemies > 0)
            {
                return MusicState.Combate;
            }

            if (festivalActive)
            {
                return MusicState.Festival;
            }

            return MusicState.Calmo;
        }

        /// <summary>Registra entrada de um inimigo em combate. Retorna o novo estado dominante.</summary>
        public MusicState EnemyEngaged()
        {
            _engagedEnemies++;
            return CurrentState;
        }

        /// <summary>
        /// Registra saída de um inimigo do combate (morte/perda de aggro).
        /// Clampa em 0 — eventos a mais nunca tornam a contagem negativa.
        /// </summary>
        public MusicState EnemyDisengaged()
        {
            if (_engagedEnemies > 0)
            {
                _engagedEnemies--;
            }

            return CurrentState;
        }

        /// <summary>Define explicitamente a contagem de engajados (clamp em 0).</summary>
        public MusicState SetEngagedEnemies(int count)
        {
            _engagedEnemies = count < 0 ? 0 : count;
            return CurrentState;
        }

        /// <summary>Liga/desliga o estado de boss (vence combate enquanto ativo).</summary>
        public MusicState SetBossActive(bool active)
        {
            _bossActive = active;
            return CurrentState;
        }

        /// <summary>Liga/desliga o estado de festival.</summary>
        public MusicState SetFestivalActive(bool active)
        {
            _festivalActive = active;
            return CurrentState;
        }

        /// <summary>Zera todas as condições (volta a Calmo). Uso: reset de sessão/teste.</summary>
        public MusicState Reset()
        {
            _engagedEnemies = 0;
            _bossActive = false;
            _festivalActive = false;
            return CurrentState;
        }
    }
}
