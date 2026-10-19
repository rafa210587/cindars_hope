using System.Collections.Generic;
using CindarsHope.Equipment;

namespace CindarsHope.World.Altars
{
    /// <summary>Resultado tipado de uma tentativa de oração numa Marca (CA-2 / CA-5).</summary>
    public enum GodMarkPrayResult
    {
        /// <summary>Buff diário concedido.</summary>
        Granted = 0,
        /// <summary>Já orou nesta Marca hoje (recusa com feedback).</summary>
        AlreadyPrayedToday = 1,
        /// <summary>Condição não satisfeita (noite/pico/festival).</summary>
        ConditionNotMet = 2,
        /// <summary>Oferenda exigida ausente (Thandra: 1 crop).</summary>
        OfferingMissing = 3,
        /// <summary>Marca só de lore (Anya): nenhum bônus, mas oração "aceita" como ato de fé.</summary>
        LoreOnly = 4,
        /// <summary>Id desconhecido.</summary>
        UnknownMark = 5
    }

    /// <summary>
    /// Contexto externo do mundo consumido pelo <see cref="GodMarkService"/> ao validar condições.
    /// Implementado por um adapter fino de runtime (TimeManager/calendário/inventário/equipamento) —
    /// mantém o service PURO e testável (sem Unity, sem busca global de cena).
    /// </summary>
    public interface IGodMarkWorldContext
    {
        /// <summary>Dia absoluto corrente (amarra flags/buffs ao dia; vira ⇒ expira).</summary>
        int CurrentDayNumber { get; }

        /// <summary>É noite agora? (Nyx — Poço Sem Lua).</summary>
        bool IsNight { get; }

        /// <summary>Há pico lunar de Alihana hoje? (Espelho de Alihana).</summary>
        bool IsAlihanaLunarPeak { get; }

        /// <summary>Há festival ou pico de Senya hoje? (Mastro de Senya).</summary>
        bool IsSenyaFestivalOrPeak { get; }

        /// <summary>O jogador tem (e pode consumir) 1 crop como oferenda? (Thandra).</summary>
        bool HasCropOffering { get; }

        /// <summary>Consome 1 crop como oferenda; retorna true se consumiu. Só chamado se a oferenda existe.</summary>
        bool TryConsumeCropOffering();

        /// <summary>Deus da relíquia atualmente equipada (None se nenhuma) — para a regra de não-stack (CA-3).</summary>
        MarkGod EquippedRelicGod { get; }
    }

    /// <summary>
    /// fable_68 — regras de oração nas Marcas dos Deuses (PURO, testável; sem Unity).
    ///
    /// Responsabilidades (apêndice A.5 / decisão 3.4):
    /// - 1 oração/dia por Marca (flag diária; 2ª recusa) — CA-2;
    /// - bônus diário ≤5% que expira ao virar o dia (sono) — CA-2;
    /// - não-stack com a relíquia do MESMO deus: vale o MAIOR efeito, nunca soma — CA-3;
    /// - condicionais: Nyx só à noite, Alihana só em pico, Senya só em festival/pico, Thandra exige
    ///   oferenda de 1 crop — CA-5;
    /// - Anya: SÓ lore, ZERO bônus — exceção inviolável.
    ///
    /// É o HOSPEDEIRO do buff diário (a spec define "buff diário = condição diária"). NÃO cria um
    /// segundo sistema de buff de combate: mantém o estado diário (ids simples) e expõe o agregado
    /// não-stack por <see cref="MarkEffectType"/> para os hooks pontuais existentes consumirem — mesmo
    /// idioma de <see cref="AccessoryEffectRouter.GetModifier"/>.
    /// </summary>
    public sealed class GodMarkService
    {
        private int _dayNumber;
        private readonly HashSet<string> _prayedToday = new HashSet<string>();
        private readonly List<GodMarkActiveBuffSaveData> _activeBuffs = new List<GodMarkActiveBuffSaveData>();
        private readonly List<GodMarkPendingBonusSaveData> _pending = new List<GodMarkPendingBonusSaveData>();

        /// <summary>Dia corrente conhecido pelo service.</summary>
        public int CurrentDay => _dayNumber;

        /// <summary>Buffs diários ativos (somente leitura).</summary>
        public IReadOnlyList<GodMarkActiveBuffSaveData> ActiveBuffs => _activeBuffs;

        /// <summary>Já orou nesta Marca hoje?</summary>
        public bool HasPrayedToday(string markId) => _prayedToday.Contains(markId);

