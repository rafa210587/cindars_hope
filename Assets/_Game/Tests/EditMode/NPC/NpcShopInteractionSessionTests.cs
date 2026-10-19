using CindarsHope.NPC;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.NPC
{
    public sealed class NpcShopInteractionSessionTests
    {
        [Test]
        public void TryBegin_RejectsReentrantInteraction()
        {
            var session = new NpcShopInteractionSession();

            Assert.That(session.TryBegin(), Is.True);
            Assert.That(session.TryBegin(), Is.False);
            Assert.That(session.IsInteracting, Is.True);
            Assert.That(session.IsClosing, Is.False);
        }

        [Test]
        public void TryBeginClosing_IsIdempotentAndRequiresActiveInteraction()
        {
            var session = new NpcShopInteractionSession();

            Assert.That(session.TryBeginClosing(), Is.False);
            session.TryBegin();
            Assert.That(session.TryBeginClosing(), Is.True);
            Assert.That(session.TryBeginClosing(), Is.False);
            Assert.That(session.IsClosing, Is.True);
        }

        [Test]
        public void QuestOfferHandoff_IsScopedToCurrentInteraction()
        {
            var session = new NpcShopInteractionSession();

            session.MarkQuestOfferHandoff();
            Assert.That(session.IsQuestOfferHandoff, Is.False);

            session.TryBegin();
            session.MarkQuestOfferHandoff();
            Assert.That(session.IsQuestOfferHandoff, Is.True);

            session.Complete();
            Assert.That(session.IsQuestOfferHandoff, Is.False);
            Assert.That(session.IsInteracting, Is.False);
            Assert.That(session.IsClosing, Is.False);
        }
    }
}
