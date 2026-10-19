using CindarsHope.Player.Progression;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save da progressão do jogador (XP, nível, pontos de habilidade).
    /// Fonte do estado: <see cref="PlayerProgressionManager"/> injetado via constructor.
    /// Fallback: seção de progressão default (nível 1) quando o manager não está disponível.
    /// </summary>
    public class ProgressionSectionProvider : ISaveSectionProvider
    {
        private readonly PlayerProgressionManager _progressionManager;

        public ProgressionSectionProvider(PlayerProgressionManager progressionManager)
        {
            _progressionManager = progressionManager;
        }

        public string ProviderId => "progression";

        public object Capture(GameSaveData existingSaveData)
        {
            return _progressionManager != null
                ? _progressionManager.CaptureSaveData()
                : new PlayerProgressionSaveData();
        }

        public void Restore(object sectionData)
        {
            if (_progressionManager == null)
            {
                return;
            }

            _progressionManager.RestoreFromSaveData(sectionData as PlayerProgressionSaveData ?? new PlayerProgressionSaveData());
        }
    }
}
