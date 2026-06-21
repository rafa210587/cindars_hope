using System.Collections.Generic;

namespace CindarsHope.NPC.Friendship
{
    /// <summary>
    /// fable_46 — elegibilidade de romance por NPC (status canônico). Fonte ÚNICA derivada do
    /// CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1 §4-6 e das decisões FABLE_DECISOES §1/§13.
    ///
    /// Decisões vinculantes refletidas aqui:
    /// - 11 candidatos elegíveis (Sylveth, Ozzra, Zrix, Yael, Thalindra, Dagna, Ser Alaric, Eiran,
    ///   Liora, Savra) + Maelor como TARDIO (LateActGated: amizade 5 + cadeia própria) — §6;
    /// - Nymiriano é tardio gated pelo Ato 3 (act_3_done, F36) e ainda NÃO está no roster runtime
    ///   (chega via F36); registrado aqui como LateActGated para a fundação não ter buraco;
    /// - todos os elegíveis são bissexuais — SEM checagem de gênero do jogador em nenhum ponto (§13);
    /// - poliamor com MÁXIMO 2 parceiros simultâneos (§13 SOBREPÕE o canon antigo de 3).
    /// </summary>
    public enum RomanceEligibilityStatus
    {
        /// <summary>NPC nunca romanceável (casado, papel não-romântico, etc.).</summary>
        Unavailable = 0,

        /// <summary>Candidato canônico, elegível sob os gates padrão (amizade 5 + cadeia F35 + confissão).</summary>
        Eligible = 1,

        /// <summary>Elegível só com gate adicional (cadeia própria/flag de ato). Ex.: Maelor, Nymiriano.</summary>
        LateActGated = 2
    }

    /// <summary>Motivo determinístico de recusa de uma tentativa de confissão (para evento/toast/teste).</summary>
    public enum RomanceConfessionRejection
    {
        None = 0,
        NotEligible = 1,        // NPC Unavailable (ou id desconhecido)
        FriendshipTooLow = 2,   // amizade abaixo do nível exigido (F26)
        ChainIncomplete = 3,    // cadeia pessoal F35 não concluída
        ActGateNotMet = 4,      // flag de ato (Maelor/Nymiriano) ausente
        PartnerLimitReached = 5,// já há 2 parceiros simultâneos (decisão §13)
        AlreadyRomanced = 6     // já existe romance com este NPC (estágio >= Interesse)
    }

    /// <summary>
    /// Tabela canônica npcId→status + constantes de gate/progressão. Estática e pura (testável e
    /// validável contra o NpcRegistry/roster pelo validador F30). Não duplica o roster — é a única
    /// projeção de romance consumida por serviço e diálogo.
    /// </summary>
    public static class RomanceEligibilityTable
    {
        /// <summary>Nível de amizade exigido para confessar (F26 IsAtLeast). Confidente = nível máximo.</summary>
        public const int ConfessFriendshipLevel = 5;

        /// <summary>Decisão FABLE_DECISOES §13: máximo de parceiros (stage &gt;= Namoro) simultâneos.</summary>
        public const int MaxSimultaneousPartners = 2;

        /// <summary>
        /// Marcos simples v1 para avançar de estágio: N interações de parceiro + 1 presente. As duas
        /// constantes são testadas; um avanço só ocorre quando AMBOS os marcos do estágio são batidos.
        /// </summary>
        public const int PartnerInteractionsPerStage = 3;
        public const int PartnerGiftsPerStage = 1;

        /// <summary>Multiplicador de pontos de presente para parceiro (Namoro+): +50% ⇒ x1.5.</summary>
        public const float PartnerGiftPointMultiplier = 1.5f;

        /// <summary>Flag do Ato 3 (F36) que destrava o romance com o Nymiriano (tardio).</summary>
        public const string NymirianoActGateFlag = "act_3_done";

        /// <summary>Id canônico do NPC tardio Nymiriano (FABLE_DECISOES §1). Ainda não no roster runtime.</summary>
        public const string NymirianoNpcId = "npc_nymiriano";

        // Tabela canônica. Apenas os IDs que aparecem aqui têm romance; ausência = Unavailable.
        private static readonly Dictionary<string, RomanceEligibilityStatus> s_table =
            new Dictionary<string, RomanceEligibilityStatus>
            {
                // 11 candidatos elegíveis padrão (roster v1.1 §6).
                { "npc_sylveth",   RomanceEligibilityStatus.Eligible },
                { "npc_ozzra",     RomanceEligibilityStatus.Eligible },
                { "npc_zrix",      RomanceEligibilityStatus.Eligible },
                { "npc_yael",      RomanceEligibilityStatus.Eligible },
                { "npc_thalindra", RomanceEligibilityStatus.Eligible },
                { "npc_dagna",     RomanceEligibilityStatus.Eligible },
                { "npc_alaric",    RomanceEligibilityStatus.Eligible },
                { "npc_eiran",     RomanceEligibilityStatus.Eligible },
                { "npc_liora",     RomanceEligibilityStatus.Eligible },
                { "npc_savra",     RomanceEligibilityStatus.Eligible },

                // Tardios (gate adicional além da amizade 5).
                { "npc_maelor",    RomanceEligibilityStatus.LateActGated },   // cadeia própria F35 (gate de cadeia)
                { NymirianoNpcId,  RomanceEligibilityStatus.LateActGated },   // act_3_done F36
            };

        /// <summary>Status canônico do NPC. Id desconhecido/Unavailable ⇒ Unavailable (nunca confessável).</summary>
        public static RomanceEligibilityStatus StatusOf(string npcId)
        {
            if (string.IsNullOrEmpty(npcId)) return RomanceEligibilityStatus.Unavailable;
            return s_table.TryGetValue(npcId, out var status) ? status : RomanceEligibilityStatus.Unavailable;
        }

        /// <summary>True se o NPC pode, em princípio, ser romanceado (Eligible ou LateActGated).</summary>
        public static bool IsRomanceCandidate(string npcId)
        {
            var status = StatusOf(npcId);
            return status == RomanceEligibilityStatus.Eligible || status == RomanceEligibilityStatus.LateActGated;
        }

        /// <summary>
        /// Flag de ato exigida para destravar este NPC tardio, ou string vazia quando o gate de ato
        /// não se aplica (Maelor é gated pela cadeia, não por flag de ato; só o Nymiriano usa flag).
        /// </summary>
        public static string ActGateFlagFor(string npcId)
        {
            return npcId == NymirianoNpcId ? NymirianoActGateFlag : string.Empty;
        }

        /// <summary>Todos os NPCs com algum status de romance (Eligible ou LateActGated), ordem de inserção.</summary>
        public static IEnumerable<string> AllRomanceableNpcIds()
        {
            foreach (var kv in s_table)
            {
                yield return kv.Key;
            }
        }
    }
}
