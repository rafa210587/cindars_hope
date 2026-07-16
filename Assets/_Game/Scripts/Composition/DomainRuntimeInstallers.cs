using CindarsHope.Audio;
using CindarsHope.Cave.Death;
using CindarsHope.Cave.Ecosystem;
using CindarsHope.Cave.Runtime;
using CindarsHope.Combat;
using CindarsHope.Combat.StatusEffect;
using CindarsHope.Combat.Telemetry;
using CindarsHope.Crafting;
using CindarsHope.City.Services;
using CindarsHope.Narrative;
using CindarsHope.NPC.Friendship;
using CindarsHope.NPC.Gifting;
using CindarsHope.NPC;
using CindarsHope.NPC.Services;
using CindarsHope.NPC.Schedule;
using CindarsHope.Quests.Runtime;
using CindarsHope.World;
using CindarsHope.World.Events;
using CindarsHope.World.Weather;
using CindarsHope.Farm.Animals;
using CindarsHope.Farm.Forage;
using CindarsHope.Farm.Lots;
using CindarsHope.Farm.Resources;
using CindarsHope.Farm.Runtime;
using CindarsHope.Farm.Shipping;
using CindarsHope.Craft;
using CindarsHope.Inventory;
using CindarsHope.Items.Runtime;
using CindarsHope.Magic;
using CindarsHope.Fonte;
using CindarsHope.Player;
using CindarsHope.Player.Conditions;
using CindarsHope.Player.Death;
using CindarsHope.Player.Movement;
using UnityEngine;

namespace CindarsHope.Composition
{
    internal static class CraftingRuntimeInstaller
    {
        public static void Install() => RecipeFirstKillUnlockHook.Install();
        public static void Uninstall() => RecipeFirstKillUnlockHook.Uninstall();
    }

    internal static class CaveRuntimeInstaller
    {
        public static void Install(Transform owner) => CaveConflictFeedbackBridge.Install(owner);
    }

    /// <summary>
    /// Serviços de cave que dependem de cena (AfterSceneLoad): morte/corpo, bridge de runtime da
    /// cave e o mercador errante. Instalados no Start() do root, nunca em InstallRuntimeServices().
    /// </summary>
    internal static class CaveSceneRuntimeInstaller
    {
        public static void Install(Transform owner)
        {
            DeathSystemBootstrap.Install(owner);
            CaveRuntimeBridge.Install(owner);
            CaveWanderingMerchant.Install();
        }
    }

    /// <summary>Telemetria de combate (fable_59) — OFF por default, gated pelo toggle de debug.</summary>
    internal static class CombatTelemetryRuntimeInstaller
    {
        public static void Install(Transform owner) => CombatTelemetryService.Bootstrap.Install(owner);
    }

    internal static class NarrativeRuntimeInstaller
    {
        public static void Install(Transform owner) => NarrativeRuntimeBootstrap.Install(owner);
    }

    internal static class QuestRuntimeInstaller
    {
        public static void Install(Transform owner) => QuestRuntimeBootstrap.Install(owner);
    }

    internal static class NpcRuntimeInstaller
    {
        public static void Install(Transform owner)
        {
            FriendshipRuntimeBootstrap.Install(owner);
            GiftGivingRuntimeBootstrap.Install(owner);
            NpcServiceRuntimeBootstrap.Install(owner);
            CityServiceRuntimeBootstrap.Install(owner);
            NpcScheduleRuntimeBootstrap.Install(owner);
            NpcDialogueExpansionBootstrap.Install();
        }
    }

    internal static class WorldRuntimeInstaller
    {
        public static void Install(Transform owner)
        {
            WorldWeatherRuntimeBootstrap.Install(owner);
            WorldEventRuntimeBootstrap.Install(owner);
            ItemDropSpawner.Install(owner);
        }
    }

    internal static class FarmRuntimeInstaller
    {
        public static void Install(Transform owner)
        {
            FarmDailyGoalRuntimeBootstrap.Install(owner);
            FarmResourceRefreshRuntimeBootstrap.Install(owner);
            FarmAnimalRuntimeBootstrap.Install(owner);
            FarmForageRuntimeBootstrap.Install(owner);
            ShippingBinRuntimeBootstrap.Install(owner);
            FarmLotRuntimeBootstrap.Install(owner);
        }
    }

