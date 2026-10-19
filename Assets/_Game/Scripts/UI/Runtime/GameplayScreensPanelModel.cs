using CindarsHope.Foundation;
using CindarsHope.UI.Modal;

namespace CindarsHope.UI.Runtime
{
    /// <summary>
    /// fable_14: pure-C# state model for the single 9-tab gameplay panel.
    ///
    /// Owns: which tab is active, whether the panel is open, the mapping from a direct
    /// keyboard shortcut to a tab, the placeholder ("em breve") classification for tabs not
    /// yet owned by a landed spec, and the canonical empty-state messages.
    ///
    /// No UnityEngine dependency — the MonoBehaviour bootstrap/view drives it from input and
    /// reflects its state into the Canvas. This is the deterministic, EditMode-testable core
    /// of the spec (CA-1 open/close + tab routing, CA-4 empty states).
    /// </summary>
    public sealed class GameplayScreensPanelModel
    {
        public const string EmptyInventoryMessage = "Inventario vazio.";
        public const string EmptyQuestsMessage = "Nenhuma quest ativa.";
        public const string EmptySkillsMessage = "Nenhum ponto de skill disponivel.";
        public const string PlaceholderMessage = "Em breve.";

        public bool IsOpen { get; private set; }

        public GameplayScreenTab ActiveTab { get; private set; } = GameplayScreenTab.Inventory;

        private const int TabCount = 9;

        /// <summary>
        /// Open the panel on a specific tab. Returns true if it transitioned from closed to
        /// open OR switched tabs while open; false when already open on the same tab (no-op,
        /// matching ModalManager's idempotent push of the same type).
        /// </summary>
        public bool Open(GameplayScreenTab tab)
        {
            if (IsOpen && ActiveTab == tab)
            {
                return false;
            }

            IsOpen = true;
            ActiveTab = tab;
            return true;
        }

        /// <summary>Close the panel (Esc on the root tab). Returns previous open state.</summary>
        public bool Close()
        {
            var wasOpen = IsOpen;
            IsOpen = false;
            return wasOpen;
        }

        /// <summary>Tab cycles forward (wrap). Only valid while open.</summary>
        public void CycleNext()
        {
            if (!IsOpen)
            {
                return;
            }

            ActiveTab = (GameplayScreenTab)(((int)ActiveTab + 1) % TabCount);
        }

        /// <summary>Shift+Tab cycles backward (wrap). Only valid while open.</summary>
        public void CyclePrevious()
        {
            if (!IsOpen)
            {
                return;
            }

            ActiveTab = (GameplayScreenTab)(((int)ActiveTab - 1 + TabCount) % TabCount);
        }

        /// <summary>
        /// True when this tab is a not-yet-implemented placeholder ("em breve"). The four
        /// core tabs of this spec are live; the rest are reserved for their owning specs.
        /// </summary>
        public static bool IsPlaceholderTab(GameplayScreenTab tab)
        {
            switch (tab)
            {
                case GameplayScreenTab.Inventory:
                case GameplayScreenTab.Equipment:
                case GameplayScreenTab.Skills:
                case GameplayScreenTab.Quests:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Map a gameplay direct-open shortcut to the tab it should land on.
        /// Keeps the SAME keys/ModalTypes the legacy IMGUI controllers used.
        /// </summary>
        public static bool TryGetTabForModalType(ModalType modalType, out GameplayScreenTab tab)
        {
            switch (modalType)
            {
                case ModalType.Inventory:
                    tab = GameplayScreenTab.Inventory;
                    return true;
                case ModalType.CharacterEquipment:
                    tab = GameplayScreenTab.Equipment;
                    return true;
                case ModalType.SkillTree:
                    tab = GameplayScreenTab.Skills;
                    return true;
                case ModalType.QuestLog:
                    tab = GameplayScreenTab.Quests;
                    return true;
                default:
                    tab = GameplayScreenTab.Inventory;
                    return false;
            }
        }

        /// <summary>The single ModalType the panel pushes on the existing ModalManager.</summary>
        public static ModalType GetModalTypeForTab(GameplayScreenTab tab)
        {
            switch (tab)
            {
                case GameplayScreenTab.Equipment:
                    return ModalType.CharacterEquipment;
                case GameplayScreenTab.Skills:
                    return ModalType.SkillTree;
                case GameplayScreenTab.Quests:
                    return ModalType.QuestLog;
                default:
                    return ModalType.Inventory;
            }
        }
    }
}
