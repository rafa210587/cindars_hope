using System;

namespace CindarsHope.Combat.Telemetry
{
    /// <summary>
    /// fable_59 — classe canônica de criatura para fins de ALVO de balance (BALANCE_CURVES §6/§7).
    /// Mapeia o papel/banda do bestiário (F33) para as quatro categorias com alvos definidos:
    /// Common (comum), Elite, Miniboss, Boss. Não é o EnemyRole completo — é só a granularidade
    /// que §6-§7 fixam alvos.
    /// </summary>
    public enum TelemetryCreatureClass
    {
        Unknown = 0,
        Common = 1,
        Elite = 2,
        Miniboss = 3,
        Boss = 4,
    }

    /// <summary>
    /// fable_59 — resultado da comparação de uma métrica contra a faixa-alvo do balance.
    /// </summary>
    public enum TelemetryEvaluation
    {
        /// <summary>Sem alvo aplicável (classe desconhecida ou métrica sem amostra).</summary>
        NoTarget = 0,
        WithinTarget = 1,
        Below = 2,
        Above = 3,
    }

    /// <summary>
    /// fable_59 — interface fina para resolver enemyId → classe canônica de criatura.
    /// Implementada sobre o registry do bestiário F33 (BestiaryTelemetryClassLookup); injetável
    /// como mock nos testes (CA-3) para não acoplar o avaliador puro ao catálogo.
    /// </summary>
    public interface ICreatureClassLookup
    {
        TelemetryCreatureClass GetClass(string enemyId);
    }

    /// <summary>
    /// fable_59 — avaliador PURO (sem Unity) que compara TTK e dano recebido contra os ALVOS
    /// citados de BALANCE_CURVES §6-§7. Nenhum número é inventado aqui: as bordas são exatamente
    /// as da direction.
    ///
    /// §6 TTK (segundos):  Common 3-6 | Elite 12-20 | Miniboss 45-90 | Boss 120-240.
    /// §7 dano recebido por hit (% do HP máx): Common 4-8 | Elite 10-15 | Boss telegrafado 18-30.
    ///   (Miniboss não tem faixa §7 própria na direction → herda a de Boss telegrafado, marcado
    ///    como herança no relatório; sem inventar número.)
    ///
    /// Bordas inclusivas: o valor IGUAL ao limite conta como WITHIN_TARGET (3.0s e 6.0s = WITHIN
    /// para Common). Abaixo do mínimo = BELOW; acima do máximo = ABOVE.
    /// </summary>
    public static class TelemetryTargetEvaluator
    {
        // ── Alvos §6 TTK (segundos) ──────────────────────────────────────────────────────────────
        public const float TtkCommonMin = 3f;
        public const float TtkCommonMax = 6f;
        public const float TtkEliteMin = 12f;
        public const float TtkEliteMax = 20f;
        public const float TtkMinibossMin = 45f;
        public const float TtkMinibossMax = 90f;
        public const float TtkBossMin = 120f;
        public const float TtkBossMax = 240f;

        // ── Alvos §7 dano recebido por hit (% do HP máx, 0..100) ─────────────────────────────────
        public const float DamagePctCommonMin = 4f;
        public const float DamagePctCommonMax = 8f;
        public const float DamagePctEliteMin = 10f;
        public const float DamagePctEliteMax = 15f;
        public const float DamagePctBossMin = 18f;
        public const float DamagePctBossMax = 30f;

        /// <summary>
        /// Retorna a faixa de TTK (segundos) para a classe; (0,0) quando não há alvo.
        /// </summary>
        public static bool TryGetTtkTarget(TelemetryCreatureClass creatureClass, out float min, out float max)
        {
            switch (creatureClass)
            {
                case TelemetryCreatureClass.Common: min = TtkCommonMin; max = TtkCommonMax; return true;
                case TelemetryCreatureClass.Elite: min = TtkEliteMin; max = TtkEliteMax; return true;
                case TelemetryCreatureClass.Miniboss: min = TtkMinibossMin; max = TtkMinibossMax; return true;
                case TelemetryCreatureClass.Boss: min = TtkBossMin; max = TtkBossMax; return true;
                default: min = 0f; max = 0f; return false;
            }
        }

        /// <summary>
        /// Retorna a faixa de dano recebido por hit (% do HP máx) para a classe; (0,0) sem alvo.
        /// Miniboss herda a faixa de Boss telegrafado (a direction §7 não dá faixa própria).
        /// </summary>
        public static bool TryGetDamageTakenPctTarget(TelemetryCreatureClass creatureClass, out float min, out float max)
        {
            switch (creatureClass)
            {
                case TelemetryCreatureClass.Common: min = DamagePctCommonMin; max = DamagePctCommonMax; return true;
                case TelemetryCreatureClass.Elite: min = DamagePctEliteMin; max = DamagePctEliteMax; return true;
                // Miniboss herda Boss (sem faixa §7 própria) — registrado no report como herança.
                case TelemetryCreatureClass.Miniboss: min = DamagePctBossMin; max = DamagePctBossMax; return true;
                case TelemetryCreatureClass.Boss: min = DamagePctBossMin; max = DamagePctBossMax; return true;
                default: min = 0f; max = 0f; return false;
            }
        }

        /// <summary>Compara um valor contra uma faixa inclusiva [min,max].</summary>
        public static TelemetryEvaluation EvaluateRange(float value, float min, float max)
        {
            if (value < min) return TelemetryEvaluation.Below;
            if (value > max) return TelemetryEvaluation.Above;
            return TelemetryEvaluation.WithinTarget;
        }

        /// <summary>
        /// Avalia o TTK médio (segundos) de uma criatura contra §6 pela sua classe.
        /// n &lt;= 0 (sem amostra) → NoTarget.
        /// </summary>
        public static TelemetryEvaluation EvaluateTtk(TelemetryCreatureClass creatureClass, float ttkSeconds, int sampleCount)
        {
            if (sampleCount <= 0) return TelemetryEvaluation.NoTarget;
            if (!TryGetTtkTarget(creatureClass, out var min, out var max)) return TelemetryEvaluation.NoTarget;
            return EvaluateRange(ttkSeconds, min, max);
        }

        /// <summary>
        /// Avalia o dano médio recebido por hit (% do HP máx) contra §7 pela classe da fonte.
        /// hitCount &lt;= 0 (sem hit dessa fonte) → NoTarget.
        /// </summary>
        public static TelemetryEvaluation EvaluateDamageTakenPct(TelemetryCreatureClass creatureClass, float pctOfMaxHp, int hitCount)
        {
            if (hitCount <= 0) return TelemetryEvaluation.NoTarget;
            if (!TryGetDamageTakenPctTarget(creatureClass, out var min, out var max)) return TelemetryEvaluation.NoTarget;
            return EvaluateRange(pctOfMaxHp, min, max);
        }

        /// <summary>String estável para o JSON ("WITHIN_TARGET"/"BELOW"/"ABOVE"/"NO_TARGET").</summary>
        public static string ToToken(TelemetryEvaluation evaluation)
        {
            switch (evaluation)
            {
                case TelemetryEvaluation.WithinTarget: return "WITHIN_TARGET";
                case TelemetryEvaluation.Below: return "BELOW";
                case TelemetryEvaluation.Above: return "ABOVE";
                default: return "NO_TARGET";
            }
        }

        /// <summary>String estável para a classe no JSON.</summary>
        public static string ClassToToken(TelemetryCreatureClass creatureClass)
        {
            return creatureClass.ToString();
        }
    }
}
