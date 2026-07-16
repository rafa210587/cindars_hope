using System;
using CindarsHope.Core.Events;
using CindarsHope.NPC;
using CindarsHope.Quests.Runtime;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.NPC
{
    /// <summary>
    /// Caracterizacao de NpcQuestInteractionPolicy.ResolveMode: reproduz o comportamento anterior de
    /// NpcController.ResolveQuestInteractionMode (spec_arch_npc_quest_boundary_residual_v1, criterio 14.1),
    /// agora exercitado via o fake de IQuestInteractionQuery em vez do QuestService concreto.
    /// </summary>
    [TestFixture]
    public sealed class NpcQuestInteractionPolicyTests
    {
        private sealed class FakeQuestInteractionQuery : IQuestInteractionQuery
        {
            public Func<string, bool> CanTurnInResult = _ => false;
            public Func<string, bool> HasQuestStateResult = _ => false;

            public bool CanTurnIn(string questId) => CanTurnInResult(questId);

            public bool HasQuestState(string questId) => HasQuestStateResult(questId);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void ResolveMode_ReturnsNoQuest_ForBlankQuestId(string questId)
        {
            var query = new FakeQuestInteractionQuery();

            var mode = NpcQuestInteractionPolicy.ResolveMode(questId, query);

            Assert.That(mode, Is.EqualTo(QuestGiverInteractionMode.NoQuest));
        }

        [Test]
        public void ResolveMode_ReturnsOffer_WhenQueryIsNull()
        {
            var mode = NpcQuestInteractionPolicy.ResolveMode("quest_first_supplies_for_cindar", null);

            Assert.That(mode, Is.EqualTo(QuestGiverInteractionMode.Offer));
        }

        [Test]
        public void ResolveMode_ReturnsTurnIn_WhenCanTurnInIsTrue()
        {
            var query = new FakeQuestInteractionQuery
            {
                CanTurnInResult = _ => true,
                HasQuestStateResult = _ => true
            };

            var mode = NpcQuestInteractionPolicy.ResolveMode("quest_first_supplies_for_cindar", query);

            Assert.That(mode, Is.EqualTo(QuestGiverInteractionMode.TurnIn));
        }

        [Test]
        public void ResolveMode_ReturnsOffer_WhenCannotTurnInAndNoQuestStateExists()
        {
            var query = new FakeQuestInteractionQuery
            {
                CanTurnInResult = _ => false,
                HasQuestStateResult = _ => false
            };

            var mode = NpcQuestInteractionPolicy.ResolveMode("quest_first_supplies_for_cindar", query);

            Assert.That(mode, Is.EqualTo(QuestGiverInteractionMode.Offer));
        }

        [Test]
        public void ResolveMode_ReturnsNoQuest_WhenCannotTurnInButQuestStateExists()
        {
            var query = new FakeQuestInteractionQuery
            {
                CanTurnInResult = _ => false,
                HasQuestStateResult = _ => true
            };

            var mode = NpcQuestInteractionPolicy.ResolveMode("quest_first_supplies_for_cindar", query);

            Assert.That(mode, Is.EqualTo(QuestGiverInteractionMode.NoQuest));
        }

        [Test]
        public void ResolveMode_PrefersCanTurnIn_OverHasQuestState_WhenBothTrue()
        {
            var query = new FakeQuestInteractionQuery
            {
                CanTurnInResult = _ => true,
                HasQuestStateResult = _ => true
            };

            var mode = NpcQuestInteractionPolicy.ResolveMode("quest_first_supplies_for_cindar", query);

            Assert.That(mode, Is.EqualTo(QuestGiverInteractionMode.TurnIn));
        }
    }
}
