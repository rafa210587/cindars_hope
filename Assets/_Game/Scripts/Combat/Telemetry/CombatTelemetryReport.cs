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

        public const int CurrentShapeVersion = 2;

        /// <summary>Seed da run da caverna (contexto, nÃ£o estado).</summary>
        public string RunId = string.Empty;

        /// <summary>Identificador da build de skills em execucao; vazio quando nao identificado.</summary>
        public string BuildId = string.Empty;

        /// <summary>NÃ­vel do jogador no inÃ­cio da coleta.</summary>
        public int PlayerLevel = -1;

        /// <summary>Variante da build de skills; vazio quando nao identificada.</summary>
        public string RunVariant = string.Empty;

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
        public float ControlWindowUptimeSeconds;
        /// <summary>-1 quando nenhuma janela de controle forte foi observada.</summary>
        public float ControlWindowUptimePercent;
        public int ControlWindowSampleCount;

        /// <summary>Eventos de aÃ§Ã£o do player capturados com outcome conhecido.</summary>
        public List<CombatTelemetryActionEventEntry> ActionEvents = new List<CombatTelemetryActionEventEntry>();

        /// <summary>Hits agregados por tipo enemyId; nao identifica instancias de alvo.</summary>
        public List<CombatTelemetryTargetHitEntry> TargetHits = new List<CombatTelemetryTargetHitEntry>();

        /// <summary>Amostras brutas de TTK por kill (sem agregação).</summary>
        public List<CombatTelemetryRawTtkSampleEntry> RawTtkSamples = new List<CombatTelemetryRawTtkSampleEntry>();

        /// <summary>Janelas de controle explicitamente registradas; sem fonte runtime validada.</summary>
        public List<CombatTelemetryControlWindowEntry> ControlWindows = new List<CombatTelemetryControlWindowEntry>();

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

    /// <summary>fable_59 â€” resoluÃ§Ã£o de aÃ§Ã£o por evento de gameplay.</summary>
    [Serializable]
    public sealed class CombatTelemetryActionEventEntry
    {
        public string ActionId = string.Empty;
        public int Rank;
        public string VariantId = string.Empty;
        public string Outcome = string.Empty;
        public string FailureKey = string.Empty;
    }

    /// <summary>fable_59 â€” contagem de hits por enemyId.</summary>
    [Serializable]
    public sealed class CombatTelemetryTargetHitEntry
    {
        public string EnemyId = string.Empty;
        public int HitCount;
    }

    /// <summary>fable_59 â€” amostra bruta de TTK (um kill).</summary>
    [Serializable]
    public sealed class CombatTelemetryRawTtkSampleEntry
    {
        public string EnemyId = string.Empty;
        public string CreatureClass = string.Empty;
        public float TtkSeconds;
    }

    /// <summary>fable_59 â€” janela de controle observada (inÃ­cio/fim em segundos do trecho).</summary>
    [Serializable]
    public sealed class CombatTelemetryControlWindowEntry
    {
        public string EnemyId = string.Empty;
        public float StartSeconds;
        public float EndSeconds;
        public float DurationSeconds;
    }
}
