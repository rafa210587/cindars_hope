namespace CindarsHope.NPC.Services
{
    /// <summary>
    /// fable_25 — os 8 efeitos canônicos dos serviços únicos de NPC (CITY_NPC_ROSTER §serviços).
    /// O <see cref="NpcServiceExecutor"/> despacha cada efeito pela interface do sistema-alvo já
    /// existente (F21/F16/F12/F31, hooks de reparo/pasto) — NENHUM efeito é reimplementado aqui.
    /// </summary>
    public enum NpcServiceEffectType
    {
        /// <summary>Thalindra — Análise de Criatura: 80g + 1 parte → GrantKnowledge (F21).</summary>
        CreatureAnalysis = 0,

        /// <summary>Brumdar — Reparo com Desconto: amizade 3+ → reparo −30% (hook no custo F49).</summary>
        RepairDiscount = 1,

        /// <summary>Yael — Encomenda de Livro: 200g → livro de conhecimento chega no dia +3.</summary>
        BookOrder = 2,

        /// <summary>Sereth — Banho Termal: 50g → remove Fatigue + buff Rested (F16/F01).</summary>
        ThermalBath = 3,

        /// <summary>Eiran — Pasto Premium: amizade 4+ → animais alimentados por 3 dias (F12).</summary>
        PremiumPasture = 4,

        /// <summary>Kael — Contrato de Caça Pessoal: semanal, alvo elite, recompensa 2× (flag/quest).</summary>
        HuntContract = 5,

        /// <summary>Mirena — Prato do Dia: 1×/dia grátis com amizade 2+ → comida com buff do dia.</summary>
        DailyDish = 6,

        /// <summary>Veska — Identificação de Relíquia: itens unidentified_* (F31) → revela por 120g.</summary>
        RelicIdentification = 7
    }
}
