#if UNITY_EDITOR
using CindarsHope.Combat.Magic;
using CindarsHope.Combat.Weapon;
using CindarsHope.Foundation;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.Editor
{
    [InitializeOnLoad]
    public class CombatDataInitializer
    {
        private const string WeaponPath = "Assets/_Game/Data/Combat/Weapons/";
        private const string SpellPath = "Assets/_Game/Data/Combat/Spells/";
        private const string InitKey = "CombatDataInitialized";

        static CombatDataInitializer()
        {
            if (!SessionState.GetBool(InitKey, false))
            {
                SessionState.SetBool(InitKey, true);
                GenerateDefaultCombatData();
            }
        }

        private static void GenerateDefaultCombatData()
        {
            CreateCombatFolders();
            CreateWeapons();
            CreateSpells();
            // CreateSkills retired 2026-06-13: it generated assets of the dead
            // CindarsHope.Combat.Skills.SkillActionSO type, incompatible with the live
            // SkillActionDatabaseSO (CindarsHope.Skills). Live catalog generation
            // belongs to fable_29 (canonical skill catalog migration).
            AssetDatabase.SaveAssets();
        }

        private static void CreateCombatFolders()
        {
            string combatDir = "Assets/_Game/Data/Combat";
            if (!AssetDatabase.IsValidFolder(combatDir))
            {
                AssetDatabase.CreateFolder("Assets/_Game/Data", "Combat");
            }
            if (!AssetDatabase.IsValidFolder(combatDir + "/Weapons"))
                AssetDatabase.CreateFolder(combatDir, "Weapons");
            if (!AssetDatabase.IsValidFolder(combatDir + "/Spells"))
                AssetDatabase.CreateFolder(combatDir, "Spells");
        }

        private static void CreateWeapons()
        {
            var weapons = new[]
            {
                ("weapon_sword_iron", "Iron Sword", WeaponType.Sword, 15, 10, 500, 15f, 5, 0),
                ("weapon_spear_wood", "Wooden Spear", WeaponType.Spear, 12, 5, 600, 20f, 3, 0),
                ("weapon_staff_oak", "Oak Staff", WeaponType.Staff, 8, 0, 1000, 10f, 0, 8),
            };

            foreach (var (id, name, type, dmg, crit, cd, stamina, str, intel) in weapons)
            {
                CreateWeapon(id, name, type, dmg, crit, cd, stamina, str, intel);
            }
        }

        private static void CreateSpells()
        {
            var spells = new[]
            {
                ("spell_fireball", "Fireball", SpellType.Fireball, SpellDiscipline.Offensive, 20, 25, 800, 4, 3),
                ("spell_ice_spike", "Ice Spike", SpellType.IceSpike, SpellDiscipline.Offensive, 18, 20, 900, 3, 4),
                ("spell_heal", "Heal", SpellType.Heal, SpellDiscipline.Spiritual, 0, 30, 1200, 5, 5),
            };

            foreach (var (id, name, type, discipline, dmg, mana, cd, intel, will) in spells)
            {
                CreateSpell(id, name, type, discipline, dmg, mana, cd, intel, will);
            }
        }

        private static void CreateWeapon(string id, string name, WeaponType type, int dmg, int crit, int cd, float stamina, int str, int intel)
        {
            var path = $"{WeaponPath}{id}.asset";
            if (AssetDatabase.LoadAssetAtPath<WeaponDataSO>(path) != null)
                return;

            var asset = ScriptableObject.CreateInstance<WeaponDataSO>();
            asset.Id = id;
            asset.DisplayName = name;
            asset.Description = $"Weapon: {name}";
            asset.Type = type;
            asset.BaseDamage = dmg;
            asset.CriticalChance = crit;
            asset.CooldownMs = cd;
            asset.StaminaCost = stamina;
            asset.RequiredStrength = str;
            asset.RequiredDexterity = intel;

            AssetDatabase.CreateAsset(asset, path);
        }

        private static void CreateSpell(string id, string name, SpellType type, SpellDiscipline discipline,
            int dmg, int mana, int cd, int intel, int will)
        {
            var path = $"{SpellPath}{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<SpellDataSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<SpellDataSO>();
                asset.Id = id;
                asset.SpellName = name;
                asset.Description = $"Spell: {name}";
                asset.Type = type;
                asset.BaseDamage = dmg;
                asset.ManaCost = mana;
                asset.CooldownMs = cd;
                asset.RequiredIntelligence = intel;
                asset.RequiredWillpower = will;
                AssetDatabase.CreateAsset(asset, path);
            }

            if (asset.Discipline != discipline)
            {
                asset.Discipline = discipline;
                EditorUtility.SetDirty(asset);
            }
        }

    }
}
#endif
