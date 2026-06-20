using System.Collections.Generic;

namespace CindarsHope.Farm.Forage
{
    /// <summary>
    /// fable_54 — tabela de forrageio por estacao (dados EM CODIGO no v1; ForageTable data-driven por
    /// SO fica fora de escopo — documentado no report). Mapeia season -> lista de ForageDefinition
    /// (consome o modulo orfao ForageDefinition existente, INTOCADO).
    ///
    /// Itens referenciam IDs ja existentes no catalogo canonico (F32): crops e materiais foragaveis.
    /// Nenhum item novo e criado por esta spec. Se faltar item dedicado de forrageio por estacao,
    /// follow-up no report (NAO expandir catalogo aqui).
    ///
    /// SeasonId usa o nome do enum Season (Primavera/Verao/Outono/Inverno). O casamento aceita o
    /// nome do enum diretamente; FarmForageRuntimeService normaliza a estacao corrente.
    ///
    /// Determinismo: a SELECAO do dia (quais N forrageiros estao ativos) e feita por StableHash
    /// (FNV-1a 32-bit, ForageStableHash) sobre (salt + day) — sem Random/GUID/timestamp
    /// (rng-and-determinism). Mesmo dia => mesma selecao apos reload.
    /// </summary>
    public static class ForageSeasonTable
    {
        /// <summary>Numero de pontos de forrageio ativos por dia (subset da tabela da estacao).</summary>
        public const int DailyActiveCount = 4;

        // season key (lower-invariant, nome do enum) -> definicoes
        private static readonly Dictionary<string, List<ForageDefinition>> Tables = BuildTables();

        public static IReadOnlyList<ForageDefinition> ForSeason(string seasonId)
        {
            var key = NormalizeKey(seasonId);
            if (key != null && Tables.TryGetValue(key, out var list))
            {
                return list;
            }

            // Estacao desconhecida (sem calendario) => usa a tabela de Primavera como fallback seguro
            // (nunca vazio, para a fazenda sempre ter presenca de forrageio).
            return Tables["primavera"];
        }

        /// <summary>Todas as definicoes (todas as estacoes) — para montar o servico orfao uma vez.</summary>
        public static Dictionary<string, ForageDefinition> AllDefinitionsById()
        {
            var byId = new Dictionary<string, ForageDefinition>();
            foreach (var list in Tables.Values)
            {
                foreach (var def in list)
                {
                    byId[def.ForageId] = def;
                }
            }
            return byId;
        }

        public static string NormalizeKey(string season)
        {
            if (string.IsNullOrWhiteSpace(season)) return null;
            switch (season.Trim().ToLowerInvariant())
            {
                case "primavera":
                case "spring":
                    return "primavera";
                case "verao":
                case "verão":
                case "summer":
                    return "verao";
                case "outono":
                case "autumn":
                case "fall":
                    return "outono";
                case "inverno":
                case "winter":
                    return "inverno";
                default:
                    return null;
            }
        }

        private static Dictionary<string, List<ForageDefinition>> BuildTables()
        {
            return new Dictionary<string, List<ForageDefinition>>
            {
                ["primavera"] = new List<ForageDefinition>
                {
                    Def("forage_spring_herbs", "Ervas Silvestres", "item_material_fiber", "Primavera"),
                    Def("forage_spring_greens", "Brotos da Primavera", "item_crop_carrot", "Primavera"),
                    Def("forage_spring_wood", "Galhos Caidos", "item_material_wood", "Primavera"),
                },
                ["verao"] = new List<ForageDefinition>
                {
                    Def("forage_summer_pepper", "Pimenta Selvagem", "item_crop_sunpepper", "Verao"),
                    Def("forage_summer_grain", "Trigo Bravo", "item_crop_wheat", "Verao"),
                    Def("forage_summer_wood", "Lenha Seca", "item_material_wood", "Verao"),
                },
                ["outono"] = new List<ForageDefinition>
                {
                    Def("forage_autumn_root", "Raiz de Outono", "item_crop_starroot", "Outono"),
                    Def("forage_autumn_cap", "Cogumelo Brilhante", "item_material_glowcap", "Outono"),
                    Def("forage_autumn_spores", "Esporos Silvestres", "item_material_spores", "Outono"),
                },
                ["inverno"] = new List<ForageDefinition>
                {
                    Def("forage_winter_bean", "Grao da Lua", "item_crop_moonbean", "Inverno"),
                    Def("forage_winter_stone", "Pedra de Superficie", "item_material_stone", "Inverno"),
                    Def("forage_winter_fiber", "Fibra Resistente", "item_material_fiber", "Inverno"),
                },
            };
        }

        private static ForageDefinition Def(string forageId, string displayName, string itemId, string season)
        {
            return new ForageDefinition
            {
                ForageId = forageId,
                DisplayName = displayName,
                ItemId = itemId,
                AllowedSeasons = new List<string> { season },
                Rarity = ForageRarity.Common,
                CanRespawnSameSeason = true,
                RespawnAfterDays = 2,
                SpawnChance = 1.0f
            };
        }
    }
}
