using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.NPC;

namespace CindarsHope.Tests.EditMode.City
{
    [TestFixture]
    public class NpcDefinitionValidationTests
    {
        private NpcDefinitionValidator _validator;

        [SetUp]
        public void SetUp() => _validator = new NpcDefinitionValidator();

        private NpcDefinition ValidNpc(string id = "npc_liora") => new NpcDefinition
        {
            NpcId = id,
            DisplayName = "Liora",
            GameplayClassPrimary = FunctionalGameplayClass.Comerciante,
            RelationshipStatus = RelationshipStatus.Single,
            RomanceEligibility = RomanceEligibility.EligibleAnyGender
        };

        [Test]
        public void Valid_Npc_NoIssues()
        {
            var issues = _validator.Validate(ValidNpc());
            Assert.AreEqual(0, issues.Count);
        }

        [Test]
        public void Npc_NullIsBlocker()
        {
            var issues = _validator.Validate(null);
            Assert.IsTrue(issues.Exists(i => i.Code == "NPC_NULL" && i.IsBlocker));
        }

        [Test]
        public void Npc_NoId_IsBlocker()
        {
            var npc = ValidNpc();
            npc.NpcId = null;
            var issues = _validator.Validate(npc);
            Assert.IsTrue(issues.Exists(i => i.Code == "NPC_NO_ID" && i.IsBlocker));
        }

        [Test]
        public void Npc_DnDClassInRoleTags_IsBlocker()
        {
            var npc = ValidNpc();
            npc.RoleTags.Add("Cleric");
            var issues = _validator.Validate(npc);
            Assert.IsTrue(issues.Exists(i => i.Code == "NPC_DND_CLASS_IN_TAGS" && i.IsBlocker));
        }

        [Test]
        public void Npc_FunctionalClass_NotBlocker()
        {
            var npc = ValidNpc();
            npc.RoleTags.Add("Guardiao");  // valid functional tag
            var issues = _validator.Validate(npc);
            Assert.IsFalse(issues.Exists(i => i.Code == "NPC_DND_CLASS_IN_TAGS"));
        }

        [Test]
        public void Npc_MarriedAndRomanceEligible_IsBlocker()
        {
            var npc = ValidNpc();
            npc.RelationshipStatus = RelationshipStatus.MarriedToNpc;
            npc.SpouseNpcId = "npc_other";
            npc.RomanceEligibility = RomanceEligibility.EligibleAnyGender;
            var issues = _validator.Validate(npc);
            Assert.IsTrue(issues.Exists(i => i.Code == "NPC_MARRIED_ROMANCE_ELIGIBLE" && i.IsBlocker));
        }

        [Test]
        public void Npc_MarriedNoSpouse_IsBlocker()
        {
            var npc = ValidNpc();
            npc.RelationshipStatus = RelationshipStatus.MarriedToNpc;
            npc.RomanceEligibility = RomanceEligibility.NotEligible;
            var issues = _validator.Validate(npc);
            Assert.IsTrue(issues.Exists(i => i.Code == "NPC_MARRIED_NO_SPOUSE" && i.IsBlocker));
        }

        [Test]
        public void Npc_TooYoungWithMarriage_IsBlocker()
        {
            var npc = ValidNpc();
            npc.RelationshipStatus = RelationshipStatus.TooYoungOrNarrativelyBlocked;
            npc.MarriageEligibility = MarriageEligibility.EligibleAfterQuestline;
            var issues = _validator.Validate(npc);
            Assert.IsTrue(issues.Exists(i => i.Code == "NPC_TOO_YOUNG_MARRIAGE" && i.IsBlocker));
        }

        [Test]
        public void Npc_AnyaTempleServiceTag_IsBlocker()
        {
            var npc = ValidNpc();
            npc.ServiceTags.Add("AnyaTemple");
            var issues = _validator.Validate(npc);
            Assert.IsTrue(issues.Exists(i => i.Code == "NPC_ANYA_ACTIVE_TEMPLE" && i.IsBlocker));
        }

        [Test]
        public void Npc_PrivateAnyaSympathy_IsAllowed()
        {
            var npc = ValidNpc();
            npc.ReligionProfile.HasPrivateAnyaSympathy = true;
            var issues = _validator.Validate(npc);
            Assert.AreEqual(0, issues.Count);
        }

        [Test]
        public void NpcStats_HasNoBreathField()
        {
            var stats = new NpcStats();
            var properties = typeof(NpcStats).GetProperties();
            foreach (var p in properties)
            {
                var name = p.Name.ToLower();
                Assert.IsFalse(name.Contains("folego") || name.Contains("breath") || name == "br",
                    $"NpcStats must not have Folego/Breath/BR field — found: {p.Name}");
            }
        }

        [Test]
        public void Roster_DuplicateId_IsBlocker()
        {
            var roster = new List<NpcDefinition>
            {
                ValidNpc("npc_liora"),
                ValidNpc("npc_liora")  // duplicate
            };
            var issues = _validator.ValidateRoster(roster);
            Assert.IsTrue(issues.Exists(i => i.Code == "NPC_DUPLICATE_ID" && i.IsBlocker));
        }

        [Test]
        public void Roster_CanonRosterIds_AllUnique()
        {
            var canonIds = new[]
            {
                "npc_corvus", "npc_mara", "npc_sylveth", "npc_brumdar", "npc_nimble",
                "npc_gurd", "npc_hund", "npc_ozzra", "npc_gruta", "npc_zrix",
                "npc_yael", "npc_thalindra", "npc_dagna", "npc_pip", "npc_alaric",
                "npc_mirela", "npc_renko", "npc_eiran", "npc_liora", "npc_orlan",
                "npc_savra", "npc_tovin", "npc_maelor"
            };
            var ids = new HashSet<string>(canonIds);
            Assert.AreEqual(canonIds.Length, ids.Count, "Canonical roster must have all unique NpcIds");
        }

        [Test]
        public void FunctionalGameplayClass_DoesNotContainDnDNames()
        {
            var classNames = System.Enum.GetNames(typeof(FunctionalGameplayClass));
            var forbidden = new[] { "Cleric", "Paladin", "Fighter", "Wizard", "Rogue", "Bard", "Druid", "Ranger", "Warlock", "Sorcerer", "Monk", "Barbarian" };
            foreach (var name in classNames)
                Assert.IsFalse(System.Array.Exists(forbidden, f => f.Equals(name, System.StringComparison.OrdinalIgnoreCase)),
                    $"FunctionalGameplayClass must not include D&D class: {name}");
        }
    }
}
