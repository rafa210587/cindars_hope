namespace CindarsHope.Core.Events
{
    public struct PauseOpenedEvent { }

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

    public readonly struct ModalCloseRequestedEvent
    {
        public readonly CindarsHope.Foundation.ModalType TargetModal;
        public ModalCloseRequestedEvent(CindarsHope.Foundation.ModalType targetModal) => TargetModal = targetModal;
        public bool Targets(CindarsHope.Foundation.ModalType modal)
            => TargetModal == CindarsHope.Foundation.ModalType.None || TargetModal == modal;
    }
}
