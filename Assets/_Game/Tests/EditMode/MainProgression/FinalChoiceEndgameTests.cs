using NUnit.Framework;
using CindarsHope.MainProgression;
using CindarsHope.Fonte;

namespace CindarsHope.Tests.EditMode.MainProgression
{
    [TestFixture]
    public class FinalChoiceEndgameTests
    {
        private FinalChoiceService _service;
        private MainProgressionService _progressionService;

        [SetUp]
        public void SetUp()
        {
            _service = new FinalChoiceService();
            _progressionService = new MainProgressionService();
        }

        private MainProgressionSection ReadyProgression()
        {
            var prog = new MainProgressionSection { Version = 1 };
            // Integrate all 4 fragments in order
            _progressionService.TryIntegrateFragment(prog, MainFragmentType.Water, 10, false);
            _progressionService.TryIntegrateFragment(prog, MainFragmentType.Memory, 15, false);
            _progressionService.TryIntegrateFragment(prog, MainFragmentType.Life, 20, false);
            _progressionService.TryIntegrateFragment(prog, MainFragmentType.Hope, 30, false);
            prog.Level100GateState = Level100GateStatus.Entered;
            prog.Level101AccessState = Level101AccessStatus.Unlocked;
            prog.FinalChoiceState = FinalChoiceStatus.Available;
            prog.CurrentAct = MainAct.Act4_Level100101AndHope;
            return prog;
        }

        private FonteAnyaSection ReadyFonte() => new FonteAnyaSection { Version = 1 };

        private FinalChoiceRequest MakeRequest(FinalChoiceType choice, bool preview = false) =>
            new FinalChoiceRequest
            {
                ChoiceType = choice, ActorId = "player", Day = 100,
                RequiredConfirmationToken = "FINAL_CHOICE_CONFIRMED",
                PreviewOnly = preview
            };

        // ---- Level100/101 Gate Tests ----

        [Test]
        public void CanUnlockLevel101_WithLifeFragment_AndLevel100Entered()
        {
            var prog = ReadyProgression();
            Assert.IsTrue(_service.CanUnlockLevel101(prog));
        }

        [Test]
        public void CannotUnlockLevel101_WithoutLevel100Entered()
        {
            var prog = ReadyProgression();
            prog.Level100GateState = Level100GateStatus.Available; // not Entered
            Assert.IsFalse(_service.CanUnlockLevel101(prog));
        }

        // ---- Final Choice Tests ----

        [Test]
        public void FinalChoice_NotAvailable_Fails()
        {
            var prog = ReadyProgression();
            prog.FinalChoiceState = FinalChoiceStatus.Unavailable;
            var result = _service.EvaluateFinalChoice(prog, ReadyFonte(), MakeRequest(FinalChoiceType.Protect));
            Assert.IsFalse(result.Success);
            Assert.AreEqual("FINAL_CHOICE_NOT_AVAILABLE", result.FailureReason);
        }

        [Test]
        public void FinalChoice_WithoutHopeFragment_Fails()
        {
            var prog = new MainProgressionSection { Version = 1 };
            _progressionService.TryIntegrateFragment(prog, MainFragmentType.Water, 10, false);
            prog.Level101AccessState = Level101AccessStatus.Unlocked;
            prog.FinalChoiceState = FinalChoiceStatus.Available;
            var result = _service.EvaluateFinalChoice(prog, ReadyFonte(), MakeRequest(FinalChoiceType.Protect));
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.FailureReason.Contains("REQUIRES_HOPE_FRAGMENT"));
        }

        [Test]
        public void FinalChoice_WithoutStrongConfirmation_Fails()
        {
            var prog = ReadyProgression();
            var req = new FinalChoiceRequest
            {
                ChoiceType = FinalChoiceType.Protect, Day = 100,
                RequiredConfirmationToken = "wrong_token"
            };
            var result = _service.EvaluateFinalChoice(prog, ReadyFonte(), req);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("FINAL_CHOICE_REQUIRES_STRONG_CONFIRMATION", result.FailureReason);
        }

