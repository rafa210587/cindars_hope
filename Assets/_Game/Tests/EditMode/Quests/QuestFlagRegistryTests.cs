using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Quests.Flags;

namespace CindarsHope.Tests.EditMode.Quests
{
    [TestFixture]
    public class QuestFlagRegistryTests
    {
        private QuestFlagRegistry _registry;
        private QuestFlagService _service;
        private QuestFlagValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _registry = new QuestFlagRegistry();
            _service = new QuestFlagService(_registry);
            _validator = new QuestFlagValidator();
        }

        private QuestFlagDefinition StoryFlag(string id, QuestFlagVisibility vis = QuestFlagVisibility.PublicKnown) => new QuestFlagDefinition
        {
            FlagId = id, FlagType = QuestFlagType.Boolean, Scope = QuestFlagScope.GlobalStory,
            Visibility = vis, OwnerSystem = "QuestSystem", Persists = true,
            CanBeUsedByConditions = true, CanBeGrantedByReward = true
        };

        [Test]
        public void Registry_Register_CanQuery()
        {
            _registry.Register(StoryFlag("flag_cindar_diary_read"));
            var result = _registry.Query(new QuestFlagRegistryQuery { FlagId = "flag_cindar_diary_read", RequestingSystem = "QuestCondition" });
            Assert.IsTrue(result.Found);
            Assert.AreEqual("flag_cindar_diary_read", result.Definition.FlagId);
        }

        [Test]
        public void Registry_UnknownFlag_NotFound()
        {
            var result = _registry.Query(new QuestFlagRegistryQuery { FlagId = "flag_unknown", RequestingSystem = "QuestCondition" });
            Assert.IsFalse(result.Found);
        }

        [Test]
        public void Registry_HiddenFlag_BlockedWithoutAllowHidden()
        {
            _registry.Register(StoryFlag("flag_secret", QuestFlagVisibility.HiddenInternal));
            var result = _registry.Query(new QuestFlagRegistryQuery { FlagId = "flag_secret", RequestingSystem = "UI", AllowHidden = false });
            Assert.IsFalse(result.Found);
        }

        [Test]
        public void Registry_HiddenFlag_AllowedWithAllowHidden()
        {
            _registry.Register(StoryFlag("flag_secret", QuestFlagVisibility.HiddenInternal));
            var result = _registry.Query(new QuestFlagRegistryQuery { FlagId = "flag_secret", RequestingSystem = "InternalSystem", AllowHidden = true });
            Assert.IsTrue(result.Found);
        }

        [Test]
        public void Registry_TypeMismatch_NotFound()
        {
            _registry.Register(StoryFlag("flag_bool"));
            var result = _registry.Query(new QuestFlagRegistryQuery { FlagId = "flag_bool", ExpectedType = QuestFlagType.Integer, RequestingSystem = "Test" });
            Assert.IsFalse(result.Found);
            Assert.IsTrue(result.FailReason.Contains("type mismatch"));
        }

        [Test]
        public void Registry_DeprecatedFlag_NotFound()
        {
            var def = StoryFlag("flag_old");
            def.IsDeprecated = true;
            def.ReplacementFlagId = "flag_new";
            _registry.Register(def);
            var result = _registry.Query(new QuestFlagRegistryQuery { FlagId = "flag_old", RequestingSystem = "Test" });
            Assert.IsFalse(result.Found);
        }

        [Test]
        public void Service_SetFlag_Idempotent()
        {
            _registry.Register(StoryFlag("flag_vaelrion_introduced", QuestFlagVisibility.PlayerKnownAfterDiscovery) );
            _service.SetFlag("flag_vaelrion_introduced", "true", "QuestSystem");
            var result = _service.SetFlag("flag_vaelrion_introduced", "true", "QuestSystem");
            Assert.IsTrue(result.Success);
            Assert.IsTrue(result.WasAlreadySet);
        }

        [Test]
        public void Service_ClearFlag_Idempotent()
        {
            _registry.Register(StoryFlag("flag_nyx_cult_rumor_known"));
            _service.SetFlag("flag_nyx_cult_rumor_known", "true", "QuestSystem");
            _service.ClearFlag("flag_nyx_cult_rumor_known", "QuestSystem");
            var result = _service.ClearFlag("flag_nyx_cult_rumor_known", "QuestSystem");
            Assert.IsTrue(result.Success);
            Assert.IsFalse(_service.IsSet("flag_nyx_cult_rumor_known"));
        }

        [Test]
        public void Service_SetFlag_AuthorizedSetter()
        {
            var def = StoryFlag("flag_town_knows_anya_rumor");
            def.AllowedSetters = new List<string> { "DialogueSystem" };
            _registry.Register(def);
            var fail = _service.SetFlag("flag_town_knows_anya_rumor", "true", "UnauthorizedSystem");
            Assert.IsFalse(fail.Success);
            var ok = _service.SetFlag("flag_town_knows_anya_rumor", "true", "DialogueSystem");
            Assert.IsTrue(ok.Success);
        }

        [Test]
        public void Service_GrantFlag_Idempotent_TrackGrantedIds()
        {
            _registry.Register(StoryFlag("flag_shop_night_unlocked"));
            _service.GrantFlag("flag_shop_night_unlocked", "RewardEngine");
            _service.GrantFlag("flag_shop_night_unlocked", "RewardEngine");
            Assert.IsTrue(_service.IsSet("flag_shop_night_unlocked"));
            Assert.IsTrue(_service.WasGranted("flag_shop_night_unlocked"));
        }

        [Test]
        public void Service_HiddenFlag_NotVisibleToUi()
        {
            _registry.Register(StoryFlag("flag_archivist_name_known", QuestFlagVisibility.SpoilerLocked));
            Assert.IsFalse(_service.IsVisibleToUi("flag_archivist_name_known"));
        }

        [Test]
        public void FonteReferenceOnly_IsReferenceOnly()
        {
            var def = new QuestFlagDefinition { FlagId = "flag_fonte_respawn_unlocked", Scope = QuestFlagScope.FonteReferenceOnly };
            Assert.IsTrue(def.IsReferenceOnly());
        }

        [Test]
        public void MainProgressionReferenceOnly_IsReferenceOnly()
        {
            var def = new QuestFlagDefinition { FlagId = "flag_fragment_water_protected", Scope = QuestFlagScope.MainProgressionReferenceOnly };
            Assert.IsTrue(def.IsReferenceOnly());
        }

        [Test]
        public void Validator_FonteOwnedByFonte_IsBlocker()
        {
            var def = new QuestFlagDefinition
            {
                FlagId = "flag_fonte_test", Scope = QuestFlagScope.FonteReferenceOnly,
                OwnerSystem = "FonteAnyaSystem", Visibility = QuestFlagVisibility.PublicKnown
            };
            var issues = _validator.Validate(def);
            Assert.IsTrue(issues.Exists(i => i.Code == "FLAG_FONTE_SCOPE_OWNER_CONFLICT" && i.IsBlocker));
        }

        [Test]
        public void Validator_DebugFlagPersists_Warning()
        {
            var def = new QuestFlagDefinition
            {
                FlagId = "flag_debug_cave_skip", Scope = QuestFlagScope.Debug,
                Visibility = QuestFlagVisibility.DebugOnly, OwnerSystem = "Debug", Persists = true
            };
            var issues = _validator.Validate(def);
            Assert.IsTrue(issues.Exists(i => i.Code == "FLAG_DEBUG_PERSISTS"));
        }
    }
}
