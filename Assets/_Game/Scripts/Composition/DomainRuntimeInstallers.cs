using CindarsHope.Cave.Ecosystem;
using CindarsHope.Crafting;
using CindarsHope.City.Services;
using CindarsHope.NPC.Friendship;
using CindarsHope.NPC.Gifting;
using CindarsHope.NPC.Services;
using CindarsHope.World.Events;
using CindarsHope.World.Weather;
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

    internal static class NpcRuntimeInstaller
    {
        public static void Install(Transform owner)
        {
            FriendshipRuntimeBootstrap.Install(owner);
            GiftGivingRuntimeBootstrap.Install(owner);
            NpcServiceRuntimeBootstrap.Install(owner);
            CityServiceRuntimeBootstrap.Install(owner);
        }
    }

    internal static class WorldRuntimeInstaller
    {
        public static void Install(Transform owner)
        {
            WorldWeatherRuntimeBootstrap.Install(owner);
            WorldEventRuntimeBootstrap.Install(owner);
        }
    }
}
