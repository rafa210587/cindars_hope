namespace CindarsHope.World.Calendar
{
    public struct GameDate
    {
        public const int DaysPerSeason = 28;
        public const int SeasonsPerYear = 4;
        public const int DaysPerYear = DaysPerSeason * SeasonsPerYear; // 112
        public const int DaysPerWeek = 7;

        public int AbsoluteDay { get; private set; }

        public GameDate(int absoluteDay)
        {
            AbsoluteDay = System.Math.Max(1, absoluteDay);
        }

        public int Year
        {
            get
            {
                return (AbsoluteDay - 1) / DaysPerYear + 1;
            }
        }

        public Season CurrentSeason
        {
            get
            {
                int dayInYear = (AbsoluteDay - 1) % DaysPerYear;
                return (Season)(dayInYear / DaysPerSeason);
            }
        }

        public int DayInSeason
        {
            get
            {
                int dayInYear = (AbsoluteDay - 1) % DaysPerYear;
                return (dayInYear % DaysPerSeason) + 1;
            }
        }

        public int DayOfWeek
        {
            get
            {
                return ((AbsoluteDay - 1) % DaysPerWeek) + 1;
            }
        }

        public static GameDate FromAbsoluteDay(int absoluteDay)
        {
            return new GameDate(absoluteDay);
        }
    }
}
