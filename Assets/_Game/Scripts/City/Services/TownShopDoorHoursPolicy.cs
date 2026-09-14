using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.NPC.Schedule;
using CindarsHope.World;
using UnityEngine;

namespace CindarsHope.City.Services
{
    /// <summary>
    /// Town-only opt-in adapter for a physical shop door. It delegates hours to the canonical
    /// NPC schedule resolver, so a vendor's shop menu and its door use the same availability
    /// windows. It never moves actors or disables a doorway directly.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TownShopDoorHoursPolicy : MonoBehaviour, IHouseDoorAccessPolicy
    {
        private const string ClosedDayMessage = "Loja fechada. Horário: 09:00–22:00.";
        private const string ClosedNightMessage = "Loja fechada. Horário: 20:00–02:00.";
        private const string UnwiredMessage = "Loja indisponível agora.";

        [SerializeField] private HouseDoorInteractable _door;
        [SerializeField] private string _providerNpcId;
        [SerializeField] private BoxCollider2D _interiorOccupancy;

        private void OnEnable()
        {
            GameEventBus.Subscribe<GameTimeTickEvent>(HandleTimeTick);
            BindDoor();
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<GameTimeTickEvent>(HandleTimeTick);
            if (_door != null) _door.ConfigureAccessPolicy(null);
        }

        /// <summary>Called by the Town generator after it attaches this policy to a classified shop door.</summary>
        public void Configure(HouseDoorInteractable door, string providerNpcId, BoxCollider2D interiorOccupancy)
        {
            _door = door;
            _providerNpcId = providerNpcId;
            _interiorOccupancy = interiorOccupancy;
            BindDoor();
        }

        public bool CanOpen(out string unavailableReason)
        {
            if (string.IsNullOrWhiteSpace(_providerNpcId) || !TryGetCurrentHour(out var hour))
            {
                unavailableReason = UnwiredMessage;
                return false;
            }

            return TownShopDoorHours.IsOpen(_providerNpcId, hour, out unavailableReason);
        }

        private void HandleTimeTick(GameTimeTickEvent _)
        {
            // HouseDoorInteractable owns safe closure. If a player/NPC occupies the doorway it
            // defers closing, then retries through its normal update path after the doorway clears.
            if (_door != null) _door.EnforceAccessPolicy();
        }

        private void BindDoor()
        {
            if (_door != null) _door.ConfigureAccessPolicy(this, _interiorOccupancy);
        }

        private static bool TryGetCurrentHour(out int hour)
        {
            var timeManager = GameBootstrap.Instance != null ? GameBootstrap.Instance.GameTimeManager : null;
            if (timeManager == null)
            {
                hour = 0;
                return false;
            }

            hour = timeManager.CurrentHourOfDay;
            return true;
        }

        public static class TownShopDoorHours
        {
            public static bool IsOpen(string providerNpcId, int hour, out string unavailableReason)
            {
                var archetype = IsNightProvider(providerNpcId)
                    ? NpcScheduleArchetype.Night
                    : NpcScheduleArchetype.Shopkeeper;
                if (NpcScheduleBlockResolver.IsAvailable(archetype, hour))
                {
                    unavailableReason = null;
                    return true;
                }

                unavailableReason = archetype == NpcScheduleArchetype.Night
                    ? ClosedNightMessage
                    : ClosedDayMessage;
                return false;
            }

            private static bool IsNightProvider(string npcId) =>
                string.Equals(npcId, "npc_yael", System.StringComparison.Ordinal) ||
                string.Equals(npcId, "npc_maelor", System.StringComparison.Ordinal);
        }
    }
}
