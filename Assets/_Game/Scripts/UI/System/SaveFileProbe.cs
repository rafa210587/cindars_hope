using System;
using System.IO;
using UnityEngine;
using CindarsHope.Save;

namespace CindarsHope.UI.SystemTab
{
    /// <summary>
    /// fable_56: adapter de runtime do <see cref="ISaveFileProbe"/>. Consulta apenas a API
    /// publica do SaveManager (SaveFilePath, slot unico v1) e inspeciona o filesystem para
    /// existencia e backup rolling. NAO reimplementa leitura de save nem muda schema
    /// (Arquivos proibidos). A deteccao de "load falhou" e setada pelo fluxo de Carregar a
    /// partir do retorno de SaveManager.LoadGame (ver SystemTabController).
    ///
    /// O backup rolling em si (3 slots manuais + .bak por gravacao) e PENDENCIA de schema do
    /// SaveManager (EMENDA 2026-06-12-D item 2 / EMENDA v3 4.8) — registrada como dívida no
    /// execution report. Este probe ja le um arquivo de backup convencional ".bak" se existir,
    /// para que a recuperacao funcione assim que o SaveManager passe a grava-lo.
    /// </summary>
    public sealed class SaveFileProbe : ISaveFileProbe
    {
        private const string BackupExtension = ".bak";

        private readonly Func<string> _savePathProvider;

        public SaveFileProbe(SaveManager saveManager)
        {
            _savePathProvider = () => saveManager != null ? saveManager.SaveFilePath : null;
        }

        public SaveFileProbe(Func<string> savePathProvider)
        {
            _savePathProvider = savePathProvider ?? (() => null);
        }

        /// <summary>Setado pelo fluxo de Carregar quando SaveManager.LoadGame retorna false.</summary>
        public bool LastLoadFailed { get; set; }

        public bool HasSave
        {
            get
            {
                var path = _savePathProvider();
                return !string.IsNullOrEmpty(path) && File.Exists(path);
            }
        }

        public bool HasBackup
        {
            get
            {
                var path = BackupPath();
                return !string.IsNullOrEmpty(path) && File.Exists(path);
            }
        }

        public string BackupDateLabel
        {
            get
            {
                var path = BackupPath();
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    return string.Empty;
                }

                try
                {
                    return File.GetLastWriteTime(path).ToString("yyyy-MM-dd HH:mm");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[fable_56] SaveFileProbe nao leu data do backup '{path}': {ex.Message}");
                    return string.Empty;
                }
            }
        }

        private string BackupPath()
        {
            var path = _savePathProvider();
            return string.IsNullOrEmpty(path) ? null : path + BackupExtension;
        }
    }
}
