using System.Collections.Generic;
using CindarsHope.NPC.Friendship;

namespace CindarsHope.NPC
{
    /// <summary>
    /// fable_46 — autor (em código) das falas de romance, montadas como <see cref="ConditionalDialogueLine"/>
    /// para o MESMO <see cref="DialogueLineSelector"/> do F28 (sem segundo sistema de fala): a "Confessar"
    /// é uma fala gated por IsPartner=false + romance None; as falas de parceiro são gated por
    /// RequiresPartner / MinRomanceStage; a recusa do 3º pedido tem um texto digno e neutro.
    ///
    /// Texto via string-key (LocalizationService, ADR-0012): cada fala carrega uma chave; a UI resolve.
    /// As chaves caem em fallback para si mesmas quando ausentes do string table (padrão do projeto),
    /// então o conteúdo final pode ser localizado depois sem mudar este código.
    /// </summary>
    public static class RomanceDialoguePool
    {
        // Mínimo da spec: 3 falas por estágio de parceiro.
        public const int MinLinesPerStage = 3;

        /// <summary>
        /// Pool de falas de PARCEIRO por estágio (Namoro/Compromisso). Cada fala é gated por
        /// MinRomanceStage, então um conhecido (None/Interesse) nunca a vê — IsPartner muda as falas.
        /// </summary>
        public static List<ConditionalDialogueLine> PartnerLines(string npcId)
        {
            var lines = new List<ConditionalDialogueLine>();

            // Namoro (stage 2) — 3 falas.
            AddStageLines(lines, npcId, RomanceStage.Namoro, count: MinLinesPerStage);
            // Compromisso (stage 3) — 3 falas (mais íntimas).
            AddStageLines(lines, npcId, RomanceStage.Compromisso, count: MinLinesPerStage);

            return lines;
        }

        /// <summary>
        /// Fala de RECUSA quando a confissão é negada. Mapeia o motivo determinístico para uma chave de
        /// texto digna/neutra (CA-2: o limite de 2 tem fala dedicada). Fallback = a própria chave.
        /// </summary>
        public static string RejectionKey(RomanceConfessionRejection reason)
        {
            switch (reason)
            {
                case RomanceConfessionRejection.PartnerLimitReached:
                    return "dialogue.romance.reject.partner_limit";
                case RomanceConfessionRejection.FriendshipTooLow:
                    return "dialogue.romance.reject.friendship_low";
                case RomanceConfessionRejection.ChainIncomplete:
                    return "dialogue.romance.reject.chain_incomplete";
                case RomanceConfessionRejection.ActGateNotMet:
                    return "dialogue.romance.reject.not_yet";
                case RomanceConfessionRejection.AlreadyRomanced:
                    return "dialogue.romance.reject.already";
                case RomanceConfessionRejection.NotEligible:
                    return "dialogue.romance.reject.not_eligible";
                default:
                    return "dialogue.romance.reject.generic";
            }
        }

        /// <summary>Chave da entrada "Confessar" oferecida quando todos os gates passam (entrada de diálogo).</summary>
        public const string ConfessEntryKey = "dialogue.romance.confess.entry";

        /// <summary>Chave da fala de confissão ACEITA (None→Interesse).</summary>
        public const string ConfessAcceptedKey = "dialogue.romance.confess.accepted";

        private static void AddStageLines(List<ConditionalDialogueLine> lines, string npcId,
            RomanceStage stage, int count)
        {
            for (int i = 1; i <= count; i++)
            {
                var condition = new DialogueLineCondition
                {
                    RequiresPartner = true,
                    MinRomanceStage = (int)stage
                };
                // Chave por NPC + estágio + índice (personalizável por NPC; fallback à chave).
                string key = $"dialogue.romance.partner.{npcId}.{(int)stage}.{i}";
                lines.Add(new ConditionalDialogueLine(key, condition));
            }
        }
    }
}
