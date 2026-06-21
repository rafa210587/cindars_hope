using System.Collections.Generic;
using CindarsHope.Combat.Weapon;
using CindarsHope.Player.Progression;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.EditorTools.Combat
{
    /// <summary>
    /// F03 — aplica a MATRIZ CANÔNICA (EQUIPMENT_MECHANICAL_BASELINES §5/§6) aos assets de
    /// arma existentes por arquétipo (WeaponType). Idempotente; loga a matriz aplicada.
    /// Custos de stamina: sword 25/40/48 · dagger 16/28/36 · spear 24/38/46 ·
    /// axe 30/48/58 · bow 18/30/38 · staff 20/32/40 (light/heavy/charged).
    /// </summary>
    public static class ApplyWeaponMechanicalBaselines
    {
        private struct ArchetypeBaseline
        {
            public float Aspd;
            public PlayerAttributeType Primary;
            public float PrimaryWeight;
            public PlayerAttributeType Secondary;
            public float SecondaryWeight;
            public float LightCost;
            public float HeavyCost;
            public float ChargedCost;
            public float PostureMod;
            public WeaponWeightClass WeightClass;
            public string ChargedProfile;
        }

        private static readonly Dictionary<WeaponType, ArchetypeBaseline> Matrix = new Dictionary<WeaponType, ArchetypeBaseline>
        {
            [WeaponType.Sword] = new ArchetypeBaseline { Aspd = 1.0f, Primary = PlayerAttributeType.Strength, PrimaryWeight = 0.6f, Secondary = PlayerAttributeType.Dexterity, SecondaryWeight = 0.3f, LightCost = 25, HeavyCost = 40, ChargedCost = 48, PostureMod = 1.0f, WeightClass = WeaponWeightClass.Medium, ChargedProfile = "charged_sword_aparar" },
            [WeaponType.Dagger] = new ArchetypeBaseline { Aspd = 1.4f, Primary = PlayerAttributeType.Dexterity, PrimaryWeight = 0.8f, Secondary = PlayerAttributeType.Strength, SecondaryWeight = 0.1f, LightCost = 16, HeavyCost = 28, ChargedCost = 36, PostureMod = 0.7f, WeightClass = WeaponWeightClass.Light, ChargedProfile = "charged_dagger_focused_crit" },
            [WeaponType.Spear] = new ArchetypeBaseline { Aspd = 0.95f, Primary = PlayerAttributeType.Strength, PrimaryWeight = 0.4f, Secondary = PlayerAttributeType.Dexterity, SecondaryWeight = 0.5f, LightCost = 24, HeavyCost = 38, ChargedCost = 46, PostureMod = 1.1f, WeightClass = WeaponWeightClass.Medium, ChargedProfile = "charged_spear_impale" },
            [WeaponType.Axe] = new ArchetypeBaseline { Aspd = 0.8f, Primary = PlayerAttributeType.Strength, PrimaryWeight = 0.9f, Secondary = PlayerAttributeType.Constitution, SecondaryWeight = 0.2f, LightCost = 30, HeavyCost = 48, ChargedCost = 58, PostureMod = 1.4f, WeightClass = WeaponWeightClass.Heavy, ChargedProfile = "charged_axe_bleed" },
            [WeaponType.Bow] = new ArchetypeBaseline { Aspd = 1.0f, Primary = PlayerAttributeType.Dexterity, PrimaryWeight = 0.7f, Secondary = PlayerAttributeType.Breath, SecondaryWeight = 0.2f, LightCost = 18, HeavyCost = 30, ChargedCost = 38, PostureMod = 0.6f, WeightClass = WeaponWeightClass.Light, ChargedProfile = "charged_bow_mark" },
            [WeaponType.Staff] = new ArchetypeBaseline { Aspd = 0.9f, Primary = PlayerAttributeType.Intelligence, PrimaryWeight = 0.8f, Secondary = PlayerAttributeType.Willpower, SecondaryWeight = 0.3f, LightCost = 20, HeavyCost = 32, ChargedCost = 40, PostureMod = 0.8f, WeightClass = WeaponWeightClass.Medium, ChargedProfile = "charged_staff_arcane_channel" }
        };

        public static void Apply()
        {
            var applied = 0;
            var skipped = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:WeaponDataSO"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var weapon = AssetDatabase.LoadAssetAtPath<WeaponDataSO>(path);
                if (weapon == null)
                {
                    continue;
                }

                if (!Matrix.TryGetValue(weapon.Type, out var baseline))
                {
                    skipped++;
                    continue;
                }

                weapon.AttackSpeedMultiplier = baseline.Aspd;
                weapon.PrimaryAttribute = baseline.Primary;
                weapon.PrimaryAttributeWeight = baseline.PrimaryWeight;
                weapon.SecondaryAttribute = baseline.Secondary;
                weapon.SecondaryAttributeWeight = baseline.SecondaryWeight;
                weapon.BaseLightStaminaCost = baseline.LightCost;
                weapon.BaseHeavyStaminaCost = baseline.HeavyCost;
                weapon.BaseChargedStaminaCost = baseline.ChargedCost;
                weapon.PostureDamageModifier = baseline.PostureMod;
                weapon.WeightClass = baseline.WeightClass;
                weapon.ChargedEffectProfileId = baseline.ChargedProfile;
                EditorUtility.SetDirty(weapon);
                applied++;

                Debug.Log($"[ApplyWeaponMechanicalBaselines] {weapon.Id} ({weapon.Type}): ASPD={baseline.Aspd}, " +
                    $"Scaling={baseline.Primary}×{baseline.PrimaryWeight}+{baseline.Secondary}×{baseline.SecondaryWeight}, " +
                    $"Stamina={baseline.LightCost}/{baseline.HeavyCost}/{baseline.ChargedCost}, Posture={baseline.PostureMod}, Profile={baseline.ChargedProfile}");
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[ApplyWeaponMechanicalBaselines] {applied} arma(s) atualizadas, {skipped} sem arquétipo (None/Unarmed).");
        }
    }
}
