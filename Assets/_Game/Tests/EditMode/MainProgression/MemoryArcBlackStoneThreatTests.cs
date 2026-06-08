using System.Linq;
using NUnit.Framework;
using CindarsHope.MainProgression;
using CindarsHope.MainProgression.Threats;

namespace CindarsHope.Tests.EditMode.MainProgression
{
    [TestFixture]
    public class MemoryArcBlackStoneThreatTests
    {
        // ---- Enum existence / value tests ----

        [Test]
        public void MemoryArcState_ValuesExist()
        {
            Assert.AreEqual(0, (int)MemoryArcState.Unknown);
            Assert.AreEqual(7, (int)MemoryArcState.CorruptedByBlackStone);
            Assert.AreEqual(11, (int)MemoryArcState.Used);
        }

        [Test]
        public void BlackStoneState_ValuesExist()
        {
            Assert.AreEqual(0, (int)BlackStoneState.Unknown);
            Assert.AreEqual(7, (int)BlackStoneState.DrainsSoul);
            Assert.AreEqual(13, (int)BlackStoneState.Sealed);
        }

        [Test]
        public void VaelrionArcState_ValuesExist()
        {
            Assert.AreEqual(0, (int)VaelrionArcState.Unknown);
            Assert.AreEqual(8, (int)VaelrionArcState.MergedIntoArchivist);
        }

        [Test]
        public void SethraCultState_ValuesExist()
        {
            Assert.AreEqual(0, (int)SethraCultState.Unknown);
            Assert.AreEqual(4, (int)SethraCultState.SethraRevealed);
        }

        [Test]
        public void CorruptionThreatLevel_FinalConvergenceExists()
        {
            Assert.AreEqual(8, (int)CorruptionThreatLevel.FinalConvergence);
        }

        // ---- Spoiler gate tests ----

        [Test]
        public void Validator_MemoryArcTerm_NotAllowedBeforeAct2()
        {
            var issues = MemoryArcBlackStoneValidator.Validate(
                MemoryArcState.TermDiscovered, BlackStoneState.Unknown,
                VaelrionArcState.Unknown, SethraCultState.Unknown, MainAct.Act1_FonteAndForgetfulness);
            Assert.IsTrue(issues.Any(i => i.Code == "MEMORY_ARC_TERM_SPOILER_BEFORE_ACT2"));
        }

        [Test]
        public void Validator_MemoryArcTerm_AllowedInAct2()
        {
            var issues = MemoryArcBlackStoneValidator.Validate(
                MemoryArcState.TermDiscovered, BlackStoneState.Unknown,
                VaelrionArcState.Unknown, SethraCultState.Unknown, MainAct.Act2_CindarAndMemoryArc);
            Assert.IsFalse(issues.Any(i => i.Code == "MEMORY_ARC_TERM_SPOILER_BEFORE_ACT2"));
        }

        [Test]
        public void Validator_SethraReveal_NotAllowedBeforeAct3()
        {
            var issues = MemoryArcBlackStoneValidator.Validate(
                MemoryArcState.Unknown, BlackStoneState.Unknown,
                VaelrionArcState.Unknown, SethraCultState.SethraRevealed, MainAct.Act2_CindarAndMemoryArc);
            Assert.IsTrue(issues.Any(i => i.Code == "SETHRA_REVEAL_BEFORE_ACT3"));
        }

        [Test]
        public void Validator_SethraReveal_AllowedInAct3()
        {
            var issues = MemoryArcBlackStoneValidator.Validate(
                MemoryArcState.Unknown, BlackStoneState.Unknown,
                VaelrionArcState.Unknown, SethraCultState.SethraRevealed, MainAct.Act3_CultBlackStoneAndLife);
            Assert.IsFalse(issues.Any(i => i.Code == "SETHRA_REVEAL_BEFORE_ACT3"));
        }

        [Test]
        public void Validator_VaelrionAntagonist_NotAllowedBeforeAct3()
        {
            var issues = MemoryArcBlackStoneValidator.Validate(
                MemoryArcState.Unknown, BlackStoneState.Unknown,
                VaelrionArcState.BoundaryCrossed, SethraCultState.Unknown, MainAct.Act2_CindarAndMemoryArc);
            Assert.IsTrue(issues.Any(i => i.Code == "VAELRION_ANTAGONIST_BEFORE_ACT3"));
        }

