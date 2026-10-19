using System;

namespace CindarsHope.NPC.Services
{
    /// <summary>
    /// fable_25 — avaliação PURA dos gates de um serviço (CA-1/CA-4). NÃO duplica regra de amizade nem
    /// de flag: recebe os predicados injetados (amizade via F26, flag via QuestFlagService) e apenas os
    /// COMPÕE numa decisão habilitado/motivo. Defaults fail-closed: sem predicado, o gate é considerado
    /// fechado (a opção aparece desabilitada com o motivo — descoberta > ocultação).
    ///
    /// Construído com a opção desabilitada de Prato do Dia/etc. em mente: o gate "já usado hoje" (limite
    /// temporal) é avaliado separadamente pelo executor/estado de pendências, não aqui (este só trata
    /// amizade + flag, que são os gates estáticos da opção).
    /// </summary>
    public static class NpcServiceGateEvaluator
    {
        /// <summary>
        /// Avalia os gates estáticos (amizade + flag) de <paramref name="def"/> e retorna o modelo de
        /// opção pronto para o Conversar. <paramref name="friendshipAtLeast"/>(npcId, level) e
        /// <paramref name="flagIsSet"/>(flagId) são os predicados injetados; null = fail-closed.
        /// </summary>
        public static NpcServiceOptionModel Evaluate(
            NpcServiceDefinition def,
            Func<string, int, bool> friendshipAtLeast,
            Func<string, bool> flagIsSet)
        {
            if (def == null)
            {
                return new NpcServiceOptionModel(null, null, string.Empty, false, "Servico indisponivel");
            }

            // Gate de amizade (F26). Sem predicado ⇒ fechado.
            if (def.HasFriendshipGate)
            {
                bool ok = false;
                try { ok = friendshipAtLeast != null && friendshipAtLeast(def.NpcId, def.FriendshipMin); }
                catch { ok = false; }

                if (!ok)
                {
                    return Disabled(def, $"Amizade {def.FriendshipMin} necessaria");
                }
            }

            // Gate de flag de quest. Sem predicado ⇒ fechado.
            if (def.HasFlagGate)
            {
                bool ok = false;
                try { ok = flagIsSet != null && flagIsSet(def.RequiredQuestFlag); }
                catch { ok = false; }

                if (!ok)
                {
                    return Disabled(def, "Requisito de missao pendente");
                }
            }

            return new NpcServiceOptionModel(def.ServiceId, def.NpcId, def.DisplayLabel, true, string.Empty);
        }

        private static NpcServiceOptionModel Disabled(NpcServiceDefinition def, string reason)
        {
            return new NpcServiceOptionModel(def.ServiceId, def.NpcId, def.DisplayLabel, false, reason);
        }
    }
}
