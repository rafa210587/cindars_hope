namespace CindarsHope.City.Services
{
    /// <summary>Resultado determinístico de uma tentativa de compra de serviço civic (fable_19).</summary>
    public enum CityServicePurchaseOutcome
    {
        Purchased = 0,      // posse concedida agora; ouro debitado
        AlreadyOwned,       // já possuía; nenhum débito (idempotente)
        InsufficientGold,   // ouro < custo; nada acontece
        UnknownService,     // serviceId não pertence ao catálogo
        Failed              // débito de ouro recusado pelo provedor (estado inalterado)
    }

    /// <summary>
    /// fable_19 — lógica PURA da compra de um serviço/licença civic, sem dependência de Unity ou de
    /// QuestFlagService concreto (CA-2/CA-3 testáveis). Recebe o estado de posse e o saldo via
    /// predicados/funções injetadas; decide o desfecho de forma idempotente.
    ///
    /// Mesmo idioma do <c>TemperingService</c>/<c>AccessoryEffectRouter</c>: o ponto de runtime
    /// (CityServiceAccess) injeta as ações reais (debitar ouro, gravar flag); aqui só a regra.
    /// </summary>
    public sealed class CityServicePurchaseResolver
    {
        public sealed class Result
        {
            public CityServicePurchaseOutcome Outcome { get; set; }
            public string ServiceId { get; set; }
            public int GoldSpent { get; set; }
            public string PossessionFlag { get; set; }
            public string Message { get; set; }
        }

        /// <summary>
        /// Decide a compra de <paramref name="serviceId"/>.
        /// </summary>
        /// <param name="serviceId">ID canônico (license_market_stall / contract_farm_registry).</param>
        /// <param name="alreadyOwns">True se a flag de posse já está setada.</param>
        /// <param name="currentGold">Saldo atual de ouro do jogador.</param>
        /// <param name="spendGold">
        /// Ação de débito: recebe o custo, retorna true se o débito foi efetivado. Só é chamada quando
        /// o saldo é suficiente e o serviço ainda não é possuído. Em teste puro pode ser um stub.
        /// </param>
        /// <param name="grantFlag">Ação de concessão da flag de posse (idempotente no destino).</param>
        public Result Resolve(
            string serviceId,
            bool alreadyOwns,
            int currentGold,
            System.Func<int, bool> spendGold,
            System.Action<string> grantFlag)
        {
            var flag = CityServiceCatalog.PossessionFlagFor(serviceId);
            if (flag == null)
            {
                return new Result
                {
                    Outcome = CityServicePurchaseOutcome.UnknownService,
                    ServiceId = serviceId,
                    Message = $"Servico desconhecido: '{serviceId}'."
                };
            }

            if (alreadyOwns)
            {
                return new Result
                {
                    Outcome = CityServicePurchaseOutcome.AlreadyOwned,
                    ServiceId = serviceId,
                    PossessionFlag = flag,
                    GoldSpent = 0,
                    Message = "Voce ja possui este servico."
                };
            }

            var cost = CityServiceCatalog.CostFor(serviceId);
            if (currentGold < cost)
            {
                return new Result
                {
                    Outcome = CityServicePurchaseOutcome.InsufficientGold,
                    ServiceId = serviceId,
                    PossessionFlag = flag,
                    GoldSpent = 0,
                    Message = $"Ouro insuficiente: precisa de {cost}g."
                };
            }

            // Débito (quando há custo). Falha de débito => estado inalterado, sem conceder flag.
            if (cost > 0 && (spendGold == null || !spendGold(cost)))
            {
                return new Result
                {
                    Outcome = CityServicePurchaseOutcome.Failed,
                    ServiceId = serviceId,
                    PossessionFlag = flag,
                    GoldSpent = 0,
                    Message = "Falha ao debitar o ouro."
                };
            }

            grantFlag?.Invoke(flag);
            return new Result
            {
                Outcome = CityServicePurchaseOutcome.Purchased,
                ServiceId = serviceId,
                PossessionFlag = flag,
                GoldSpent = cost,
                Message = $"Servico adquirido por {cost}g."
            };
        }
    }
}
