using System.Collections.Generic;
using NUnit.Framework;
using CindarsHope.Save;
using CindarsHope.Save.Providers;
using CindarsHope.UI.HUD;
using CindarsHope.UI.Onboarding;
using CindarsHope.UI.SystemTab;

namespace CindarsHope.Tests.EditMode.UI
{
    /// <summary>
    /// fable_62: EditMode tests da logica deterministica dos hints de onboarding.
    /// Cobre CA-1 (primeira-ocorrencia), CA-2 (gatilho/prioridade da caverna), CA-3 (round-trip e
    /// save legado da secao HintsSeen) e a integridade do catalogo/tela Controles. Sem dependencia
    /// de scene/canvas (isso fica no cenario humano do lote).
    /// </summary>
    [TestFixture]
    public class OnboardingHintTests
    {
        // ---- CA-1: primeira-ocorrencia ----

        [Test]
        public void TryMarkSeen_FirstTime_ReturnsTrue()
        {
            var tracker = new OnboardingHintTracker();
            Assert.IsTrue(tracker.TryMarkSeen(OnboardingHintCatalog.HintMove));
        }

        [Test]
        public void TryMarkSeen_SecondTrigger_DoesNotReFire()
        {
            var tracker = new OnboardingHintTracker();
            Assert.IsTrue(tracker.TryMarkSeen(OnboardingHintCatalog.HintInteract), "1a ocorrencia deve disparar");
            Assert.IsFalse(tracker.TryMarkSeen(OnboardingHintCatalog.HintInteract), "2o gatilho NAO deve re-disparar");
            Assert.IsTrue(tracker.HasSeen(OnboardingHintCatalog.HintInteract));
        }

        [Test]
        public void TryMarkSeen_AllFiveHints_EachFiresExactlyOnce()
        {
            var tracker = new OnboardingHintTracker();
            foreach (var entry in OnboardingHintCatalog.All)
            {
                Assert.IsTrue(tracker.TryMarkSeen(entry.HintId), $"1a ocorrencia de {entry.HintId}");
                Assert.IsFalse(tracker.TryMarkSeen(entry.HintId), $"re-trigger de {entry.HintId} deve ser no-op");
            }
        }

        [Test]
        public void TryMarkSeen_UnknownId_ReturnsFalseAndDoesNotMark()
        {
            var tracker = new OnboardingHintTracker();
            Assert.IsFalse(tracker.TryMarkSeen("hint_inexistente"));
            Assert.IsFalse(tracker.HasSeen("hint_inexistente"));
        }

        [Test]
        public void TryMarkSeen_NullOrEmpty_ReturnsFalse()
        {
            var tracker = new OnboardingHintTracker();
            Assert.IsFalse(tracker.TryMarkSeen(null));
            Assert.IsFalse(tracker.TryMarkSeen(string.Empty));
        }

        // ---- CA-3: round-trip e save legado ----

        [Test]
        public void RoundTrip_SeenHintsSurvive()
        {
            var tracker = new OnboardingHintTracker();
            tracker.TryMarkSeen(OnboardingHintCatalog.HintMove);
            tracker.TryMarkSeen(OnboardingHintCatalog.HintCaveDanger);

            var captured = tracker.GetSeenHintIds();

            var restored = new OnboardingHintTracker();
            restored.RestoreSeen(captured);

            Assert.IsTrue(restored.HasSeen(OnboardingHintCatalog.HintMove));
            Assert.IsTrue(restored.HasSeen(OnboardingHintCatalog.HintCaveDanger));
            Assert.IsFalse(restored.HasSeen(OnboardingHintCatalog.HintInteract));
        }

        [Test]
        public void RestoreSeen_NullSection_LegacySave_AllHintsEligible()
        {
            var tracker = new OnboardingHintTracker();
            tracker.RestoreSeen(null); // secao ausente em save legado

            // Lista vazia => nenhum hint visto => todos elegiveis de novo (seguro).
            foreach (var entry in OnboardingHintCatalog.All)
            {
                Assert.IsFalse(tracker.HasSeen(entry.HintId));
                Assert.IsTrue(tracker.TryMarkSeen(entry.HintId), $"{entry.HintId} deve estar elegivel apos save legado");
            }
        }

        [Test]
        public void RestoreSeen_IgnoresUnknownIds()
        {
            var tracker = new OnboardingHintTracker();
            tracker.RestoreSeen(new List<string> { "hint_removido_no_futuro", OnboardingHintCatalog.HintStatus });

            Assert.IsTrue(tracker.HasSeen(OnboardingHintCatalog.HintStatus));
            Assert.IsFalse(tracker.HasSeen("hint_removido_no_futuro"));
        }

        // ---- CA-3: provider (padrao ISaveSectionProvider) com save legado ----

