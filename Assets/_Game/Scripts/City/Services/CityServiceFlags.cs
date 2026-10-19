using System.Collections.Generic;
using CindarsHope.Quests.Flags;

namespace CindarsHope.City.Services
{
    /// <summary>
    /// fable_19 — IDs canônicos dos 2 serviços/licenças urbanos demonstráveis e o registro das suas
    /// flags no <see cref="QuestFlagRegistry"/> (regra de não-duplicação: posse persistida por flag
    /// de quest, SEM nova seção de save). city_rules.md Rule 5 / tabela "Civic services".
    ///
    /// Setter canônico: <see cref="CityServiceSetter"/> — somente este sistema pode conceder as flags
    /// (AllowedSetters), mantendo a autoria de civic services rastreável.
    /// </summary>
    public static class CityServiceFlags
    {
        /// <summary>Sistema autorizado a setar/limpar as flags de serviço (AllowedSetters/Clearers).</summary>
        public const string CityServiceSetter = "city_services";

        /// <summary>Licença de barraca de mercado (Tovin, 200g) — exigida pelo ponto de venda URBANO.</summary>
        public const string LicenseMarketStallId = "license_market_stall";

        /// <summary>Contrato de registro de fazenda (Mara, 100g) — +5% no preço do shipping da fazenda.</summary>
        public const string ContractFarmRegistryId = "contract_farm_registry";

        /// <summary>Flag de posse da licença de barraca (gravada via QuestFlagService na compra).</summary>
        public const string LicenseMarketStallFlag = "flag_license_market_stall";

        /// <summary>Flag de posse do contrato de registro de fazenda.</summary>
        public const string ContractFarmRegistryFlag = "flag_contract_farm_registry";

        /// <summary>
        /// Registra as 2 flags de civic service no registry fornecido (idempotente: re-registrar
        /// sobrescreve a mesma definição). Chamado pelo bootstrap de runtime ao montar o
        /// QuestFlagService. Escopo City, persistente, visível ao jogador após descoberta.
        /// </summary>
        public static void RegisterFlags(QuestFlagRegistry registry)
        {
            if (registry == null) return;

            registry.Register(BuildFlagDefinition(
                LicenseMarketStallFlag,
                "city.service.license_market_stall.name",
                "city.service.license_market_stall.desc"));

            registry.Register(BuildFlagDefinition(
                ContractFarmRegistryFlag,
                "city.service.contract_farm_registry.name",
                "city.service.contract_farm_registry.desc"));
        }

        /// <summary>IDs de flag das 2 posses (para validação/teste e iteração).</summary>
        public static IReadOnlyList<string> AllPossessionFlags { get; } = new List<string>
        {
            LicenseMarketStallFlag,
            ContractFarmRegistryFlag
        };

        private static QuestFlagDefinition BuildFlagDefinition(string flagId, string nameKey, string descKey)
        {
            return new QuestFlagDefinition
            {
                FlagId = flagId,
                DisplayNameKey = nameKey,
                DescriptionKey = descKey,
                FlagType = QuestFlagType.Boolean,
                Scope = QuestFlagScope.City,
                Visibility = QuestFlagVisibility.PlayerKnownAfterDiscovery,
                OwnerSystem = CityServiceSetter,
                AllowedSetters = new List<string> { CityServiceSetter },
                AllowedClearers = new List<string> { CityServiceSetter },
                Persists = true,
                DefaultValue = "false",
                CanAppearInQuestLog = false,
                CanBeUsedByConditions = true,
                CanBeGrantedByReward = false
            };
        }
    }
}
