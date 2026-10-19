using CindarsHope.NPC.Friendship;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save da amizade por NPC. Fonte do estado:
    /// <see cref="FriendshipService.Instance"/> (singleton DontDestroyOnLoad).
    /// Fallback: todos os NPCs nível 0 (Unknown) em saves legados sem a seção.
    /// </summary>
    public class FriendshipSectionProvider : ISaveSectionProvider
    {
        public string ProviderId => "friendship";

        public object Capture(GameSaveData existingSaveData)
        {
            var service = FriendshipService.Instance;
            return service != null
                ? service.CaptureSaveData()
                : (existingSaveData?.Friendship ?? new FriendshipSaveData());
        }

        public void Restore(object sectionData)
        {
            var service = FriendshipService.Instance;
            if (service == null)
            {
                return;
            }

            // sectionData null (save legado) = estado limpo, todos os NPCs nível 0.
            service.RestoreFromSaveData(sectionData as FriendshipSaveData);
        }
    }
}
