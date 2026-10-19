using CindarsHope.Cave.Runtime;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save da seção de caverna legada (posição/estado do CaveRunManager da cena).
    /// Fonte do estado: <see cref="CaveRunManager"/> injetado via constructor. Preserva os dados
    /// existentes quando o manager não está ativo (o jogador não está na cena de caverna).
    /// </summary>
    public class CaveSectionProvider : ISaveSectionProvider
    {
        private readonly CaveRunManager _caveRunManager;

        public CaveSectionProvider(CaveRunManager caveRunManager)
        {
            _caveRunManager = caveRunManager;
        }

        public string ProviderId => "cave";

        public object Capture(GameSaveData existingSaveData)
        {
            if (_caveRunManager != null)
            {
                return _caveRunManager.CaptureSaveData();
            }

            return existingSaveData?.Cave ?? new CaveSaveData();
        }

        public void Restore(object sectionData)
        {
            if (_caveRunManager == null)
            {
                return;
            }

            var data = sectionData as CaveSaveData;
            if (data != null)
            {
                _caveRunManager.RestoreFromSaveData(data);
            }
        }
    }
}
