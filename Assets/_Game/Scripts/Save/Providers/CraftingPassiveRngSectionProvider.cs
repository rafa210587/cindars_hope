using CindarsHope.Skills.Runtime;

namespace CindarsHope.Save.Providers
{
    public sealed class CraftingPassiveRngSectionProvider : ISaveSectionProvider
    {
        private readonly CraftingPassiveRngState _state;

        public CraftingPassiveRngSectionProvider(CraftingPassiveRngState state)
        {
            _state = state;
        }

        public string ProviderId => "crafting_passive_rng";

        public object Capture(GameSaveData existingSaveData)
        {
            if (_state != null)
            {
                return _state.CaptureSaveData();
            }

            return existingSaveData?.CraftingPassiveRng ?? new CraftingPassiveRngSaveData();
        }

        public void Restore(object sectionData)
        {
            if (_state == null)
            {
                return;
            }

            _state.RestoreFromSaveData(sectionData as CraftingPassiveRngSaveData);
        }
    }
}
