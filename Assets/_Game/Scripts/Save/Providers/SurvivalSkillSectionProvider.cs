using CindarsHope.Skills.Runtime;

namespace CindarsHope.Save.Providers
{
    public sealed class SurvivalSkillSectionProvider : ISaveSectionProvider
    {
        private readonly SurvivalSkillState _state;

        public SurvivalSkillSectionProvider(SurvivalSkillState state)
        {
            _state = state;
        }

        public string ProviderId => "survival_skills";

        public object Capture(GameSaveData existingSaveData)
        {
            if (_state != null)
            {
                return _state.CaptureSaveData();
            }

            return existingSaveData?.SurvivalSkills ?? new SurvivalSkillSaveData();
        }

        public void Restore(object sectionData)
        {
            RestoreForContext(sectionData, string.Empty, 0);
        }

        public void RestoreForContext(object sectionData, string expectedRunId, int expectedCaveLevel)
        {
            if (_state == null || sectionData is not SurvivalSkillSaveData data)
            {
                return;
            }

            _state.RestoreFromSaveData(data, expectedRunId, expectedCaveLevel);
        }
    }
}
