using System;
using System.Collections.Generic;

namespace CindarsHope.Skills.Runtime
{
    [Serializable]
    public sealed class CraftingSkillSaveData
    {
        public int Version = 1;
        public int CurrentDayIndex = 1;
        public List<int> ConsumedDayIndices = new List<int>();
        public List<CraftingSkillReservationSaveData> PendingReservations =
            new List<CraftingSkillReservationSaveData>();
    }

    [Serializable]
    public sealed class CraftingSkillReservationSaveData
    {
        public int DayIndex;
        public string ReservationToken = string.Empty;
    }
}
