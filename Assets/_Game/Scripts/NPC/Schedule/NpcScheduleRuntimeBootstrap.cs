using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using UnityEngine;

namespace CindarsHope.NPC.Schedule
{
    /// <summary>
    /// Ensures the <see cref="NpcScheduleService"/> singleton exists after scene load and wires it:
    /// registers every <see cref="NpcScheduleAnchor"/>, injects the <see cref="GameTimeManager"/> hour
    /// source, and registers all NPC controllers + their schedule profiles.
    ///
    /// fable_11: the controller/profile registration closes the SCENE_WIRING_DEBT — without it the
    /// WAVE25 service stayed inert. FindObjectsOfType is permitted in bootstrap self-wiring (not
    /// runtime gameplay communication); the existing pragma idiom is preserved.
    /// </summary>
    public static class NpcScheduleRuntimeBootstrap
    {
#pragma warning disable CS0618
        public static NpcScheduleService Install(Transform owner)
        {
            var service = NpcScheduleService.Instance;
            if (service == null)
            {
                var go = new GameObject("NpcScheduleService");
                if (owner != null) go.transform.SetParent(owner, false);
                else Object.DontDestroyOnLoad(go);
                service = go.AddComponent<NpcScheduleService>();
                Debug.Log("[NpcScheduleRuntimeBootstrap] NpcScheduleService created.");
            }

            WireTimeSource(service);
            RegisterAnchors(service);
            RegisterControllers(service);
            return service;
        }
#pragma warning restore CS0618

        private static void WireTimeSource(NpcScheduleService service)
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                return;
            }

            var timeManager = bootstrap.GetComponent<GameTimeManager>();
            if (timeManager != null)
            {
                service.SetTimeManager(timeManager);
            }
        }

        private static void RegisterAnchors(NpcScheduleService service)
        {
#pragma warning disable CS0618
            var anchors = Object.FindObjectsOfType<NpcScheduleAnchor>();
#pragma warning restore CS0618
            foreach (var anchor in anchors)
            {
                service.RegisterAnchor(anchor);
            }

            if (anchors.Length > 0)
            {
                Debug.Log($"[NpcScheduleRuntimeBootstrap] Registered {anchors.Length} NpcScheduleAnchor(s).");
            }
        }

        private static void RegisterControllers(NpcScheduleService service)
        {
#pragma warning disable CS0618
            var dialogueControllers = Object.FindObjectsOfType<NpcController>();
            var shopControllers = Object.FindObjectsOfType<NpcShopController>();
#pragma warning restore CS0618

            foreach (var controller in dialogueControllers)
            {
                RegisterWithProfile(service, controller.NpcData, hasShop: false);
                service.RegisterNpcController(controller);
            }

            foreach (var controller in shopControllers)
            {
                RegisterWithProfile(service, controller.NpcData, hasShop: true);
                service.RegisterShopController(controller);
            }

            if (dialogueControllers.Length + shopControllers.Length > 0)
            {
                Debug.Log($"[NpcScheduleRuntimeBootstrap] Registered {dialogueControllers.Length} dialogue + " +
                          $"{shopControllers.Length} shop NPC controller(s) with schedule profiles.");
            }
        }

        private static void RegisterWithProfile(NpcScheduleService service, NpcDataSO npcData, bool hasShop)
        {
            if (npcData == null || string.IsNullOrEmpty(npcData.NpcId))
            {
                return;
            }

            var archetype = ResolveArchetype(npcData.NpcId, hasShop);
            service.RegisterProfile(NpcScheduleProfile.CreateForArchetype(npcData.NpcId, archetype));
        }

        /// <summary>
        /// Canonical archetype for a town NPC (fable_11 / city_rules.md Rule 4):
        /// Yael &amp; Maelor are night vendors (inverted); other shop NPCs are day shopkeepers; NPCs
        /// without a shop default to wanderer/social. Guards keep shopkeeper-equivalent daytime hours.
        /// </summary>
        private static NpcScheduleArchetype ResolveArchetype(string npcId, bool hasShop)
        {
            if (npcId == "npc_yael" || npcId == "npc_maelor")
            {
                return NpcScheduleArchetype.Night;
            }

            return hasShop ? NpcScheduleArchetype.Shopkeeper : NpcScheduleArchetype.Wanderer;
        }
    }
}
