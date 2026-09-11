#if UNITY_EDITOR
using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core.Data;
using CindarsHope.Foundation;
using CindarsHope.Inventory.Data;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.EditorTools.Combat
{
    /// <summary>
    /// Fecha o débito WEAPON_ITEM_NO_WEAPON_ID: para cada ItemDataSO de arma do catálogo
    /// (item_weapon_*), garante que exista um WeaponDataSO correspondente (weapon_&lt;sufixo&gt;) no
    /// WeaponDatabase e LIGA item.WeaponId determinístico por id. PREFERE LINKAR a já-existentes
    /// (system-reuse-audit): se o weapon_&lt;sufixo&gt; já existe, só liga o item — não recria nem reseta.
    ///
    /// IMPORTANTE (no-magic-balance): este gerador NÃO inventa números de balanceamento por tier.
    /// Os campos MECÂNICOS canônicos por arquétipo (ASPD, scaling, custo de stamina, posture, peso,
    /// charged profile) são preenchidos pelo gerador canônico já existente
    /// <see cref="ApplyWeaponMechanicalBaselines"/> — que roda DEPOIS deste no orquestrador. Aqui só
    /// criamos o esqueleto do WeaponDataSO (Id, DisplayName, Type, DamageType e os campos de combate
    /// clonados do baseline de referência do MESMO arquétipo quando existe). Arquétipos sem asset de
    /// referência (dagger/axe/hammer/wand/tool) recebem um baseline conservador e a diferenciação de
    /// tier/dano fica DEFERIDA até uma tabela canônica de dano por tier ser autorada (logado por arma).
    ///
    /// Idempotente por id. Não deleta. Run via menu CindarsHope/Inicializar Projeto (orquestrador) ou
    /// batchmode -executeMethod CindarsHope.EditorTools.Combat.GenerateWeaponDataFromCatalog.Generate.
    /// </summary>
    public static class GenerateWeaponDataFromCatalog
    {
        private const string ItemsDir = "Assets/_Game/Data/Items";
        private const string WeaponsDir = "Assets/_Game/Data/Combat/Weapons";
        private const string WeaponDatabasePath = "Assets/_Game/Data/Combat/WeaponDatabase.asset";
        private const string Tag = "[GenWeaponData]";
        private const string ItemWeaponPrefix = "item_weapon_";

        // Baselines de referência por arquétipo (assets canônicos já existentes). Quando há um, os
        // campos de combate do WeaponDataSO novo são clonados dele (sem inventar números). Quando não há,
        // usa-se o fallback conservador de ResolveFallbackCombat — diferenciação de tier DEFERIDA.
        private static readonly Dictionary<WeaponType, string> ReferenceBaselineByType =
            new Dictionary<WeaponType, string>
            {
                { WeaponType.Sword, "weapon_sword_iron" },
                { WeaponType.Bow,   "weapon_bow_basic" },
                { WeaponType.Spear, "weapon_spear_wood" },
                { WeaponType.Staff, "weapon_staff_oak" },
            };

        public static void Generate()
        {
            EnsureFolder(WeaponsDir);

            var weaponItems = LoadWeaponItems();
            var referenceCache = new Dictionary<WeaponType, WeaponDataSO>();

            int weaponsCreated = 0, weaponsLinked = 0, itemsLinked = 0, deferredTier = 0;
            var generated = new List<WeaponDataSO>();

            foreach (var item in weaponItems)
            {
                var suffix = item.Id.Substring(ItemWeaponPrefix.Length);
                var weaponId = "weapon_" + suffix;
                var weaponPath = $"{WeaponsDir}/{weaponId}.asset";
                var type = InferWeaponType(item.Id);

                var weapon = AssetDatabase.LoadAssetAtPath<WeaponDataSO>(weaponPath);
                bool created = false;
                if (weapon == null)
                {
                    weapon = ScriptableObject.CreateInstance<WeaponDataSO>();
                    AssetDatabase.CreateAsset(weapon, weaponPath);
                    created = true;
                    weaponsCreated++;
                }
                else
                {
                    weaponsLinked++;
                }

                if (created)
                {
                    weapon.Id = weaponId;
                    weapon.DisplayName = string.IsNullOrEmpty(item.DisplayName) ? suffix : item.DisplayName;
                    weapon.Description = $"Weapon: {weapon.DisplayName}";
                    weapon.Type = type;

                    var reference = ResolveReference(type, referenceCache, weaponPath);
                    if (reference != null)
                    {
                        CloneCombatFields(reference, weapon);
                    }
                    else
                    {
                        ResolveFallbackCombat(type, weapon);
                        deferredTier++;
                        Debug.Log($"{Tag} '{weaponId}' ({type}): sem baseline de referência do arquétipo — " +
                                  "baseline conservador aplicado; diferenciação de tier/dano DEFERIDA " +
                                  "(autorar tabela canônica de dano por tier).");
                    }

                    EditorUtility.SetDirty(weapon);
                    generated.Add(weapon);
                }

                bool requiresTwoHands = RequiresTwoHands(type);
                if (weapon.RequiresTwoHands != requiresTwoHands)
                {
                    weapon.RequiresTwoHands = requiresTwoHands;
                    EditorUtility.SetDirty(weapon);
                }

                // Liga o item à arma (determinístico). Só escreve se mudou (idempotência).
                if (item.WeaponId != weaponId)
                {
                    item.WeaponId = weaponId;
                    EditorUtility.SetDirty(item);
                    itemsLinked++;
                }
            }

            // Legacy weapons may predate item_weapon_* catalog entries. Handedness is a combat
            // contract, so materialize it for every WeaponDataSO in the canonical directory.
            foreach (var guid in AssetDatabase.FindAssets("t:WeaponDataSO", new[] { WeaponsDir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var weapon = AssetDatabase.LoadAssetAtPath<WeaponDataSO>(path);
                if (weapon == null) continue;
                var type = weapon.Type != WeaponType.None ? weapon.Type : InferWeaponType(weapon.Id ?? path);
                bool requiresTwoHands = RequiresTwoHands(type);
                if (weapon.RequiresTwoHands == requiresTwoHands) continue;
                weapon.RequiresTwoHands = requiresTwoHands;
                EditorUtility.SetDirty(weapon);
            }

            int registered = RegisterInWeaponDatabase(generated);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"{Tag} Done. WeaponItems={weaponItems.Count}, WeaponsCreated={weaponsCreated}, " +
                      $"WeaponsAlreadyExisted={weaponsLinked}, ItemWeaponIdLinked={itemsLinked}, " +
                      $"RegisteredInDb={registered}, TierTuningDeferred={deferredTier}. " +
                      "Rode ApplyWeaponMechanicalBaselines em seguida (orquestrador faz isso) para os campos mecânicos.");
        }

        private static List<ItemDataSO> LoadWeaponItems()
        {
            var list = new List<ItemDataSO>();
            foreach (var guid in AssetDatabase.FindAssets("t:ItemDataSO", new[] { ItemsDir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);
                if (item == null || string.IsNullOrEmpty(item.Id))
                {
                    continue;
                }

                // Armas do catálogo: prefixo canônico item_weapon_ (cobre os ~30 ids). Wands/tools
                // entram pelo mesmo prefixo (item_weapon_wand_*, item_weapon_tool_*).
                if (item.Id.StartsWith(ItemWeaponPrefix, System.StringComparison.Ordinal))
                {
                    list.Add(item);
                }
            }
            return list;
        }

        // Infere o WeaponType a partir do id (item_weapon_<arquetipo>_<material>). Determinístico.
        private static WeaponType InferWeaponType(string itemId)
        {
            var id = itemId.ToLowerInvariant();
            if (id.Contains("_dagger_")) return WeaponType.Dagger;
            if (id.Contains("_sword_")) return WeaponType.Sword;
            if (id.Contains("_spear_")) return WeaponType.Spear;
            if (id.Contains("_axe_")) return WeaponType.Axe;
            if (id.Contains("_hammer_")) return WeaponType.Hammer;
            if (id.Contains("_bow_")) return WeaponType.Bow;
            if (id.Contains("_staff_")) return WeaponType.Staff;
            if (id.Contains("_wand_")) return WeaponType.Wand;
            if (id.Contains("_tool_")) return WeaponType.Tool;
            return WeaponType.Sword; // default seguro de arquétipo (melee 1-mão).
        }

        internal static bool RequiresTwoHands(WeaponType type)
        {
            return type == WeaponType.Spear
                || type == WeaponType.Axe
                || type == WeaponType.Hammer
                || type == WeaponType.Bow
                || type == WeaponType.Staff;
        }

        private static WeaponDataSO ResolveReference(
            WeaponType type, Dictionary<WeaponType, WeaponDataSO> cache, string skipPath)
        {
            if (cache.TryGetValue(type, out var cached))
            {
                return cached;
            }

            WeaponDataSO reference = null;
            if (ReferenceBaselineByType.TryGetValue(type, out var refId))
            {
                var refPath = $"{WeaponsDir}/{refId}.asset";
                if (refPath != skipPath)
                {
                    reference = AssetDatabase.LoadAssetAtPath<WeaponDataSO>(refPath);
                }
            }

            cache[type] = reference;
            return reference;
        }

        // Copia SÓ os campos de combate "brutos" do baseline de referência do mesmo arquétipo. Não copia
        // Id/DisplayName/Icon. Os campos mecânicos canônicos (§2) ficam para ApplyWeaponMechanicalBaselines.
        private static void CloneCombatFields(WeaponDataSO from, WeaponDataSO to)
        {
            to.DamageType = from.DamageType;
            to.BaseDamage = Mathf.Max(1, from.BaseDamage);
            to.BaseCooldownSeconds = from.BaseCooldownSeconds;
            to.StaminaCost = from.StaminaCost;
            to.Range = from.Range;
            to.ArcDegrees = from.ArcDegrees;
            to.AttackSpeedMultiplier = from.AttackSpeedMultiplier;
            to.CriticalChance = from.CriticalChance;
            to.CooldownMs = from.CooldownMs;
            to.RequiredStrength = from.RequiredStrength;
            to.RequiredDexterity = from.RequiredDexterity;
            to.DurabilityMax = from.DurabilityMax;
            to.ProjectileSpeed = from.ProjectileSpeed;
            to.ProjectilePrefab = from.ProjectilePrefab;
        }

        // Baseline conservador para arquétipos SEM asset de referência. Usa apenas valores estruturais
        // (range/arc/cooldown por forma da arma) e um dano base mínimo placeholder (clamp do OnValidate)
        // — NUNCA uma curva de dano por tier inventada. Diferenciação real fica deferida.
        private static void ResolveFallbackCombat(WeaponType type, WeaponDataSO w)
        {
            w.DamageType = DamageType.Physical;
            w.BaseDamage = 1;            // placeholder mínimo (OnValidate clampa para >=1); tier deferido.
            w.BaseCooldownSeconds = 0.5f;
            w.StaminaCost = 10f;
            w.CriticalChance = 5;
            w.CooldownMs = 500;
            w.DurabilityMax = 100;

            switch (type)
            {
                case WeaponType.Dagger:
                    w.Range = 0.9f; w.ArcDegrees = 90f; break;
                case WeaponType.Axe:
                case WeaponType.Hammer:
                    w.Range = 1.1f; w.ArcDegrees = 140f; break;
                case WeaponType.Wand:
                    w.DamageType = DamageType.Arcane; w.Range = 4f; w.ArcDegrees = 30f; break;
                case WeaponType.Tool:
                    w.Range = 1f; w.ArcDegrees = 90f; break;
                default:
                    w.Range = 1f; w.ArcDegrees = 120f; break;
            }
        }

        private static int RegisterInWeaponDatabase(List<WeaponDataSO> weapons)
        {
            if (weapons == null || weapons.Count == 0)
            {
                return 0;
            }

            var database = AssetDatabase.LoadAssetAtPath<WeaponDatabaseSO>(WeaponDatabasePath);
            if (database == null)
            {
                Debug.LogWarning($"{Tag} WeaponDatabase não encontrado em {WeaponDatabasePath}; armas criadas mas não registradas.");
                return 0;
            }

            var so = new SerializedObject(database);
            var prop = so.FindProperty("_items");
            if (prop == null || !prop.isArray)
            {
                Debug.LogWarning($"{Tag} '_items' não encontrado no WeaponDatabase; registro pulado.");
                return 0;
            }

            var present = new HashSet<Object>();
            for (int i = 0; i < prop.arraySize; i++)
            {
                var existing = prop.GetArrayElementAtIndex(i).objectReferenceValue;
                if (existing != null)
                {
                    present.Add(existing);
                }
            }

            int added = 0;
            foreach (var weapon in weapons)
            {
                if (weapon == null || present.Contains(weapon))
                {
                    continue;
                }

                prop.arraySize++;
                prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = weapon;
                present.Add(weapon);
                added++;
            }

            if (added > 0)
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(database);
            }

            return added;
        }

        private static void EnsureFolder(string dir)
        {
            if (AssetDatabase.IsValidFolder(dir))
            {
                return;
            }

            var parts = dir.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
#endif
