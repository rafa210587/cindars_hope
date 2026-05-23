#if UNITY_EDITOR
using CindarsHope.Equipment;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor
{
    [InitializeOnLoad]
    public class EquipmentDataInitializer
    {
        private const string EquipmentPath = "Assets/_Game/Data/Equipment/";
        private const string InitKey = "EquipmentDataInitialized";

        static EquipmentDataInitializer()
        {
            if (!SessionState.GetBool(InitKey, false))
            {
                SessionState.SetBool(InitKey, true);
                GenerateDefaultEquipment();
            }
        }

        private static void GenerateDefaultEquipment()
        {
            var equipDir = "Assets/_Game/Data/Equipment";
            if (!AssetDatabase.IsValidFolder(equipDir))
            {
                var parentDir = AssetDatabase.IsValidFolder("Assets/_Game/Data") ? "Assets/_Game/Data" : "Assets/_Game";
                AssetDatabase.CreateFolder(parentDir, "Equipment");
            }

            CreateWeapons();
            CreateArmor();
            CreateAccessories();

            AssetDatabase.SaveAssets();
        }

        private static void CreateWeapons()
        {
            var weapons = new[]
            {
                ("equipment_weapon_sword_iron", "Iron Sword", EquipmentType.Weapon, 10, 3, 0, 0, 0, 0, 0, 50),
                ("equipment_weapon_spear_wood", "Wooden Spear", EquipmentType.Weapon, 8, 0, 2, 0, 0, 0, 0, 30),
                ("equipment_weapon_staff_oak", "Oak Staff", EquipmentType.Weapon, 5, 0, 0, 3, 1, 0, 0, 40),
            };

            foreach (var (id, name, type, defense, str, dex, intel, will, const_, breath, value) in weapons)
            {
                CreateEquipmentDataSO(id, name, type, defense, str, dex, intel, will, const_, breath, 0, 0, 10, value);
            }
        }

        private static void CreateArmor()
        {
            var armor = new[]
            {
                ("equipment_armor_leather", "Leather Armor", EquipmentType.Armor, 5, 0, 1, 0, 0, 1, 0, 15, 60),
                ("equipment_armor_cloth", "Cloth Armor", EquipmentType.Armor, 2, 0, 0, 1, 1, 0, 0, 5, 40),
                ("equipment_armor_iron", "Iron Armor", EquipmentType.Armor, 10, 0, 0, 0, 0, 2, 0, 20, 80),
            };

            foreach (var (id, name, type, defense, str, dex, intel, will, const_, breath, heat, value) in armor)
            {
                CreateEquipmentDataSO(id, name, type, defense, str, dex, intel, will, const_, breath, heat, 0, 25, value);
            }
        }

        private static void CreateAccessories()
        {
            var accessories = new[]
            {
                ("equipment_amulet_fire", "Fire Amulet", EquipmentType.Accessory, 0, 0, 0, 1, 0, 0, 0, 10, 50),
                ("equipment_ring_strength", "Strength Ring", EquipmentType.Accessory, 0, 2, 0, 0, 0, 0, 0, 0, 40),
            };

            foreach (var (id, name, type, defense, str, dex, intel, will, const_, breath, heat, value) in accessories)
            {
                CreateEquipmentDataSO(id, name, type, defense, str, dex, intel, will, const_, breath, heat, 0, 5, value);
            }
        }

        private static void CreateEquipmentDataSO(
            string id,
            string displayName,
            EquipmentType type,
            int defense,
            int str,
            int dex,
            int intel,
            int will,
            int const_,
            int breath,
            int heat,
            int cold,
            int weight,
            int value)
        {
            var path = $"{EquipmentPath}{id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<EquipmentDataSO>(path);
            if (existing != null)
                return;

            var asset = ScriptableObject.CreateInstance<EquipmentDataSO>();
            asset.Id = id;
            asset.DisplayName = displayName;
            asset.Description = $"Equipment: {displayName}";
            asset.Type = type;
            asset.BaseDefense = defense;
            asset.StrengthBonus = str;
            asset.DexterityBonus = dex;
            asset.IntelligenceBonus = intel;
            asset.WillpowerBonus = will;
            asset.ConstitutionBonus = const_;
            asset.BreathBonus = breath;
            asset.HeatResistance = heat;
            asset.ColdResistance = cold;
            asset.Weight = weight;
            asset.BaseValue = value;

            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
#endif
