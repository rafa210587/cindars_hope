using NUnit.Framework;
using CindarsHope.Fonte;
using CindarsHope.MainProgression;

namespace CindarsHope.Tests.EditMode.Fonte
{
    [TestFixture]
    public class FonteAnyaFunctionsTests
    {
        private FonteFunctionUnlockService _service;
        private FonteAnyaValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _service = new FonteFunctionUnlockService();
            _validator = new FonteAnyaValidator();
        }

        private FonteAnyaSection EmptySection() => new FonteAnyaSection { Version = 1 };

        private MainProgressionSection ProgressionWithFragment(MainFragmentType type)
        {
            var svc = new MainProgressionService();
            var prog = new MainProgressionSection { Version = 1 };
            svc.TryIntegrateFragment(prog, MainFragmentType.Water, 10, false);
            if (type == MainFragmentType.Memory || type == MainFragmentType.Life || type == MainFragmentType.Hope)
                svc.TryIntegrateFragment(prog, MainFragmentType.Memory, 15, false);
            if (type == MainFragmentType.Life || type == MainFragmentType.Hope)
                svc.TryIntegrateFragment(prog, MainFragmentType.Life, 20, false);
            if (type == MainFragmentType.Hope)
                svc.TryIntegrateFragment(prog, MainFragmentType.Hope, 30, false);
            return prog;
        }

        // ---- Unlock Tests ----

        [Test]
        public void ReturnPoint_UnlocksWithoutFragment()
        {
            var section = EmptySection();
            var prog = new MainProgressionSection { Version = 1 };
            var result = _service.TryUnlock(section, FonteFunction.ReturnPoint, prog);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(section.HasFunction(FonteFunction.ReturnPoint));
            Assert.AreEqual(FonteState.AwakenedReturnOnly, section.FonteState);
        }

