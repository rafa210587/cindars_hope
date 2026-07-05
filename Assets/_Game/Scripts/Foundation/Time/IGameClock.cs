namespace CindarsHope.Foundation.Time
{
    public interface IGameClock
    {
        int CurrentDay { get; }
        int CurrentHourOfDay { get; }
        bool IsDaytime { get; }
    }
}
