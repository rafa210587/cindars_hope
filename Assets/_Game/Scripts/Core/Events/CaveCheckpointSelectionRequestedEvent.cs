using System.Collections.Generic;

namespace CindarsHope.Core.Events
{
    public readonly struct CaveCheckpointSelectionRequestedEvent
    {
        public readonly IReadOnlyList<int> AvailableCheckpoints;

        public CaveCheckpointSelectionRequestedEvent(IReadOnlyList<int> availableCheckpoints)
        {
            AvailableCheckpoints = availableCheckpoints;
        }
    }
}