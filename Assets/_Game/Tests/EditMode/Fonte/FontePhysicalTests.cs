using CindarsHope.Fonte;
using CindarsHope.MainProgression;
using NUnit.Framework;

namespace CindarsHope.Tests.EditMode.Fonte
{
    /// <summary>
    /// F17 — Fonte física. O FonteFunctionUnlockService (WAVE 10) já tem validator próprio;
    /// aqui validamos a visibilidade honesta, a concessão diária e a recarga.
    /// </summary>
    public class FontePhysicalTests
    {
        private static MainProgressionSection ProgressionWith(params MainFragmentType[] fragments)
        {
            var progression = new MainProgressionSection();
            foreach (var fragment in fragments)
            {
                progression.FragmentStates.Add(new FragmentStateRecord
                {
                    FragmentType = fragment,
                    AcquisitionState = FragmentAcquisitionState.Integrated
                });
            }

            return progression;
        }

        [Test]
        public void ReturnPoint_UnlocksWithoutFragment()
        {
            var service = new FonteFunctionUnlockService();
            var section = new FonteAnyaSection();
            var result = service.TryUnlock(section, FonteFunction.ReturnPoint, new MainProgressionSection());

            Assert.IsTrue(result.Success);
            Assert.IsTrue(section.HasFunction(FonteFunction.ReturnPoint));
        }

        [Test]
        public void LivingWater_RequiresWaterFragment()
        {
            var service = new FonteFunctionUnlockService();
            var section = new FonteAnyaSection();

            var locked = service.TryUnlock(section, FonteFunction.LimitedLivingWater, new MainProgressionSection());
            Assert.IsFalse(locked.Success, "Sem fragmento: selado (menu mostra ???).");

            var unlocked = service.TryUnlock(section, FonteFunction.LimitedLivingWater, ProgressionWith(MainFragmentType.Water));
            Assert.IsTrue(unlocked.Success);
            Assert.AreEqual(FonteState.WaterFlowing, section.FonteState);
        }

        [Test]
        public void Respec_RequiresMemoryFragment()
        {
            var service = new FonteFunctionUnlockService();
            var section = new FonteAnyaSection();

            Assert.IsFalse(service.TryUnlock(section, FonteFunction.Respec, ProgressionWith(MainFragmentType.Water)).Success);
            // Memória exige Água integrada antes (ordem canônica) — com ambas destrava.
            Assert.IsTrue(service.TryUnlock(section, FonteFunction.Respec, ProgressionWith(MainFragmentType.Water, MainFragmentType.Memory)).Success);
        }

        [Test]
        public void DailyGrant_IsIdempotentPerDay()
        {
            Assert.IsTrue(FonteRuntimeService.CanGrantToday(-1, 1), "Nunca concedeu: pode.");
            Assert.IsTrue(FonteRuntimeService.CanGrantToday(1, 2), "Dia novo: pode.");
            Assert.IsFalse(FonteRuntimeService.CanGrantToday(2, 2), "Mesmo dia: bloqueado.");
        }

        [Test]
        public void UseRequest_FailsWithoutCharges()
        {
            var service = new FonteFunctionUnlockService();
            var section = new FonteAnyaSection();
            service.TryUnlock(section, FonteFunction.LimitedLivingWater, ProgressionWith(MainFragmentType.Water));
            section.LivingWater.Unlocked = true;
            section.LivingWater.CurrentCharges = 0;

            var result = service.EvaluateUseRequest(section, new FonteUseRequest
            {
                RequestedFunction = FonteFunction.LimitedLivingWater,
                CurrentDay = 3
            });

            Assert.IsFalse(result.Success);
            Assert.AreEqual("LIVING_WATER_NO_CHARGES", result.FailureReason);
        }

        [Test]
        public void UseRequest_ConsumesOneCharge()
        {
            var service = new FonteFunctionUnlockService();
            var section = new FonteAnyaSection();
            service.TryUnlock(section, FonteFunction.LimitedLivingWater, ProgressionWith(MainFragmentType.Water));
            section.LivingWater.Unlocked = true;
            section.LivingWater.CurrentCharges = 3;

            var result = service.EvaluateUseRequest(section, new FonteUseRequest
            {
                RequestedFunction = FonteFunction.LimitedLivingWater,
                CurrentDay = 3
            });

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.LivingWaterChargesConsumed);
        }

        [Test]
        public void FonteState_NeverRegresses()
        {
            var service = new FonteFunctionUnlockService();
            var section = new FonteAnyaSection();

            service.TryUnlock(section, FonteFunction.LimitedLivingWater, ProgressionWith(MainFragmentType.Water, MainFragmentType.Memory));
            service.TryUnlock(section, FonteFunction.Respec, ProgressionWith(MainFragmentType.Water, MainFragmentType.Memory));
            Assert.AreEqual(FonteState.MemoryEchoing, section.FonteState);

            // Re-destravar ReturnPoint (estado menor) não regride o estado.
            service.TryUnlock(section, FonteFunction.ReturnPoint, ProgressionWith(MainFragmentType.Water, MainFragmentType.Memory));
            Assert.AreEqual(FonteState.MemoryEchoing, section.FonteState);
        }
    }
}
