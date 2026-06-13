namespace CindarsHope.Core.Events
{
    public struct PauseOpenedEvent { }

    public struct PauseClosedEvent { }

    public readonly struct NotificationToastRequestedEvent
    {
        public readonly string Message;
        public NotificationToastRequestedEvent(string message) { Message = message ?? string.Empty; }
    }

    public struct InventoryPanelOpenedEvent { }

    public struct EquipmentPanelOpenedEvent { }

    public struct CheckpointMenuOpenedEvent { }

    public struct CheckpointMenuClosedEvent { }

    public struct DeathScreenOpenedEvent { }

    public struct DeathScreenClosedEvent { }

    public struct ModalCloseRequestedEvent { }
}
