using System;
using System.Collections.Generic;
using CindarsHope.City.Services;
using CindarsHope.World;
using UnityEngine;

namespace CindarsHope.Editor.SceneCreation
{
    /// <summary>
    /// Generator-side, Town-only composition of the existing physical door with the canonical
    /// NPC schedule. This is deliberately a small mapping rather than a second shop schedule:
    /// provider IDs already own availability in NpcScheduleBlockResolver/NpcScheduleService.
    /// </summary>
    public static class TownShopDoorHoursWiring
    {
        private static readonly IReadOnlyDictionary<string, string> ProviderByHouse =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["House_Bakery"] = "npc_mella",
                ["House_Fishery"] = "npc_sael",
                ["House_AlchemyLab"] = "npc_ozzra",
                ["House_Blacksmith"] = "npc_brumdar",
                ["House_Inn"] = "npc_orlan",
                ["House_Tannery"] = "npc_hess",
                ["House_Workshop"] = "npc_nimble",
                ["House_Residential_2"] = "npc_renko",
                ["House_Residential_3"] = "npc_mirela"
            };

        public static bool TryGetProvider(string houseName, out string providerNpcId) =>
            ProviderByHouse.TryGetValue(houseName ?? string.Empty, out providerNpcId);

        /// <summary>
        /// Attaches a policy only to a shop-hours-gated Town lot. The interior trigger is supplied
        /// by the existing reveal setup so the shared door can keep an occupant inside while closed
        /// hours block only external entry.
        /// </summary>
        public static void Configure(string houseName, HouseDoorInteractable door, BoxCollider2D interiorOccupancy)
        {
            if (TownKeyartDoorClassification.For(houseName) != TownKeyartDoorClassification.ShopHoursGated)
            {
                return;
            }
            if (door == null) throw new ArgumentNullException(nameof(door));
            if (interiorOccupancy == null) throw new ArgumentNullException(nameof(interiorOccupancy));
            if (!TryGetProvider(houseName, out var providerNpcId))
            {
                throw new InvalidOperationException("Missing Town shop provider mapping: " + houseName);
            }

            var policy = door.GetComponent<TownShopDoorHoursPolicy>();
            if (policy == null) policy = door.gameObject.AddComponent<TownShopDoorHoursPolicy>();
            policy.Configure(door, providerNpcId, interiorOccupancy);
        }

        public static bool Validate(out string reason)
        {
            foreach (var lot in TownCityLayout.AllBuildings)
            {
                var isShop = TownKeyartDoorClassification.For(lot.Name) == TownKeyartDoorClassification.ShopHoursGated;
                if (isShop != ProviderByHouse.ContainsKey(lot.Name))
                {
                    reason = "Town shop-hours mapping diverges from classification: " + lot.Name;
                    return false;
                }
            }

            reason = "All nine shop-hours-gated Town lots map to canonical provider NPC IDs.";
            return true;
        }
    }
}
