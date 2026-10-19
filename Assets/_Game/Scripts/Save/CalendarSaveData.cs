namespace CindarsHope.Save
{
    public class CalendarSaveData
    {
        public int AbsoluteDayIndex { get; set; } = 1;

        public CalendarSaveData() { }

        public CalendarSaveData(int absoluteDayIndex)
        {
            AbsoluteDayIndex = System.Math.Max(1, absoluteDayIndex);
        }
    }
}
