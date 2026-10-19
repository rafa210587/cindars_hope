using System.Collections.Generic;
using CindarsHope.Inventory.Data;
using UnityEngine;
using CindarsHope.Foundation;

namespace CindarsHope.Combat
{
    /// <summary>
    /// fable_48 â€” Resolvedor PURO de balÃ­stica de flecha. Ponto ÃšNICO (regra de nÃ£o-duplicaÃ§Ã£o)
    /// que mapeia um item de muniÃ§Ã£o (id do catÃ¡logo Â§8) para seu perfil mecÃ¢nico do Â§19:
    /// <c>{arrowDamage, damageType, tags[], statusEffectId, statusChance}</c>.
    ///
    /// DeterminÃ­stico e sem efeitos colaterais (sem Random, sem Unity scene API). O dano e as tags
    /// daqui sÃ£o SOMADOS pelo <see cref="BowArrowAttackService"/> ao dano do arco (fÃ³rmula Â§18) e
    /// anexados ao projÃ©til â€” as tags entram no matching de vulnerabilidade da F06
    /// (<see cref="VulnerabilityMatcher"/>), que sÃ³ concede bÃ´nus contra vulnerabilidade declarada.
    ///
    /// Restrito aos 6 tipos do catÃ¡logo v1 (ITEM_CATALOG Â§8). Flecha desconhecida cai no
    /// comportamento WoodenArrow (<see cref="IsKnown"/> = false) com warning â€” nunca dispara erro.
    /// </summary>
    public static class ArrowBallisticsResolver
    {
        // Id legado do arsenal inicial/hotbar (SaveManager defaults + PlayerData StartingItems).
        // NÃ£o Ã© um dos 6 perfis elementais; usa a balÃ­stica da flecha de madeira (wood) â€” registrado
        // na Table abaixo para disparar SEM warning de fallback (IsCanonicalArrow=true).
        public const string ArrowBasicId = "item_ammo_arrow_basic";

        // Ids canÃ´nicos do catÃ¡logo (fable_32 CanonicalItemCatalog â€” AddOilsAndArrows).
        public const string ArrowWoodId = "item_ammo_arrow_wood";
        public const string ArrowIronId = "item_ammo_arrow_iron";
        public const string ArrowSteelId = "item_ammo_arrow_steel";
        public const string ArrowSilverId = "item_ammo_arrow_silver";
        public const string ArrowFireId = "item_ammo_arrow_fire";
        public const string ArrowFrostId = "item_ammo_arrow_frost";

        // Tags de material/elemento no MESMO vocabulÃ¡rio das tags de arma (WeaponDataSO.MaterialTagsApplied)
        // que o matching F06 consome. "Pierce" Ã© a tag base de toda flecha (Â§19); "Silver"/"Fire"/"Ice"
        // sÃ£o as tags adicionais por tipo.
        public const string TagPierce = "Pierce";
        public const string TagSilver = "Silver";
        public const string TagFire = "Fire";
        public const string TagIce = "Ice";

        // Ids canÃ´nicos de status effect (StatusEffectDatabaseSO â€” gerados pela F01).
        public const string StatusBurnId = "status_burn";
        public const string StatusChillId = "status_chill";

        // Chance ÃšNICA canÃ´nica de aplicar o status elemental on-hit (Â§19 "chance baixa").
        // Definida em UM ponto (regra de nÃ£o-duplicaÃ§Ã£o) â€” fireâ†’Burn e frostâ†’Chill compartilham.
        public const float ElementalStatusChance = 0.25f;

        private static readonly Dictionary<string, ArrowBallistics> Table = BuildTable();

        /// <summary>Ordem canÃ´nica do catÃ¡logo (barataâ†’cara): usada pela auto-seleÃ§Ã£o determinÃ­stica.</summary>
        // NOTE: a flecha bÃ¡sica (starter) NÃƒO entra aqui â€” esta Ã© a ordem dos 6 perfis elementais
        // canÃ´nicos usada pela auto-seleÃ§Ã£o pÃ³s-consumo. A bÃ¡sica Ã© um alias de wood na Table acima
        // (IsCanonicalArrow=true para disparar sem warning), mas nÃ£o Ã© alvo de auto-equipar.
        public static readonly IReadOnlyList<string> CanonicalOrder = new[]
        {
            ArrowWoodId, ArrowIronId, ArrowSteelId, ArrowSilverId, ArrowFireId, ArrowFrostId
        };

        /// <summary>True se <paramref name="ammoItemId"/> Ã© um dos 6 tipos canÃ´nicos do catÃ¡logo.</summary>
        public static bool IsCanonicalArrow(string ammoItemId)
        {
            return !string.IsNullOrEmpty(ammoItemId) && Table.ContainsKey(ammoItemId);
        }

