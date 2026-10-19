using System.Collections.Generic;

namespace CindarsHope.World.Altars
{
    /// <summary>
    /// fable_68 — deus maior do panteão de Vaalara associado a uma Marca. Estende a taxonomia das 4
    /// relíquias (<see cref="CindarsHope.Equipment.RelicGod"/>) para os 11 deuses com Marca + a exceção
    /// canônica de Anya. Usado como ID estável no save (string) e para a regra de não-stack com a
    /// relíquia do MESMO deus (apêndice A.5 — decisão 3.4 do Refinamento v2).
    /// </summary>
    public enum MarkGod
    {
        None = 0,
        Finan = 1,
        Thoren = 2,
        Kaand = 3,
        Nyx = 4,
        Tandra = 5,
        Thandra = 6,
        Kanthor = 7,
        Merithus = 8,
        Alihana = 9,
        Senya = 10,
        Anya = 11
    }

    /// <summary>Onde a Marca vive fisicamente (apêndice A.5).</summary>
    public enum MarkLocation
    {
        Cave = 0,
        Farm = 1,
        City = 2,
        AnyaSacred = 3
    }

    /// <summary>
    /// Condição que precisa estar satisfeita para a oração render bônus (apêndice A.5).
    /// <see cref="None"/> = sempre disponível (1/dia). As demais consomem estado externo
    /// (TimeManager/calendário/inventário) validado pelo <see cref="GodMarkService"/>.
    /// </summary>
    public enum MarkCondition
    {
        None = 0,
        NightOnly = 1,        // Nyx — Poço Sem Lua: só à noite
        AlihanaLunarPeak = 2, // Alihana — Espelho: noite de pico de Alihana
        SenyaFestivalOrPeak = 3, // Senya — Mastro: festival/pico de Senya
        CropOffering = 4      // Thandra — Nicho: consome 1 crop como oferenda
    }

    /// <summary>
    /// Tipo de efeito de uma Marca (apêndice A.5). Cada efeito é descritivo/numérico simples; a Marca
    /// NÃO duplica o sistema de combate — entra como condição diária via <see cref="GodMarkService"/>.
    /// </summary>
    public enum MarkEffectType
    {
        None = 0,
        GoldFindPercent = 1,           // Finan +5% gold find
        DurabilityLossReductionPercent = 2, // Thoren -5% perda de durabilidade
        DamageDealtAndTakenPercent = 3,// Kaand +3% dano causado E recebido
        SecretItemChancePercent = 4,   // Nyx +5% chance de itens secretos (noite)
        BeastDropPercent = 5,          // Tandra +5% drops de partes Beast
        TomorrowSilverPercent = 6,     // Thandra +2% Silver amanhã (oferenda)
        BlockStabilityPercent = 7,     // Kanthor +2% block stability
        ShippingValuePercent = 8,      // Merithus +2% valor do shipping do dia
        RareSeedChancePercent = 9,     // Alihana +5% semente rara (+ sonho-pista)
        MagicXpPercent = 10            // Senya +5% XP de magia
    }

    /// <summary>
    /// fable_68 — definição imutável de uma Marca dos Deuses (uma linha do apêndice A.5). Pure C#
    /// (testável), descreve só a camada de gameplay. Marcas de caverna trazem <see cref="CaveBandMin"/>
    /// / <see cref="CaveBandMax"/> (banda de profundidade onde podem nascer); marcas urbanas/rurais
    /// têm banda 0/0 (não se aplicam à geração da caverna).
    /// </summary>
    public sealed class GodMarkDefinition
    {
        public string Id { get; }
        public MarkGod God { get; }
        public MarkLocation Location { get; }
        public MarkEffectType Effect { get; }
        public float Magnitude { get; }
        public MarkCondition Condition { get; }
        public int CaveBandMin { get; }
        public int CaveBandMax { get; }
        public string DisplayNameKey { get; }
        public string LoreKey { get; }

        /// <summary>True quando a Marca não concede bônus algum (exceção de Anya — lore apenas).</summary>
        public bool IsLoreOnly => Effect == MarkEffectType.None;

        public GodMarkDefinition(
            string id, MarkGod god, MarkLocation location, MarkEffectType effect, float magnitude,
            MarkCondition condition, int caveBandMin, int caveBandMax, string displayNameKey, string loreKey)
        {
            Id = id;
            God = god;
            Location = location;
            Effect = effect;
            Magnitude = magnitude;
            Condition = condition;
            CaveBandMin = caveBandMin;
            CaveBandMax = caveBandMax;
            DisplayNameKey = displayNameKey;
            LoreKey = loreKey;
        }
    }

    /// <summary>
    /// fable_68 — catálogo PURO (sem Unity, testável) das 11 Marcas dos Deuses do apêndice A.5
    /// (decisão 3.4 do Refinamento v2). Fonte ÚNICA de id→Marca em runtime. NÃO recria o sistema de
    /// relíquias (F23) nem o de condições (E06); referencia o panteão por <see cref="MarkGod"/>.
    ///
    /// Exceção canônica inviolável: Anya (<see cref="MarkGod.Anya"/>) tem Marca SÓ de lore — NENHUM
    /// bônus de oração (efeito <see cref="MarkEffectType.None"/>).
    /// </summary>
    public static class GodMarkCatalog
    {
        // IDs canônicos (apêndice A.5). Públicos para testes/hooks.
        public const string FinanCoin = "mark_finan_coin";
        public const string ThorenAnvil = "mark_thoren_anvil";
        public const string KaandStone = "mark_kaand_stone";
        public const string NyxPool = "mark_nyx_pool";
        public const string TandraRoot = "mark_tandra_root";

