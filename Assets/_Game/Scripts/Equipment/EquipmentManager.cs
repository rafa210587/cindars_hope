using CindarsHope.Tools;
using UnityEngine;

namespace CindarsHope.Equipment
{
    [DisallowMultipleComponent]
    public class EquipmentManager : MonoBehaviour
    {
        [SerializeField] private string _equippedToolId = string.Empty;
        [SerializeField] private ToolType _equippedToolType = ToolType.None;
        [SerializeField] private ToolTier _equippedToolTier = ToolTier.None;
        [SerializeField] private string _equippedWeaponId = string.Empty;

        public string EquippedToolId => _equippedToolId;
        public ToolType EquippedToolType => _equippedToolType;
        public ToolTier EquippedToolTier => _equippedToolTier;
        public string EquippedWeaponId => _equippedWeaponId;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                CycleDebugTool();
            }
        }

        public void EquipTool(string toolId, ToolType toolType, ToolTier tier)
        {
            _equippedToolId = toolId ?? string.Empty;
            _equippedToolType = toolType;
            _equippedToolTier = tier;
            Debug.Log($"EquipmentManager: equipped tool {_equippedToolId} ({_equippedToolType}/{_equippedToolTier}).", this);
        }

        public void EquipWeapon(string weaponId)
        {
            _equippedWeaponId = weaponId ?? string.Empty;
            Debug.Log($"EquipmentManager: equipped weapon {_equippedWeaponId}.", this);
        }

        public EquipmentSaveData CaptureSaveData()
        {
            return new EquipmentSaveData
            {
                EquippedToolId = _equippedToolId,
                EquippedWeaponId = _equippedWeaponId
            };
        }

        public void RestoreFromSaveData(EquipmentSaveData saveData)
        {
            if (saveData == null)
            {
                _equippedToolId = string.Empty;
                _equippedToolType = ToolType.None;
                _equippedToolTier = ToolTier.None;
                _equippedWeaponId = string.Empty;
                return;
            }

            _equippedToolId = saveData.EquippedToolId ?? string.Empty;
            _equippedWeaponId = saveData.EquippedWeaponId ?? string.Empty;
            InferEquippedToolFromId();
        }

        private void CycleDebugTool()
        {
            switch (_equippedToolType)
            {
                case ToolType.None:
                    EquipTool("item_tool_hoe_basic", ToolType.Hoe, ToolTier.Basic);
                    break;
                case ToolType.Hoe:
                    EquipTool("item_tool_axe_basic", ToolType.Axe, ToolTier.Basic);
                    break;
                case ToolType.Axe:
                    EquipTool("item_tool_pickaxe_basic", ToolType.Pickaxe, ToolTier.Basic);
                    break;
                case ToolType.Pickaxe:
                    EquipTool("item_tool_fishing_rod_basic", ToolType.FishingRod, ToolTier.Basic);
                    break;
                default:
                    EquipTool(string.Empty, ToolType.None, ToolTier.None);
                    break;
            }
        }

        private void InferEquippedToolFromId()
        {
            _equippedToolTier = string.IsNullOrWhiteSpace(_equippedToolId) ? ToolTier.None : ToolTier.Basic;

            if (_equippedToolId.Contains("hoe"))
            {
                _equippedToolType = ToolType.Hoe;
            }
            else if (_equippedToolId.Contains("axe"))
            {
                _equippedToolType = ToolType.Axe;
            }
            else if (_equippedToolId.Contains("pickaxe"))
            {
                _equippedToolType = ToolType.Pickaxe;
            }
            else if (_equippedToolId.Contains("fishing_rod"))
            {
                _equippedToolType = ToolType.FishingRod;
            }
            else
            {
                _equippedToolType = ToolType.None;
                _equippedToolTier = ToolTier.None;
            }
        }
    }
}