        /// <summary>
        /// Resolve a balÃ­stica pela id da muniÃ§Ã£o. Desconhecida => WoodenArrow (IsKnown=false) + warning.
        /// </summary>
        public static ArrowBallistics Resolve(string ammoItemId)
        {
            if (!string.IsNullOrEmpty(ammoItemId) && Table.TryGetValue(ammoItemId, out var ballistics))
            {
                return ballistics;
            }

            Debug.LogWarning(
                $"CombatLog: ArrowBallisticsFallback. Reason=UnknownAmmoId, AmmoItemId='{ammoItemId}', Fallback={ArrowWoodId}");
            return Table[ArrowWoodId].AsFallback();
        }

        /// <summary>ConveniÃªncia: resolve direto de um <see cref="ItemDataSO"/> de muniÃ§Ã£o.</summary>
        public static ArrowBallistics Resolve(ItemDataSO ammo)
        {
            return Resolve(ammo != null ? ammo.Id : null);
        }

        private static Dictionary<string, ArrowBallistics> BuildTable()
        {
            // Â§19 (EQUIPMENT_MECHANICAL_BASELINES) restrito aos 6 do Â§8 (ITEM_CATALOG):
            //   wood  +2  Physical  [Pierce]
            //   iron  +4  Physical  [Pierce]
            //   steel +6  Physical  [Pierce]
            //   silver +4 Physical  [Pierce, Silver]            (anti-undead/shadow SE declarado)
            //   fire  +3  Fire      [Pierce, Fire]  Burn  baixa
            //   frost +3  Ice       [Pierce, Ice]   Chill baixa
            return new Dictionary<string, ArrowBallistics>
            {
                // Flecha bÃ¡sica (id do starter/hotbar): mesma balÃ­stica da wood, sob a prÃ³pria id para
                // nÃ£o cair no fallback com warning. AmmoItemId preserva a id real para os logs.
                [ArrowBasicId] = new ArrowBallistics(ArrowBasicId, 2, DamageType.Physical, new[] { TagPierce }, null, 0f),
                [ArrowWoodId] = new ArrowBallistics(ArrowWoodId, 2, DamageType.Physical, new[] { TagPierce }, null, 0f),
                [ArrowIronId] = new ArrowBallistics(ArrowIronId, 4, DamageType.Physical, new[] { TagPierce }, null, 0f),
                [ArrowSteelId] = new ArrowBallistics(ArrowSteelId, 6, DamageType.Physical, new[] { TagPierce }, null, 0f),
                [ArrowSilverId] = new ArrowBallistics(ArrowSilverId, 4, DamageType.Physical, new[] { TagPierce, TagSilver }, null, 0f),
                [ArrowFireId] = new ArrowBallistics(ArrowFireId, 3, DamageType.Fire, new[] { TagPierce, TagFire }, StatusBurnId, ElementalStatusChance),
                [ArrowFrostId] = new ArrowBallistics(ArrowFrostId, 3, DamageType.Ice, new[] { TagPierce, TagIce }, StatusChillId, ElementalStatusChance),
            };
        }
    }

    /// <summary>
    /// fable_48 â€” perfil imutÃ¡vel de uma flecha (Â§19). Valor puro; sem refs Unity. As tags usam o
    /// mesmo vocabulÃ¡rio de WeaponDataSO.MaterialTagsApplied para casar no matching F06.
    /// </summary>
    public sealed class ArrowBallistics
    {
        public string AmmoItemId { get; }
        public int ArrowDamage { get; }
        public DamageType DamageType { get; }
        public string[] Tags { get; }
        public string StatusEffectId { get; }
        public float StatusChance { get; }

        /// <summary>True quando a flecha foi resolvida por id canÃ´nico; false quando Ã© o fallback WoodenArrow.</summary>
        public bool IsKnown { get; }

        public bool HasStatus => !string.IsNullOrEmpty(StatusEffectId) && StatusChance > 0f;

        public ArrowBallistics(
            string ammoItemId,
            int arrowDamage,
            DamageType damageType,
            string[] tags,
            string statusEffectId,
            float statusChance,
            bool isKnown = true)
        {
            AmmoItemId = ammoItemId ?? string.Empty;
            ArrowDamage = Mathf.Max(0, arrowDamage);
            DamageType = damageType;
            Tags = tags ?? System.Array.Empty<string>();
            StatusEffectId = statusEffectId;
            StatusChance = Mathf.Clamp01(statusChance);
            IsKnown = isKnown;
        }

        /// <summary>CÃ³pia marcada como fallback (IsKnown=false), preservando os stats WoodenArrow.</summary>
        public ArrowBallistics AsFallback()
        {
            return new ArrowBallistics(AmmoItemId, ArrowDamage, DamageType, Tags, StatusEffectId, StatusChance, isKnown: false);
        }
    }
}