        [Test]
        public void Provider_RestoreNullSection_NoThrow_WithoutRuntimeService()
        {
            // Sem OnboardingHintService.Instance (EditMode), Restore deve ser no-op silencioso.
            var provider = new OnboardingHintsSectionProvider();
            Assert.DoesNotThrow(() => provider.Restore(null));
            Assert.AreEqual("onboarding_hints", provider.ProviderId);
        }

        [Test]
        public void Provider_CaptureWithoutService_FallsBackToExistingSaveData()
        {
            var provider = new OnboardingHintsSectionProvider();
            var existing = new GameSaveData
            {
                OnboardingHints = new OnboardingHintsSaveData
                {
                    SeenHintIds = new List<string> { OnboardingHintCatalog.HintMove }
                }
            };

            var captured = provider.Capture(existing) as OnboardingHintsSaveData;
            Assert.IsNotNull(captured);
            CollectionAssert.Contains(captured.SeenHintIds, OnboardingHintCatalog.HintMove);
        }

        [Test]
        public void Provider_CaptureWithoutServiceAndNoExisting_ReturnsEmptySection()
        {
            var provider = new OnboardingHintsSectionProvider();
            var captured = provider.Capture(null) as OnboardingHintsSaveData;
            Assert.IsNotNull(captured);
            Assert.AreEqual(0, captured.SeenHintIds.Count);
        }

        // ---- Integridade do catalogo ----

        [Test]
        public void Catalog_HasFiveHints()
        {
            Assert.AreEqual(5, OnboardingHintCatalog.All.Count);
        }

        [Test]
        public void Catalog_HintIdsAreUniqueAndNonEmpty()
        {
            var seen = new HashSet<string>();
            foreach (var entry in OnboardingHintCatalog.All)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.HintId), "hintId nao pode ser vazio");
                Assert.IsTrue(seen.Add(entry.HintId), $"hintId duplicado: {entry.HintId}");
            }
        }

        [Test]
        public void Catalog_AllTextsAreNonEmpty()
        {
            foreach (var entry in OnboardingHintCatalog.All)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.Text), $"texto vazio em {entry.HintId}");
            }
        }

        // ---- CA-2: gatilho da caverna marcado Important ----

        [Test]
        public void Catalog_CaveDangerHint_IsImportantPriority()
        {
            Assert.IsTrue(OnboardingHintCatalog.TryGet(OnboardingHintCatalog.HintCaveDanger, out var entry));
            Assert.AreEqual(FeedbackMessagePriority.Important, entry.Priority);
        }

        [Test]
        public void Catalog_CaveDangerHint_MentionsDodgeAndBlock()
        {
            Assert.IsTrue(OnboardingHintCatalog.TryGet(OnboardingHintCatalog.HintCaveDanger, out var entry));
            StringAssert.Contains("Space", entry.Text);
            StringAssert.Contains("Shift", entry.Text);
        }

        [Test]
        public void Catalog_NonCaveHints_AreNotImportant()
        {
            foreach (var entry in OnboardingHintCatalog.All)
            {
                if (entry.HintId == OnboardingHintCatalog.HintCaveDanger)
                {
                    continue;
                }

                Assert.AreNotEqual(FeedbackMessagePriority.Important, entry.Priority,
                    $"{entry.HintId} nao deveria furar a fila");
            }
        }

        // ---- CA-4: conteudo da tela Controles (espelha input_map.md/F67) ----

        [Test]
        public void ControlsReference_HasGroupsAndLines()
        {
            Assert.Greater(ControlsReferenceContent.All.Count, 0);
            foreach (var group in ControlsReferenceContent.All)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(group.Title));
                Assert.Greater(group.Lines.Count, 0, $"grupo {group.Title} sem linhas");
                foreach (var line in group.Lines)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(line.Keys));
                    Assert.IsFalse(string.IsNullOrWhiteSpace(line.Action));
                }
            }
        }

        [Test]
        public void ControlsReference_MentionsCanonicalCombatKeys()
        {
            var text = ControlsReferenceScreen.BuildDisplayText();
            // Teclas canonicas reais (input_map.md F67).
            StringAssert.Contains("WASD", text);
            StringAssert.Contains("Espaco", text);
            StringAssert.Contains("Shift", text);
            StringAssert.Contains("Esc", text);
        }

        [Test]
        public void ControlsReference_DoesNotExposeDebugKeys()
        {
            var text = ControlsReferenceScreen.BuildDisplayText();
            // Teclas debug/dev (Tab=advance day, F5/F9 save/load, B/O/P) sao non-contract:
            // NUNCA podem aparecer na tela de controles (input_map.md F67).
            StringAssert.DoesNotContain("F5", text);
            StringAssert.DoesNotContain("F9", text);
            StringAssert.DoesNotContain("Advance", text);
        }
    }
}
