using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using UnityEngine;
using UnityEngine.UI;

namespace CindarsHope.UI.HUD
{
    [DisallowMultipleComponent]
    public class EquipmentHUD : MonoBehaviour
    {
        [SerializeField] private EquipmentManager _equipmentManager;
        [SerializeField] private Image _headSlot;
        [SerializeField] private Image _chestSlot;
        [SerializeField] private Image _legsSlot;
        [SerializeField] private Image _bootsSlot;
        [SerializeField] private Image _leftHandSlot;
        [SerializeField] private Image _rightHandSlot;
        [SerializeField] private Image _ring1Slot;
        [SerializeField] private Image _ring2Slot;
        [SerializeField] private Image _accessorySlot;

        private void OnEnable()
        {
            GameEventBus.Subscribe<EquipmentSlotChangedEvent>(UpdateDisplay);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<EquipmentSlotChangedEvent>(UpdateDisplay);
        }

        private void UpdateDisplay()
        {
            if (_equipmentManager == null)
                return;

            UpdateSlotDisplay(_headSlot, _equipmentManager.GetEquippedItem(EquipmentSlot.Head));
            UpdateSlotDisplay(_chestSlot, _equipmentManager.GetEquippedItem(EquipmentSlot.Chest));
            UpdateSlotDisplay(_legsSlot, _equipmentManager.GetEquippedItem(EquipmentSlot.Legs));
            UpdateSlotDisplay(_bootsSlot, _equipmentManager.GetEquippedItem(EquipmentSlot.Boots));
            UpdateSlotDisplay(_leftHandSlot, _equipmentManager.GetEquippedItem(EquipmentSlot.LeftHand));
            UpdateSlotDisplay(_rightHandSlot, _equipmentManager.GetEquippedItem(EquipmentSlot.RightHand));
            UpdateSlotDisplay(_ring1Slot, _equipmentManager.GetEquippedItem(EquipmentSlot.Ring1));
            UpdateSlotDisplay(_ring2Slot, _equipmentManager.GetEquippedItem(EquipmentSlot.Ring2));
            UpdateSlotDisplay(_accessorySlot, _equipmentManager.GetEquippedItem(EquipmentSlot.Accessory));
        }

        private void UpdateSlotDisplay(Image slotImage, string itemInstanceId)
        {
            if (slotImage == null)
                return;

            if (string.IsNullOrEmpty(itemInstanceId))
            {
                slotImage.sprite = null;
                slotImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
            else
            {
                slotImage.color = Color.white;
            }
        }

        private void UpdateDisplay(EquipmentSlotChangedEvent evt)
        {
            UpdateDisplay();
        }
    }
}
