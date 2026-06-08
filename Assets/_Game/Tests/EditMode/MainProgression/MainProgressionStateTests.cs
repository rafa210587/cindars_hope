using NUnit.Framework;
using CindarsHope.MainProgression;

namespace CindarsHope.Tests.EditMode.MainProgression
{
    [TestFixture]
    public class MainProgressionStateTests
    {
        private MainProgressionService _service;
        private MainProgressionValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _service = new MainProgressionService();
            _validator = new MainProgressionValidator();
        }

        private MainProgressionSection EmptySection() => new MainProgressionSection { Version = 1 };

        private MainProgressionSection SectionWithFragment(MainFragmentType type)
        {
            var section = EmptySection();
            _service.TryIntegrateFragment(section, type, 10, false);
            return section;
        }

        // ---- Fragment Integration Tests ----

        [Test]
        public void Water_FirstFragment_IntegratesSuccessfully()
        {
            var section = EmptySection();
            var result = _service.TryIntegrateFragment(section, MainFragmentType.Water, 10, false);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(section.IsFragmentIntegrated(MainFragmentType.Water));
        }

        [Test]
        public void Memory_WithoutWater_Fails()
        {
            var section = EmptySection();
            var result = _service.TryIntegrateFragment(section, MainFragmentType.Memory, 10, false);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailureReason.Contains("FRAGMENT_OUT_OF_ORDER"));
        }

        [Test]
        public void Memory_AfterWater_Succeeds()
        {
            var section = SectionWithFragment(MainFragmentType.Water);
            var result = _service.TryIntegrateFragment(section, MainFragmentType.Memory, 15, false);
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void Life_WithoutMemory_Fails()
        {
            var section = SectionWithFragment(MainFragmentType.Water);
            var result = _service.TryIntegrateFragment(section, MainFragmentType.Life, 10, false);
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void AlreadyIntegrated_Idempotent()
        {
            var section = EmptySection();
            _service.TryIntegrateFragment(section, MainFragmentType.Water, 10, false);
            var result = _service.TryIntegrateFragment(section, MainFragmentType.Water, 11, false);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.AlreadyIntegrated);
        }

        [Test]
        public void AlreadyGranted_Flag_Idempotent()
        {
            var section = EmptySection();
            var result = _service.TryIntegrateFragment(section, MainFragmentType.Water, 10, alreadyGranted: true);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.AlreadyIntegrated);
        }

        // ---- Act Transition Tests ----

        [Test]
        public void Act1_Start_NoPrereqs()
        {
            var section = EmptySection();
            var result = _service.TryTransitionAct(section, MainAct.Act1_FonteAndForgetfulness);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(MainAct.Act1_FonteAndForgetfulness, section.CurrentAct);
        }

        [Test]
        public void Act2_WithoutWater_Fails()
        {
            var section = EmptySection();
            section.CurrentAct = MainAct.Act1_FonteAndForgetfulness;
            var result = _service.TryTransitionAct(section, MainAct.Act2_CindarAndMemoryArc);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailureReason.Contains("ACT2_REQUIRES_WATER"));
        }

        [Test]
        public void Act2_AfterWaterIntegrated_Succeeds()
        {
            var section = EmptySection();
            section.CurrentAct = MainAct.Act1_FonteAndForgetfulness;
            _service.TryIntegrateFragment(section, MainFragmentType.Water, 10, false);
            var result = _service.TryTransitionAct(section, MainAct.Act2_CindarAndMemoryArc);
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void Act_Regression_Fails()
        {
            var section = EmptySection();
            section.CurrentAct = MainAct.Act3_CultBlackStoneAndLife;
            var result = _service.TryTransitionAct(section, MainAct.Act2_CindarAndMemoryArc);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailureReason.Contains("ACT_REGRESSION"));
        }

        [Test]
        public void SameAct_Idempotent()
        {
            var section = EmptySection();
            section.CurrentAct = MainAct.Act2_CindarAndMemoryArc;
            var result = _service.TryTransitionAct(section, MainAct.Act2_CindarAndMemoryArc);
            Assert.IsTrue(result.Success);
        }

        // ---- Spoiler Reveal Tests ----

        [Test]
        public void CindarReveal_HiddenInAct1()
        {
            var section = EmptySection();
            section.CurrentAct = MainAct.Act1_FonteAndForgetfulness;
            Assert.IsFalse(_service.IsRevealAllowed("Cindar_FullIdentity", section));
        }

        [Test]
        public void CindarReveal_AllowedFromAct2()
        {
            var section = EmptySection();
            section.CurrentAct = MainAct.Act2_CindarAndMemoryArc;
            Assert.IsTrue(_service.IsRevealAllowed("Cindar_FullIdentity", section));
        }

        [Test]
        public void Archivist_HiddenUntilAct4()
        {
            var section = EmptySection();
            section.CurrentAct = MainAct.Act3_CultBlackStoneAndLife;
            Assert.IsFalse(_service.IsRevealAllowed("Archivist_Final", section));
            section.CurrentAct = MainAct.Act4_Level100101AndHope;
            Assert.IsTrue(_service.IsRevealAllowed("Archivist_Final", section));
        }

        // ---- Validator Tests ----

        [Test]
        public void Validator_ValidSection_NoBlockers()
        {
            var section = EmptySection();
            _service.TryIntegrateFragment(section, MainFragmentType.Water, 10, false);
            _service.TryTransitionAct(section, MainAct.Act1_FonteAndForgetfulness);
            var issues = _validator.Validate(section);
            Assert.IsFalse(issues.Exists(i => i.IsBlocker));
        }

        [Test]
        public void Validator_MemoryWithoutWater_Blocker()
        {
            var section = EmptySection();
            section.FragmentStates.Add(new FragmentStateRecord
                { FragmentType = MainFragmentType.Memory, AcquisitionState = FragmentAcquisitionState.Integrated });
            var issues = _validator.Validate(section);
            Assert.IsTrue(issues.Exists(i => i.Code == "FRAGMENT_ORDER_MEMORY_WITHOUT_WATER" && i.IsBlocker));
        }

        [Test]
        public void SpoilerStage_DerivesFromAct()
        {
            var section = EmptySection();
            section.CurrentAct = MainAct.Act1_FonteAndForgetfulness;
            Assert.AreEqual(StorySpoilerStage.Act1_Intro, section.GetCurrentSpoilerStage());
            section.CurrentAct = MainAct.Act4_Level100101AndHope;
            Assert.AreEqual(StorySpoilerStage.Act4_FinalArcAndLevel101, section.GetCurrentSpoilerStage());
        }

        [Test]
        public void Section_SeparateFromQuestState()
        {
            // Structural test: MainProgressionSection has no QuestState fields
            var section = new MainProgressionSection();
            // If this compiles and has no QuestStates field, the separation is maintained
            Assert.IsNull(section.CurrentMainQuestId); // optional string, null is expected
            Assert.AreEqual(0, section.FragmentStates.Count);
        }
    }
}
