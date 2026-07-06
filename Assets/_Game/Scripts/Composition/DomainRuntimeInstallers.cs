using CindarsHope.Cave.Death;
using CindarsHope.Cave.Ecosystem;
using CindarsHope.Cave.Runtime;
using CindarsHope.Combat.StatusEffect;
using CindarsHope.Combat.Telemetry;
using CindarsHope.Crafting;
using CindarsHope.City.Services;
using CindarsHope.Narrative;
using CindarsHope.NPC.Friendship;
using CindarsHope.NPC.Gifting;
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
            PlayerDeathController.Install(owner);
            PlayerMovementActionRuntimeBootstrap.Install(owner);
            PlayerSprintControllerBootstrap.Install(owner);
            PlayerVitalsApplierBootstrap.Install(owner);
        }
    }
}
