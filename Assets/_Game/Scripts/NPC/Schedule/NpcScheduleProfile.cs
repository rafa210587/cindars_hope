using System.Collections.Generic;

namespace CindarsHope.NPC.Schedule
{
    [System.Serializable]
    public class NpcScheduleProfile
    {
        public string ScheduleId;
        public string NpcId;
        public List<NpcScheduleBlock> Blocks = new List<NpcScheduleBlock>();
    }
}
