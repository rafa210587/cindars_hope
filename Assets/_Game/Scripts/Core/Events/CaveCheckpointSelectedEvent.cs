namespace CindarsHope.Core.Events
{
    public readonly struct CaveCheckpointSelectedEvent
    {
        public readonly int SelectedCheckpointLevel;

        public CaveCheckpointSelectedEvent(int selectedCheckpointLevel)
        {
            SelectedCheckpointLevel = selectedCheckpointLevel;
        }
    }
}