        [Test]
        public void Validator_BlackStoneSoulDrain_NotAllowedBeforeAct3()
        {
            var issues = MemoryArcBlackStoneValidator.Validate(
                MemoryArcState.Unknown, BlackStoneState.DrainsSoul,
                VaelrionArcState.Unknown, SethraCultState.Unknown, MainAct.Act2_CindarAndMemoryArc);
            Assert.IsTrue(issues.Any(i => i.Code == "BLACKSTONE_SOUL_DRAIN_BEFORE_ACT3"));
        }

        [Test]
        public void Validator_MemoryArcFinal_NotAllowedBeforeAct4()
        {
            var issues = MemoryArcBlackStoneValidator.Validate(
                MemoryArcState.FinalActivated, BlackStoneState.Unknown,
                VaelrionArcState.Unknown, SethraCultState.Unknown, MainAct.Act3_CultBlackStoneAndLife);
            Assert.IsTrue(issues.Any(i => i.Code == "MEMORY_ARC_FINAL_BEFORE_ACT4"));
        }

        [Test]
        public void Validator_CleanState_ProducesNoErrors()
        {
            var issues = MemoryArcBlackStoneValidator.Validate(
                MemoryArcState.Unknown, BlackStoneState.ObservedSmall,
                VaelrionArcState.ArrivedAsScholar, SethraCultState.NightShopRumors, MainAct.Act1_FonteAndForgetfulness);
            Assert.IsTrue(issues.TrueForAll(i => !i.IsError));
        }

        // ---- BlackStone commodity guard tests ----

        [Test]
        public void ExposureRecord_CultistBlackStone_NotCommodity()
        {
            var record = new BlackStoneExposureRecord
            {
                ExposureId = "exp_001", SourceType = BlackStoneSourceType.CultRitual,
                ThreatLevel = CorruptionThreatLevel.MemoryDrain, IsCommodity = true
            };
            var issues = MemoryArcBlackStoneValidator.ValidateExposureRecord(record);
            Assert.IsTrue(issues.Any(i => i.Code == "BLACKSTONE_CULTIST_COMMODITY" || i.Code == "BLACKSTONE_DANGEROUS_COMMODITY"));
        }

        [Test]
        public void ExposureRecord_SmallBlackStone_NotCommodityByDefault()
        {
            var record = new BlackStoneExposureRecord
            {
                ExposureId = "exp_002", SourceType = BlackStoneSourceType.Cave,
                ThreatLevel = CorruptionThreatLevel.Hint, IsCommodity = false
            };
            var issues = MemoryArcBlackStoneValidator.ValidateExposureRecord(record);
            Assert.IsFalse(issues.Any(i => i.IsError));
        }

        // ---- Purification table tests ----

        [Test]
        public void PurificationTable_SoulDrain_CannotBeFullyCured()
        {
            var table = PurificationCompatibility.BuildCanonicalTable();
            var entry = table.First(e => e.ThreatLevel == CorruptionThreatLevel.SoulDrain);
            Assert.IsFalse(entry.CanBeFullyCured);
            Assert.IsFalse(entry.AllowsMinorPurification);
        }

        [Test]
        public void PurificationTable_FinalConvergence_CannotBeAdvancedPurified()
        {
            var table = PurificationCompatibility.BuildCanonicalTable();
            var entry = table.First(e => e.ThreatLevel == CorruptionThreatLevel.FinalConvergence);
            Assert.IsFalse(entry.AllowsAdvancedPurification);
            Assert.IsFalse(entry.CanBeFullyCured);
        }

        [Test]
        public void PurificationTable_LivingWaterCorruption_RequiresLifeFragment()
        {
            var table = PurificationCompatibility.BuildCanonicalTable();
            var entry = table.First(e => e.ThreatLevel == CorruptionThreatLevel.LivingWaterCorruption);
            Assert.IsTrue(entry.RequiresLifeFragment);
            Assert.IsFalse(entry.AllowsMinorPurification);
        }

        [Test]
        public void PurificationTable_Validation_PassesCanonicalTable()
        {
            var table = PurificationCompatibility.BuildCanonicalTable();
            var issues = MemoryArcBlackStoneValidator.ValidatePurificationTable(table);
            Assert.IsFalse(issues.Any(i => i.IsError));
        }

        // ---- CorruptedLivingWater contract test ----

        [Test]
        public void CorruptedLivingWaterRecord_IsNotHealingItem_ByDefault()
        {
            var record = new CorruptedLivingWaterRecord { RecordId = "clw_001" };
            Assert.IsFalse(record.IsHealingItem);
            Assert.IsTrue(record.RequiresAdvancedPurification);
        }
    }
}
