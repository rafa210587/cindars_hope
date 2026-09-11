using System;
using CindarsHope.Editor.SceneCreation;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.City
{
    /// <summary>Pure evidence-comparator regressions; no scene objects or claim of applied Unity scaling.</summary>
    [TestFixture]
    public sealed class TownKeyartActorScaleTests
    {
        [Test]
        public void ComparePhysics_AllowsNpcMovementWithoutChangingItsRelativeShape()
        {
            var before = Actor(); var after = Actor();
            after.position = new Vector3(37, -25, 0);
            Assert.AreEqual(0, TownKeyartActorScale.ComparePhysics(before, after));
        }

        [Test]
        public void ComparePhysics_RejectsCornerGrowthEvenWhenCenterIsUnchanged()
        {
            var before = Actor(); var after = Actor();
            after.shapes[0].cornersRelativeToActor[2] += new Vector2(.02f, 0);
            Assert.Throws<InvalidOperationException>(() => TownKeyartActorScale.ComparePhysics(before, after));
        }

        [Test]
        public void ComparePhysics_RejectsOffsetGrowthWithUnchangedBoxSize()
        {
            var before = Actor(); var after = Actor();
            after.shapes[0].centerRelativeToActor += new Vector2(0, .02f);
            Assert.Throws<InvalidOperationException>(() => TownKeyartActorScale.ComparePhysics(before, after));
        }

        [Test]
        public void ComparePhysics_RejectsInteractionRadiusGrowth()
        {
            var before = Actor(); var after = Actor();
            after.shapes[1].radius += .02f;
            Assert.Throws<InvalidOperationException>(() => TownKeyartActorScale.ComparePhysics(before, after));
        }

        [Test]
        public void ComparePhysics_RejectsSolidChangedIntoTrigger()
        {
            var before = Actor(); var after = Actor(); after.shapes[0].trigger = true;
            Assert.Throws<InvalidOperationException>(() => TownKeyartActorScale.ComparePhysics(before, after));
        }

        [Test]
        public void ComparePhysics_RejectsDisabledOrRemovedCollider()
        {
            var before = Actor(); var disabled = Actor(); disabled.shapes[0].enabled = false;
            Assert.Throws<InvalidOperationException>(() => TownKeyartActorScale.ComparePhysics(before, disabled));
            var removed = Actor(); removed.shapes = new[] { removed.shapes[0] };
            Assert.Throws<InvalidOperationException>(() => TownKeyartActorScale.ComparePhysics(before, removed));
        }

        [Test]
        public void ComparePhysics_RejectsEmptyOrDifferentActorEvidence()
        {
            var empty = Actor(); empty.shapes = Array.Empty<TownKeyartActorScale.PhysicalShape>();
            Assert.Throws<InvalidOperationException>(() => TownKeyartActorScale.ComparePhysics(empty, empty));
            var other = Actor(); other.id = "different_actor";
            Assert.Throws<InvalidOperationException>(() => TownKeyartActorScale.ComparePhysics(Actor(), other));
        }

        [TestCase(.009f, true)]
        [TestCase(.011f, false)]
        public void ComparePhysics_UsesDocumentedWorldTolerance(float delta, bool accepted)
        {
            var before = Actor(); var after = Actor(); after.shapes[0].centerRelativeToActor.x += delta;
            if (accepted) Assert.AreEqual(delta, TownKeyartActorScale.ComparePhysics(before, after), .00001f);
            else Assert.Throws<InvalidOperationException>(() => TownKeyartActorScale.ComparePhysics(before, after));
        }

        private static TownKeyartActorScale.ActorState Actor() => new TownKeyartActorScale.ActorState
        {
            id = "actor", position = Vector3.zero,
            shapes = new[]
            {
                new TownKeyartActorScale.PhysicalShape
                {
                    path = "SolidBody:BoxCollider2D:0", type = "BoxCollider2D", rigidbodyPath = ".",
                    active = true, enabled = true, trigger = false, layer = 8,
                    centerRelativeToActor = new Vector2(0, .55f),
                    cornersRelativeToActor = new[] { new Vector2(-.275f, .35f), new Vector2(.275f, .35f), new Vector2(.275f, .75f), new Vector2(-.275f, .75f) }
                },
                new TownKeyartActorScale.PhysicalShape
                {
                    path = "InteractionTrigger:CircleCollider2D:0", type = "CircleCollider2D", rigidbodyPath = ".",
                    active = true, enabled = true, trigger = true, layer = 8, radius = 1.1f,
                    centerRelativeToActor = Vector2.zero, cornersRelativeToActor = Array.Empty<Vector2>()
                }
            }
        };
    }
}
