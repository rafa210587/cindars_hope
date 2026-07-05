using System.Reflection;
using CindarsHope.UI.HUD.Views;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.UI
{
    public class HudPhaseOneTests
    {
        [TestCase(typeof(InteractionPromptHudView))]
        [TestCase(typeof(StatusBarsHudView))]
        [TestCase(typeof(QuestTrackerHudView))]
        public void PlaceholderHudView_DoesNotRegisterAnEmptyUpdate(System.Type viewType)
        {
            MethodInfo update = viewType.GetMethod(
                "Update",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            MethodInfo initialize = viewType.GetMethod(
                "Initialize",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            Assert.That(update, Is.Null);
            Assert.That(initialize, Is.Not.Null, "Existing binder API must remain available.");
        }
    }
}