        [Test]
        public void FinalChoice_PreviewOnly_NoStateChange()
        {
            var prog = ReadyProgression();
            var result = _service.EvaluateFinalChoice(prog, ReadyFonte(), MakeRequest(FinalChoiceType.Protect, preview: true));
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.ChoiceApplied);
            Assert.AreEqual(MainAct.Act4_Level100101AndHope, prog.CurrentAct); // unchanged
        }

        [Test]
        public void FinalChoice_Protect_AppliesCorrectly()
        {
            var prog = ReadyProgression();
            var fonte = ReadyFonte();
            var result = _service.EvaluateFinalChoice(prog, fonte, MakeRequest(FinalChoiceType.Protect));
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.ChoiceApplied);
            Assert.AreEqual("ending_protect", result.EndingEffectProfileId);
            Assert.AreEqual(MainAct.PostGame, prog.CurrentAct);
            Assert.AreEqual(FonteState.FinalizedProtected, fonte.FonteState);
            Assert.AreEqual(FinalChoiceStatus.Resolved, prog.FinalChoiceState);
        }

        [Test]
        public void FinalChoice_Seal_AppliesCorrectly()
        {
            var prog = ReadyProgression();
            var fonte = ReadyFonte();
            var result = _service.EvaluateFinalChoice(prog, fonte, MakeRequest(FinalChoiceType.Seal));
            Assert.IsTrue(result.Success);
            Assert.AreEqual("ending_seal", result.EndingEffectProfileId);
            Assert.AreEqual(FonteState.FinalizedSealed, fonte.FonteState);
        }

        [Test]
        public void FinalChoice_Use_AppliesCorrectly()
        {
            var prog = ReadyProgression();
            var fonte = ReadyFonte();
            var result = _service.EvaluateFinalChoice(prog, fonte, MakeRequest(FinalChoiceType.Use));
            Assert.IsTrue(result.Success);
            Assert.AreEqual("ending_use", result.EndingEffectProfileId);
            Assert.AreEqual(FonteState.FinalizedUsed, fonte.FonteState);
        }

        [Test]
        public void FinalChoice_Idempotent_AlreadyApplied()
        {
            var prog = ReadyProgression();
            _service.EvaluateFinalChoice(prog, ReadyFonte(), MakeRequest(FinalChoiceType.Protect));
            var result = _service.EvaluateFinalChoice(prog, ReadyFonte(), MakeRequest(FinalChoiceType.Seal));
            Assert.IsTrue(result.AlreadyApplied);
        }

        [Test]
        public void FinalChoice_None_Fails()
        {
            var prog = ReadyProgression();
            var result = _service.EvaluateFinalChoice(prog, ReadyFonte(), MakeRequest(FinalChoiceType.None));
            Assert.IsFalse(result.Success);
        }

        // ---- EndingEffectProfile Tests ----

        [Test]
        public void EndingProfile_Protect_HasCorrectPolicies()
        {
            var p = EndingEffectProfile.Protect();
            Assert.AreEqual("RareNatural", p.ManaBloomPolicy);
            Assert.IsTrue(p.RequiresStrongConfirmation);
        }

        [Test]
        public void EndingProfile_Seal_HasMelancholicPolicies()
        {
            var p = EndingEffectProfile.Seal();
            Assert.AreEqual("FinalizedSealed", p.FonteFinalState);
            Assert.AreEqual("StrongSeal", p.CorruptionContainmentPolicy);
        }

        [Test]
        public void ArchivistRevealState_HiddenInAct1()
        {
            // Structural: Archivist enum exists and Hidden is value 0
            Assert.AreEqual(0, (int)ArchivistRevealState.Hidden);
            Assert.AreEqual(7, (int)ArchivistRevealState.Resolved);
        }

        [Test]
        public void FinalChoiceStatus_UnvailableIsDefault()
        {
            var section = new MainProgressionSection();
            Assert.AreEqual(FinalChoiceStatus.Unavailable, section.FinalChoiceState);
        }
    }
}
