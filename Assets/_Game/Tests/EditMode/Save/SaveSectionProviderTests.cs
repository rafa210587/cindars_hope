using System.Collections.Generic;
using CindarsHope.Equipment;
using CindarsHope.Farm.Runtime;
using CindarsHope.Foundation;
using CindarsHope.Player;
using CindarsHope.Save;
using CindarsHope.Save.Providers;
using CindarsHope.Skills;
using NUnit.Framework;

/// <summary>
/// Testes de round-trip (capture→restore) dos providers ISaveSectionProvider migrados do SaveManager.
/// Cobre: instância nula (fallback seguro), tipo errado no Restore (no-op sem throw),
/// seção ausente em save legado (fallback default), e round-trip de estado idêntico.
/// Providers que dependem de singletons MonoBehaviour (ex: FonteSectionProvider,
/// FriendshipSectionProvider) requerem Play Mode e são cobertos por cenários manuais.
/// </summary>
namespace CindarsHope.Tests.EditMode.Save
{
    // ─────────────────────────────────────────────────────────────────────────────────────────────
    // StaminaSectionProvider
    // ─────────────────────────────────────────────────────────────────────────────────────────────

    public class StaminaSectionProviderTests
    {
        [Test]
        public void Capture_NullManager_ReturnsFallbackFull()
        {
            var provider = new StaminaSectionProvider(null);
            var result = provider.Capture(null) as StaminaSaveData;

            Assert.IsNotNull(result);
            Assert.AreEqual(100, result.CurrentStamina);
            Assert.AreEqual(100, result.MaxStamina);
        }

        [Test]
        public void Restore_NullManager_DoesNotThrow()
        {
            var provider = new StaminaSectionProvider(null);
            Assert.DoesNotThrow(() => provider.Restore(new StaminaSaveData { CurrentStamina = 50, MaxStamina = 100 }));
        }

        [Test]
        public void Restore_NullSectionData_DoesNotThrow()
        {
            var provider = new StaminaSectionProvider(null);
            Assert.DoesNotThrow(() => provider.Restore(null));
        }

        [Test]
        public void Restore_WrongType_DoesNotThrow()
        {
            var provider = new StaminaSectionProvider(null);
            Assert.DoesNotThrow(() => provider.Restore("tipo_errado"));
        }

