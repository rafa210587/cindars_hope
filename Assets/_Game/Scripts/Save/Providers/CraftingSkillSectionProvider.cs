using CindarsHope.Skills.Runtime;

namespace CindarsHope.Save.Providers
{
    public sealed class CraftingSkillSectionProvider : ISaveSectionProvider
    {
        private readonly CraftingSkillState _state;

        public CraftingSkillSectionProvider(CraftingSkillState state)
        {
            _state = state;
        }

        public string ProviderId => "crafting_skills";

        public object Capture(GameSaveData existingSaveData)
        {
            if (_state != null)
                return _state.CaptureSaveData();

            return existingSaveData?.CraftingSkills ?? new CraftingSkillSaveData();
        }

        public void Restore(object sectionData)
        {
            if (_state == null || sectionData is not CraftingSkillSaveData data)
                return;

            _state.RestoreFromSaveData(data);
        }
    }
}
