using System;
using System.Collections.Generic;
using CindarsHope.Audio;
using CindarsHope.Core.Events;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Audio
{
    /// <summary>
    /// fable_58 — testes EditMode da lógica determinística do áudio:
    /// CA-1 ganho/canal; CA-2 mapeamento evento→categoria; CA-3 specs procedurais
    /// distintos + samples válidos; CA-4 cooldown anti-spam + fallback; CA-5 máquina
    /// de estado de música (prioridade + retorno) e crossfade. Sem AudioSource —
    /// toda a lógica testada vive em classes puras.
    /// </summary>
    public class AudioSfxHooksTests
    {
        // ============================================================ CA-1 ganho/canal

        [Test]
        public void Gain_MasterTimesChannel()
        {
            Assert.AreEqual(0.5f, AudioGainCalculator.ComputeGain(100, 50), 0.0001f);
            Assert.AreEqual(0.48f, AudioGainCalculator.ComputeGain(80, 60), 0.0001f);
            Assert.AreEqual(0f, AudioGainCalculator.ComputeGain(0, 100), 0.0001f);
            Assert.AreEqual(1f, AudioGainCalculator.ComputeGain(100, 100), 0.0001f);
        }

        [Test]
        public void Gain_ClampsOutOfRangeVolume()
        {
            Assert.AreEqual(0, AudioGainCalculator.ClampVolume(-20));
            Assert.AreEqual(100, AudioGainCalculator.ClampVolume(250));
            // master 150 -> 100, channel -10 -> 0 => 0
            Assert.AreEqual(0f, AudioGainCalculator.ComputeGain(150, -10), 0.0001f);
            // both above max clamp to 1
            Assert.AreEqual(1f, AudioGainCalculator.ComputeGain(150, 150), 0.0001f);
        }

        [Test]
        public void Gain_Defaults_AreConservative()
        {
            Assert.AreEqual(80, AudioGainCalculator.DefaultMasterVolume);
            Assert.AreEqual(60, AudioGainCalculator.DefaultSfxVolume, "SFX conservador para evitar metralhadora de beep.");
            Assert.AreEqual(70, AudioGainCalculator.DefaultMusicVolume);
        }

        // ============================================================ CA-2 mapeamento

        [Test]
        public void EventMap_AllV1AudibleEventsHaveCategory()
        {
            // Eventos audíveis v1 que EXISTEM no projeto (DamageBlockedEvent não existe — ver report).
            var expected = new Dictionary<Type, SfxCategory>
            {
                { typeof(PlayerChargedAttackEvent), SfxCategory.Charged },
                { typeof(EnemyPostureBrokenEvent), SfxCategory.PostureBreak },
                { typeof(PlayerPerfectBlockEvent), SfxCategory.PerfectBlock },
                { typeof(StatusEffectAppliedEvent), SfxCategory.Status },
                { typeof(DamageAppliedEvent), SfxCategory.Hit },
                { typeof(PlayerDamagedEvent), SfxCategory.Hit },
                { typeof(EnemyKilledEvent), SfxCategory.EnemyKilled },
                { typeof(NotificationToastRequestedEvent), SfxCategory.UiToast },
                { typeof(PlayerActionFeedbackEvent), SfxCategory.UiToast },
                { typeof(ItemPickedUpEvent), SfxCategory.Pickup },
                { typeof(ItemCraftedEvent), SfxCategory.Craft },
                { typeof(CropHarvestedEvent), SfxCategory.Harvest },
                { typeof(FishCaughtEvent), SfxCategory.Fish },
                { typeof(PlayerLevelChangedEvent), SfxCategory.LevelUp },
                { typeof(DayStartedEvent), SfxCategory.DayStart },
                { typeof(GameSavedEvent), SfxCategory.Save },
                { typeof(HouseDoorTransitionCompletedEvent), SfxCategory.Door }
            };

            foreach (var pair in expected)
            {
                Assert.AreEqual(pair.Value, SfxEventMap.CategoryFor(pair.Key),
                    $"Evento {pair.Key.Name} deve mapear para {pair.Value}.");
            }

            Assert.AreEqual(expected.Count, SfxEventMap.Count, "Tabela deve cobrir exatamente os eventos audíveis v1.");
        }

        [Test]
        public void EventMap_UnknownEventReturnsNone()
        {
            Assert.AreEqual(SfxCategory.None, SfxEventMap.CategoryFor(typeof(string)));
            Assert.AreEqual(SfxCategory.None, SfxEventMap.CategoryFor(null));
        }

        [Test]
        public void DoorEvent_BridgeMappingUsesDoorCategoryAndCooldown()
        {
            Assert.AreEqual(SfxCategory.Door, SfxEventMap.CategoryFor<HouseDoorTransitionCompletedEvent>());
            var gate = new SfxCooldownGate(0.05f);
            Assert.IsTrue(SfxEventBridge.TryConsumeMappedEvent<HouseDoorTransitionCompletedEvent>(gate, 1f));
            Assert.IsFalse(SfxEventBridge.TryConsumeMappedEvent<HouseDoorTransitionCompletedEvent>(gate, 1.02f));
            Assert.IsTrue(SfxEventBridge.TryConsumeMappedEvent<HouseDoorTransitionCompletedEvent>(gate, 1.06f));
        }

        [Test]
        public void CategoryIds_AreStableAndDistinct()
        {
            var seen = new HashSet<string>();
            foreach (var category in ProceduralSfxLibrary.AllSfxCategories())
            {
                var id = SfxCategoryIds.ToStableId(category);
                Assert.IsFalse(string.IsNullOrEmpty(id), $"Categoria {category} deve ter id estável.");
                Assert.IsTrue(seen.Add(id), $"Id duplicado: {id}");
            }

            Assert.AreEqual(string.Empty, SfxCategoryIds.ToStableId(SfxCategory.None));
        }

        // ============================================================ CA-3 placeholders procedurais

        [Test]
        public void SfxSpecs_AllValid_AndDistinct()
        {
            var signatures = new HashSet<string>();
            foreach (var category in ProceduralSfxLibrary.AllSfxCategories())
            {
                var spec = ProceduralSfxLibrary.ForSfx(category);
                Assert.IsTrue(spec.IsValid, $"Spec da categoria {category} deve ser válida (freq>0, dur>0, amp>0).");
                Assert.Less(spec.DurationSeconds, 1.2f, $"SFX {category} deve ser curto (<1.2s).");

                // assinatura única por categoria (parâmetros distintos)
                string sig = $"{spec.StartFrequency}|{spec.EndFrequency}|{spec.DurationSeconds}|{spec.Shape}|{spec.Amplitude}|{spec.NoteCount}";
                Assert.IsTrue(signatures.Add(sig), $"Categoria {category} tem parâmetros idênticos a outra (devem soar diferentes).");
            }
        }

        [Test]
        public void MusicSpecs_AllStatesHavePlaceholder_AndDistinct()
        {
            var signatures = new HashSet<string>();
            foreach (var state in ProceduralSfxLibrary.AllMusicStates())
            {
                var spec = ProceduralSfxLibrary.ForMusic(state);
                Assert.IsTrue(spec.IsValid, $"Estado {state} deve ter placeholder válido.");
                string sig = $"{spec.StartFrequency}|{spec.EndFrequency}|{spec.Shape}|{spec.Amplitude}|{spec.NoteCount}";
                Assert.IsTrue(signatures.Add(sig), $"Estado {state} tem placeholder idêntico a outro.");
            }
        }

        [Test]
        public void FillSamples_ProducesNonZeroBoundedAudio()
        {
            var spec = ProceduralSfxLibrary.ForSfx(SfxCategory.Hit);
            int sampleCount = (int)(spec.DurationSeconds * ProceduralSfxFactory.SampleRate);
            var samples = new float[sampleCount];

            ProceduralSfxFactory.FillSamples(samples, spec, ProceduralSfxFactory.SampleRate);

            Assert.Greater(samples.Length, 0, "Deve gerar samples (duração esperada > 0).");

            bool anyNonZero = false;
            float maxAbs = 0f;
            foreach (var s in samples)
            {
                Assert.IsFalse(float.IsNaN(s), "Sample não pode ser NaN.");
                float abs = s < 0f ? -s : s;
                if (abs > 0.0001f)
                {
                    anyNonZero = true;
                }

                if (abs > maxAbs)
                {
                    maxAbs = abs;
                }
            }

            Assert.IsTrue(anyNonZero, "Clipe não pode ser silêncio total.");
            Assert.LessOrEqual(maxAbs, 1.0001f, "Amplitude deve ficar dentro de [-1,1] (sem clipping).");
        }

        [Test]
        public void FillSamples_DoesNotThrowOnDegenerateInput()
        {
            Assert.DoesNotThrow(() => ProceduralSfxFactory.FillSamples(null, default, 44100));
            Assert.DoesNotThrow(() => ProceduralSfxFactory.FillSamples(new float[0], default, 44100));
            Assert.DoesNotThrow(() => ProceduralSfxFactory.FillSamples(new float[10], ProceduralSfxLibrary.ForSfx(SfxCategory.LevelUp), 0));
        }

        [Test]
        public void InvalidSpec_IsNotValid_FactoryStaysSilent()
        {
            // None não tem spec -> default(struct) -> inválida (sem exceção)
            var spec = ProceduralSfxLibrary.ForSfx(SfxCategory.None);
            Assert.IsFalse(spec.IsValid);
        }

        // ============================================================ CA-4 cooldown / fallback

        [Test]
        public void Cooldown_SuppressesBurstWithinWindow()
        {
            var gate = new SfxCooldownGate(0.05f);

            Assert.IsTrue(gate.TryConsume(SfxCategory.Hit, 1.00f), "Primeiro hit toca.");
            Assert.IsFalse(gate.TryConsume(SfxCategory.Hit, 1.02f), "Hit dentro de 50ms é suprimido (anti-spam).");
            Assert.IsTrue(gate.TryConsume(SfxCategory.Hit, 1.06f), "Após a janela, toca de novo.");
        }

        [Test]
        public void Cooldown_IsPerCategory()
        {
            var gate = new SfxCooldownGate(0.05f);
            Assert.IsTrue(gate.TryConsume(SfxCategory.Hit, 1.00f));
            Assert.IsTrue(gate.TryConsume(SfxCategory.Pickup, 1.00f), "Categoria diferente não compartilha cooldown.");
        }

        [Test]
        public void Cooldown_NoneNeverPlays()
        {
            var gate = new SfxCooldownGate(0.05f);
            Assert.IsFalse(gate.TryConsume(SfxCategory.None, 1.00f));
            Assert.IsFalse(gate.TryConsume(SfxCategory.None, 100f));
        }

        // ============================================================ CA-5 música: prioridade + crossfade

        [Test]
        public void MusicPriority_BossBeatsCombatBeatsFestivalBeatsCalm()
        {
            Assert.AreEqual(MusicState.Calmo, MusicStateResolver.Resolve(0, false, false));
            Assert.AreEqual(MusicState.Festival, MusicStateResolver.Resolve(0, false, true));
            Assert.AreEqual(MusicState.Combate, MusicStateResolver.Resolve(2, false, true), "Combate vence Festival.");
            Assert.AreEqual(MusicState.Boss, MusicStateResolver.Resolve(2, true, true), "Boss vence tudo.");
            Assert.AreEqual(MusicState.Boss, MusicStateResolver.Resolve(0, true, false), "Boss sem combate ainda é Boss.");
        }

        [Test]
        public void MusicResolver_ReturnsToLowerPriorityWhenConditionCeases()
        {
            var r = new MusicStateResolver();
            Assert.AreEqual(MusicState.Calmo, r.CurrentState);

            r.SetFestivalActive(true);
            Assert.AreEqual(MusicState.Festival, r.CurrentState);

            r.EnemyEngaged();
            r.EnemyEngaged();
            Assert.AreEqual(MusicState.Combate, r.CurrentState, "Combate sobrepõe Festival.");

            r.SetBossActive(true);
            Assert.AreEqual(MusicState.Boss, r.CurrentState);

            // boss morto -> ainda há combate
            r.SetBossActive(false);
            Assert.AreEqual(MusicState.Combate, r.CurrentState);

            // inimigos somem -> festival ainda ativo
            r.EnemyDisengaged();
            r.EnemyDisengaged();
            Assert.AreEqual(MusicState.Festival, r.CurrentState);

            // festival acaba -> Calmo
            r.SetFestivalActive(false);
            Assert.AreEqual(MusicState.Calmo, r.CurrentState);
        }

        [Test]
        public void MusicResolver_EngagementCountNeverGoesNegative()
        {
            var r = new MusicStateResolver();
            r.EnemyDisengaged();
            r.EnemyDisengaged();
            Assert.AreEqual(0, r.EngagedEnemies);
            Assert.AreEqual(MusicState.Calmo, r.CurrentState);

            r.EnemyEngaged();
            Assert.AreEqual(MusicState.Combate, r.CurrentState);
        }

        [Test]
        public void Crossfade_LinearCrossWithClampedProgress()
        {
            // duração 0.6s
            float dur = MusicCrossfade.DefaultDurationSeconds;
            Assert.AreEqual(0f, MusicCrossfade.Progress(0f, dur), 0.0001f);
            Assert.AreEqual(0.5f, MusicCrossfade.Progress(0.3f, dur), 0.0001f);
            Assert.AreEqual(1f, MusicCrossfade.Progress(0.6f, dur), 0.0001f);
            Assert.AreEqual(1f, MusicCrossfade.Progress(10f, dur), 0.0001f, "Progresso clampa em 1.");
            Assert.AreEqual(0f, MusicCrossfade.Progress(-1f, dur), 0.0001f, "Progresso clampa em 0.");

            // fatores complementares
            float p = 0.25f;
            Assert.AreEqual(0.75f, MusicCrossfade.FadeOutFactor(p), 0.0001f);
            Assert.AreEqual(0.25f, MusicCrossfade.FadeInFactor(p), 0.0001f);
            Assert.AreEqual(1f, MusicCrossfade.FadeOutFactor(p) + MusicCrossfade.FadeInFactor(p), 0.0001f);
        }

        [Test]
        public void Crossfade_CompletesAndZeroDurationIsInstant()
        {
            Assert.IsTrue(MusicCrossfade.IsComplete(1f));
            Assert.IsFalse(MusicCrossfade.IsComplete(0.5f));
            // duração <= mínima => progresso instantâneo (1)
            Assert.AreEqual(1f, MusicCrossfade.Progress(0f, 0f), 0.0001f);
            Assert.IsTrue(MusicCrossfade.IsComplete(MusicCrossfade.Progress(0f, 0.01f)));
        }
    }
}
