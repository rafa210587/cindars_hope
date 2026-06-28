using CindarsHope.Craft;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// Provider de save do estado de crafting em andamento. Fonte do estado:
    /// <see cref="CraftingRuntime"/> injetado via constructor. Fallback: estado de crafting vazio
    /// (nenhum job ativo) quando o runtime não está disponível.
    /// </summary>
    public class CraftingSectionProvider : ISaveSectionProvider
    {
        private readonly CraftingRuntime _craftingRuntime;

        public CraftingSectionProvider(CraftingRuntime craftingRuntime)
        {
            _craftingRuntime = craftingRuntime;
        }

        public string ProviderId => "crafting";

        public object Capture(GameSaveData existingSaveData)
        {
            return _craftingRuntime != null
                ? _craftingRuntime.CaptureSaveData()
                : new CraftingRuntimeSaveData();
        }

        public void Restore(object sectionData)
        {
            if (_craftingRuntime == null)
            {
                return;
            }

            var data = sectionData as CraftingRuntimeSaveData;
            if (data != null)
            {
                _craftingRuntime.LoadFromSaveData(data);
            }
        }
    }
}
