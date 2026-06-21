using CindarsHope.UI.SystemTab;

namespace CindarsHope.UI.Title
{
    /// <summary>fable_56: opcoes da tela de titulo (ordem de foco do padrao F14).</summary>
    public enum TitleAction
    {
        NewGame = 0,
        Continue = 1,
        Quit = 2
    }

    /// <summary>
    /// fable_56: projection pura e testavel da tela de titulo minima (padrao F14).
    ///
    /// Continue so e visivel/habilitado quando existe save (via <see cref="ISaveFileProbe"/>
    /// injetado, CA-4). Continue usa o MESMO caminho de load da aba Sistema, incluindo a
    /// recuperacao de backup quando o save principal falha (CA-6). New Game limpa o estado
    /// em memoria e NUNCA deleta slot_1.json (CA-3). Sem UnityEngine.
    /// </summary>
    public sealed class TitleFlowViewModel
    {
        private readonly ISaveFileProbe _probe;

        public TitleFlowViewModel(ISaveFileProbe probe)
        {
            _probe = probe;
        }

        /// <summary>Continue aparece/habilita somente com save existente (CA-4).</summary>
        public bool CanContinue => _probe != null && _probe.HasSave;

        /// <summary>New Game sempre disponivel.</summary>
        public bool CanStartNewGame => true;

        /// <summary>
        /// Resolve o que o Continue do titulo deve fazer, espelhando o ConfirmLoad da aba:
        /// load do save principal, oferta de backup se corrompido + backup presente, ou
        /// mensagem de erro se corrompido sem backup. NoOp se nao ha save.
        /// </summary>
        public SystemLoadOutcome ResolveContinue()
        {
            if (!CanContinue)
            {
                return SystemLoadOutcome.NoOp;
            }

            if (_probe.LastLoadFailed)
            {
                return _probe.HasBackup
                    ? SystemLoadOutcome.OfferBackup
                    : SystemLoadOutcome.LoadFailedNoBackup;
            }

            return SystemLoadOutcome.LoadedMainSave;
        }
    }
}
