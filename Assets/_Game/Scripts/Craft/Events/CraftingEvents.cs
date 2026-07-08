using CindarsHope.Craft.Data;

namespace CindarsHope.Craft.Events
{
    // arch: Core|Craft (spec_arch_core_craft_cycle_reduction_v32) — movido de Core/Events para
    // Craft/Events: eventos de crafting referenciam WorkshopType (Craft.Data), o que criava um
    // ciclo mutuo Core<->Craft quando hospedados em CindarsHope.Core.Events. Precedente: World/Events
    // ja hospeda eventos de dominio fora de Core.Events (WorldEventHooks etc.). Continuam publicados/
    // assinados via GameEventBus normalmente — nenhuma mudanca de comunicacao direta MonoBehaviour-a-
    // MonoBehaviour.

    /// <summary>
    /// Pedido para ABRIR o craft de uma estação física (forja, alambique, tear…). Publicado por
    /// CraftingStationInteractable ao apertar E; assinado pelo CraftingModal, que abre o craft filtrado
    /// pelo WorkshopType. Mantém a estação desacoplada da UI (regra do GameEventBus).
    /// </summary>
    public readonly struct OpenCraftingStationRequestedEvent
    {
        public OpenCraftingStationRequestedEvent(string stationInstanceId, WorkshopType workshopType)
        {
            StationInstanceId = stationInstanceId;
            WorkshopType = workshopType;
        }

        public string StationInstanceId { get; }
        public WorkshopType WorkshopType { get; }
    }

    public readonly struct CraftingStationOpenedEvent
    {
        public CraftingStationOpenedEvent(string stationInstanceId) => StationInstanceId = stationInstanceId;
        public string StationInstanceId { get; }
    }

    public readonly struct CraftingStationClosedEvent
    {
        public CraftingStationClosedEvent(string stationInstanceId) => StationInstanceId = stationInstanceId;
        public string StationInstanceId { get; }
    }

    public readonly struct CraftingJobStartedEvent
    {
        public CraftingJobStartedEvent(string stationInstanceId, string recipeId, string jobId)
        {
            StationInstanceId = stationInstanceId;
            RecipeId = recipeId;
            JobId = jobId;
        }

        public string StationInstanceId { get; }
        public string RecipeId { get; }
        public string JobId { get; }
    }

    public readonly struct CraftingJobCompletedEvent
    {
        public CraftingJobCompletedEvent(string stationInstanceId, string recipeId, string jobId)
        {
            StationInstanceId = stationInstanceId;
            RecipeId = recipeId;
            JobId = jobId;
        }

        public string StationInstanceId { get; }
        public string RecipeId { get; }
        public string JobId { get; }
    }

    public readonly struct CraftingJobCancelledEvent
    {
        public CraftingJobCancelledEvent(string stationInstanceId, string recipeId, string jobId)
        {
            StationInstanceId = stationInstanceId;
            RecipeId = recipeId;
            JobId = jobId;
        }

        public string StationInstanceId { get; }
        public string RecipeId { get; }
        public string JobId { get; }
    }

    public readonly struct CraftingOutputCollectedEvent
    {
        public CraftingOutputCollectedEvent(string stationInstanceId, string recipeId, string itemId, int amount)
        {
            StationInstanceId = stationInstanceId;
            RecipeId = recipeId;
            ItemId = itemId;
            Amount = amount;
        }

        public string StationInstanceId { get; }
        public string RecipeId { get; }
        public string ItemId { get; }
        public int Amount { get; }
    }

    public readonly struct CraftingFailedEvent
    {
        public CraftingFailedEvent(string stationInstanceId, string recipeId, string message)
        {
            StationInstanceId = stationInstanceId;
            RecipeId = recipeId;
            Message = message;
        }

        public string StationInstanceId { get; }
        public string RecipeId { get; }
        public string Message { get; }
    }
}
