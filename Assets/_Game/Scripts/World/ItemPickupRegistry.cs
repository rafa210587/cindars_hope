using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.World
{
    [DisallowMultipleComponent]
    public class ItemPickupRegistry : MonoBehaviour
    {
        [SerializeField] private ItemPickup[] _pickups;

        public IReadOnlyList<ItemPickup> Pickups => _pickups;

        public void Configure(ItemPickup[] pickups)
        {
            _pickups = pickups ?? new ItemPickup[0];
        }

        public List<ItemPickupSaveData> CaptureSaveData()
        {
            var pickups = new List<ItemPickupSaveData>();
            if (_pickups == null)
            {
                return pickups;
            }

            foreach (var pickup in _pickups)
            {
                if (pickup == null)
                {
                    continue;
                }

                pickups.Add(pickup.CaptureSaveData());
            }

            return pickups;
        }

        public void RestoreFromSaveData(List<ItemPickupSaveData> pickupsData)
        {
            if (pickupsData == null || _pickups == null)
            {
                return;
            }

            foreach (var pickupData in pickupsData)
            {
                if (pickupData == null)
                {
                    continue;
                }

                var pickup = GetPickupByIndex(pickupData.PickupIndex);
                if (pickup == null)
                {
                    Debug.LogWarning($"ItemPickupRegistry skipped saved pickup index {pickupData.PickupIndex} because no matching pickup exists.", this);
                    continue;
                }

                pickup.RestoreFromSaveData(pickupData);
            }
        }

        private ItemPickup GetPickupByIndex(int pickupIndex)
        {
            if (_pickups == null)
            {
                return null;
            }

            foreach (var pickup in _pickups)
            {
                if (pickup != null && pickup.PickupIndex == pickupIndex)
                {
                    return pickup;
                }
            }

            return null;
        }
    }
}
