using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Foundation;
using CindarsHope.Skills;
using CindarsHope.MainProgression.Runtime;

namespace CindarsHope.Tests.EditMode.MainProgression
{
    /// <summary>
    /// fable_43 — Cindrathel mirror-build mapper. Deterministic mapping + canonical fallback (CA-3).
    /// </summary>
    [TestFixture]
    public class MirrorBuildMapperTests
    {
        private static MirrorBuildInput Build(
            SkillTreeId tree, bool hasDominant, MirrorWeaponFamily weapon, params string[] skills)
            => new MirrorBuildInput(tree, hasDominant, weapon, skills);

        [Test]
        public void EmptyLoadout_UsesBasicWarriorFallback()
        {
            var cfg = MirrorBuildMapper.Map(Build(SkillTreeId.Melee, false, MirrorWeaponFamily.Unknown));
            Assert.IsTrue(cfg.UsedFallback);
            Assert.AreEqual(MirrorArchetype.Bruiser, cfg.Archetype);
            Assert.AreEqual(MirrorBuildMapper.MinPhases, cfg.PhaseCount);
            Assert.AreEqual(0, cfg.MirroredSkillIds.Count);
        }

        [Test]
        public void Deterministic_SameInputSameOutput()
        {
            var input = Build(SkillTreeId.Magic, true, MirrorWeaponFamily.Staff, "skill_fireball", "skill_frost");
            var a = MirrorBuildMapper.Map(input);
            var b = MirrorBuildMapper.Map(input);
            Assert.AreEqual(a.Archetype, b.Archetype);
            Assert.AreEqual(a.PhaseCount, b.PhaseCount);
            CollectionAssert.AreEqual(a.MirroredSkillIds, b.MirroredSkillIds);
        }

        [Test]
        public void Archetype_FollowsDominantTree()
        {
            Assert.AreEqual(MirrorArchetype.Bruiser, MirrorBuildMapper.Map(Build(SkillTreeId.Melee, true, MirrorWeaponFamily.Unknown)).Archetype);
            Assert.AreEqual(MirrorArchetype.Marksman, MirrorBuildMapper.Map(Build(SkillTreeId.Ranged, true, MirrorWeaponFamily.Unknown)).Archetype);
            Assert.AreEqual(MirrorArchetype.Caster, MirrorBuildMapper.Map(Build(SkillTreeId.Magic, true, MirrorWeaponFamily.Unknown)).Archetype);
            Assert.AreEqual(MirrorArchetype.Skirmisher, MirrorBuildMapper.Map(Build(SkillTreeId.Survival, true, MirrorWeaponFamily.Unknown)).Archetype);
            Assert.AreEqual(MirrorArchetype.Support, MirrorBuildMapper.Map(Build(SkillTreeId.Crafting, true, MirrorWeaponFamily.Unknown)).Archetype);
        }

        [Test]
        public void Archetype_FallsBackToWeaponWhenNoDominant()
        {
            var cfg = MirrorBuildMapper.Map(Build(SkillTreeId.Melee, false, MirrorWeaponFamily.Bow, "skill_shot"));
            Assert.IsFalse(cfg.UsedFallback);
            Assert.AreEqual(MirrorArchetype.Marksman, cfg.Archetype);
        }

        [Test]
        public void PhaseCount_ScalesWithSlottedSkills_ClampedToMax()
        {
            Assert.AreEqual(2, MirrorBuildMapper.Map(Build(SkillTreeId.Melee, true, MirrorWeaponFamily.Sword, "a")).PhaseCount);
            Assert.AreEqual(3, MirrorBuildMapper.Map(Build(SkillTreeId.Melee, true, MirrorWeaponFamily.Sword, "a", "b")).PhaseCount);
            Assert.AreEqual(4, MirrorBuildMapper.Map(Build(SkillTreeId.Melee, true, MirrorWeaponFamily.Sword, "a", "b", "c", "d", "e")).PhaseCount);
        }

        [Test]
        public void MirroredSkills_AreDeduplicatedAndStable()
        {
            var cfg = MirrorBuildMapper.Map(Build(SkillTreeId.Magic, true, MirrorWeaponFamily.Staff, "x", "x", "y", null, ""));
            CollectionAssert.AreEqual(new List<string> { "x", "y" }, new List<string>(cfg.MirroredSkillIds));
        }
    }
}
