using System;
using System.Collections.Generic;

namespace CindarsHope.Foundation
{
    [Serializable]
    public class HotbarSaveData
    {
        public List<string> SlotItemIds = new List<string>();
        public int SelectedSlotIndex;
    }
}
