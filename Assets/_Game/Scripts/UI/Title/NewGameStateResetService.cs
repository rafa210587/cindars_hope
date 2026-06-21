using System.Collections.Generic;

namespace CindarsHope.UI.Title
{
    /// <summary>
    /// fable_56: ponto UNICO de reset de estado em memoria para New Game e Sair-para-o-titulo.
    ///
    /// Itera as superficies registradas (<see cref="IResettableGameState"/>) e chama
    /// ResetToNewGame em cada uma. Idempotente; nao toca disco. NUNCA deleta slot_1.json:
    /// limpa apenas o estado runtime (CA-3 — jogo novo nao herda inventario/ouro/tempo/
    /// quests/flags; o arquivo de save anterior fica intacto ate o primeiro save explicito).
    ///
    /// Pura (sem UnityEngine) para teste EditMode; o adapter de runtime registra os managers
    /// reais via injecao do GameBootstrap (sem GameObject.Find).
    /// </summary>
    public sealed class NewGameStateResetService
    {
        private readonly List<IResettableGameState> _surfaces = new List<IResettableGameState>();

        public IReadOnlyList<IResettableGameState> Surfaces => _surfaces;

        /// <summary>Registra uma superficie de reset (no-op se nula ou ja registrada).</summary>
        public void Register(IResettableGameState surface)
        {
            if (surface == null || _surfaces.Contains(surface))
            {
                return;
            }

            _surfaces.Add(surface);
        }

        /// <summary>Numero de superficies que ja foram resetadas na ultima chamada de ResetAll.</summary>
        public int LastResetCount { get; private set; }

        /// <summary>Limpa todas as superficies registradas (ponto unico). Idempotente.</summary>
        public void ResetAll()
        {
            var count = 0;
            for (var i = 0; i < _surfaces.Count; i++)
            {
                _surfaces[i]?.ResetToNewGame();
                count++;
            }

            LastResetCount = count;
        }
    }
}
