using System.Collections.Generic;
using System.Linq;
using CindarsHope.Combat.Magic;
using CindarsHope.Foundation;
using CindarsHope.Skills;
using NUnit.Framework;
using UnityEditor;

namespace CindarsHope.Tests.EditMode.Skills
{
    [TestFixture]
    public sealed class SpellDisciplineCatalogTests
    {
        private const string SpellDirectory = "Assets/_Game/Data/Combat/Spells";
        private const string SkillActionDirectory = "Assets/_Game/Data/Skills/Actions";

        private static readonly IReadOnlyDictionary<string, SpellDiscipline> ExpectedSpells =
            new Dictionary<string, SpellDiscipline>
            {
                { "anya_echo", SpellDiscipline.Spiritual },
                { "arcane_projectile", SpellDiscipline.Offensive },
                { "senya_rupture", SpellDiscipline.Offensive },
                { "spell_arcane_barrier", SpellDiscipline.Spiritual },
                { "spell_fireball", SpellDiscipline.Offensive },
                { "spell_flame_cone", SpellDiscipline.Offensive },
                { "spell_heal", SpellDiscipline.Spiritual },
                { "spell_ice_nova", SpellDiscipline.Offensive },
                { "spell_ice_spike", SpellDiscipline.Offensive },
                { "spell_minor_heal", SpellDiscipline.Spiritual },
            };

        private static readonly IReadOnlyDictionary<string, SpellDiscipline> ExpectedMagicActions =
            new Dictionary<string, SpellDiscipline>
            {
                { "skill_magic_chama_breve", SpellDiscipline.Offensive },
                { "skill_magic_elemental_ward", SpellDiscipline.None },
                { "skill_magic_fire_spark", SpellDiscipline.Offensive },
                { "skill_magic_ice_bind", SpellDiscipline.Offensive },
                { "skill_magic_lightning_chain", SpellDiscipline.Offensive },
                { "skill_magic_rajada_gelida", SpellDiscipline.Offensive },
                { "skill_magic_slowing_sigils", SpellDiscipline.None },
                { "skill_magic_toxic_cloud", SpellDiscipline.Offensive },
            };

        [Test]
        public void SpellAssets_EveryCanonicalIdHasExplicitDiscipline()
        {
            var actual = AssetDatabase.FindAssets("t:SpellDataSO", new[] { SpellDirectory })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<SpellDataSO>)
                .Where(spell => spell != null)
                .ToDictionary(spell => spell.Id, spell => spell.Discipline);

            CollectionAssert.AreEquivalent(ExpectedSpells.Keys, actual.Keys,
                "The discipline matrix must be reviewed whenever the canonical spell catalog changes.");
            foreach (var expected in ExpectedSpells)
                Assert.That(actual[expected.Key], Is.EqualTo(expected.Value), expected.Key);
        }

        [Test]
        public void DefaultSkillActions_EveryMagicActionHasReviewedDiscipline()
        {
            var actions = DefaultSkillActionCatalog.BuildAll();
            try
            {
                var actual = actions
                    .Where(action => action.SkillActionId.StartsWith("skill_magic_"))
                    .ToDictionary(action => action.SkillActionId, action => action.SpellDiscipline);

                CollectionAssert.AreEquivalent(ExpectedMagicActions.Keys, actual.Keys,
                    "The discipline matrix must be reviewed whenever an active magic skill is added.");
                foreach (var expected in ExpectedMagicActions)
                    Assert.That(actual[expected.Key], Is.EqualTo(expected.Value), expected.Key);
            }
            finally
            {
                foreach (var action in actions)
                    UnityEngine.Object.DestroyImmediate(action);
            }
        }

        [Test]
        public void GeneratedSkillActionAssets_PreserveReviewedDiscipline()
        {
            foreach (var expected in ExpectedMagicActions)
            {
                var path = $"{SkillActionDirectory}/SkillAction_{expected.Key}.asset";
                var action = AssetDatabase.LoadAssetAtPath<SkillActionSO>(path);
                Assert.That(action, Is.Not.Null, path);
                Assert.That(action.SpellDiscipline, Is.EqualTo(expected.Value), expected.Key);
            }
        }
    }
}
