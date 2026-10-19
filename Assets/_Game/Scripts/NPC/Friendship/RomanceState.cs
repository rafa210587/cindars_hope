using System;
using System.Collections.Generic;

namespace CindarsHope.NPC.Friendship
{
    /// <summary>
    /// fable_46 — núcleo PURO (sem Unity) do estado de romance por NPC, por cima da amizade (F26).
    /// Mantém, por npcId: estágio (RomanceStage), progresso do estágio atual (interações + presentes)
    /// e o dia da confissão. Aplica os gates de confissão, o limite de 2 parceiros simultâneos
    /// (decisão §13) e a progressão por marcos. Os gates dependentes de outros sistemas (amizade,
    /// cadeia F35, flag de ato F36) entram por delegates injetados a cada chamada, então este núcleo
    /// permanece testável em EditMode sem cena.
    ///
    /// Sem refs Unity, sem GameEventBus aqui — o MonoBehaviour <see cref="RomanceService"/> envolve
    /// este núcleo, injeta as probes reais e publica os eventos.
    /// </summary>
    public sealed class RomanceState
    {
        private sealed class Entry
        {
            public RomanceStage Stage;
            public int StageProgress;   // interações de parceiro acumuladas no estágio atual
            public int StageGifts;      // presentes de parceiro acumulados no estágio atual
            public int ConfessedDay = -1;
        }

        private readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>();

        /// <summary>Probes de gate injetadas pelo serviço (leitura pura de outros sistemas).</summary>
        public readonly struct GateProbe
        {
            /// <summary>(npcId, level) ⇒ amizade do NPC é pelo menos o nível (F26 IsAtLeast).</summary>
            public readonly Func<string, int, bool> FriendshipAtLeast;

            /// <summary>(npcId) ⇒ a cadeia pessoal F35 do NPC está concluída (done flag do passo final).</summary>
            public readonly Func<string, bool> ChainComplete;

            /// <summary>(flagId) ⇒ flag de história/ato setada (F36; só consultada para NPC tardio com gate de ato).</summary>
            public readonly Func<string, bool> FlagSet;

            public GateProbe(Func<string, int, bool> friendshipAtLeast, Func<string, bool> chainComplete,
                Func<string, bool> flagSet)
            {
                FriendshipAtLeast = friendshipAtLeast;
                ChainComplete = chainComplete;
                FlagSet = flagSet;
            }
        }

        /// <summary>Resultado de uma tentativa de confissão.</summary>
        public readonly struct ConfessResult
        {
            public readonly bool Accepted;
            public readonly RomanceConfessionRejection Rejection;
            public readonly RomanceStage NewStage;

            public ConfessResult(bool accepted, RomanceConfessionRejection rejection, RomanceStage newStage)
            {
                Accepted = accepted;
                Rejection = rejection;
                NewStage = newStage;
            }
        }

        /// <summary>Resultado de uma tentativa de avanço de estágio.</summary>
        public readonly struct AdvanceResult
        {
            public readonly bool Changed;
            public readonly RomanceStage PreviousStage;
            public readonly RomanceStage NewStage;

            public AdvanceResult(bool changed, RomanceStage previousStage, RomanceStage newStage)
            {
                Changed = changed;
                PreviousStage = previousStage;
                NewStage = newStage;
            }
        }

        // ── Consultas ────────────────────────────────────────────────────────────────────────────

        public RomanceStage GetStage(string npcId)
        {
            if (string.IsNullOrEmpty(npcId)) return RomanceStage.None;
            return _entries.TryGetValue(npcId, out var e) ? e.Stage : RomanceStage.None;
        }

        /// <summary>True quando o NPC é parceiro (stage &gt;= Namoro): conta no limite, falas e bônus.</summary>
        public bool IsPartner(string npcId) => GetStage(npcId) >= RomanceStage.Namoro;

        /// <summary>Lista (ordem de inserção) dos npcIds que são parceiros atuais (stage &gt;= Namoro).</summary>
        public IReadOnlyList<string> Partners()
        {
            var list = new List<string>();
            foreach (var kv in _entries)
            {
                if (kv.Value.Stage >= RomanceStage.Namoro) list.Add(kv.Key);
            }
            return list;
        }

        public int PartnerCount()
        {
            int n = 0;
            foreach (var kv in _entries)
            {
                if (kv.Value.Stage >= RomanceStage.Namoro) n++;
            }
            return n;
        }

        // ── Confissão (CA-1 / CA-2 / CA-3) ─────────────────────────────────────────────────────────