    internal static class ItemRuntimeInstaller
    {
        public static void Install(Transform owner)
        {
            ItemUseManager.Install(owner);
            ConsumableItemRuntimeBootstrap.Install(owner);
            MagicItemRuntimeBootstrap.Install(owner);
            PlayerSpellbookRuntimeBootstrap.Install(owner);
            CraftingStationRuntimeBootstrap.Install(owner);
        }
    }

    internal static class PlayerServiceRuntimeInstaller
    {
        public static void Install(Transform owner)
        {
            FonteRuntimeBootstrap.Install(owner);
            PlayerConditionRuntimeBootstrap.Install(owner);
            InferredClassRuntimeBootstrap.Install();
        }
    }

    /// <summary>
    /// Serviços de lifecycle do player (AfterSceneLoad): status receiver, death controller,
    /// respawn na Fonte, movement actions (dash/dodge/block), sprint e vitals applier.
    /// Instalados no Start() do root, nunca em InstallRuntimeServices(). O alvo de attach de
    /// cada serviço é preservado: alguns criam host novo (SetParent no root), outros anexam
    /// componente em GameObject já existente (statusManager, player, GameBootstrap) — nesse
    /// caso o owner é ignorado, exatamente como no bootstrap original.
    /// </summary>
    internal static class PlayerLifecycleRuntimeInstaller
    {
        public static void Install(Transform owner)
        {
            PlayerStatusReceiverBootstrap.Install(owner);
            AnyaFountainRespawnFlow.Install(owner);
            // arch: quebra do par mutuo Combat|Player (2026-07-16) — registra a porta neutra
            // Foundation.SpawnGraceProvider ANTES do death controller (cujo OnEnable ja invoca a
            // graca de spawn de forma sincrona no proprio Install()). PlayerDeathController le a
            // porta em vez de nomear CindarsHope.Combat.PlayerDamageReceiver diretamente.
            CindarsHope.Foundation.SpawnGraceProvider.GrantSpawnGrace = PlayerDamageReceiver.GrantSpawnGrace;
            PlayerDeathController.Install(owner);
            PlayerMovementActionRuntimeBootstrap.Install(owner);
            PlayerSprintControllerBootstrap.Install(owner);
            PlayerVitalsApplierBootstrap.Install(owner);
        }
    }

    /// <summary>
    /// Serviços de apresentação (narrative intro + UI/HUD/cena) que dependem de cena
    /// (AfterSceneLoad): intro sequence, painéis IMGUI de character/inventory/skill tree,
    /// death screen canvas, HUD gameplay e overlay de fade de cena. Instalados no Start() do
    /// root, nunca em InstallRuntimeServices(). Todos são singletons cross-scene
    /// (DontDestroyOnLoad) — o owner vira o parent do host novo, preservando esse lifecycle.
    /// </summary>
    internal static class PresentationRuntimeInstaller
    {
        public static void Install(Transform owner)
        {
            CindarsHope.Narrative.IntroSequenceController.Install(owner);
            CindarsHope.UI.Character.CharacterEquipmentPanelController.Install(owner);
            CindarsHope.UI.Death.DeathScreenCanvasController.Install(owner);
            CindarsHope.UI.HUD.GameplayHudBootstrap.Install(owner);
            CindarsHope.UI.InventoryPanelController.Install(owner);
            CindarsHope.UI.Quests.Runtime.QuestOfferPanelController.Install(owner);
            CindarsHope.UI.Quests.Runtime.QuestLogPanelController.Install(owner);
            CindarsHope.UI.Skills.SkillTreeGameplayPanelController.Install(owner);
            CindarsHope.World.Scenes.SceneFadeOverlayBootstrap.Install(owner);
        }
    }

    /// <summary>
    /// Áudio (fable_58) — host de SFX/música e o bridge de eventos. Instalado por ÚLTIMO no
    /// Start() do root (pós-cena, AfterSceneLoad-equivalente): o AudioManager decide criar seu
    /// AudioListener único só depois que o AudioListener da cena (se houver) já existe,
    /// preservando a semântica original. AudioManager antes do SfxEventBridge (que consome PlaySfx).
    /// </summary>
    internal static class AudioRuntimeInstaller
    {
        public static void Install(Transform owner)
        {
            AudioManager.Install(owner);
            SfxEventBridge.Install(owner);
        }
    }
}
