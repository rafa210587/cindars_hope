namespace CindarsHope.UI.SystemTab
{
    /// <summary>fable_56: entradas navegaveis da aba Sistema (ordem de foco do padrao F14).</summary>
    public enum SystemTabAction
    {
        Save = 0,
        Load = 1,
        Volume = 2,
        Video = 3,
        QuitToTitle = 4
    }

    /// <summary>
    /// fable_56: estado da confirmacao modal pendente da aba Sistema. None = sem modal.
    /// Carregar e Sair exigem confirmacao (progresso nao salvo se perde). Recuperacao de
    /// backup (CA-6) e oferecida automaticamente quando o load falha.
    /// </summary>
    public enum SystemConfirmState
    {
        None = 0,
        ConfirmLoad = 1,
        ConfirmQuitToTitle = 2,
        OfferBackupRestore = 3,
        LoadFailedNoBackup = 4
    }

    /// <summary>
    /// fable_56: projection pura e testavel (EditMode) da aba Sistema do painel unico F14.
    ///
    /// Sem dependencia de UnityEngine: o controller MonoBehaviour le isto e reflete no Canvas.
    /// IO de existencia/corrupcao/backup do save vem de um <see cref="ISaveFileProbe"/> injetado
    /// (abstraido), NUNCA de leitura direta de arquivo aqui. Cobre CA-1 (navegacao/estado),
    /// CA-2 (state machine de Salvar/Carregar com confirmacao), CA-6 (recuperacao de backup).
    /// </summary>
    public sealed class SystemTabViewModel
    {
        public const string ConfirmLoadMessage =
            "Carregar o jogo salvo? O progresso nao salvo sera perdido.";
        public const string ConfirmQuitMessage =
            "Sair para o titulo? O progresso nao salvo sera descartado.";
        public const string LoadDisabledMessage = "Nenhum jogo salvo.";
        public const string LoadFailedNoBackupMessage =
            "O save nao pode ser carregado e nao ha backup disponivel.";

        private readonly ISaveFileProbe _probe;

        public SystemTabViewModel(ISaveFileProbe probe)
        {
            _probe = probe;
        }

        /// <summary>Salvar sempre disponivel (slot unico v1; sobrescreve o save atual).</summary>
        public bool CanSave => true;

        /// <summary>Carregar habilitado apenas quando existe arquivo de save.</summary>
        public bool CanLoad => _probe != null && _probe.HasSave;

        public SystemConfirmState ConfirmState { get; private set; } = SystemConfirmState.None;

        /// <summary>True quando ha um modal de confirmacao aberto (input de gameplay bloqueado).</summary>
        public bool HasPendingConfirm => ConfirmState != SystemConfirmState.None;

        /// <summary>Pediu Carregar: abre confirmacao se houver save; no-op se desabilitado.</summary>
        public bool RequestLoad()
        {
            if (!CanLoad)
            {
                return false;
            }

            ConfirmState = SystemConfirmState.ConfirmLoad;
            return true;
        }

        /// <summary>Pediu Sair para o titulo: sempre exige confirmacao.</summary>
        public void RequestQuitToTitle()
        {
            ConfirmState = SystemConfirmState.ConfirmQuitToTitle;
        }

        /// <summary>
        /// Confirma o Carregar pendente. Retorna a acao resolvida. Se o probe sinaliza que o
        /// load falhou (corrompido), NAO entra no jogo: transita para a oferta de backup
        /// (se houver) ou para a mensagem de erro (CA-6). O save corrompido permanece intacto.
        /// </summary>
        public SystemLoadOutcome ConfirmLoad()
        {
            if (ConfirmState != SystemConfirmState.ConfirmLoad)
            {
                return SystemLoadOutcome.NoOp;
            }

            if (_probe != null && _probe.LastLoadFailed)
            {
                if (_probe.HasBackup)
                {
                    ConfirmState = SystemConfirmState.OfferBackupRestore;
                    return SystemLoadOutcome.OfferBackup;
                }

                ConfirmState = SystemConfirmState.LoadFailedNoBackup;
                return SystemLoadOutcome.LoadFailedNoBackup;
            }

            ConfirmState = SystemConfirmState.None;
            return SystemLoadOutcome.LoadedMainSave;
        }

        /// <summary>Aceita restaurar o backup rolling oferecido (CA-6).</summary>
        public SystemLoadOutcome ConfirmBackupRestore()
        {
            if (ConfirmState != SystemConfirmState.OfferBackupRestore)
            {
                return SystemLoadOutcome.NoOp;
            }

            ConfirmState = SystemConfirmState.None;
            return SystemLoadOutcome.RestoredBackup;
        }

        /// <summary>Confirma o Sair para o titulo pendente.</summary>
        public bool ConfirmQuitToTitle()
        {
            if (ConfirmState != SystemConfirmState.ConfirmQuitToTitle)
            {
                return false;
            }

            ConfirmState = SystemConfirmState.None;
            return true;
        }

        /// <summary>Esc/Cancelar fecha qualquer confirmacao sem efeito destrutivo.</summary>
        public void CancelConfirm()
        {
            ConfirmState = SystemConfirmState.None;
        }

        /// <summary>Texto da data do backup para a oferta de restauracao (CA-6).</summary>
        public string BackupOfferMessage()
        {
            var date = _probe != null ? _probe.BackupDateLabel : string.Empty;
            if (string.IsNullOrEmpty(date))
            {
                return "Save danificado - restaurar o backup do ultimo save bom?";
            }

            return $"Save danificado - restaurar o backup de {date}?";
        }
    }

    /// <summary>fable_56: resultado resolvido de uma tentativa de carregar (aba ou titulo).</summary>
    public enum SystemLoadOutcome
    {
        NoOp = 0,
        LoadedMainSave = 1,
        OfferBackup = 2,
        RestoredBackup = 3,
        LoadFailedNoBackup = 4
    }
}
