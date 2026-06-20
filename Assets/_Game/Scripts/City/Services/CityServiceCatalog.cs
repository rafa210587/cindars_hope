using System.Collections.Generic;

namespace CindarsHope.City.Services
{
    /// <summary>
    /// fable_19 — catálogo ESTÁTICO canônico dos 2 serviços urbanos demonstráveis, REUSANDO os tipos
    /// órfãos já existentes (<see cref="CityServiceDefinition"/>, <see cref="LicenseDefinition"/>,
    /// <see cref="ContractDefinition"/>, <see cref="CityServiceAvailabilityResolver"/>) — sem criar
    /// sistema paralelo. Liga a identidade Tovin/Mara (CITY_NPC_ROSTER §0/§3) a posse mecânica.
    ///
    /// Pure C# (testável fora do Unity). Os IDs/custos/efeitos são os de city_rules.md Rule 5:
    ///   license_market_stall (Tovin, 200g) → exigida pelo ponto de venda urbano;
    ///   contract_farm_registry (Mara, 100g) → +5% no shipping da fazenda.
    /// </summary>
    public static class CityServiceCatalog
    {
        /// <summary>NPC vendedor da licença de barraca.</summary>
        public const string TovinNpcId = "npc_tovin";

        /// <summary>NPC emissor do contrato de registro de fazenda.</summary>
        public const string MaraNpcId = "npc_mara";

        /// <summary>Custo em ouro da licença de barraca (city_rules.md Rule 5).</summary>
        public const int LicenseMarketStallCost = 200;

        /// <summary>Custo em ouro do contrato de registro de fazenda.</summary>
        public const int ContractFarmRegistryCost = 100;

        /// <summary>Bônus de preço do shipping da fazenda concedido pelo contrato (+5%).</summary>
        public const float FarmRegistryShippingBonus = 0.05f;

        /// <summary>Definição da licença de barraca (Tovin). GrantedFlag = posse persistida por flag.</summary>
        public static LicenseDefinition MarketStallLicense { get; } = new LicenseDefinition
        {
            LicenseId = CityServiceFlags.LicenseMarketStallId,
            DisplayName = "Licenca de Barraca de Mercado",
            IssuerServiceId = "service_tovin_market_stall",
            RequiredGold = LicenseMarketStallCost,
            GrantedFlag = CityServiceFlags.LicenseMarketStallFlag,
            GrantedPermission = "sell_at_urban_point"
        };

        /// <summary>Definição do contrato de registro de fazenda (Mara). Grava flag de posse na compra.</summary>
        public static ContractDefinition FarmRegistryContract { get; } = new ContractDefinition
        {
            ContractId = CityServiceFlags.ContractFarmRegistryId,
            ProviderServiceId = "service_mara_farm_registry",
            ContractType = "farm_registry",
            RepeatPolicy = ContractRepeatPolicy.OneTime
        };

        /// <summary>Serviço (acesso) da licença de barraca, vendido por Tovin.</summary>
        public static CityServiceDefinition MarketStallService { get; } = new CityServiceDefinition
        {
            ServiceId = "service_tovin_market_stall",
            ServiceType = CityServiceType.TownHallLicense,
            DisplayName = "Licenca de Barraca (Tovin)",
            ProviderNpcIds = new List<string> { TovinNpcId },
            ProviderBuildingId = "building_market_row",
            LicenseDefinitionId = CityServiceFlags.LicenseMarketStallId
        };

        /// <summary>Serviço (acesso) do contrato de registro de fazenda, emitido por Mara.</summary>
        public static CityServiceDefinition FarmRegistryService { get; } = new CityServiceDefinition
        {
            ServiceId = "service_mara_farm_registry",
            ServiceType = CityServiceType.TownHallContract,
            DisplayName = "Registro de Fazenda (Mara)",
            ProviderNpcIds = new List<string> { MaraNpcId },
            ProviderBuildingId = "building_temple_district"
        };

        /// <summary>
        /// True se <paramref name="npcId"/> vende algum serviço civic deste catálogo (Tovin/Mara).
        /// Ponto único de decisão para a opção de diálogo "Servicos" (NpcShopController).
        /// </summary>
        public static bool IsServiceProvider(string npcId)
        {
            if (string.IsNullOrEmpty(npcId)) return false;
            return MatchesNpc(npcId, TovinNpcId) || MatchesNpc(npcId, MaraNpcId);
        }

        /// <summary>
        /// ID do serviço (license/contract) vendido por <paramref name="npcId"/>, ou null se nenhum.
        /// </summary>
        public static string ServiceIdFor(string npcId)
        {
            if (MatchesNpc(npcId, TovinNpcId)) return CityServiceFlags.LicenseMarketStallId;
            if (MatchesNpc(npcId, MaraNpcId)) return CityServiceFlags.ContractFarmRegistryId;
            return null;
        }

        /// <summary>Custo em ouro do serviço por ID; 0 se desconhecido.</summary>
        public static int CostFor(string serviceId)
        {
            if (serviceId == CityServiceFlags.LicenseMarketStallId) return LicenseMarketStallCost;
            if (serviceId == CityServiceFlags.ContractFarmRegistryId) return ContractFarmRegistryCost;
            return 0;
        }

        /// <summary>Flag de posse para o serviço por ID; null se desconhecido.</summary>
        public static string PossessionFlagFor(string serviceId)
        {
            if (serviceId == CityServiceFlags.LicenseMarketStallId) return CityServiceFlags.LicenseMarketStallFlag;
            if (serviceId == CityServiceFlags.ContractFarmRegistryId) return CityServiceFlags.ContractFarmRegistryFlag;
            return null;
        }

        /// <summary>Rótulo curto do serviço para a opção de diálogo; null se desconhecido.</summary>
        public static string DisplayLabelFor(string serviceId)
        {
            if (serviceId == CityServiceFlags.LicenseMarketStallId)
                return $"Comprar Licenca de Barraca ({LicenseMarketStallCost}g)";
            if (serviceId == CityServiceFlags.ContractFarmRegistryId)
                return $"Registrar Fazenda ({ContractFarmRegistryCost}g)";
            return null;
        }

        private static bool MatchesNpc(string actual, string canonical)
        {
            return !string.IsNullOrEmpty(actual) &&
                   actual.Equals(canonical, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
