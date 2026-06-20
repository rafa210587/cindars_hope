using System;
using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Cave.Traps
{
    /// <summary>
    /// fable_60 — identificadores canônicos das 10 armadilhas do subset v1 (§23 da direction
    /// CAVE_LEVEL_GENERATION PARTE H). Os ~10 tipos restantes do catálogo ficam para o v2 (ver
    /// "Fora de escopo" na spec). Cada valor mapeia para uma <see cref="TrapDefinition"/> estática.
    /// </summary>
    public enum TrapId
    {
        None = 0,
        SpikeFloor = 1,        // trap_spike_floor — espinhos (dano físico)
        ClockworkDart = 2,     // trap_clockwork_dart — dardos (dano físico)
        SporePod = 3,          // trap_spore_pod — gás tóxico (Poison + Slow)
        IcePlate = 4,          // trap_ice_plate — gelo (Chill)
        LooseRocks = 5,        // trap_loose_rocks — queda de pedras (dano físico)
        EmberVent = 6,         // trap_ember_vent — fogo (Burn)
        RuneLockPulse = 7,     // trap_rune_lock_pulse — runa arcana (Stun/Slow)
        FalseChest = 8,        // trap_false_chest — baú falso (spawna Hoardmaw)
        RootSnare = 9,         // trap_root_snare — raiz/root curto (Root)
        FrostBurstRune = 10    // trap_frost_burst_rune — estouro frio com windup (Chill + dano)
    }

    /// <summary>
    /// fable_60 — categoria do efeito de uma armadilha. Define qual caminho EXISTENTE a armadilha
    /// consome ao disparar (regra de não duplicação): dano direto pelo pipeline
    /// (<c>PlayerDamageReceiver</c>), status pelos canônicos F01 (<c>PlayerStatusReceiver</c>), ou
    /// spawn de inimigo por ID pelo caminho de spawn existente. Nunca um segundo pipeline.
    /// </summary>
    public enum TrapEffectCategory
    {
        /// <summary>Apenas dano direto.</summary>
        DirectDamage = 0,

        /// <summary>Apenas status (F01) — sem dano direto.</summary>
        Status = 1,

        /// <summary>Dano direto + status (F01) no mesmo gatilho.</summary>
        DamageAndStatus = 2,

        /// <summary>Spawn de inimigo por ID (baú falso → Hoardmaw). Sem dano direto.</summary>
        SpawnEnemy = 3
    }

    /// <summary>
    /// fable_60 — definição canônica (catálogo EM CÓDIGO, sem assets no v1) de uma armadilha:
    /// efeito, status F01, dano base por tier, telegraph, desarme e pools de bioma §26.
    ///
    /// Determinismo (ADR-0005 / cave-stable-run): a definição é PURA — não rola nada. O sorteio de
    /// posição/tipo/quantidade é responsabilidade do <see cref="CaveTrapPlanner"/> por StableHash.
    ///
    /// "Tier" mapeia para a banda de bioma da caverna (stone/fungal/ice/fire/ruins/deep/void), na
    /// mesma régua do <c>CaveBiomeLayoutProfile</c> e do bestiário. O dano base por tier nunca
    /// excede o hit de elite da banda (BALANCE §7 como teto; valores conservadores no v1).
    /// </summary>
    public sealed class TrapDefinition
    {
        public TrapId Id { get; }
        public string TrapKey { get; }           // chave canônica (ex.: "trap_spike_floor")
        public string DisplayName { get; }
        public TrapEffectCategory Category { get; }

        /// <summary>Status canônico F01 aplicado ao disparar (vazio quando categoria = DirectDamage/SpawnEnemy).</summary>
        public string StatusId { get; }

        /// <summary>Status secundário opcional (ex.: SporePod = Poison + Slow). Vazio se não houver.</summary>
        public string SecondaryStatusId { get; }

        /// <summary>Segundos de telegraph ANTES do efeito. Sempre &gt; 0 (counterplay obrigatório §27/CA-2).</summary>
        public float TelegraphSeconds { get; }

        /// <summary>Pode ser desarmada por interação? false para o baú falso (só detectável).</summary>
        public bool Disarmable { get; }

        /// <summary>Spawn por ID (baú falso → enemy_hoardmaw). Vazio quando não é categoria SpawnEnemy.</summary>
        public string SpawnEnemyId { get; }

        /// <summary>Bandas de bioma onde a armadilha pode aparecer (§26). Vazio = nenhuma.</summary>
        public IReadOnlyList<string> Biomes { get; }

        private readonly int _baseDamage;
        private readonly int _damagePerBand;

        private TrapDefinition(
            TrapId id,
            string trapKey,
            string displayName,
            TrapEffectCategory category,
            string statusId,
            string secondaryStatusId,
            float telegraphSeconds,
            bool disarmable,
            string spawnEnemyId,
            int baseDamage,
            int damagePerBand,
            string[] biomes)
        {
            Id = id;
            TrapKey = trapKey;
            DisplayName = displayName;
            Category = category;
            StatusId = statusId ?? string.Empty;
            SecondaryStatusId = secondaryStatusId ?? string.Empty;
            TelegraphSeconds = telegraphSeconds;
            Disarmable = disarmable;
            SpawnEnemyId = spawnEnemyId ?? string.Empty;
            _baseDamage = baseDamage;
            _damagePerBand = damagePerBand;
            Biomes = biomes ?? Array.Empty<string>();
        }

        /// <summary>
        /// Dano direto da armadilha para uma banda 1..7 (stone=1 … void=7). Escala suave por banda,
        /// com teto pelo hit de elite (valores conservadores no v1). 0 quando a categoria não causa
        /// dano direto (Status puro / SpawnEnemy).
        /// </summary>
        public int ResolveDamage(int band)
        {
            if (Category == TrapEffectCategory.Status || Category == TrapEffectCategory.SpawnEnemy)
            {
                return 0;
            }

            var clampedBand = Mathf.Clamp(band, 1, 7);
            return Mathf.Max(0, _baseDamage + _damagePerBand * (clampedBand - 1));
        }

        // --- Catálogo estático dos 10 tipos v1 ----------------------------------------------------
        //
        // Pools de bioma (§26) usando as bandas canônicas do CaveBiomeLayoutProfile:
        //   stone, fungal, ice, fire, ruins, deep, void.
        // Cobertura: todo bioma v1 tem ao menos 1 tipo (validado por teste).

        public const string BiomeStone = "stone";
        public const string BiomeFungal = "fungal";
        public const string BiomeIce = "ice";
        public const string BiomeFire = "fire";
        public const string BiomeRuins = "ruins";
        public const string BiomeDeep = "deep";
        public const string BiomeVoid = "void";

        // IDs de status canônicos F01 (assets confirmados em Data/Combat/StatusEffects).
        private const string StatusPoison = "status_poison";
        private const string StatusSlow = "status_slow";
        private const string StatusStun = "status_stun";
        private const string StatusChill = "status_chill";
        private const string StatusBurn = "status_burn";
        private const string StatusRoot = "status_root";

        public const string HoardmawEnemyId = "enemy_hoardmaw";

        private static readonly TrapDefinition[] CatalogArray =
        {
            // Espinhos: dano físico puro. Onipresente em biomas "estruturais".
            new TrapDefinition(
                TrapId.SpikeFloor, "trap_spike_floor", "Piso de Espinhos",
                TrapEffectCategory.DirectDamage, string.Empty, string.Empty,
                telegraphSeconds: 0.6f, disarmable: true, spawnEnemyId: string.Empty,
                baseDamage: 6, damagePerBand: 3,
                biomes: new[] { BiomeStone, BiomeRuins, BiomeDeep, BiomeFire }),

            // Dardos de mecanismo: dano físico, telegraph um pouco maior (mecanismo arma).
            new TrapDefinition(
                TrapId.ClockworkDart, "trap_clockwork_dart", "Dardos de Mecanismo",
                TrapEffectCategory.DirectDamage, string.Empty, string.Empty,
                telegraphSeconds: 0.7f, disarmable: true, spawnEnemyId: string.Empty,
                baseDamage: 5, damagePerBand: 3,
                biomes: new[] { BiomeRuins, BiomeStone, BiomeVoid }),

            // Cápsula de esporos: gás tóxico = Poison + Slow (sem dano direto).
            new TrapDefinition(
                TrapId.SporePod, "trap_spore_pod", "Cápsula de Esporos",
                TrapEffectCategory.Status, StatusPoison, StatusSlow,
                telegraphSeconds: 0.8f, disarmable: true, spawnEnemyId: string.Empty,
                baseDamage: 0, damagePerBand: 0,
                biomes: new[] { BiomeFungal, BiomeDeep }),

            // Placa de gelo: Chill (sem dano direto).
            new TrapDefinition(
                TrapId.IcePlate, "trap_ice_plate", "Placa de Gelo",
                TrapEffectCategory.Status, StatusChill, string.Empty,
                telegraphSeconds: 0.7f, disarmable: true, spawnEnemyId: string.Empty,
                baseDamage: 0, damagePerBand: 0,
                biomes: new[] { BiomeIce, BiomeDeep }),

            // Queda de pedras: dano físico telegrafado.
            new TrapDefinition(
                TrapId.LooseRocks, "trap_loose_rocks", "Queda de Pedras",
                TrapEffectCategory.DirectDamage, string.Empty, string.Empty,
                telegraphSeconds: 0.65f, disarmable: true, spawnEnemyId: string.Empty,
                baseDamage: 7, damagePerBand: 3,
                biomes: new[] { BiomeStone, BiomeIce, BiomeFire, BiomeRuins, BiomeDeep, BiomeVoid }),

            // Respiradouro de brasa: fogo = Burn + dano.
            new TrapDefinition(
                TrapId.EmberVent, "trap_ember_vent", "Respiradouro de Brasa",
                TrapEffectCategory.DamageAndStatus, StatusBurn, string.Empty,
                telegraphSeconds: 0.75f, disarmable: true, spawnEnemyId: string.Empty,
                baseDamage: 5, damagePerBand: 2,
                biomes: new[] { BiomeFire, BiomeDeep }),

            // Pulso de runa-trava: arcano = Stun + Slow.
            new TrapDefinition(
                TrapId.RuneLockPulse, "trap_rune_lock_pulse", "Pulso de Runa-Trava",
                TrapEffectCategory.Status, StatusStun, StatusSlow,
                telegraphSeconds: 0.9f, disarmable: true, spawnEnemyId: string.Empty,
                baseDamage: 0, damagePerBand: 0,
                biomes: new[] { BiomeRuins, BiomeVoid }),

            // Baú falso: aparenta baú, spawna Hoardmaw. NUNCA desarmável (só detectável).
            new TrapDefinition(
                TrapId.FalseChest, "trap_false_chest", "Baú Falso",
                TrapEffectCategory.SpawnEnemy, string.Empty, string.Empty,
                telegraphSeconds: 0.5f, disarmable: false, spawnEnemyId: HoardmawEnemyId,
                baseDamage: 0, damagePerBand: 0,
                biomes: new[] { BiomeRuins, BiomeDeep, BiomeVoid }),

            // Laço de raiz: Root curto.
            new TrapDefinition(
                TrapId.RootSnare, "trap_root_snare", "Laço de Raiz",
                TrapEffectCategory.Status, StatusRoot, string.Empty,
                telegraphSeconds: 0.7f, disarmable: true, spawnEnemyId: string.Empty,
                baseDamage: 0, damagePerBand: 0,
                biomes: new[] { BiomeFungal, BiomeDeep }),

            // Estouro frio com windup: Chill + dano.
            new TrapDefinition(
                TrapId.FrostBurstRune, "trap_frost_burst_rune", "Runa de Estouro Frio",
                TrapEffectCategory.DamageAndStatus, StatusChill, string.Empty,
                telegraphSeconds: 0.85f, disarmable: true, spawnEnemyId: string.Empty,
                baseDamage: 4, damagePerBand: 2,
                biomes: new[] { BiomeIce, BiomeVoid })
        };

        private static readonly Dictionary<TrapId, TrapDefinition> ById = BuildById();
        private static readonly Dictionary<string, List<TrapDefinition>> ByBiome = BuildByBiome();

        private static Dictionary<TrapId, TrapDefinition> BuildById()
        {
            var map = new Dictionary<TrapId, TrapDefinition>();
            foreach (var def in CatalogArray)
            {
                map[def.Id] = def;
            }

            return map;
        }

        private static Dictionary<string, List<TrapDefinition>> BuildByBiome()
        {
            var map = new Dictionary<string, List<TrapDefinition>>(StringComparer.Ordinal);
            foreach (var def in CatalogArray)
            {
                foreach (var biome in def.Biomes)
                {
                    if (!map.TryGetValue(biome, out var list))
                    {
                        list = new List<TrapDefinition>();
                        map[biome] = list;
                    }

                    list.Add(def);
                }
            }

            return map;
        }

        /// <summary>Todas as 10 definições canônicas v1 (ordem estável do catálogo).</summary>
        public static IReadOnlyList<TrapDefinition> All => CatalogArray;

        /// <summary>Definição por id (ou null se TrapId.None / desconhecido).</summary>
        public static TrapDefinition Get(TrapId id)
        {
            return ById.TryGetValue(id, out var def) ? def : null;
        }

        /// <summary>
        /// Pool de tipos permitidos no bioma/banda (§26), em ordem estável do catálogo. Lista vazia
        /// se o bioma não tiver tipos no subset v1 (o planner então não gera armadilha). Nunca null.
        /// </summary>
        public static IReadOnlyList<TrapDefinition> PoolForBiome(string biomeBandId)
        {
            if (!string.IsNullOrWhiteSpace(biomeBandId) && ByBiome.TryGetValue(biomeBandId, out var list))
            {
                return list;
            }

            return Array.Empty<TrapDefinition>();
        }
    }
}