        /// <summary>
        /// Avalia o motivo de recusa de uma confissão SEM mutar estado (usado pela entrada de diálogo
        /// e pelo evento de recusa). Retorna None quando a confissão seria aceita.
        /// </summary>
        public RomanceConfessionRejection EvaluateConfession(string npcId, GateProbe probe)
        {
            var status = RomanceEligibilityTable.StatusOf(npcId);
            if (status == RomanceEligibilityStatus.Unavailable)
            {
                return RomanceConfessionRejection.NotEligible;
            }

            // Já existe romance com este NPC (não reconfessar).
            if (GetStage(npcId) >= RomanceStage.Interesse)
            {
                return RomanceConfessionRejection.AlreadyRomanced;
            }

            // Gate de amizade (F26).
            bool friendshipOk = probe.FriendshipAtLeast == null
                || probe.FriendshipAtLeast(npcId, RomanceEligibilityTable.ConfessFriendshipLevel);
            if (!friendshipOk)
            {
                return RomanceConfessionRejection.FriendshipTooLow;
            }

            // Gate de cadeia pessoal F35.
            bool chainOk = probe.ChainComplete == null || probe.ChainComplete(npcId);
            if (!chainOk)
            {
                return RomanceConfessionRejection.ChainIncomplete;
            }

            // Gate de ato (só NPC tardio com flag — Nymiriano/act_3_done).
            string actFlag = RomanceEligibilityTable.ActGateFlagFor(npcId);
            if (!string.IsNullOrEmpty(actFlag))
            {
                bool actOk = probe.FlagSet != null && probe.FlagSet(actFlag);
                if (!actOk)
                {
                    return RomanceConfessionRejection.ActGateNotMet;
                }
            }

            // Limite de 2 parceiros simultâneos (decisão §13). Confessar leva a Interesse (ainda não
            // parceiro), mas a recusa por limite acontece JÁ na confissão para uma fala digna.
            if (PartnerCount() >= RomanceEligibilityTable.MaxSimultaneousPartners)
            {
                return RomanceConfessionRejection.PartnerLimitReached;
            }

            return RomanceConfessionRejection.None;
        }

        /// <summary>True quando todos os gates de confissão estão satisfeitos (entrada de diálogo "Confessar").</summary>
        public bool CanConfess(string npcId, GateProbe probe)
        {
            return EvaluateConfession(npcId, probe) == RomanceConfessionRejection.None;
        }

        /// <summary>
        /// Tenta confessar. Em sucesso, leva o NPC a Interesse e registra o dia. Em recusa, estado
        /// inalterado e o motivo determinístico é devolvido.
        /// </summary>
        public ConfessResult Confess(string npcId, GateProbe probe, int currentDay)
        {
            var rejection = EvaluateConfession(npcId, probe);
            if (rejection != RomanceConfessionRejection.None)
            {
                return new ConfessResult(false, rejection, GetStage(npcId));
            }

            var entry = GetOrCreate(npcId);
            entry.Stage = RomanceStage.Interesse;
            entry.StageProgress = 0;
            entry.StageGifts = 0;
            entry.ConfessedDay = currentDay;
            return new ConfessResult(true, RomanceConfessionRejection.None, RomanceStage.Interesse);
        }

        // ── Progressão por marcos (CA-1 progressão) ─────────────────────────────────────────────────

        /// <summary>
        /// Registra uma interação de parceiro no estágio atual e, se os marcos do estágio (N interações
        /// + 1 presente) estiverem batidos, avança um estágio (até Compromisso). Antes de Compromisso,
        /// o avanço para Namoro respeita o limite de 2 parceiros (uma 3ª promoção a parceiro é negada).
        /// </summary>
        public AdvanceResult RegisterPartnerInteraction(string npcId)
        {
            if (GetStage(npcId) < RomanceStage.Interesse) return Noop(npcId);
            var entry = GetOrCreate(npcId);
            entry.StageProgress++;
            return TryAdvance(npcId, entry);
        }

        /// <summary>
        /// Registra um presente de parceiro/pretendente no estágio atual e tenta avançar pelos marcos.
        /// Chamado pelo serviço dentro do fluxo único de presente (F26), só quando o presente é aceito.
        /// </summary>
        public AdvanceResult RegisterPartnerGift(string npcId)
        {
            if (GetStage(npcId) < RomanceStage.Interesse) return Noop(npcId);
            var entry = GetOrCreate(npcId);
            entry.StageGifts++;
            return TryAdvance(npcId, entry);
        }