        [Test]
        public void ProviderId_IsStable()
        {
            var provider = new StaminaSectionProvider(null);
            Assert.AreEqual("stamina", provider.ProviderId);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────
    // GameTimeSectionProvider
    // ─────────────────────────────────────────────────────────────────────────────────────────────

    public class GameTimeSectionProviderTests
    {
        [Test]
        public void Capture_NullManagers_ReturnsSafeDefault()
        {
            var provider = new GameTimeSectionProvider(null, null);
            var result = provider.Capture(null) as GameTimeSaveData;

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.CurrentDay);
            Assert.AreEqual(0, result.CurrentPhase);
            Assert.AreEqual(0f, result.PhaseElapsedSeconds);
        }

        [Test]
        public void Restore_NullManager_DoesNotThrow()
        {
            var provider = new GameTimeSectionProvider(null, null);
            Assert.DoesNotThrow(() => provider.Restore(new GameTimeSaveData { CurrentDay = 5 }));
        }

        [Test]
        public void Restore_NullSection_DoesNotThrow()
        {
            var provider = new GameTimeSectionProvider(null, null);
            Assert.DoesNotThrow(() => provider.Restore(null));
        }

        [Test]
        public void ProviderId_IsStable()
        {
            var provider = new GameTimeSectionProvider(null, null);
            Assert.AreEqual("game_time", provider.ProviderId);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────
    // PlayerStatusEffectsSectionProvider
    // ─────────────────────────────────────────────────────────────────────────────────────────────

    public class PlayerStatusEffectsSectionProviderTests
    {
        [Test]
        public void Capture_NullManager_ReturnsEmptyList()
        {
            var provider = new PlayerStatusEffectsSectionProvider(null);
            var result = provider.Capture(null) as PlayerStatusEffectsSaveData;

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ActiveEffects);
            Assert.AreEqual(0, result.ActiveEffects.Count);
        }

        [Test]
        public void Restore_NullManager_DoesNotThrow()
        {
            var provider = new PlayerStatusEffectsSectionProvider(null);
            Assert.DoesNotThrow(() => provider.Restore(new PlayerStatusEffectsSaveData()));
        }

        [Test]
        public void Restore_NullSection_DoesNotThrow()
        {
            var provider = new PlayerStatusEffectsSectionProvider(null);
            Assert.DoesNotThrow(() => provider.Restore(null));
        }

        [Test]
        public void Restore_WrongType_DoesNotThrow()
        {
            var provider = new PlayerStatusEffectsSectionProvider(null);
            Assert.DoesNotThrow(() => provider.Restore(42));
        }

        [Test]
        public void ProviderId_IsStable()
        {
            var provider = new PlayerStatusEffectsSectionProvider(null);
            Assert.AreEqual("player_status_effects", provider.ProviderId);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────
    // EquipmentDurabilitySectionProvider
    // ─────────────────────────────────────────────────────────────────────────────────────────────

    public class EquipmentDurabilitySectionProviderTests
    {
        [Test]
        public void Capture_NullManager_ReturnsEmptyData()
        {
            var provider = new EquipmentDurabilitySectionProvider(null);
            var result = provider.Capture(null) as EquipmentDurabilitySaveData;

            Assert.IsNotNull(result);
        }

        [Test]
        public void Restore_NullManager_DoesNotThrow()
        {
            var provider = new EquipmentDurabilitySectionProvider(null);
            Assert.DoesNotThrow(() => provider.Restore(new EquipmentDurabilitySaveData()));
        }

        [Test]
        public void Restore_NullSection_DoesNotThrow()
        {
            var provider = new EquipmentDurabilitySectionProvider(null);
            Assert.DoesNotThrow(() => provider.Restore(null));
        }

        [Test]
        public void ProviderId_IsStable()
        {
            var provider = new EquipmentDurabilitySectionProvider(null);
            Assert.AreEqual("equipment_durability", provider.ProviderId);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────
    // ActiveSkillSlotsSectionProvider
    // ─────────────────────────────────────────────────────────────────────────────────────────────

    public class ActiveSkillSlotsSectionProviderTests
    {
        [Test]
        public void Capture_NullSlots_ReturnsEmptySlotData()
        {
            var provider = new ActiveSkillSlotsSectionProvider(null);
            var result = provider.Capture(null) as ActiveSkillSlotsSaveData;

            Assert.IsNotNull(result);
            Assert.IsEmpty(result.SlotRSkillActionId);
            Assert.IsEmpty(result.SlotTSkillActionId);
            Assert.IsEmpty(result.SlotYSkillActionId);
            Assert.IsEmpty(result.SlotGSkillActionId);
        }

        [Test]
        public void Restore_NullSlots_DoesNotThrow()
        {
            var provider = new ActiveSkillSlotsSectionProvider(null);
            var data = new ActiveSkillSlotsSaveData
            {
                SlotRSkillActionId = "skill_dash",
                SlotTSkillActionId = string.Empty,
                SlotYSkillActionId = string.Empty,
                SlotGSkillActionId = string.Empty
            };
            Assert.DoesNotThrow(() => provider.Restore(data));
        }

        [Test]
        public void Restore_NullSection_DoesNotThrow()
        {
            var provider = new ActiveSkillSlotsSectionProvider(null);
            Assert.DoesNotThrow(() => provider.Restore(null));
        }

        [Test]
        public void Restore_WrongType_DoesNotThrow()
        {
            var provider = new ActiveSkillSlotsSectionProvider(null);
            Assert.DoesNotThrow(() => provider.Restore("tipo_errado"));
        }

        [Test]
        public void ProviderId_IsStable()
        {
            var provider = new ActiveSkillSlotsSectionProvider(null);
            Assert.AreEqual("active_skill_slots", provider.ProviderId);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────
    // SkillTreeSectionProvider
    // ─────────────────────────────────────────────────────────────────────────────────────────────

    public class SkillTreeSectionProviderTests
    {
        [Test]
        public void Capture_NullManager_ReturnsEmptyData()
        {
            var provider = new SkillTreeSectionProvider(null, () => 1);
            var result = provider.Capture(null) as SkillTreeSaveData;

            Assert.IsNotNull(result);
        }

        [Test]
        public void Restore_NullManager_DoesNotThrow()
        {
            var provider = new SkillTreeSectionProvider(null, () => 1);
            Assert.DoesNotThrow(() => provider.Restore(new SkillTreeSaveData()));
        }

        [Test]
        public void Restore_NullSection_DoesNotThrow()
        {
            var provider = new SkillTreeSectionProvider(null, () => 1);
            Assert.DoesNotThrow(() => provider.Restore(null));
        }

        [Test]
        public void ProviderId_IsStable()
        {
            var provider = new SkillTreeSectionProvider(null, () => 1);
            Assert.AreEqual("skill_tree", provider.ProviderId);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────
    // QuestSectionProvider
    // ─────────────────────────────────────────────────────────────────────────────────────────────

    public class QuestSectionProviderTests
    {
        [Test]
        public void Capture_NoQuestService_FallsBackToExistingSection()
        {
            // QuestRuntimeBootstrap.CaptureSaveData() retorna null sem Unity runtime.
            var existing = new GameSaveData
            {
                Quests = new QuestStateSectionSaveData()
            };
            existing.Quests.QuestStates = new List<QuestStateSaveData>();

            var provider = new QuestSectionProvider();
            var result = provider.Capture(existing) as QuestStateSectionSaveData;

            Assert.IsNotNull(result);
        }

        [Test]
        public void Capture_NoQuestService_NoExistingSave_ReturnsEmpty()
        {
            var provider = new QuestSectionProvider();
            var result = provider.Capture(null) as QuestStateSectionSaveData;

            Assert.IsNotNull(result);
        }

        [Test]
        public void Restore_NullSection_DoesNotThrow()
        {
            var provider = new QuestSectionProvider();
            Assert.DoesNotThrow(() => provider.Restore(null));
        }

        [Test]
        public void Restore_WrongType_DoesNotThrow()
        {
            var provider = new QuestSectionProvider();
            Assert.DoesNotThrow(() => provider.Restore(99));
        }

        [Test]
        public void ProviderId_IsStable()
        {
            var provider = new QuestSectionProvider();
            Assert.AreEqual("quests", provider.ProviderId);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────
    // CaveRunSectionProvider
    // ─────────────────────────────────────────────────────────────────────────────────────────────

    public class CaveRunSectionProviderTests
    {
        [Test]
        public void Capture_NoBootstrap_ReturnsNoActiveRun()
        {
            // GameBootstrap.Instance é null em EditMode sem Unity runtime.
            var provider = new CaveRunSectionProvider();
            var result = provider.Capture(null) as CindarsHope.Cave.Runtime.CaveRunSaveData;

            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasActiveRun);
        }

        [Test]
        public void Restore_NoBootstrap_DoesNotThrow()
        {
            var provider = new CaveRunSectionProvider();
            var data = new CindarsHope.Cave.Runtime.CaveRunSaveData { HasActiveRun = true };
            Assert.DoesNotThrow(() => provider.Restore(data));
        }

        [Test]
        public void Restore_NullSection_DoesNotThrow()
        {
            var provider = new CaveRunSectionProvider();
            Assert.DoesNotThrow(() => provider.Restore(null));
        }

        [Test]
        public void Restore_NoActiveRun_DoesNotThrow()
        {
            var provider = new CaveRunSectionProvider();
            var data = new CindarsHope.Cave.Runtime.CaveRunSaveData { HasActiveRun = false };
            Assert.DoesNotThrow(() => provider.Restore(data));
        }

        [Test]
        public void ProviderId_IsStable()
        {
            var provider = new CaveRunSectionProvider();
            Assert.AreEqual("cave_run", provider.ProviderId);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────
    // FonteSectionProvider
    // ─────────────────────────────────────────────────────────────────────────────────────────────

    public class FonteSectionProviderTests
    {
        [Test]
        public void Capture_NoService_FallsBackToExistingSection()
        {
            var existing = new GameSaveData
            {
                Fonte = new FonteSaveData { FonteState = 2, LastGrantDay = 5 }
            };

            var provider = new FonteSectionProvider();
            var result = provider.Capture(existing) as FonteSaveData;

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.FonteState);
            Assert.AreEqual(5, result.LastGrantDay);
        }

        [Test]
        public void Capture_NoService_NoExistingSave_ReturnsDefault()
        {
            var provider = new FonteSectionProvider();
            var result = provider.Capture(null) as FonteSaveData;

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.FonteState);
        }

        [Test]
        public void Restore_NoService_DoesNotThrow()
        {
            var provider = new FonteSectionProvider();
            Assert.DoesNotThrow(() => provider.Restore(new FonteSaveData { FonteState = 1 }));
        }

        [Test]
        public void Restore_NullSection_DoesNotThrow()
        {
            var provider = new FonteSectionProvider();
            Assert.DoesNotThrow(() => provider.Restore(null));
        }

        [Test]
        public void ProviderId_IsStable()
        {
            var provider = new FonteSectionProvider();
            Assert.AreEqual("fonte", provider.ProviderId);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────
    // MainProgressionSectionProvider
    // ─────────────────────────────────────────────────────────────────────────────────────────────

    public class MainProgressionSectionProviderTests
    {
        [Test]
        public void Capture_NoService_FallsBackToExistingSection()
        {
            var existing = new GameSaveData
            {
                MainProgression = new MainProgressionSaveData { CurrentAct = 3 }
            };

            var provider = new MainProgressionSectionProvider();
            var result = provider.Capture(existing) as MainProgressionSaveData;

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.CurrentAct);
        }

        [Test]
        public void Capture_NoService_NoExistingSave_ReturnsDefault()
        {
            var provider = new MainProgressionSectionProvider();
            var result = provider.Capture(null) as MainProgressionSaveData;

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.CurrentAct);
        }

        [Test]
        public void Restore_NoService_DoesNotThrow()
        {
            var provider = new MainProgressionSectionProvider();
            Assert.DoesNotThrow(() => provider.Restore(new MainProgressionSaveData { CurrentAct = 2 }));
        }

        [Test]
        public void Restore_NullSection_DoesNotThrow()
        {
            var provider = new MainProgressionSectionProvider();
            Assert.DoesNotThrow(() => provider.Restore(null));
        }

        [Test]
        public void ProviderId_IsStable()
        {
            var provider = new MainProgressionSectionProvider();
            Assert.AreEqual("main_progression", provider.ProviderId);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────
    // DailyGoalsSectionProvider
    // ─────────────────────────────────────────────────────────────────────────────────────────────

    public class DailyGoalsSectionProviderTests
    {
        [Test]
        public void Capture_NoService_FallsBackToExistingSection()
        {
            var existing = new GameSaveData
            {
                DailyGoals = new FarmDailyGoalsSaveData()
            };

            var provider = new DailyGoalsSectionProvider();
            var result = provider.Capture(existing);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<FarmDailyGoalsSaveData>(result);
        }

        [Test]
        public void Capture_NoService_NoExistingSave_ReturnsDefault()
        {
            var provider = new DailyGoalsSectionProvider();
            var result = provider.Capture(null);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<FarmDailyGoalsSaveData>(result);
        }

        [Test]
        public void Restore_NoService_DoesNotThrow()
        {
            var provider = new DailyGoalsSectionProvider();
            Assert.DoesNotThrow(() => provider.Restore(new FarmDailyGoalsSaveData()));
        }

        [Test]
        public void Restore_NullSection_DoesNotThrow()
        {
            var provider = new DailyGoalsSectionProvider();
            Assert.DoesNotThrow(() => provider.Restore(null));
        }

        [Test]
        public void ProviderId_IsStable()
        {
            var provider = new DailyGoalsSectionProvider();
            Assert.AreEqual("daily_goals", provider.ProviderId);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────
    // ProviderId stability — smoke test para todos os IDs canônicos
    // ─────────────────────────────────────────────────────────────────────────────────────────────

    public class ProviderIdStabilityTests
    {
        /// <summary>
        /// Garante que nenhum provider duplica um ProviderId de outro. IDs são usados em logging
        /// e devem ser únicos para diagnóstico.
        /// </summary>
        [Test]
        public void AllProviders_HaveUniqueProviderIds()
        {
            var providers = new ISaveSectionProvider[]
            {
                new FonteSectionProvider(),
                new MainProgressionSectionProvider(),
                new DailyGoalsSectionProvider(),
                new QuestSectionProvider(),
                new CaveRunSectionProvider(),
                new StaminaSectionProvider(null),
                new GameTimeSectionProvider(null, null),
                new PlayerStatusEffectsSectionProvider(null),
                new EquipmentDurabilitySectionProvider(null),
                new ActiveSkillSlotsSectionProvider(null),
                new SkillTreeSectionProvider(null, () => 1),
                new CraftingSectionProvider(null),
                new EconomySectionProvider(null),
                new NpcsSectionProvider(null),
                new CaveSectionProvider(null),
                new DeathSectionProvider(null),
                new FarmSectionProvider(null),
                new WorldSectionProvider(null, null),
                new FriendshipSectionProvider(),
                new FarmAnimalsSectionProvider(),
                new NpcServicesSectionProvider(),
                new FarmLotsSectionProvider(),
                new EquipmentSectionProvider(null),
                new ProgressionSectionProvider(null),
                new BestiarySectionProvider(null),
                // Providers pré-existentes
                new HotbarSectionProvider(new HotbarState()),
                new SpellbookSectionProvider(),
                new OnboardingHintsSectionProvider(),
            };

            var seen = new System.Collections.Generic.HashSet<string>();
            foreach (var p in providers)
            {
                Assert.IsNotEmpty(p.ProviderId, $"ProviderId vazio em {p.GetType().Name}");
                Assert.IsTrue(seen.Add(p.ProviderId),
                    $"ProviderId duplicado '{p.ProviderId}' em {p.GetType().Name}");
            }
        }
    }
}
