using System;
using System.Collections.Generic;
using CindarsHope.Foundation;

namespace CindarsHope.Combat.Telemetry
{
    /// <summary>
    /// fable_59 â€” DTO serializÃ¡vel do relatÃ³rio de telemetria de combate (JSON local, fora do save).
    /// Apenas tipos simples + listas de DTOs simples (regra de save DTO aplicada por precauÃ§Ã£o, ainda
    /// que isto NUNCA entre no GameSaveData). <see cref="ShapeVersion"/> versiona o shape (CA-4).
    /// [Serializable] para JsonUtility; nenhum campo Ã© UnityEngine.Object.
    /// </summary>
    [Serializable]
    public sealed class CombatTelemetryReport
    {
        /// <summary>VersÃ£o do shape do relatÃ³rio (CA-4: versÃ£o no payload). Incrementar ao mudar campos.</summary>
        public int ShapeVersion = CurrentShapeVersion;

        public const int CurrentShapeVersion = 1;

        /// <summary>Seed da run da caverna (contexto, nÃ£o estado).</summary>
        public string RunId = string.Empty;

        /// <summary>NÃ­vel da caverna a que este relatÃ³rio se refere (-1 = sessÃ£o sem nÃ­vel conhecido).</summary>
        public int CaveLevel = -1;

        /// <summary>Banda temÃ¡tica do nÃ­vel (1..7) quando conhecida; 0 = desconhecida.</summary>
        public int Band;

        /// <summary>MarcaÃ§Ã£o ISO-8601 (UTC) de quando o relatÃ³rio foi gerado.</summary>
        public string GeneratedAtUtc = string.Empty;

        /// <summary>DuraÃ§Ã£o do trecho coletado (entrada no nÃ­vel â†’ flush), em segundos.</summary>
        public float DurationSeconds;

        // â”€â”€ Agregados de combate â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public List<CombatTelemetryKillEntry> Kills = new List<CombatTelemetryKillEntry>();

        public int DamageDealtTotal;
        public int DamageTakenTotal;

        /// <summary>MÃ©dia do dano recebido por hit em % do HP mÃ¡x (todos os hits do trecho).</summary>
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

        /// <summary>MÃ©tricas Â§53 sem evento disponÃ­vel neste momento (follow-up enriquece eventos).</summary>
        public List<string> Gaps = new List<string>();
    }

    /// <summary>fable_59 â€” TTK agregado por enemyId, com a avaliaÃ§Ã£o contra Â§6.</summary>
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

    /// <summary>fable_59 â€” dano recebido agregado por fonte (sourceId), com % mÃ©dio e avaliaÃ§Ã£o Â§7.</summary>
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

    /// <summary>fable_59 â€” dano DADO agregado por tipo de dano (quando o evento carrega o tipo).</summary>
    [Serializable]
    public sealed class CombatTelemetryDamageByTypeEntry
    {
        public string DamageType = string.Empty;
        public int DamageTotal;
        public int HitCount;
    }
}