        /// <summary>
        /// Tenta orar na Marca <paramref name="markId"/> dado o <paramref name="context"/> do mundo.
        /// Aplica TODAS as regras (1/dia, condição, oferenda, não-stack). Idempotente por dia: uma 2ª
        /// chamada no mesmo dia retorna <see cref="GodMarkPrayResult.AlreadyPrayedToday"/>.
        /// </summary>
        public GodMarkPrayResult TryPray(string markId, IGodMarkWorldContext context)
        {
            if (!GodMarkCatalog.TryGetById(markId, out var def))
            {
                return GodMarkPrayResult.UnknownMark;
            }

            SyncDay(context.CurrentDayNumber);

            // Anya: lore puro, nenhum bônus, mas não marca flag de "orou" (pode revisitar livremente).
            if (def.IsLoreOnly)
            {
                return GodMarkPrayResult.LoreOnly;
            }

            if (_prayedToday.Contains(def.Id))
            {
                return GodMarkPrayResult.AlreadyPrayedToday;
            }

            // Condições (CA-5).
            switch (def.Condition)
            {
                case MarkCondition.NightOnly:
                    if (!context.IsNight) return GodMarkPrayResult.ConditionNotMet;
                    break;
                case MarkCondition.AlihanaLunarPeak:
                    if (!context.IsAlihanaLunarPeak) return GodMarkPrayResult.ConditionNotMet;
                    break;
                case MarkCondition.SenyaFestivalOrPeak:
                    if (!context.IsSenyaFestivalOrPeak) return GodMarkPrayResult.ConditionNotMet;
                    break;
                case MarkCondition.CropOffering:
                    if (!context.HasCropOffering) return GodMarkPrayResult.OfferingMissing;
                    break;
            }

            // Oferenda (consome) — só Thandra. Falha de consumo = recusa (não marca flag).
            if (def.Condition == MarkCondition.CropOffering)
            {
                if (!context.TryConsumeCropOffering())
                {
                    return GodMarkPrayResult.OfferingMissing;
                }
            }

            // Concede o efeito. Thandra é um bônus PARA AMANHÃ; as demais são buffs diários imediatos.
            if (def.Effect == MarkEffectType.TomorrowSilverPercent)
            {
                _pending.Add(new GodMarkPendingBonusSaveData
                {
                    MarkId = def.Id,
                    God = (int)def.God,
                    EffectType = (int)def.Effect,
                    Magnitude = def.Magnitude,
                    ApplyOnDay = _dayNumber + 1
                });
            }
            else
            {
                _activeBuffs.Add(new GodMarkActiveBuffSaveData
                {
                    MarkId = def.Id,
                    God = (int)def.God,
                    EffectType = (int)def.Effect,
                    Magnitude = def.Magnitude
                });
            }

            _prayedToday.Add(def.Id);
            return GodMarkPrayResult.Granted;
        }

        /// <summary>
        /// Magnitude EFETIVA de um efeito de Marca ativo hoje, aplicando NÃO-STACK com a relíquia do
        /// MESMO deus (CA-3): se uma relíquia do mesmo deus está equipada, o MAIOR vale — nunca soma.
        /// Como cada efeito de Marca é único por deus, o agregado de Marca já é não-stack por tipo;
        /// o cruzamento com a relíquia é resolvido aqui pelo deus.
        /// 0 se a Marca não foi orada hoje.
        /// </summary>
        public float GetEffectiveMarkMagnitude(MarkEffectType effect, MarkGod god, float equippedRelicGodMagnitude)
        {
            var markMagnitude = 0f;
            foreach (var buff in _activeBuffs)
            {
                if (buff.EffectType == (int)effect && buff.God == (int)god)
                {
                    if (buff.Magnitude > markMagnitude) markMagnitude = buff.Magnitude;
                }
            }

            if (markMagnitude <= 0f) return 0f;

            // Não-stack por deus: se há relíquia do mesmo deus, vale o MAIOR (não a soma).
            return equippedRelicGodMagnitude > markMagnitude ? equippedRelicGodMagnitude : markMagnitude;
        }

        /// <summary>
        /// Magnitude agregada (não-stack) de um <see cref="MarkEffectType"/> entre os buffs ativos hoje.
        /// Ponto único de consulta para hooks que não precisam distinguir o deus (ex.: +5% gold find).
        /// </summary>
        public float GetActiveEffectMagnitude(MarkEffectType effect)
        {
            var max = 0f;
            foreach (var buff in _activeBuffs)
            {
                if (buff.EffectType == (int)effect && buff.Magnitude > max) max = buff.Magnitude;
            }

            return max;
        }

