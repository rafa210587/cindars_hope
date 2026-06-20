using System;
using System.Collections.Generic;

namespace CindarsHope.Combat.Telemetry
{
    /// <summary>
    /// fable_59 — DTO serializável do relatório de telemetria de combate (JSON local, fora do save).
    /// Apenas tipos simples + listas de DTOs simples (regra de save DTO aplicada por precaução, ainda
    /// que isto NUNCA entre no GameSaveData). <see cref="ShapeVersion"/> versiona o shape (CA-4).
    /// [Serializable] para JsonUtility; nenhum campo é UnityEngine.Object.
    /// </summary>
    [Serializable]
    public sealed class CombatTelemetryReport
    {
        /// <summary>Versão do shape do relatório (CA-4: versão no payload). Incrementar ao mudar campos.</summary>
        public int ShapeVersion = CurrentShapeVersion;

        public const int CurrentShapeVersion = 1;

        /// <summary>Seed da run da caverna (contexto, não estado).</summary>
        public string RunId = string.Empty;

        /// <summary>Nível da caverna a que este relatório se refere (-1 = sessão sem nível conhecido).</summary>
        public int CaveLevel = -1;

        /// <summary>Banda temática do nível (1..7) quando conhecida; 0 = desconhecida.</summary>
        public int Band;

        /// <summary>Marcação ISO-8601 (UTC) de quando o relatório foi gerado.</summary>
        public string GeneratedAtUtc = string.Empty;

        /// <summary>Duração do trecho coletado (entrada no nível → flush), em segundos.</summary>
        public float DurationSeconds;

        // ── Agregados de combate ─────────────────────────────────────────────────────────────────
        public List<CombatTelemetryKillEntry> Kills = new List<CombatTelemetryKillEntry>();

        public int DamageDealtTotal;
        public int DamageTakenTotal;

        /// <summary>Média do dano recebido por hit em % do HP máx (todos os hits do trecho).</summary>
        public float DamageTakenPctAvg;

        public List<CombatTelemetryDamageBySourceEntry> DamageTakenBySource =
            new List<CombatTelemetryDamageBySourceEntry>();

        public List<CombatTelemetryDamageByTypeEntry> DamageDealtByType =
            new List<CombatTelemetryDamageByTypeEntry>();

        public int StaminaSpent;
        public int MpSpent;
        public int Dodges;
        public int Blocks;
        public int PerfectBlocks;
        public int PostureBreaks;
        public int ChargedAttacks;
        public int Deaths;

        /// <summary>Métricas §53 sem evento disponível neste momento (follow-up enriquece eventos).</summary>
        public List<string> Gaps = new List<string>();
    }

    /// <summary>fable_59 — TTK agregado por enemyId, com a avaliação contra §6.</summary>
    [Serializable]
    public sealed class CombatTelemetryKillEntry
    {
        public string EnemyId = string.Empty;
        public string CreatureClass = string.Empty; // TelemetryCreatureClass token
        public int KillCount;
        public float TtkAvgSeconds;
        public float TtkMinSeconds;
        public float TtkMaxSeconds;
        public string TtkEvaluation = "NO_TARGET"; // WITHIN_TARGET / BELOW / ABOVE / NO_TARGET
    }

    /// <summary>fable_59 — dano recebido agregado por fonte (sourceId), com % médio e avaliação §7.</summary>
    [Serializable]
    public sealed class CombatTelemetryDamageBySourceEntry
    {
        public string SourceId = string.Empty;
        public string CreatureClass = string.Empty;
        public int HitCount;
        public int DamageTotal;
        public float DamagePctAvg;
        public string DamagePctEvaluation = "NO_TARGET";
    }

    /// <summary>fable_59 — dano DADO agregado por tipo de dano (quando o evento carrega o tipo).</summary>
    [Serializable]
    public sealed class CombatTelemetryDamageByTypeEntry
    {
        public string DamageType = string.Empty;
        public int DamageTotal;
        public int HitCount;
    }
}
