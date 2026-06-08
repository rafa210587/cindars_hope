using System.Collections.Generic;

namespace CindarsHope.Farm.Animals
{
    public enum AnimalHousingBuildingType
    {
        Coop = 0,
        Barn = 1,
        Pasture = 2,
        StableFuture = 3
    }

    public class AnimalHousingCapacityState
    {
        public string HomeBuildingId { get; set; }
        public AnimalHousingBuildingType BuildingType { get; set; }
        public int Capacity { get; set; }
        public List<string> AnimalInstanceIds { get; set; } = new List<string>();
        public string FeedStorageId { get; set; }
        public bool IsAccessible { get; set; } = true;

        public AnimalHousingCapacityState()
        {
        }

        public AnimalHousingCapacityState(string homeBuildingId, AnimalHousingBuildingType buildingType, int capacity)
        {
            HomeBuildingId = homeBuildingId;
            BuildingType = buildingType;
            Capacity = capacity;
            AnimalInstanceIds = new List<string>();
        }

        public bool CanAddAnimal()
        {
            return IsAccessible && AnimalInstanceIds.Count < Capacity;
        }

        public void AddAnimal(string animalInstanceId)
        {
            if (!CanAddAnimal())
            {
                throw new System.InvalidOperationException(
                    $"Cannot add animal to {HomeBuildingId}: capacity full or not accessible");
            }
            AnimalInstanceIds.Add(animalInstanceId);
        }

        public void RemoveAnimal(string animalInstanceId)
        {
            AnimalInstanceIds.Remove(animalInstanceId);
        }

        public int GetAvailableCapacity()
        {
            return Capacity - AnimalInstanceIds.Count;
        }
    }
}