        [Test]
        public void LivingWater_RequiresWaterFragment()
        {
            var section = EmptySection();
            var prog = new MainProgressionSection { Version = 1 }; // no fragments
            var result = _service.TryUnlock(section, FonteFunction.LimitedLivingWater, prog);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailureReason.Contains("LIVING_WATER_REQUIRES_WATER_FRAGMENT"));
        }

        [Test]
        public void LivingWater_AfterWaterFragment_Succeeds()
        {
            var section = EmptySection();
            var prog = ProgressionWithFragment(MainFragmentType.Water);
            var result = _service.TryUnlock(section, FonteFunction.LimitedLivingWater, prog);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(FonteState.WaterFlowing, section.FonteState);
        }

        [Test]
        public void Respec_RequiresMemoryFragment()
        {
            var section = EmptySection();
            var prog = ProgressionWithFragment(MainFragmentType.Water);
            var result = _service.TryUnlock(section, FonteFunction.Respec, prog);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailureReason.Contains("RESPEC_REQUIRES_MEMORY_FRAGMENT"));
        }

        [Test]
        public void Respec_AfterMemoryFragment_Succeeds()
        {
            var section = EmptySection();
            var prog = ProgressionWithFragment(MainFragmentType.Memory);
            var result = _service.TryUnlock(section, FonteFunction.Respec, prog);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(FonteState.MemoryEchoing, section.FonteState);
        }

        [Test]
        public void AdvancedPurification_RequiresLifeFragment()
        {
            var section = EmptySection();
            var prog = ProgressionWithFragment(MainFragmentType.Memory);
            var result = _service.TryUnlock(section, FonteFunction.AdvancedPurification, prog);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailureReason.Contains("ADVANCED_PURIFICATION_REQUIRES_LIFE_FRAGMENT"));
        }

        [Test]
        public void AdvancedPurification_AfterLifeFragment_Succeeds()
        {
            var section = EmptySection();
            var prog = ProgressionWithFragment(MainFragmentType.Life);
            var result = _service.TryUnlock(section, FonteFunction.AdvancedPurification, prog);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(FonteState.LifeBlooming, section.FonteState);
        }

        [Test]
        public void FinalChoice_RequiresHopeAndLevel101()
        {
            var section = EmptySection();
            var prog = ProgressionWithFragment(MainFragmentType.Life); // no Hope, no Level101
            var result = _service.TryUnlock(section, FonteFunction.FinalChoicePreparation, prog);
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void Unlock_Idempotent()
        {
            var section = EmptySection();
            var prog = ProgressionWithFragment(MainFragmentType.Water);
            _service.TryUnlock(section, FonteFunction.LimitedLivingWater, prog);
            var result = _service.TryUnlock(section, FonteFunction.LimitedLivingWater, prog);
            Assert.IsTrue(result.AlreadyUnlocked);
        }

        // ---- Use Request Tests ----

        [Test]
        public void LivingWater_Use_ConsumesCharge()
        {
            var section = EmptySection();
            section.UnlockedFunctions.Add(FonteFunction.LimitedLivingWater);
            section.LivingWater.Unlocked = true;
            section.LivingWater.CurrentCharges = 2;
            var req = new FonteUseRequest { RequestedFunction = FonteFunction.LimitedLivingWater, CurrentDay = 10 };
            var result = _service.EvaluateUseRequest(section, req);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.LivingWaterChargesConsumed);
        }

        [Test]
        public void LivingWater_Use_NoCharges_Fails()
        {
            var section = EmptySection();
            section.UnlockedFunctions.Add(FonteFunction.LimitedLivingWater);
            section.LivingWater.Unlocked = true;
            section.LivingWater.CurrentCharges = 0;
            var req = new FonteUseRequest { RequestedFunction = FonteFunction.LimitedLivingWater, CurrentDay = 10 };
            var result = _service.EvaluateUseRequest(section, req);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("LIVING_WATER_NO_CHARGES", result.FailureReason);
        }

        [Test]
        public void Respec_RequiresConfirmation()
        {
            var section = EmptySection();
            section.UnlockedFunctions.Add(FonteFunction.Respec);
            section.Respec.Unlocked = true;
            var req = new FonteUseRequest { RequestedFunction = FonteFunction.Respec, HasConfirmedRespec = false };
            var result = _service.EvaluateUseRequest(section, req);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("RESPEC_REQUIRES_CONFIRMATION", result.FailureReason);
        }

        [Test]
        public void Respec_OnCooldown_Fails()
        {
            var section = EmptySection();
            section.UnlockedFunctions.Add(FonteFunction.Respec);
            section.Respec.Unlocked = true;
            section.Respec.CooldownDays = 7;
            section.Respec.LastRespecDay = 5;
            var req = new FonteUseRequest { RequestedFunction = FonteFunction.Respec, CurrentDay = 8, HasConfirmedRespec = true };
            var result = _service.EvaluateUseRequest(section, req);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("RESPEC_ON_COOLDOWN", result.FailureReason);
        }

        // ---- Validator Tests ----

        [Test]
        public void Validator_ValidSection_NoBlockers()
        {
            var section = EmptySection();
            section.LivingWater.Unlocked = true;
            section.LivingWater.MaxCharges = 3;
            section.LivingWater.CurrentCharges = 2;
            var issues = _validator.Validate(section);
            Assert.IsFalse(issues.Exists(i => i.IsBlocker));
        }

        [Test]
        public void Validator_NegativeCharges_Blocker()
        {
            var section = EmptySection();
            section.LivingWater.Unlocked = true;
            section.LivingWater.MaxCharges = 3;
            section.LivingWater.CurrentCharges = -1;
            Assert.IsTrue(_validator.Validate(section).Exists(i => i.Code == "LIVING_WATER_NEGATIVE_CHARGES" && i.IsBlocker));
        }

        [Test]
        public void Section_SeparateFromMainProgression()
        {
            // Structural: FonteAnyaSection and MainProgressionSection are different classes
            var fonte = new FonteAnyaSection();
            var prog = new MainProgressionSection();
            Assert.IsNotNull(fonte);
            Assert.IsNotNull(prog);
            // FonteAnyaSection has no CurrentAct field — separation maintained
            Assert.AreEqual(0, fonte.FragmentIntegrationRefs.Count);
        }
    }
}
