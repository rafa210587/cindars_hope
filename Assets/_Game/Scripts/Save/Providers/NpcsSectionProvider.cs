using CindarsHope.NPC;
using UnityEngine.SceneManagement;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save do estado dos NPCs (schedules, diálogos). Fonte do estado:
    /// <see cref="NpcManager"/> injetado via constructor. Captura apenas quando na TownScene;
    /// caso contrário preserva os dados existentes do save (evita perda ao salvar fora da cidade).
    /// </summary>
    public class NpcsSectionProvider : ISaveSectionProvider
    {
        private const string TownSceneName = "TownScene";

        private readonly NpcManager _npcManager;

        public NpcsSectionProvider(NpcManager npcManager)
        {
            _npcManager = npcManager;
        }

        public string ProviderId => "npcs";

        public object Capture(GameSaveData existingSaveData)
        {
            if (SceneManager.GetActiveScene().name == TownSceneName && _npcManager != null)
            {
                return _npcManager.CaptureSaveData();
            }

            return existingSaveData?.Npcs ?? new NpcManagerSaveData();
        }

        public void Restore(object sectionData)
        {
            if (_npcManager == null)
            {
                return;
            }

            var data = sectionData as NpcManagerSaveData;
            if (data != null)
            {
                _npcManager.RestoreFromSaveData(data);
            }
        }
    }
}
