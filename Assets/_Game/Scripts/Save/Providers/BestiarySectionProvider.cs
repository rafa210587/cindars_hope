using CindarsHope.Enemy;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save do bestiário (conhecimento de inimigos descobertos). Fonte do estado:
    /// <see cref="BestiaryManager"/> injetado via constructor. Fallback: bestiário vazio
    /// (nenhum inimigo descoberto) quando o manager não está disponível.
    /// </summary>
    public class BestiarySectionProvider : ISaveSectionProvider
    {
        private readonly BestiaryManager _bestiaryManager;

        public BestiarySectionProvider(BestiaryManager bestiaryManager)
        {
            _bestiaryManager = bestiaryManager;
        }

        public string ProviderId => "bestiary";

        public object Capture(GameSaveData existingSaveData)
        {
            return _bestiaryManager != null
                ? _bestiaryManager.CaptureSaveData()
                : new BestiarySaveData();
        }

        public void Restore(object sectionData)
        {
            if (_bestiaryManager == null)
            {
                return;
            }

            var data = sectionData as BestiarySaveData;
            if (data != null)
            {
                _bestiaryManager.RestoreFromSaveData(data);
            }
        }
    }
}
