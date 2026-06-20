namespace CindarsHope.NPC.Services
{
    /// <summary>
    /// fable_25 — HOOK PONTUAL do "Reparo com Desconto" do Brumdar (custo de reparo F49). Acessor
    /// estático único (padrão <c>TemperingForgeAccess</c>): o serviço ARMA um desconto fracionário; o
    /// caminho de reparo (RepairUpgradeViewModel/forja) CONSOME-o ao calcular o próximo custo via
    /// <see cref="ApplyToRepairCost"/>. Mantém o desconto numa fonte única, sem espalhar um if pelos
    /// sistemas e sem reimplementar o reparo aqui.
    ///
    /// Consumo pelo forge é a 1-linha pontual prevista no escopo; enquanto a UI de reparo não estiver
    /// ligada em cena (Play Mode humano), o desconto fica armado e é aplicado assim que o caminho de
    /// reparo o consultar — comportamento determinístico e EditMode-testável.
    /// </summary>
    public static class NpcRepairDiscountState
    {
        /// <summary>Fração de desconto armada para o próximo reparo (0..1). 0 = sem desconto.</summary>
        public static float PendingDiscountFraction { get; private set; }

        public static bool HasPendingDiscount => PendingDiscountFraction > 0f;

        /// <summary>Arma o desconto (clamp 0..1). Chamado pelo efeito RepairDiscount do executor.</summary>
        public static void ArmDiscount(float fraction)
        {
            if (fraction < 0f) fraction = 0f;
            if (fraction > 1f) fraction = 1f;
            if (fraction > PendingDiscountFraction) PendingDiscountFraction = fraction;
        }

        /// <summary>
        /// Aplica e CONSOME o desconto armado a um custo de reparo (custo × (1 − fração)), arredondado
        /// para baixo, mínimo 0. Sem desconto armado, retorna o custo inalterado. Idempotente após o
        /// consumo (o desconto é one-shot até ser re-armado por novo uso do serviço).
        /// </summary>
        public static int ApplyToRepairCost(int baseCost)
        {
            if (baseCost <= 0 || PendingDiscountFraction <= 0f) return baseCost;
            int discounted = (int)System.Math.Floor(baseCost * (1f - PendingDiscountFraction));
            PendingDiscountFraction = 0f; // consome (one-shot)
            return discounted < 0 ? 0 : discounted;
        }

        /// <summary>Peek sem consumir (preview de custo). Não altera o estado armado.</summary>
        public static int PreviewRepairCost(int baseCost)
        {
            if (baseCost <= 0 || PendingDiscountFraction <= 0f) return baseCost;
            int discounted = (int)System.Math.Floor(baseCost * (1f - PendingDiscountFraction));
            return discounted < 0 ? 0 : discounted;
        }

        /// <summary>Limpa o desconto (teardown de teste / novo jogo).</summary>
        public static void Reset()
        {
            PendingDiscountFraction = 0f;
        }
    }
}
