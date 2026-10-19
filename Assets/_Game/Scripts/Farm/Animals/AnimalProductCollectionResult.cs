using System.Collections.Generic;

namespace CindarsHope.Farm.Animals
{
    public enum AnimalProductCollectionFailureReason
    {
        None = 0,
        AnimalNotFound,
        AnimalUnavailable,
        ProductNotReady,
        AnimalNotFed,
        InventoryFull,
        InvalidProductDefinition,
        DuplicateCollection,
        SaveStateInvalid,
        LateGameReserved
    }

    public class AnimalProductCollectionResult
    {
        public bool Success { get; set; }
        public AnimalProductCollectionFailureReason FailureReason { get; set; }
        public string OutputItemId { get; set; }
        public AnimalProductQuality Quality { get; set; }
        public int Quantity { get; set; }
        public bool ProductReadyConsumed { get; set; }
        public List<string> Events { get; set; } = new List<string>();

        public static AnimalProductCollectionResult Ok(string itemId, AnimalProductQuality quality, int quantity)
        {
            return new AnimalProductCollectionResult
            {
                Success = true,
                OutputItemId = itemId,
                Quality = quality,
                Quantity = quantity,
                ProductReadyConsumed = true,
                Events = new List<string> { "AnimalProductCollected" }
            };
        }

        public static AnimalProductCollectionResult Fail(AnimalProductCollectionFailureReason reason)
        {
            return new AnimalProductCollectionResult
            {
                Success = false,
                FailureReason = reason,
                ProductReadyConsumed = false
            };
        }
    }
}