        public const string ThandraNiche = "mark_thandra_niche";
        public const string KanthorOath = "mark_kanthor_oath";
        public const string MerithusSeal = "mark_merithus_seal";
        public const string AlihanaMirror = "mark_alihana_mirror";
        public const string SenyaMast = "mark_senya_mast";

        public const string AnyaWaters = "mark_anya_waters";

        private static readonly List<GodMarkDefinition> _all = BuildAll();
        private static readonly Dictionary<string, GodMarkDefinition> _byId = BuildIndex(_all);

        /// <summary>Todas as 11 Marcas canônicas.</summary>
        public static IReadOnlyList<GodMarkDefinition> All => _all;

        /// <summary>Definição EXATA por id canônico, ou null.</summary>
        public static GodMarkDefinition GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return _byId.TryGetValue(id, out var def) ? def : null;
        }

        public static bool TryGetById(string id, out GodMarkDefinition def)
        {
            def = GetById(id);
            return def != null;
        }

        /// <summary>Marcas que nascem na caverna como special rooms (com banda de profundidade).</summary>
        public static IEnumerable<GodMarkDefinition> CaveMarks()
        {
            foreach (var def in _all)
            {
                if (def.Location == MarkLocation.Cave) yield return def;
            }
        }

        private static Dictionary<string, GodMarkDefinition> BuildIndex(List<GodMarkDefinition> all)
        {
            var map = new Dictionary<string, GodMarkDefinition>(all.Count);
            foreach (var def in all) map[def.Id] = def;
            return map;
        }

        private static List<GodMarkDefinition> BuildAll()
        {
            return new List<GodMarkDefinition>
            {
                // ── CAVERNA (special rooms, raras, banda de profundidade) ─────────────────────────
                new GodMarkDefinition(FinanCoin, MarkGod.Finan, MarkLocation.Cave,
                    MarkEffectType.GoldFindPercent, 0.05f, MarkCondition.None,
                    1, 100, "mark_finan_coin_name", "mark_finan_coin_lore"),
                new GodMarkDefinition(ThorenAnvil, MarkGod.Thoren, MarkLocation.Cave,
                    MarkEffectType.DurabilityLossReductionPercent, 0.05f, MarkCondition.None,
                    41, 55, "mark_thoren_anvil_name", "mark_thoren_anvil_lore"),
                new GodMarkDefinition(KaandStone, MarkGod.Kaand, MarkLocation.Cave,
                    MarkEffectType.DamageDealtAndTakenPercent, 0.03f, MarkCondition.None,
                    41, 85, "mark_kaand_stone_name", "mark_kaand_stone_lore"),
                new GodMarkDefinition(NyxPool, MarkGod.Nyx, MarkLocation.Cave,
                    MarkEffectType.SecretItemChancePercent, 0.05f, MarkCondition.NightOnly,
                    71, 85, "mark_nyx_pool_name", "mark_nyx_pool_lore"),
                new GodMarkDefinition(TandraRoot, MarkGod.Tandra, MarkLocation.Cave,
                    MarkEffectType.BeastDropPercent, 0.05f, MarkCondition.None,
                    11, 25, "mark_tandra_root_name", "mark_tandra_root_lore"),

                // ── CIDADE / FAZENDA (interactables fixos de cena) ────────────────────────────────
                new GodMarkDefinition(ThandraNiche, MarkGod.Thandra, MarkLocation.Farm,
                    MarkEffectType.TomorrowSilverPercent, 0.02f, MarkCondition.CropOffering,
                    0, 0, "mark_thandra_niche_name", "mark_thandra_niche_lore"),
                new GodMarkDefinition(KanthorOath, MarkGod.Kanthor, MarkLocation.City,
                    MarkEffectType.BlockStabilityPercent, 0.02f, MarkCondition.None,
                    0, 0, "mark_kanthor_oath_name", "mark_kanthor_oath_lore"),
                new GodMarkDefinition(MerithusSeal, MarkGod.Merithus, MarkLocation.City,
                    MarkEffectType.ShippingValuePercent, 0.02f, MarkCondition.None,
                    0, 0, "mark_merithus_seal_name", "mark_merithus_seal_lore"),
                new GodMarkDefinition(AlihanaMirror, MarkGod.Alihana, MarkLocation.Farm,
                    MarkEffectType.RareSeedChancePercent, 0.05f, MarkCondition.AlihanaLunarPeak,
                    0, 0, "mark_alihana_mirror_name", "mark_alihana_mirror_lore"),
                new GodMarkDefinition(SenyaMast, MarkGod.Senya, MarkLocation.City,
                    MarkEffectType.MagicXpPercent, 0.05f, MarkCondition.SenyaFestivalOrPeak,
                    0, 0, "mark_senya_mast_name", "mark_senya_mast_lore"),

                // ── EXCEÇÃO — Anya: SÓ lore, ZERO bônus (As Três Águas) ───────────────────────────
                new GodMarkDefinition(AnyaWaters, MarkGod.Anya, MarkLocation.AnyaSacred,
                    MarkEffectType.None, 0f, MarkCondition.None,
                    0, 0, "mark_anya_waters_name", "mark_anya_waters_lore"),
            };
        }
    }
}
