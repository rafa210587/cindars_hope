using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;

namespace CindarsHope.Items.Runtime
{
    /// <summary>
    /// fable_31 — Runtime implementation of <see cref="IMagicTimeGateway"/> over the existing time system.
    /// Reads the absolute day from <c>GameBootstrap.Instance.TimeManager.CurrentDay</c> and, for the hourglass,
    /// publishes <see cref="AdvanceToDawnRequestedEvent"/> for the time system to honor (the TimeManager owns the
    /// phase clock; magic items do not mutate it directly — no parallel time system).
    /// </summary>
    public sealed class RuntimeMagicTimeGateway : IMagicTimeGateway
    {
        public int CurrentDay
        {
            get
            {
                var bootstrap = GameBootstrap.Instance;
                var time = bootstrap != null ? bootstrap.TimeManager : null;
                return time != null ? time.CurrentDay : 1;
            }
        }

        public void AdvanceToDawn()
        {
            GameEventBus.Publish(new AdvanceToDawnRequestedEvent(CurrentDay));
        }
    }
}
