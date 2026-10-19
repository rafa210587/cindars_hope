namespace CindarsHope.Core.Events
{
    public class GameTimeTickEvent
    {
        public float GameTime { get; }
        public int CurrentDay { get; }

        public GameTimeTickEvent(float gameTime, int currentDay)
        {
            GameTime = gameTime;
            CurrentDay = currentDay;
        }
    }
}
