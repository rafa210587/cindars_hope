using System.Collections.Generic;

namespace CindarsHope.NPC.Friendship
{
    /// <summary>
    /// fable_26 — núcleo PURO (sem Unity) do estado de amizade por NPC. Mantém pontos e marcadores
    /// de dia dos caps, aplica thresholds 0-5 (10/30/60/100/150), caps diários por fonte, gosto de
    /// presente (precedência + clamp em 0) e a API de gate IsAtLeast. O MonoBehaviour FriendshipService
    /// envolve este núcleo, traduz hooks de evento e publica FriendshipLevelChangedEvent.
    ///
    /// Decaimento OFF. npcId desconhecido = nível 0/false, sem exceção.
    /// </summary>
    public sealed class FriendshipState
    {
        // Thresholds canônicos: índice = nível, valor = pontos mínimos. Fonte de verdade dos níveis.
        public static readonly int[] LevelThresholds = { 0, 10, 30, 60, 100, 150 };
        public const int MaxLevel = 5;

        private sealed class Entry
        {
            public int Points;
            public int LastTalkDay = -1;
            public int LastGiftDay = -1;
            public int LastPurchaseDay = -1;
        }

        private readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>();

        // Idempotência de quest por sessão (o save não persiste o set; QuestService já não re-publica
        // QuestCompletedEvent para uma quest concluída — idempotência cross-reload vem de lá).
        private readonly HashSet<string> _creditedQuestIds = new HashSet<string>();

        /// <summary>Resultado de uma aplicação de pontos: variação de nível para o caller publicar evento.</summary>
        public readonly struct ApplyResult
        {
            public readonly bool Applied;        // true se houve QUALQUER variação de pontos
            public readonly int PointsDelta;     // delta efetivo aplicado (pode ser negativo ou 0)
            public readonly int PreviousLevel;
            public readonly int NewLevel;

            public ApplyResult(bool applied, int pointsDelta, int previousLevel, int newLevel)
            {
                Applied = applied;
                PointsDelta = pointsDelta;
                PreviousLevel = previousLevel;
                NewLevel = newLevel;
            }

            public bool LevelChanged => NewLevel != PreviousLevel;
        }

        // ── Consultas (API de gate consumível por F25/F28/F35) ───────────────────────────────────

        public int GetPoints(string npcId)
        {
            if (string.IsNullOrEmpty(npcId)) return 0;
            return _entries.TryGetValue(npcId, out var e) ? e.Points : 0;
        }

        public int GetLevel(string npcId)
        {
            return LevelForPoints(GetPoints(npcId));
        }

        public FriendshipLevel GetLevelEnum(string npcId)
        {
            return (FriendshipLevel)GetLevel(npcId);
        }

        /// <summary>Contrato estável de gate. npcId desconhecido ⇒ false (nível 0). Decaimento OFF.</summary>
        public bool IsAtLeast(string npcId, int level)
        {
            return GetLevel(npcId) >= level;
        }

        public bool IsAtLeast(string npcId, FriendshipLevel level)
        {
            return IsAtLeast(npcId, (int)level);
        }

        /// <summary>Converte pontos para nível usando os thresholds canônicos.</summary>
        public static int LevelForPoints(int points)
        {
            int level = 0;
            for (int i = 1; i <= MaxLevel; i++)
            {
                if (points >= LevelThresholds[i]) level = i;
                else break;
            }
            return level;
        }

        // ── Aplicação genérica de pontos (clamp no piso 0; sem teto além do nível 5) ──────────────

        /// <summary>
        /// Soma <paramref name="delta"/> aos pontos do NPC (cria entrada se necessário), com clamp em 0.
        /// Não aplica caps — quem aplica caps são os métodos de fonte. Retorna a variação de nível.
        /// </summary>
        public ApplyResult AddPoints(string npcId, int delta)
        {
            if (string.IsNullOrEmpty(npcId))
            {
                return new ApplyResult(false, 0, 0, 0);
            }

            var entry = GetOrCreate(npcId);
            int before = entry.Points;
            int previousLevel = LevelForPoints(before);

            int after = before + delta;
            if (after < 0) after = 0; // clamp no piso do nível 0 (emenda V3 §2)
            entry.Points = after;

            int newLevel = LevelForPoints(after);
            int effectiveDelta = after - before;
            return new ApplyResult(effectiveDelta != 0, effectiveDelta, previousLevel, newLevel);
        }

        // ── Fonte 1: primeira conversa do dia (+1, 1×/dia/NPC) ───────────────────────────────────

        public ApplyResult RegisterDailyConversation(string npcId, int currentDay)
        {
            if (string.IsNullOrEmpty(npcId)) return Noop(npcId);
            var entry = GetOrCreate(npcId);
            if (entry.LastTalkDay == currentDay)
            {
                return Noop(npcId); // cap: já conversou hoje
            }
            entry.LastTalkDay = currentDay;
            return AddPoints(npcId, 1);
        }

        // ── Fonte 2: side quest do NPC concluída (+8, idempotente por questId) ────────────────────

        public ApplyResult RegisterQuestCompleted(string npcId, string questId)
        {
            if (string.IsNullOrEmpty(npcId) || string.IsNullOrEmpty(questId)) return Noop(npcId);
            if (_creditedQuestIds.Contains(questId))
            {
                return Noop(npcId); // idempotência por quest
            }
            _creditedQuestIds.Add(questId);
            return AddPoints(npcId, 8);
        }

        // ── Fonte 3: presente (delta pelo gosto; cap DailyGiftLimit/dia/NPC) ──────────────────────

