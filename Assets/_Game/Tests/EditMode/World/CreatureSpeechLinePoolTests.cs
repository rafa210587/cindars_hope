using System;
using NUnit.Framework;
using CindarsHope.Enemy.Speech;

namespace CindarsHope.Tests.EditMode.World
{
    /// <summary>
    /// Cobre o pool puro de falas ambiente de criatura (CreatureSpeechLinePool): classificação por
    /// palavra-chave do enemyId, fallback por band, determinismo por seed e que toda fala é curta.
    /// </summary>
    [TestFixture]
    public class CreatureSpeechLinePoolTests
    {
        [Test]
        public void Classify_ByKeyword_PicksExpectedVoice()
        {
            Assert.AreEqual(CreatureSpeechLinePool.Voice.Eldritch, CreatureSpeechLinePool.Classify("abyssal_void_reaver", 7));
            Assert.AreEqual(CreatureSpeechLinePool.Voice.Undead, CreatureSpeechLinePool.Classify("corrupted_bone_knight", 1));
            Assert.AreEqual(CreatureSpeechLinePool.Voice.Vermin, CreatureSpeechLinePool.Classify("cave_mite", 1));
            Assert.AreEqual(CreatureSpeechLinePool.Voice.Goblinoid, CreatureSpeechLinePool.Classify("goblin_grashnaar_scavenger", 1));
            Assert.AreEqual(CreatureSpeechLinePool.Voice.Draconic, CreatureSpeechLinePool.Classify("draconic_ashspitter", 4));
            Assert.AreEqual(CreatureSpeechLinePool.Voice.Construct, CreatureSpeechLinePool.Classify("clockwork_guard", 5));
            Assert.AreEqual(CreatureSpeechLinePool.Voice.Frost, CreatureSpeechLinePool.Classify("frost_wailer", 3));
            Assert.AreEqual(CreatureSpeechLinePool.Voice.Fungal, CreatureSpeechLinePool.Classify("blackroot_sprout", 2));
        }

        [Test]
        public void Classify_UnknownId_FallsBackByBand()
        {
            Assert.AreEqual(CreatureSpeechLinePool.Voice.Stone, CreatureSpeechLinePool.Classify("mystery_thing", 1));
            Assert.AreEqual(CreatureSpeechLinePool.Voice.Eldritch, CreatureSpeechLinePool.Classify("mystery_thing", 7));
            Assert.AreEqual(CreatureSpeechLinePool.Voice.Frost, CreatureSpeechLinePool.Classify("mystery_thing", 3));
        }

        [Test]
        public void TryGetLine_KnownCreatures_ReturnsNonEmptyShortLine()
        {
            string[] ids =
            {
                "abyssal_void_reaver", "corrupted_bone_knight", "cave_mite", "goblin_urudakh_trapper",
                "draconic_void_wyrm", "clockwork_guard", "frost_gnawer", "blackroot_sprout", "cave_bat"
            };

            var rng = new Random(123);
            foreach (var id in ids)
            {
                Assert.IsTrue(CreatureSpeechLinePool.TryGetLine(id, 1, rng, out var line),
                    $"Expected a line for {id}.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(line), $"Line for {id} must be non-empty.");
                Assert.LessOrEqual(line.Length, 32, $"Line for {id} should be short (balão de HQ): '{line}'.");
            }
        }

        [Test]
        public void TryGetLine_IsDeterministic_ForSameSeed()
        {
            var a = new Random(777);
            var b = new Random(777);
            for (int i = 0; i < 20; i++)
            {
                CreatureSpeechLinePool.TryGetLine("corrupted_bone_knight", 1, a, out var la);
                CreatureSpeechLinePool.TryGetLine("corrupted_bone_knight", 1, b, out var lb);
                Assert.AreEqual(la, lb, "Mesmo seed deve produzir a mesma sequência de falas.");
            }
        }

        [Test]
        public void TryGetLine_NullRng_ReturnsFalse()
        {
            Assert.IsFalse(CreatureSpeechLinePool.TryGetLine("cave_bat", 1, null, out var line));
            Assert.AreEqual(string.Empty, line);
        }
    }
}
