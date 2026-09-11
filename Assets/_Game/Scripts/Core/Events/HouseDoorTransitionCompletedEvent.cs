namespace CindarsHope.Core.Events
{
    /// <summary>Publicado uma vez quando uma porta física termina de abrir ou fechar.</summary>
    public readonly struct HouseDoorTransitionCompletedEvent
    {
        public readonly bool IsOpen;
        public HouseDoorTransitionCompletedEvent(bool isOpen) { IsOpen = isOpen; }
    }
}
