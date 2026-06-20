using System;
using System.Collections.Generic;

namespace CindarsHope.NPC.Services
{
    /// <summary>
    /// fable_25 — seção de save PEQUENA e ADITIVA das pendências de serviço (CA-5). Padrão WI-18:
    /// só tipos simples + IDs estáveis (ADR-0006), sem refs Unity, default vazio, sem migration. Save
    /// legado (sem este campo) ⇒ Restore(null) = nenhuma pendência, sem erro.
    /// </summary>
    [Serializable]
    public sealed class NpcServicesSaveData
    {
        /// <summary>Encomendas de livro pendentes (Yael): banda escolhida + dia de entrega absoluto.</summary>
        public List<NpcBookOrderSaveData> PendingBookOrders = new List<NpcBookOrderSaveData>();

        /// <summary>Contrato de caça semanal ativo (Kael/Zrix): alvo + semana + concluído.</summary>
        public NpcHuntContractSaveData HuntContract;

        /// <summary>Último dia absoluto em que o Prato do Dia (Mirena/Orlan) foi usado. 0 = nunca.</summary>
        public int DailyDishLastUsedDay;

        /// <summary>Pasto Premium (Eiran): dia absoluto até o qual os animais são alimentados
        /// automaticamente (inclusive). 0 = inativo. Reativável; o host re-alimenta a cada DayStarted.</summary>
        public int PremiumPastureActiveUntilDay;
    }

    /// <summary>fable_25 — uma encomenda de livro pendente. Item entregue resolvido pela banda.</summary>
    [Serializable]
    public sealed class NpcBookOrderSaveData
    {
        /// <summary>ID do item de livro a entregar (banda à escolha do jogador, resolvida na compra).</summary>
        public string DeliveredBookItemId;

        /// <summary>Dia absoluto do calendário em que a encomenda chega (dia da compra + 3).</summary>
        public int DeliveryDay;
    }

    /// <summary>fable_25 — estado do contrato de caça semanal.</summary>
    [Serializable]
    public sealed class NpcHuntContractSaveData
    {
        /// <summary>ID do alvo elite da banda atual.</summary>
        public string TargetEnemyId;

        /// <summary>Número absoluto da semana em que o contrato foi emitido (DayNumber/7).</summary>
        public int IssuedWeek;

        /// <summary>True quando o alvo foi abatido e a recompensa 2× já foi concedida.</summary>
        public bool Completed;
    }
}
