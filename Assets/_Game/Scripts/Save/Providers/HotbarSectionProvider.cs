using CindarsHope.UI.Hotbar;

namespace CindarsHope.Save.Providers
{
    /// <summary>
    /// SPEC_10: Hotbar save provider. Isolates hotbar capture/restore from SaveManager.
    /// Delegates to HotbarState (stateful, maintains the binding in memory).
    /// </summary>
    public class HotbarSectionProvider : ISaveSectionProvider
    {
        private readonly HotbarState _hotbarState;

        public string ProviderId => "hotbar";

        public HotbarSectionProvider(HotbarState hotbarState)
        {
            _hotbarState = hotbarState;
        }

        public object Capture(GameSaveData existingSaveData)
        {
            if (_hotbarState == null)
            {
                // Fallback: if no hotbar state, use existing save data
                return existingSaveData?.Hotbar ?? new HotbarSaveData();
            }

            return _hotbarState.CaptureSaveData();
        }

        public void Restore(object sectionData)
        {
            if (_hotbarState == null || sectionData == null)
            {
                return;
            }

            var hotbarData = sectionData as HotbarSaveData;
            if (hotbarData != null)
            {
                _hotbarState.RestoreFromSaveData(hotbarData);
            }
        }
    }
}
