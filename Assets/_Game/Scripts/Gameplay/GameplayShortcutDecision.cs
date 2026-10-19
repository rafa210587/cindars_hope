namespace CindarsHope.UI.Routing
{
    public enum GameplayInputCommand
    {
        None = 0,
        CloseModal = 1,
        OpenPause = 2,
        OpenInventory = 3,
        OpenEquipment = 4,
        OpenSkillTree = 5
    }

    public readonly struct GameplayShortcutFrame
    {
        public readonly bool EscapePressed;
        public readonly bool InventoryPressed;
        public readonly bool EquipmentPressed;
        public readonly bool SkillTreePressed;

        public GameplayShortcutFrame(bool escapePressed, bool inventoryPressed,
            bool equipmentPressed, bool skillTreePressed)
        {
            EscapePressed = escapePressed;
            InventoryPressed = inventoryPressed;
            EquipmentPressed = equipmentPressed;
            SkillTreePressed = skillTreePressed;
        }
    }

    /// <summary>Pure command decision used by the Unity input adapter.</summary>
    public static class GameplayShortcutDecision
    {
        public static GameplayInputCommand Resolve(GameplayShortcutFrame frame, bool hasModal)
        {
            if (frame.EscapePressed)
                return hasModal ? GameplayInputCommand.CloseModal : GameplayInputCommand.OpenPause;

            if (hasModal) return GameplayInputCommand.None;
            if (frame.InventoryPressed) return GameplayInputCommand.OpenInventory;
            if (frame.EquipmentPressed) return GameplayInputCommand.OpenEquipment;
            if (frame.SkillTreePressed) return GameplayInputCommand.OpenSkillTree;
            return GameplayInputCommand.None;
        }
    }
}
