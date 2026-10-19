using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Quests;
using CindarsHope.Quests.Log;
using CindarsHope.Quests.Save;

namespace CindarsHope.Tests.EditMode.Quests
{
    [TestFixture]
    public class QuestLogProjectionTests
    {
        private QuestLogProjectionService _service;

        [SetUp]
        public void SetUp() { _service = new QuestLogProjectionService(); }

        private QuestStateRecord ActiveRecord(string questId = "quest_intro") => new QuestStateRecord
        { QuestId = questId, State = (int)QuestStateStatus.Active, Tracked = false, Discovered = true };

        private QuestDefinition SideDef(string id = "quest_intro") => new QuestDefinition
        { QuestId = id, Category = QuestCategory.Side, DisplayNameKey = "key_intro_title", DescriptionKey = "key_intro_desc" };

        [Test]
        public void Projection_SideQuest_Active_HasTitle()
        {
            var vm = _service.ProjectEntry(ActiveRecord(), SideDef(), QuestVisibilityPolicy.Default());
            Assert.IsNotNull(vm);
            Assert.AreEqual("key_intro_title", vm.DisplayTitle);
        }

        [Test]
        public void Projection_Hidden_Unknown_ReturnsNull()
        {
            var rec = ActiveRecord();
            rec.Discovered = false;
            var policy = new QuestVisibilityPolicy { VisibilityPolicyId = "hidden", VisibilityState = QuestVisibilityState.Hidden };
            var vm = _service.ProjectEntry(rec, SideDef(), policy);
            Assert.IsNull(vm);
        }

        [Test]
        public void Projection_HighSpoiler_NoDiscover_MaskedTitle()
        {
            var def = new QuestDefinition { QuestId = "quest_archivist_silence", Category = QuestCategory.Main, DisplayNameKey = "key_archivist", HiddenDisplayNameKey = "???" };
            var rec = ActiveRecord("quest_archivist_silence");
            var vm = _service.ProjectEntry(rec, def, QuestVisibilityPolicy.Default(), playerDiscoveredSpoilerTier: 0);
            Assert.AreEqual("???", vm.DisplayTitle);
            Assert.IsFalse(vm.SpoilerSafe);
        }

        [Test]
        public void Projection_HighSpoiler_WithDiscover_ShowsTitle()
        {
            var def = new QuestDefinition { QuestId = "quest_archivist_silence", Category = QuestCategory.Main, DisplayNameKey = "key_archivist", HiddenDisplayNameKey = "???" };
            var rec = ActiveRecord("quest_archivist_silence");
            var vm = _service.ProjectEntry(rec, def, QuestVisibilityPolicy.Default(), playerDiscoveredSpoilerTier: 3);
            Assert.AreEqual("key_archivist", vm.DisplayTitle);
        }

        [Test]
        public void Projection_Active_CanTrack()
        {
            var vm = _service.ProjectEntry(ActiveRecord(), SideDef(), QuestVisibilityPolicy.Default());
            Assert.IsTrue(vm.CanTrack);
        }

        [Test]
        public void Projection_Completed_CannotTrack()
        {
            var rec = ActiveRecord();
            rec.State = (int)QuestStateStatus.Completed;
            var vm = _service.ProjectEntry(rec, SideDef(), QuestVisibilityPolicy.Default());
            Assert.IsFalse(vm.CanTrack);
        }

        [Test]
        public void Projection_Deadline_Shown()
        {
            var rec = ActiveRecord();
            rec.ExpiresAtDay = 30;
            var vm = _service.ProjectEntry(rec, SideDef(), QuestVisibilityPolicy.Default());
            Assert.AreEqual(30, vm.KnownDeadline);
        }

        [Test]
        public void Projection_Detail_KnownObjectivesOnly()
        {
            var rec = ActiveRecord();
            rec.ObjectiveStates.Add(new QuestObjectiveStateRecord { ObjectiveId = "obj_known", CurrentProgress = 1, RequiredProgress = 3, IsKnown = true });
            rec.ObjectiveStates.Add(new QuestObjectiveStateRecord { ObjectiveId = "obj_hidden", CurrentProgress = 0, RequiredProgress = 1, IsKnown = false });
            rec.KnownObjectiveIds.Add("obj_known");
            var vm = _service.ProjectDetail(rec, SideDef(), QuestVisibilityPolicy.Default());
            Assert.AreEqual(1, vm.CurrentObjectiveRows.Count);
            Assert.AreEqual("obj_known", vm.CurrentObjectiveRows[0].ObjectiveId);
        }

        [Test]
        public void Projection_Detail_KnownHints()
        {
            var rec = ActiveRecord();
            rec.KnownHints.Add("hint_go_to_inn");
            var vm = _service.ProjectDetail(rec, SideDef(), QuestVisibilityPolicy.Default());
            Assert.IsTrue(vm.KnownHints.Contains("hint_go_to_inn"));
        }

        [Test]
        public void ProjectionService_NullRecord_ReturnsNull()
        {
            var vm = _service.ProjectEntry(null, SideDef(), QuestVisibilityPolicy.Default());
            Assert.IsNull(vm);
        }

        [Test]
        public void ProjectAll_SkipsUnknownState()
        {
            var section = new QuestStateSection();
            section.QuestStates.Add(new QuestStateRecord { QuestId = "q_unknown", State = (int)QuestStateStatus.Unknown });
            section.QuestStates.Add(new QuestStateRecord { QuestId = "q_active", State = (int)QuestStateStatus.Active, Discovered = true });
            var defs = new Dictionary<string, QuestDefinition>
            {
                ["q_unknown"] = SideDef("q_unknown"),
                ["q_active"] = SideDef("q_active")
            };
            var result = _service.ProjectAll(section, defs, new Dictionary<string, QuestVisibilityPolicy>());
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("q_active", result[0].QuestId);
        }
    }
}
