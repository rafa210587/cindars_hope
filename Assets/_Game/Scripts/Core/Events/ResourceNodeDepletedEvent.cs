namespace CindarsHope.Core.Events
{
    public readonly struct ResourceNodeDepletedEvent
    {
        public readonly string NodeInstanceId;
        public readonly string ResourceNodeId;
        public readonly string DropItemId;

        public ResourceNodeDepletedEvent(string nodeInstanceId, string resourceNodeId, string dropItemId)
        {
            NodeInstanceId = nodeInstanceId ?? string.Empty;
            ResourceNodeId = resourceNodeId ?? string.Empty;
            DropItemId = dropItemId ?? string.Empty;
        }
    }
}
