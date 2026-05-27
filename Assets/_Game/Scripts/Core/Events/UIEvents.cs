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

    public struct InventoryPanelClosedEvent { }

    public struct EquipmentPanelOpenedEvent { }

    public struct EquipmentPanelClosedEvent { }

    public struct CraftingPanelOpenedEvent { }

    public struct CraftingPanelClosedEvent { }

    public struct SkillTreePanelClosedEvent { }

    public struct CheckpointMenuOpenedEvent { }

    public struct CheckpointMenuClosedEvent { }

    public struct DeathScreenOpenedEvent { }

    public struct DeathScreenClosedEvent { }

    public struct ModalCloseRequestedEvent { }

    public struct DebugHudToggledEvent
    {
        public bool IsEnabled;
        public DebugHudToggledEvent(bool isEnabled) { IsEnabled = isEnabled; }
    }
}
