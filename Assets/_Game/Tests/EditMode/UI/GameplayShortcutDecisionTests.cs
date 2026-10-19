using CindarsHope.UI.Routing;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.UI
{
    public class GameplayShortcutDecisionTests
    {
        [Test]
        public void Escape_ClosesModalBeforeAnyOtherShortcut()
        {
            var frame = new GameplayShortcutFrame(true, true, true, true);
            Assert.AreEqual(GameplayInputCommand.CloseModal,
                GameplayShortcutDecision.Resolve(frame, hasModal: true));
        }

        [Test]
        public void Modal_BlocksGameplayShortcuts()
        {
            var frame = new GameplayShortcutFrame(false, true, false, false);
            Assert.AreEqual(GameplayInputCommand.None,
                GameplayShortcutDecision.Resolve(frame, hasModal: true));
        }

        [TestCase(true, false, false, GameplayInputCommand.OpenInventory)]
        [TestCase(false, true, false, GameplayInputCommand.OpenEquipment)]
        [TestCase(false, false, true, GameplayInputCommand.OpenSkillTree)]
        public void NoModal_RoutesSingleShortcut(bool inventory, bool equipment, bool skills,
            GameplayInputCommand expected)
        {
            var frame = new GameplayShortcutFrame(false, inventory, equipment, skills);
            Assert.AreEqual(expected, GameplayShortcutDecision.Resolve(frame, hasModal: false));
        }

        [Test]
        public void EscapeWithoutModal_RequestsPause()
        {
            var frame = new GameplayShortcutFrame(true, false, false, false);
            Assert.AreEqual(GameplayInputCommand.OpenPause,
                GameplayShortcutDecision.Resolve(frame, hasModal: false));
        }
    }
}
