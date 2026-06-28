using CindarsHope.Farm.Animals;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save dos animais de fazenda. Fonte do estado:
    /// <see cref="FarmAnimalRegistry.Instance"/> (singleton DontDestroyOnLoad).
    /// Fallback: zero animais em saves legados sem a seção (domínio global aditivo).
    /// </summary>
    public class FarmAnimalsSectionProvider : ISaveSectionProvider
    {
        public string ProviderId => "farm_animals";

        public object Capture(GameSaveData existingSaveData)
        {
            var registry = FarmAnimalRegistry.Instance;
            return registry != null
                ? registry.CaptureSaveData()
                : (existingSaveData?.FarmAnimals ?? new FarmAnimalsSaveData());
        }

        public void Restore(object sectionData)
        {
            var registry = FarmAnimalRegistry.Instance;
            if (registry == null)
            {
                return;
            }

            // sectionData null (save legado) = zero animais registrados.
            registry.RestoreFromSaveData(sectionData as FarmAnimalsSaveData);
        }
    }
}