        /// <summary>
        /// Resultado de presentear: distingue recusa por cap/Giftable de aplicação efetiva.
        /// </summary>
        public readonly struct GiftResult
        {
            public readonly bool Accepted;       // false se recusado (cap diário) — item NÃO deve ser consumido
            public readonly GiftTaste Taste;
            public readonly ApplyResult Apply;

            public GiftResult(bool accepted, GiftTaste taste, ApplyResult apply)
            {
                Accepted = accepted;
                Taste = taste;
                Apply = apply;
            }
        }

        /// <summary>
        /// Presenteia: classifica o item pelo gosto do NPC, aplica o delta (com clamp) e marca o dia.
        /// O caller já deve ter validado ItemTag.Giftable (recusa silenciosa fora daqui).
        /// Acima do DailyGiftLimit no mesmo dia ⇒ recusa amigável (Accepted=false, sem ganho/perda).
        ///
        /// fable_57: <paramref name="pointsMultiplier"/> (≥1; default 1) multiplica os pontos do
        /// presente — usado pelo aniversário do NPC (×2). O cap diário é checado ANTES do
        /// multiplicador, logo um 2º presente no mesmo dia continua sendo recusado mesmo no
        /// aniversário (o multiplicador NÃO fura o cap). Multiplicador inválido (≤0) é tratado como 1.
        /// </summary>
        public GiftResult RegisterGift(string npcId, GiftTaste taste, int currentDay, int dailyGiftLimit,
            int pointsMultiplier = 1, float bonusMultiplier = 1f)
        {
            if (string.IsNullOrEmpty(npcId))
            {
                return new GiftResult(false, taste, Noop(npcId));
            }

            var entry = GetOrCreate(npcId);
            int limit = dailyGiftLimit > 0 ? dailyGiftLimit : 1;

            // Com DailyGiftLimit padrão = 1, basta comparar o marcador de dia. (>1/dia é refinamento
            // futuro; o contrato atual é 1 presente relevante por dia por NPC.)
            if (limit <= 1 && entry.LastGiftDay == currentDay)
            {
                return new GiftResult(false, taste, Noop(npcId)); // cap diário atingido (mesmo no aniversário)
            }

            entry.LastGiftDay = currentDay;
            int multiplier = pointsMultiplier > 0 ? pointsMultiplier : 1;

            // fable_46: bonusMultiplier (>0; default 1) é o bônus de PARCEIRO (+50% ⇒ 1.5). Aplicado
            // SÓ APÓS o cap diário (acima), no ponto único de ganho por presente — sem segundo caminho
            // de pontos e sem furar o cap. Combina multiplicativamente com o multiplicador de
            // aniversário do fable_57. Arredonda para o inteiro mais próximo (preserva o sinal do gosto).
            float bonus = bonusMultiplier > 0f ? bonusMultiplier : 1f;
            int baseDelta = GiftTasteClassifier.DeltaFor(taste) * multiplier;
            int finalDelta = (int)System.Math.Round(baseDelta * bonus, System.MidpointRounding.AwayFromZero);
            var apply = AddPoints(npcId, finalDelta);
            return new GiftResult(true, taste, apply);
        }

        // ── Fonte 4: compra na loja do NPC (+1, 1×/dia/NPC) ──────────────────────────────────────

        public ApplyResult RegisterShopPurchase(string npcId, int currentDay)
        {
            if (string.IsNullOrEmpty(npcId)) return Noop(npcId);
            var entry = GetOrCreate(npcId);
            if (entry.LastPurchaseDay == currentDay)
            {
                return Noop(npcId); // cap: já comprou hoje
            }
            entry.LastPurchaseDay = currentDay;
            return AddPoints(npcId, 1);
        }

        // ── Save (round-trip / legado) ───────────────────────────────────────────────────────────

        public FriendshipSaveData CaptureSaveData()
        {
            var data = new FriendshipSaveData();
            foreach (var kv in _entries)
            {
                data.Entries.Add(new FriendshipEntrySaveData
                {
                    NpcId = kv.Key,
                    Points = kv.Value.Points,
                    LastTalkDay = kv.Value.LastTalkDay,
                    LastGiftDay = kv.Value.LastGiftDay,
                    LastPurchaseDay = kv.Value.LastPurchaseDay
                });
            }
            return data;
        }

        /// <summary>
        /// Restaura. saveData null/vazio (legado) ⇒ estado limpo (todos nível 0), sem erro.
        /// Entradas com NpcId inválido são ignoradas; pontos negativos são clampados em 0.
        /// </summary>
        public void RestoreFromSaveData(FriendshipSaveData saveData)
        {
            _entries.Clear();
            // _creditedQuestIds NÃO é persistido; mantém-se intra-sessão (QuestService dá idempotência
            // cross-reload). Limpa para refletir um carregamento de jogo novo.
            _creditedQuestIds.Clear();

            if (saveData == null || saveData.Entries == null)
            {
                return;
            }

            foreach (var saved in saveData.Entries)
            {
                if (saved == null || string.IsNullOrEmpty(saved.NpcId))
                {
                    continue; // id inválido ignorado (sem erro)
                }

                _entries[saved.NpcId] = new Entry
                {
                    Points = saved.Points < 0 ? 0 : saved.Points,
                    LastTalkDay = saved.LastTalkDay,
                    LastGiftDay = saved.LastGiftDay,
                    LastPurchaseDay = saved.LastPurchaseDay
                };
            }
        }

        // ── Internos ─────────────────────────────────────────────────────────────────────────────

        private Entry GetOrCreate(string npcId)
        {
            if (!_entries.TryGetValue(npcId, out var entry))
            {
                entry = new Entry();
                _entries[npcId] = entry;
            }
            return entry;
        }

        private ApplyResult Noop(string npcId)
        {
            int level = GetLevel(npcId);
            return new ApplyResult(false, 0, level, level);
        }
    }
}
