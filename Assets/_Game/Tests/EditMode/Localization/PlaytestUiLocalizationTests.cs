using CindarsHope.Localization;
using CindarsHope.UI.Crafting;
using NUnit.Framework;
namespace CindarsHope.Tests.EditMode.Localization
{
    public class PlaytestUiLocalizationTests
    {
        [SetUp] public void SetUp() => LocalizationStringTable.OverrideForTests(null);
        [TestCase("ui.pause.title")]
        [TestCase("ui.settings.master")]
        [TestCase("ui.inventory.use_unavailable")]
        [TestCase("interact.chest.unavailable")]
        [TestCase("ui.crafting.cancel_unavailable")]
        [TestCase("ui.equipment.slot_lefthand")]
        public void NewKeys_ResolveToPlayerText(string key)
            => Assert.That(LocalizationService.Get(key), Is.Not.EqualTo(key));
        [Test] public void CraftSuccess_FormatsNameAndQuantity()
        {
            var text = string.Format(LocalizationService.Get("ui.crafting.created"), "Madeira", 2);
            Assert.That(text, Does.Contain("Madeira"));
            Assert.That(text, Does.Contain("2"));
        }
        [Test] public void UnknownFailure_PreservesDiagnosticFallback()
            => Assert.That(CraftingModal.PresentFailure("future.failure"), Is.EqualTo("future.failure"));
        [Test] public void MissingIngredient_DoesNotLeakItemIdentifier()
        {
            var text = CraftingModal.PresentFailure("Missing ingredient item_material_wood.");
            Assert.That(text, Is.EqualTo(LocalizationService.Get("ui.crafting.material_missing")));
            Assert.That(text, Does.Not.Contain("item_material_wood"));
        }
    }
}
