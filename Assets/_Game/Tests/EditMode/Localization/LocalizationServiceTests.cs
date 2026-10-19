using System.Collections.Generic;
using CindarsHope.Localization;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Localization
{
    // fable_73 — Localization String Table Runtime (ADR-0012).
    // Covers the deterministic lookup + fallback-to-id contract of LocalizationService and the
    // static/lazy LocalizationStringTable. Pure EditMode (no scene, no asset): the service is a
    // pure id -> string resolution with a visible fallback (parity with the legacy "?? QuestId").
    [TestFixture]
    public class LocalizationServiceTests
    {
        private const string PresentId = "quest.test.title";
        private const string PresentValue = "Titulo de Teste";
        private const string AbsentId = "quest.test.missing";

        [SetUp]
        public void SetUp()
        {
            // Deterministic table for each test; reset in TearDown.
            LocalizationStringTable.OverrideForTests(new Dictionary<string, string>
            {
                { PresentId, PresentValue },
                { "dialogue.test.greeting", "Ola" }
            });
        }

        [TearDown]
        public void TearDown()
        {
            LocalizationStringTable.OverrideForTests(null);
        }

        // CA-1 — lookup id -> string PT-BR works (Get/TryGet/Has for a present id).
        [Test]
        public void Get_PresentId_ReturnsStoredValue()
        {
            Assert.AreEqual(PresentValue, LocalizationService.Get(PresentId));
        }

        [Test]
        public void TryGet_PresentId_ReturnsTrueAndValue()
        {
            bool found = LocalizationService.TryGet(PresentId, out var value);
            Assert.IsTrue(found, "CA-1: TryGet returns true for a present id.");
            Assert.AreEqual(PresentValue, value);
        }

        [Test]
        public void Has_PresentId_ReturnsTrue()
        {
            Assert.IsTrue(LocalizationService.Has(PresentId));
        }

        // CA-2 — fallback to the id is deterministic and visible (parity with "?? QuestId").
        [Test]
        public void Get_AbsentId_ReturnsTheIdItself()
        {
            Assert.AreEqual(AbsentId, LocalizationService.Get(AbsentId));
        }

        [Test]
        public void Get_AbsentId_ReturnsNonEmptyString()
        {
            string result = LocalizationService.Get(AbsentId);
            Assert.IsFalse(string.IsNullOrEmpty(result), "CA-2: absent non-empty id never resolves to empty/null.");
        }

        [Test]
        public void TryGet_AbsentId_ReturnsFalseAndIdAsValue()
        {
            bool found = LocalizationService.TryGet(AbsentId, out var value);
            Assert.IsFalse(found, "CA-2: TryGet returns false for an absent id.");
            Assert.AreEqual(AbsentId, value, "CA-2: value carries the fallback (the id) for caller convenience.");
        }

        [Test]
        public void Has_AbsentId_ReturnsFalse()
        {
            Assert.IsFalse(LocalizationService.Has(AbsentId));
        }

        // CA-2 parity — the service reproduces the legacy ad-hoc "?? id" behavior exactly,
        // so QuestLogProjectionService can later swap "def.DisplayNameKey ?? def.QuestId" for
        // LocalizationService.Get(def.DisplayNameKey ?? def.QuestId) without changing behavior.
        [Test]
        public void Get_MatchesLegacyNullCoalesceFallback_ForAbsentKey()
        {
            // Legacy pattern: key is the text, fallback to the quest id when the key is null.
            string legacyKey = null;
            const string questId = "quest_first_supplies";
            string legacyResolved = legacyKey ?? questId; // == questId

            // New pattern resolving the same already-coalesced value: absent => id (the value itself).
            string serviceResolved = LocalizationService.Get(legacyResolved);

            Assert.AreEqual(legacyResolved, serviceResolved,
                "Parity: resolving an absent id returns that id, identical to the legacy '?? id'.");
        }

        // CA-3 — edges (null / empty) are deterministic, no NullReferenceException.
        [Test]
        public void Get_Null_ReturnsNullWithoutThrowing()
        {
            Assert.AreEqual(null, LocalizationService.Get(null));
        }

        [Test]
        public void Get_Empty_ReturnsEmptyWithoutThrowing()
        {
            Assert.AreEqual(string.Empty, LocalizationService.Get(string.Empty));
        }

        [Test]
        public void TryGet_Null_ReturnsFalse()
        {
            bool found = LocalizationService.TryGet(null, out var value);
            Assert.IsFalse(found);
            Assert.AreEqual(null, value);
        }

        [Test]
        public void TryGet_Empty_ReturnsFalse()
        {
            bool found = LocalizationService.TryGet(string.Empty, out var value);
            Assert.IsFalse(found);
            Assert.AreEqual(string.Empty, value);
        }

        [Test]
        public void Has_NullOrEmpty_ReturnsFalse()
        {
            Assert.IsFalse(LocalizationService.Has(null));
            Assert.IsFalse(LocalizationService.Has(string.Empty));
        }

        // Idempotence — same input, same result across repeated calls.
        [Test]
        public void Get_IsIdempotent_ForPresentAndAbsent()
        {
            Assert.AreEqual(LocalizationService.Get(PresentId), LocalizationService.Get(PresentId));
            Assert.AreEqual(LocalizationService.Get(AbsentId), LocalizationService.Get(AbsentId));
        }

        // Table loading — the seeded (non-overridden) table loads lazily and resolves its demo keys.
        [Test]
        public void SeededTable_LoadsAndResolvesConventionDemoKeys()
        {
            LocalizationStringTable.OverrideForTests(null); // back to the seeded catalogue
            Assert.Greater(LocalizationStringTable.Count, 0, "Seeded table is non-empty on lazy init.");
            Assert.IsTrue(LocalizationStringTable.Contains("quest.sample.title"),
                "Convention demo key resolves from the seeded table.");
            Assert.AreEqual("Missao de Exemplo", LocalizationService.Get("quest.sample.title"));
        }

        // Table-level contract — Contains/TryGetValue agree with the service on edges.
        [Test]
        public void Table_TryGetValue_FalseForAbsentAndEmpty()
        {
            Assert.IsFalse(LocalizationStringTable.TryGetValue(AbsentId, out _));
            Assert.IsFalse(LocalizationStringTable.TryGetValue(string.Empty, out _));
            Assert.IsFalse(LocalizationStringTable.TryGetValue(null, out _));
        }

        [Test]
        public void DefaultLanguage_IsPtBr()
        {
            Assert.AreEqual("pt-BR", LocalizationStringTable.DefaultLanguage,
                "CA-4: single PT-BR authoring locale; no runtime locale switch.");
        }
    }
}
