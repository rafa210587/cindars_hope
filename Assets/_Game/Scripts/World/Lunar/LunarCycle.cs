namespace CindarsHope.World.Lunar
{
    public struct LunarCycle
    {
        public const int DaysPerLunarCycle = 28; // 8 phases × ~3.5 days

        public int AbsoluteDay { get; private set; }

        public LunarCycle(int absoluteDay)
        {
            AbsoluteDay = System.Math.Max(1, absoluteDay);
        }

        public LunarPhase CurrentPhase
        {
            get
            {
                return (LunarPhase)((AbsoluteDay - 1) % DaysPerLunarCycle / 3);
            }
        }

        public int DayInPhase
        {
            get
            {
                int dayInCycle = (AbsoluteDay - 1) % DaysPerLunarCycle;
                return (dayInCycle % 3) + 1;
            }
        }

        public static LunarCycle FromAbsoluteDay(int absoluteDay)
        {
            return new LunarCycle(absoluteDay);
        }
    }
}
