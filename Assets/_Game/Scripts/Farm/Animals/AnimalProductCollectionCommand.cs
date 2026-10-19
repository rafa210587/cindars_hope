namespace CindarsHope.Farm.Animals
{
    public class AnimalProductCollectionCommand
    {
        public string AnimalInstanceId { get; set; }
        public string ActorId { get; set; }
        public int RequestedDay { get; set; }
        public string TargetInventoryId { get; set; }
        public bool AllowOverflowHandling { get; set; } = true;

        public AnimalProductCollectionCommand()
        {
        }

        public AnimalProductCollectionCommand(string animalInstanceId, int requestedDay, string targetInventoryId)
        {
            AnimalInstanceId = animalInstanceId;
            RequestedDay = requestedDay;
            TargetInventoryId = targetInventoryId;
        }
    }
}
