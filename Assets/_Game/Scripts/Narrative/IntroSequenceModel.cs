using System.Collections.Generic;

namespace CindarsHope.Narrative
{
    /// <summary>
    /// fable_63 — state machine PURA (sem UnityEngine) da sequencia de abertura do New Game.
    ///
    /// 3-5 telas de texto PT-BR; Advance() avanca uma tela; Skip() encerra de qualquer ponto;
    /// Finished indica que o gameplay pode liberar. Testavel em EditMode.
    ///
    /// TODOS os textos sao PLACEHOLDER_LORE (refinamento Nymirianos/Cindar pendente) — trocar
    /// texto depois e diff trivial; os ids das telas e a contagem sao estaveis.
    /// </summary>
    public sealed class IntroSequenceModel
    {
        /// <summary>Uma tela da intro: id estavel + texto (PLACEHOLDER_LORE).</summary>
        public sealed class IntroScreen
        {
            public string ScreenId { get; }
            public string Text { get; }

            public IntroScreen(string screenId, string text)
            {
                ScreenId = screenId;
                Text = text;
            }
        }

        // PLACEHOLDER_LORE — texto provisorio (refinamento de lore pendente). Cada string e um
        // ponto de troca documentado no execution report. Ids estaveis (nunca renomear).
        private static readonly IReadOnlyList<IntroScreen> DefaultScreens = new List<IntroScreen>
        {
            new IntroScreen("intro_01",
                "PLACEHOLDER_LORE: A estrada terminou aqui. Depois de dias de viagem, voce chega " +
                "a uma fazenda esquecida nos arredores de Cindar's Hope."),
            new IntroScreen("intro_02",
                "PLACEHOLDER_LORE: O antigo dono se foi e deixou a terra para voce. Os campos estao " +
                "secos, mas a fonte que dorme sob o solo ainda guarda um proposito."),
            new IntroScreen("intro_03",
                "PLACEHOLDER_LORE: Dizem que a cidade vive em torno de um templo antigo, e que Corvus, " +
                "o guardiao, espera por alguem ha muito tempo."),
            new IntroScreen("intro_04",
                "PLACEHOLDER_LORE: Por ora, descanse. Amanha o trabalho comeca — e talvez uma carta " +
                "deixada na sua cama tenha as primeiras respostas."),
        };

        private readonly IReadOnlyList<IntroScreen> _screens;

        public IntroSequenceModel() : this(DefaultScreens) { }

        public IntroSequenceModel(IReadOnlyList<IntroScreen> screens)
        {
            _screens = (screens != null && screens.Count > 0) ? screens : DefaultScreens;
        }

        /// <summary>Indice da tela atual (0-based). -1 quando ja terminou.</summary>
        public int CurrentIndex { get; private set; }

        /// <summary>True quando a sequencia foi concluida ou pulada — gameplay pode liberar.</summary>
        public bool Finished { get; private set; }

        public int ScreenCount => _screens.Count;

        /// <summary>Todas as telas (para o controller renderizar / testes inspecionarem).</summary>
        public IReadOnlyList<IntroScreen> Screens => _screens;

        /// <summary>Tela atual, ou null se Finished.</summary>
        public IntroScreen Current =>
            (!Finished && CurrentIndex >= 0 && CurrentIndex < _screens.Count) ? _screens[CurrentIndex] : null;

        /// <summary>True se a tela atual e a ultima (Advance encerra a sequencia).</summary>
        public bool IsOnLastScreen => !Finished && CurrentIndex == _screens.Count - 1;

        /// <summary>
        /// Avanca para a proxima tela. Na ultima tela, encerra (Finished=true). No-op se ja terminou.
        /// </summary>
        public void Advance()
        {
            if (Finished) return;
            if (CurrentIndex >= _screens.Count - 1)
            {
                Finish();
                return;
            }
            CurrentIndex++;
        }

        /// <summary>Pula toda a sequencia de qualquer tela (Esc). Idempotente.</summary>
        public void Skip()
        {
            Finish();
        }

        private void Finish()
        {
            Finished = true;
            CurrentIndex = -1;
        }
    }
}
