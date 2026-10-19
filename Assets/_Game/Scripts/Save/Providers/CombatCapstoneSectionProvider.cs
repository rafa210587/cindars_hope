using CindarsHope.Skills.Runtime;

namespace CindarsHope.Save.Providers
{
    public sealed class CombatCapstoneSectionProvider : ISaveSectionProvider
    {
        private readonly CombatCapstoneState _state;

        public CombatCapstoneSectionProvider(CombatCapstoneState state) => _state = state;

        public string ProviderId => "combat_capstones";

        public object Capture(GameSaveData existingSaveData)
            => _state != null
                ? _state.CaptureSaveData()
                : existingSaveData?.CombatCapstones ?? new CombatCapstoneSaveData();

        public void Restore(object sectionData)
            => _state?.RestoreFromSaveData(sectionData as CombatCapstoneSaveData);
    }
}