        private AdvanceResult TryAdvance(string npcId, Entry entry)
        {
            if (entry.Stage >= RomanceStage.Compromisso) return new AdvanceResult(false, entry.Stage, entry.Stage);

            bool milestonesMet = entry.StageProgress >= RomanceEligibilityTable.PartnerInteractionsPerStage
                                 && entry.StageGifts >= RomanceEligibilityTable.PartnerGiftsPerStage;
            if (!milestonesMet) return new AdvanceResult(false, entry.Stage, entry.Stage);

            var next = entry.Stage + 1;

            // Promover a parceiro (Interesse→Namoro) respeita o limite de 2 simultâneos.
            if (next == RomanceStage.Namoro && PartnerCount() >= RomanceEligibilityTable.MaxSimultaneousPartners)
            {
                return new AdvanceResult(false, entry.Stage, entry.Stage);
            }

            var previous = entry.Stage;
            entry.Stage = next;
            entry.StageProgress = 0;
            entry.StageGifts = 0;
            return new AdvanceResult(true, previous, next);
        }

        // ── Save (round-trip / legado) ───────────────────────────────────────────────────────────

        /// <summary>Aplica os campos aditivos de romance sobre as entradas de amizade já capturadas.</summary>
        public void WriteInto(FriendshipSaveData data)
        {
            if (data == null) return;
            if (data.Entries == null) return;

            foreach (var saved in data.Entries)
            {
                if (saved == null || string.IsNullOrEmpty(saved.NpcId)) continue;
                if (_entries.TryGetValue(saved.NpcId, out var entry))
                {
                    saved.RomanceStage = (int)entry.Stage;
                    saved.RomanceStageProgress = entry.StageProgress;
                    saved.RomanceStageGifts = entry.StageGifts;
                    saved.RomanceConfessedDay = entry.ConfessedDay;
                }
            }

            // NPCs com romance mas SEM entrada de amizade (improvável: romance exige amizade 5) —
            // garante persistência mesmo assim, criando a entrada aditiva.
            foreach (var kv in _entries)
            {
                if (kv.Value.Stage == RomanceStage.None && kv.Value.ConfessedDay < 0) continue;
                bool exists = false;
                foreach (var saved in data.Entries)
                {
                    if (saved != null && saved.NpcId == kv.Key) { exists = true; break; }
                }
                if (exists) continue;
                data.Entries.Add(new FriendshipEntrySaveData
                {
                    NpcId = kv.Key,
                    RomanceStage = (int)kv.Value.Stage,
                    RomanceStageProgress = kv.Value.StageProgress,
                    RomanceStageGifts = kv.Value.StageGifts,
                    RomanceConfessedDay = kv.Value.ConfessedDay
                });
            }
        }

        /// <summary>
        /// Restaura os campos de romance. saveData null/legado (campos ausentes ⇒ default 0/-1) ⇒
        /// todos None. Estágio fora do range é clampado para None (defensivo).
        /// </summary>
        public void RestoreFrom(FriendshipSaveData saveData)
        {
            _entries.Clear();
            if (saveData == null || saveData.Entries == null) return;

            foreach (var saved in saveData.Entries)
            {
                if (saved == null || string.IsNullOrEmpty(saved.NpcId)) continue;

                var stage = ClampStage(saved.RomanceStage);
                // Entrada inerte: sem romance e sem confissão ⇒ não materializa estado (mantém None).
                if (stage == RomanceStage.None && saved.RomanceConfessedDay < 0
                    && saved.RomanceStageProgress == 0 && saved.RomanceStageGifts == 0)
                {
                    continue;
                }

                _entries[saved.NpcId] = new Entry
                {
                    Stage = stage,
                    StageProgress = saved.RomanceStageProgress < 0 ? 0 : saved.RomanceStageProgress,
                    StageGifts = saved.RomanceStageGifts < 0 ? 0 : saved.RomanceStageGifts,
                    ConfessedDay = saved.RomanceConfessedDay
                };
            }
        }

        // ── Internos ─────────────────────────────────────────────────────────────────────────────

        private static RomanceStage ClampStage(int raw)
        {
            if (raw < (int)RomanceStage.None || raw > (int)RomanceStage.Compromisso) return RomanceStage.None;
            return (RomanceStage)raw;
        }

        private Entry GetOrCreate(string npcId)
        {
            if (!_entries.TryGetValue(npcId, out var entry))
            {
                entry = new Entry();
                _entries[npcId] = entry;
            }
            return entry;
        }

        private AdvanceResult Noop(string npcId)
        {
            var stage = GetStage(npcId);
            return new AdvanceResult(false, stage, stage);
        }
    }
}
