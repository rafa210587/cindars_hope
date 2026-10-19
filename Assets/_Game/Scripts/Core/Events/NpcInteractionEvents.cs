namespace CindarsHope.Core.Events
{
    public readonly struct NpcInteractionStartedEvent
    {
        public readonly string NpcId;

        public NpcInteractionStartedEvent(string npcId)
        {
            NpcId = npcId;
        }
    }

    public readonly struct NpcInteractionEndedEvent
    {
        public readonly string NpcId;

        public NpcInteractionEndedEvent(string npcId)
        {
            NpcId = npcId;
        }
    }
}
