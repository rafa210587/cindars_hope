namespace CindarsHope.City
{
    /// <summary>
    /// fable_57 — núcleo PURO (sem Unity) da cobrança da diária da estalagem. Decide, de forma
    /// determinística e testável, se a hospedagem pode prosseguir: lê o ouro corrente, e SÓ debita
    /// (via a ação injetada) quando há saldo. Mantém o gate desacoplado do PlayerManager — o
    /// runtime injeta os acessores de ouro (no padrão de CityServiceAccess).
    ///
    /// Invariante (CA-3 / risco "cobrança sem dormir"): o ouro só é debitado quando o débito
    /// efetivamente ocorre; sem saldo ⇒ recusa SEM débito. A delegação ao fluxo de dormir (F16) é
    /// responsabilidade do chamador APÓS um <see cref="Outcome.Paid"/>.
    /// </summary>
    public static class InnLodgingPaymentResolver
    {
        /// <summary>Diária padrão da cama de hóspede da estalagem (alinhada à régua de economia — Fase 0).</summary>
        public const int DefaultNightlyRate = 50;

        public enum Outcome
        {
            /// <summary>Diária debitada com sucesso — o chamador deve disparar o fluxo de dormir.</summary>
            Paid,
            /// <summary>Ouro insuficiente — recusa amigável, sem débito.</summary>
            NotEnoughGold,
            /// <summary>Configuração inválida (acessores ausentes / custo ≤ 0) — sem débito.</summary>
            Invalid
        }

        public readonly struct Result
        {
            public readonly Outcome Outcome;
            public readonly int Cost;
            public readonly int GoldBefore;

            public Result(Outcome outcome, int cost, int goldBefore)
            {
                Outcome = outcome;
                Cost = cost;
                GoldBefore = goldBefore;
            }

            public bool Paid => Outcome == Outcome.Paid;
        }

        /// <summary>
        /// Tenta cobrar a diária. <paramref name="currentGold"/> lê o saldo; <paramref name="trySpend"/>
        /// efetiva o débito (custo → debitado?). Ordem: valida acessores e custo → checa saldo →
        /// debita. Nenhum débito ocorre fora do caminho <see cref="Outcome.Paid"/>.
        /// </summary>
        public static Result TryPayNightlyRate(
            int cost,
            System.Func<int> currentGold,
            System.Func<int, bool> trySpend)
        {
            if (cost <= 0 || currentGold == null || trySpend == null)
            {
                return new Result(Outcome.Invalid, cost, 0);
            }

            int gold;
            try { gold = currentGold(); }
            catch { return new Result(Outcome.Invalid, cost, 0); }

            if (gold < cost)
            {
                return new Result(Outcome.NotEnoughGold, cost, gold);
            }

            bool spent;
            try { spent = trySpend(cost); }
            catch { spent = false; }

            if (!spent)
            {
                // Saldo dizia que dava, mas o débito falhou: trate como sem-ouro (sem efeito colateral).
                return new Result(Outcome.NotEnoughGold, cost, gold);
            }

            return new Result(Outcome.Paid, cost, gold);
        }
    }
}
