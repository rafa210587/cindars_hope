namespace CindarsHope.City.Services
{
    /// <summary>
    /// fable_19 — fachada ESTÁTICA de acesso a posse de serviços civic, no padrão de acessor único já
    /// usado por <c>TemperingForgeAccess</c> e <c>AccessoryEffectRouter.Active</c>. Mantém os pontos
    /// de consumo (EconomyManager / ShippingPriceResolver / NpcShopController) DESACOPLADOS de
    /// QuestFlagService: a posse é um predicado injetado e a concessão uma ação injetada — ambos
    /// default fail-closed/no-op, ligados pelo runtime bridge.
    ///
    /// Pure-friendly: sem busca global de cena. O runtime liga <see cref="OwnsServiceQuery"/> /
    /// <see cref="GrantPossessionAction"/> ao QuestFlagService real e <see cref="SpendGoldFunc"/> /
    /// <see cref="CurrentGoldFunc"/> ao PlayerManager.
    /// </summary>
    public static class CityServiceAccess
    {
        private static readonly CityServicePurchaseResolver PurchaseResolver = new CityServicePurchaseResolver();

        /// <summary>
        /// Predicado de posse: recebe a flag de posse (ex.: flag_license_market_stall) e retorna se
        /// está setada. Default => FECHADO (sem posse): o ponto de venda urbano recusa até o runtime
        /// ligar o resolver real. (city_rules.md Rule 5 constraint.)
        /// </summary>
        public static System.Func<string, bool> OwnsServiceQuery { get; set; }

        /// <summary>Ação de concessão da flag de posse (gravar via QuestFlagService). Default => no-op.</summary>
        public static System.Action<string> GrantPossessionAction { get; set; }

        /// <summary>Função de débito de ouro (custo → efetivado?). Default => recusa (fail-closed).</summary>
        public static System.Func<int, bool> SpendGoldFunc { get; set; }

        /// <summary>Saldo atual de ouro do jogador. Default => 0.</summary>
        public static System.Func<int> CurrentGoldFunc { get; set; }

        /// <summary>True se o jogador possui o serviço/licença <paramref name="serviceId"/> (fail-closed).</summary>
        public static bool OwnsService(string serviceId)
        {
            var flag = CityServiceCatalog.PossessionFlagFor(serviceId);
            if (flag == null) return false;
            try
            {
                return OwnsServiceQuery != null && OwnsServiceQuery(flag);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// CA-2 — o ponto de venda URBANO só é permitido com a licença de barraca. Fonte ÚNICA da
        /// decisão (sem if espalhado): consumida pelo EconomyManager ao receber SellAll do ponto
        /// urbano. Venda a NPC e shipping da fazenda nunca passam por aqui.
        /// </summary>
        public static bool UrbanSellAllowed() => OwnsService(CityServiceFlags.LicenseMarketStallId);

        /// <summary>True se o contrato de registro de fazenda está ativo (posse).</summary>
        public static bool HasFarmRegistryContract() => OwnsService(CityServiceFlags.ContractFarmRegistryId);

        /// <summary>
        /// CA-2 — aplica o bônus do contrato de registro (+5%) a um preço de shipping da fazenda.
        /// Ponto único consumido pelo <c>ShippingPriceResolver</c>. Sem contrato => preço inalterado.
        /// Determinístico/testável.
        /// </summary>
        public static float ApplyFarmRegistryContract(float basePrice)
        {
            if (basePrice <= 0f) return basePrice;
            return HasFarmRegistryContract()
                ? basePrice * (1f + CityServiceCatalog.FarmRegistryShippingBonus)
                : basePrice;
        }

        /// <summary>
        /// Tenta comprar o serviço civic <paramref name="serviceId"/> usando o resolver puro + as
        /// ações injetadas (débito de ouro + concessão de flag). Idempotente (já possui => sem débito).
        /// Retorna o resultado determinístico para o chamador (NpcShopController) dar feedback.
        /// </summary>
        public static CityServicePurchaseResolver.Result TryPurchase(string serviceId)
        {
            var gold = 0;
            try { gold = CurrentGoldFunc?.Invoke() ?? 0; } catch { gold = 0; }

            return PurchaseResolver.Resolve(
                serviceId,
                OwnsService(serviceId),
                gold,
                cost =>
                {
                    try { return SpendGoldFunc != null && SpendGoldFunc(cost); }
                    catch { return false; }
                },
                flag =>
                {
                    try { GrantPossessionAction?.Invoke(flag); } catch { /* fail-closed */ }
                });
        }

        /// <summary>Limpa todos os resolvers (usado em teardown de teste para isolar estado estático).</summary>
        public static void ResetForTests()
        {
            OwnsServiceQuery = null;
            GrantPossessionAction = null;
            SpendGoldFunc = null;
            CurrentGoldFunc = null;
        }
    }
}
