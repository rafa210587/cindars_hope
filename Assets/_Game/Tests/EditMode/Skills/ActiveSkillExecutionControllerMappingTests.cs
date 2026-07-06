using CindarsHope.Skills.Runtime.Effects;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Skills
{
    /// <summary>
    /// spec_codex_05 — corrige o mapeamento legado de skill_crafting_field_patch e garante que
    /// todo effectId do catálogo puro tenha um executor registrado.
    ///
    /// O catálogo puro é inspecionado diretamente; não há reflection em estado privado do controller.
    /// </summary>
    [TestFixture]
    public class ActiveSkillExecutionControllerMappingTests
    {
        [Test]
        public void FieldPatch_NoLongerMapsToFarmWaterSkill()
        {
            bool found = ActiveSkillExecutionController.TryGetEffectIdForValidation(
                "skill_crafting_field_patch", out var effectId);

            Assert.IsTrue(found, "skill_crafting_field_patch deve continuar no dicionario (ID nao removido).");
            Assert.AreNotEqual("farm.crop.water_skill", effectId,
                "skill_crafting_field_patch NAO pode mais apontar para o efeito de regar (mapeamento legado errado).");
        }

        [Test]
        public void IrrigadorPortatil_MapsToRealFarmWaterEffect()
        {
            bool found = ActiveSkillExecutionController.TryGetEffectIdForValidation(
                "skill_crafting_irrigador_portatil", out var effectId);

            Assert.IsTrue(found, "skill_crafting_irrigador_portatil deve estar no dicionario.");
            Assert.AreEqual("farm.crop.water_skill", effectId,
                "irrigador_portatil (irrigador = regar) deve receber o efeito real de regar, realocado de field_patch.");
        }

        [Test]
        public void Catalog_HasCanonicalMappingsAndRejectsUnknownIds()
        {
            Assert.That(SkillActionEffectCatalog.All.Count, Is.EqualTo(30));
            Assert.That(SkillActionEffectCatalog.TryGetEffectId(null, out _), Is.False);
            Assert.That(SkillActionEffectCatalog.TryGetEffectId("skill_unknown", out _), Is.False);
        }

        [Test]
        public void EveryMappedEffectId_HasARegisteredExecutor()
        {
            var mapping = SkillActionEffectCatalog.All;
            var registry = ActiveSkillExecutorCatalog.CreateRegistry();
            Assert.IsNotNull(mapping);
            Assert.Greater(mapping.Count, 0);

            foreach (var kvp in mapping)
            {
                Assert.IsTrue(registry.HasExecutor(kvp.Value),
                    $"Skill '{kvp.Key}' maps to effectId '{kvp.Value}', which has no registered executor (orphan effectId).");
            }
        }
    }
}
