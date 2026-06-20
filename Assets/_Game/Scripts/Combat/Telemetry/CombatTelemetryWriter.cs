using System;
using System.IO;
using UnityEngine;

namespace CindarsHope.Combat.Telemetry
{
    /// <summary>
    /// fable_59 — escreve o relatório de telemetria como JSON local em
    /// <c>Application.persistentDataPath/telemetry/</c>. NUNCA no GameSaveData. IO tolerante a
    /// falha: qualquer exceção de disco/permissão é logada como aviso e DESCARTADA (a run nunca
    /// quebra por causa da telemetria). A serialização pura (<see cref="Serialize"/>) e a montagem
    /// do nome (<see cref="BuildFileName"/>) são separadas do disco para serem testáveis em EditMode.
    /// </summary>
    public static class CombatTelemetryWriter
    {
        public const string TelemetryFolderName = "telemetry";

        /// <summary>Serializa o relatório em JSON estável (JsonUtility, pretty-print).</summary>
        public static string Serialize(CombatTelemetryReport report)
        {
            if (report == null)
            {
                return "{}";
            }

            return JsonUtility.ToJson(report, true);
        }

        /// <summary>
        /// Nome de arquivo determinístico: combat_&lt;timestamp&gt;_&lt;runId&gt;_level&lt;N&gt;.json.
        /// runId é saneado para caracteres de path seguros.
        /// </summary>
        public static string BuildFileName(string runId, int caveLevel, string timestampToken)
        {
            var safeRun = SanitizeToken(string.IsNullOrEmpty(runId) ? "norun" : runId);
            var safeTs = SanitizeToken(string.IsNullOrEmpty(timestampToken) ? "nots" : timestampToken);
            var levelToken = caveLevel >= 0 ? caveLevel.ToString() : "session";
            return $"combat_{safeTs}_{safeRun}_level{levelToken}.json";
        }

        /// <summary>Token de timestamp (UTC) seguro para nome de arquivo.</summary>
        public static string BuildTimestampToken(DateTime utcNow)
        {
            return utcNow.ToString("yyyyMMdd_HHmmss");
        }

        /// <summary>
        /// Grava o relatório no diretório de telemetria. Retorna o caminho escrito, ou string vazia
        /// em caso de falha (sempre logada, nunca propagada). Cria a pasta se necessário.
        /// </summary>
        public static string Write(CombatTelemetryReport report, string telemetryDirectory)
        {
            if (report == null)
            {
                return string.Empty;
            }

            try
            {
                if (!Directory.Exists(telemetryDirectory))
                {
                    Directory.CreateDirectory(telemetryDirectory);
                }

                var fileName = BuildFileName(report.RunId, report.CaveLevel, BuildTimestampToken(DateTime.UtcNow));
                var fullPath = Path.Combine(telemetryDirectory, fileName);
                var json = Serialize(report);
                File.WriteAllText(fullPath, json);
                Debug.Log($"[CombatTelemetryWriter] Relatorio gravado: {fullPath}");
                return fullPath;
            }
            catch (Exception ex)
            {
                // IO tolerante: nunca quebrar a run por telemetria.
                Debug.LogWarning($"[CombatTelemetryWriter] Falha ao gravar relatorio de telemetria (descartado): {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>Caminho canônico do diretório de telemetria (fora do save).</summary>
        public static string DefaultDirectory()
        {
            return Path.Combine(Application.persistentDataPath, TelemetryFolderName);
        }

        private static string SanitizeToken(string token)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var chars = token.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                var c = chars[i];
                if (Array.IndexOf(invalid, c) >= 0 || c == ' ')
                {
                    chars[i] = '_';
                }
            }

            return new string(chars);
        }
    }
}
