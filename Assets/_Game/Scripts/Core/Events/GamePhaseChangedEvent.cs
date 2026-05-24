namespace CindarsHope.Core.Events
{
    public class GamePhaseChangedEvent
    {
        public enum GamePhase { Day, Night }

        public GamePhase NewPhase { get; }
        public int CurrentDay { get; }

        public GamePhaseChangedEvent(GamePhase newPhase, int currentDay)
        {
            NewPhase = newPhase;
            CurrentDay = currentDay;
        }
    }
}