        /// <summary>True se a Marca <paramref name="god"/> não soma com a relíquia equipada do mesmo deus.</summary>
        public bool IsNonStackingWithRelic(MarkGod god, MarkGod equippedRelicGod)
        {
            return god != MarkGod.None && god == equippedRelicGod;
        }

        /// <summary>
        /// Avança para <paramref name="newDay"/>: expira buffs/flags do dia anterior (CA-2) e aplica
        /// bônus pendentes "de amanhã" que vencem hoje. Idempotente se o dia não mudou.
        /// </summary>
        public void OnDayStarted(int newDay)
        {
            SyncDay(newDay);
        }

        private void SyncDay(int newDay)
        {
            if (newDay < 0) newDay = 0;
            if (newDay == _dayNumber) return;

            // Vira o dia: expira tudo do dia anterior (sono).
            _prayedToday.Clear();
            _activeBuffs.Clear();

            _dayNumber = newDay;

            // Aplica pendências cujo ApplyOnDay chegou; remove vencidas/passadas.
            for (var i = _pending.Count - 1; i >= 0; i--)
            {
                var p = _pending[i];
                if (p.ApplyOnDay <= _dayNumber)
                {
                    if (p.ApplyOnDay == _dayNumber)
                    {
                        _activeBuffs.Add(new GodMarkActiveBuffSaveData
                        {
                            MarkId = p.MarkId,
                            God = p.God,
                            EffectType = p.EffectType,
                            Magnitude = p.Magnitude
                        });
                    }

                    _pending.RemoveAt(i); // vencido (igual aplicou, anterior descarta)
                }
            }
        }

        // ── Save round-trip (CA / Testing Quality Gate: DTO round-trip) ──────────────────────────

        /// <summary>Captura o estado atual num DTO simples (sem refs Unity).</summary>
        public GodMarkSaveData Capture()
        {
            var data = new GodMarkSaveData
            {
                DayNumber = _dayNumber,
                PrayedTodayMarkIds = new List<string>(_prayedToday),
                ActiveBuffs = new List<GodMarkActiveBuffSaveData>(),
                PendingTomorrowBonuses = new List<GodMarkPendingBonusSaveData>()
            };

            foreach (var b in _activeBuffs)
            {
                data.ActiveBuffs.Add(new GodMarkActiveBuffSaveData
                {
                    MarkId = b.MarkId, God = b.God, EffectType = b.EffectType, Magnitude = b.Magnitude
                });
            }

            foreach (var p in _pending)
            {
                data.PendingTomorrowBonuses.Add(new GodMarkPendingBonusSaveData
                {
                    MarkId = p.MarkId, God = p.God, EffectType = p.EffectType,
                    Magnitude = p.Magnitude, ApplyOnDay = p.ApplyOnDay
                });
            }

            return data;
        }

        /// <summary>Restaura o estado de um DTO (ausente/null ⇒ estado vazio — migração trivial).</summary>
        public void Restore(GodMarkSaveData data)
        {
            _prayedToday.Clear();
            _activeBuffs.Clear();
            _pending.Clear();
            _dayNumber = 0;

            if (data == null) return;

            _dayNumber = data.DayNumber < 0 ? 0 : data.DayNumber;

            if (data.PrayedTodayMarkIds != null)
            {
                foreach (var id in data.PrayedTodayMarkIds)
                {
                    if (!string.IsNullOrEmpty(id)) _prayedToday.Add(id);
                }
            }

            if (data.ActiveBuffs != null)
            {
                foreach (var b in data.ActiveBuffs)
                {
                    if (b == null) continue;
                    _activeBuffs.Add(new GodMarkActiveBuffSaveData
                    {
                        MarkId = b.MarkId, God = b.God, EffectType = b.EffectType, Magnitude = b.Magnitude
                    });
                }
            }

            if (data.PendingTomorrowBonuses != null)
            {
                foreach (var p in data.PendingTomorrowBonuses)
                {
                    if (p == null) continue;
                    _pending.Add(new GodMarkPendingBonusSaveData
                    {
                        MarkId = p.MarkId, God = p.God, EffectType = p.EffectType,
                        Magnitude = p.Magnitude, ApplyOnDay = p.ApplyOnDay
                    });
                }
            }
        }
    }
}
