using System;

namespace CindarsHope.Core.Events
{
    public readonly struct CavePlayerDefeatedEvent
    {
        public readonly int CaveLevel;
        public readonly DateTime DefeatedAt;

        public CavePlayerDefeatedEvent(int caveLevel)
        {
            CaveLevel = caveLevel;
            DefeatedAt = DateTime.UtcNow;
        }
    }
}
