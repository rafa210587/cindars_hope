using System;
using System.Collections.Generic;

namespace CindarsHope.World.Altars
{
    /// <summary>
    /// fable_68 — DTO de save da seção <c>godMarks</c>. Apenas tipos simples + IDs estáveis (string)
    /// — ZERO referências Unity (rule unity-architecture §3 / save_rules). Aditivo: ausente = nunca
    /// orou (migração trivial). <see cref="DayNumber"/> amarra as flags ao dia corrente; ao virar o
    /// dia as flags/buffs do dia anterior são descartados (expiração ao dormir, CA-2).
    /// </summary>
    [Serializable]
    public class GodMarkSaveData
    {
        /// <summary>Dia a que as flags/buffs abaixo pertencem (0 = estado vazio / nunca orou).</summary>
        public int DayNumber;

        /// <summary>IDs das Marcas em que o jogador já orou HOJE (impede 2ª oração no mesmo dia).</summary>
        public List<string> PrayedTodayMarkIds = new List<string>();

        /// <summary>Buffs diários ativos concedidos por oração hoje (expiram ao virar o dia).</summary>
        public List<GodMarkActiveBuffSaveData> ActiveBuffs = new List<GodMarkActiveBuffSaveData>();

        /// <summary>
        /// Bônus "amanhã" pendentes (ex.: Thandra +2% Silver amanhã). Aplicam-se no dia
        /// <see cref="GodMarkPendingBonusSaveData.ApplyOnDay"/> e então são removidos.
        /// </summary>
        public List<GodMarkPendingBonusSaveData> PendingTomorrowBonuses = new List<GodMarkPendingBonusSaveData>();
    }

    /// <summary>Um buff diário ativo (id da Marca + tipo/efeito numérico simples).</summary>
    [Serializable]
    public class GodMarkActiveBuffSaveData
    {
        public string MarkId = string.Empty;
        public int God;        // (int)MarkGod — estável
        public int EffectType; // (int)MarkEffectType — estável
        public float Magnitude;
    }

    /// <summary>Um bônus pendente para o dia seguinte (oferenda de Thandra etc.).</summary>
    [Serializable]
    public class GodMarkPendingBonusSaveData
    {
        public string MarkId = string.Empty;
        public int God;
        public int EffectType;
        public float Magnitude;
        public int ApplyOnDay;
    }
}
