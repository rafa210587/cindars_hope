using System;

namespace CindarsHope.UI.Hotbar
{
    [Serializable]
    public class HotbarState
    {
        public const int SlotCount = 6;

        private readonly string[] _slotItemIds = new string[SlotCount];

        public int SelectedSlotIndex { get; private set; }
        public string SelectedItemId => _slotItemIds[SelectedSlotIndex] ?? string.Empty;

        public bool SetSlot(int slotIndex, string itemId)
        {
            if (slotIndex < 0 || slotIndex >= SlotCount)
            {
                return false;
            }

            _slotItemIds[slotIndex] = itemId ?? string.Empty;
            return true;
        }

        public bool SelectSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SlotCount)
            {
                return false;
            }

            SelectedSlotIndex = slotIndex;
            return true;
        }

        public string GetSlotItemId(int slotIndex)
        {
            return slotIndex < 0 || slotIndex >= SlotCount ? string.Empty : _slotItemIds[slotIndex] ?? string.Empty;
        }

        public HotbarSaveData CaptureSaveData()
        {
            var saveData = new HotbarSaveData { SelectedSlotIndex = SelectedSlotIndex };
            for (int i = 0; i < SlotCount; i++)
            {
                saveData.SlotItemIds.Add(_slotItemIds[i] ?? string.Empty);
            }

            return saveData;
        }

        public void RestoreFromSaveData(HotbarSaveData saveData)
        {
            Array.Clear(_slotItemIds, 0, _slotItemIds.Length);

            if (saveData?.SlotItemIds != null)
            {
                int count = Math.Min(SlotCount, saveData.SlotItemIds.Count);
                for (int i = 0; i < count; i++)
                {
                    _slotItemIds[i] = saveData.SlotItemIds[i] ?? string.Empty;
                }
            }

            SelectSlot(saveData == null ? 0 : saveData.SelectedSlotIndex);
        }
    }
}
