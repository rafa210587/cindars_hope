namespace CindarsHope.UI.SystemTab
{
    /// <summary>
    /// fable_56: probe abstrato sobre o estado em disco de um slot de save, para tornar a
    /// logica de disponibilidade (Continue / Carregar) e a recuperacao de save corrompido
    /// (EMENDA v3 4.8) testavel em EditMode SEM tocar o SaveManager nem o filesystem real.
    ///
    /// O adapter de runtime consulta apenas a API publica do SaveManager (slot unico v1 +
    /// SaveFilePath); nao reimplementa leitura de save nem muda schema (Arquivos proibidos).
    /// </summary>
    public interface ISaveFileProbe
    {
        /// <summary>Existe um arquivo de save principal carregavel para o slot ativo.</summary>
        bool HasSave { get; }

        /// <summary>
        /// O save principal existe mas falhou ao carregar (corrompido / ilegivel / schema
        /// irrecuperavel). Sinalizado pelo SaveManager; o save corrompido NUNCA e apagado.
        /// </summary>
        bool LastLoadFailed { get; }

        /// <summary>Existe um backup rolling do ultimo save bom para o slot ativo.</summary>
        bool HasBackup { get; }

        /// <summary>Data legivel do backup rolling (ex.: "2026-06-20 14:32"), ou vazio.</summary>
        string BackupDateLabel { get; }
    }
}
