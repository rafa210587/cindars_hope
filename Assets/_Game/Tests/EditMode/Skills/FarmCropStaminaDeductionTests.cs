using NUnit.Framework;
using CindarsHope.Farm;
using CindarsHope.Player;
using CindarsHope.Skills.Runtime.Effects;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Skills
{
    [TestFixture]
    public class FarmCropStaminaDeductionTests
    {
        private GameObject _casterGo;
        private GameObject _targetGo;
        private FarmPlot _farmPlot;
        private StaminaManager _stamina;

        [SetUp]
        public void SetUp()
        {
            _casterGo = new GameObject("Caster");
            _stamina = _casterGo.AddComponent<StaminaManager>();
            _targetGo = new GameObject("FarmTarget");
            _farmPlot = _targetGo.AddComponent<FarmPlot>();
            _farmPlot.SetState(FarmPlotState.TilledDry);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_casterGo);
            Object.DestroyImmediate(_targetGo);
        }

        [Test]
        public void Execute_NullCaster_SkipsStaminaDeduction_ReturnsSuccess()
        {
            var executor = new FarmCropSkillEffectExecutor();
            var context = new SkillEffectContext
            {
                SkillActionId = "skill_crafting_field_patch",
                EffectId = "farm.crop.water_skill",
                Caster = null,
                Target = _targetGo
            };

            var result = executor.Execute(context);

            Assert.IsTrue(result.Success, result.FailureReason);
            Assert.IsFalse(result.CostSpent, "CostSpent should be false when caster is null");
        }

        [Test]
        public void Execute_ZeroStamina_ReturnsInsufficientStamina()
        {
            _stamina.Initialize(maxStamina: 100, startingStamina: 0);
            var executor = new FarmCropSkillEffectExecutor();
            var context = new SkillEffectContext
            {
                SkillActionId = "skill_crafting_field_patch",
                EffectId = "farm.crop.water_skill",
                Caster = _casterGo,
                Target = _targetGo
            };

            var result = executor.Execute(context);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("InsufficientStamina", result.FailureReason);
        }

        [Test]
        public void Execute_EnoughStamina_DeductsStaminaAndSucceeds()
        {
            _stamina.Initialize(maxStamina: 100, startingStamina: 100);
            var executor = new FarmCropSkillEffectExecutor();
            var context = new SkillEffectContext
            {
                SkillActionId = "skill_crafting_field_patch",
                EffectId = "farm.crop.water_skill",
                Caster = _casterGo,
                Target = _targetGo
            };

            var result = executor.Execute(context);

            Assert.IsTrue(result.Success, result.FailureReason);
            Assert.IsTrue(result.CostSpent, "CostSpent should be true when stamina was deducted");
            Assert.AreEqual(90, _stamina.CurrentStamina, "Stamina should drop by WaterSkillStaminaCost(10)");
        }
    }
}
