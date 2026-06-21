using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CindarsHope.EditorTools.Build
{
    /// <summary>
    /// F61 — Pipeline de build standalone Windows para -executeMethod batchmode.
    ///
    /// Fluxo:
    ///   1. Garante o registro de cenas (delega a <see cref="BuildSceneRegistrar"/> — lista
    ///      canonica unica, sem hardcode duplicado).
    ///   2. Aplica identidade minima via PlayerSettings API (productName/companyName/version)
    ///      — NUNCA editando o YAML do ProjectSettings na mao (ADR-0008); o diff resultante e
    ///      gerado pelo Unity.
    ///   3. Executa BuildPipeline.BuildPlayer (StandaloneWindows64) para Builds/Windows/.
    ///   4. Le o BuildReport: result != Succeeded => EditorApplication.Exit(1). Loga erros/
    ///      warnings/tamanho. Sucesso => Exit(0).
    ///
    /// Identidade (EMENDA 2026-06-12-D / Decisoes v2): productName "Cindar's Hope",
    /// bundleVersion 0.1.0. companyName "Cindar's Hope" como placeholder ate confirmacao humana
    /// na Fase 0 (decisao registrada no execution report).
    /// </summary>
    public static class StandaloneBuildPipeline
    {
        // Identidade minima (Fase 0). Valores comerciais finais sao decisao humana — placeholders
        // documentados no execution report ate confirmacao.
        public const string ProductName = "Cindar's Hope";
        public const string CompanyName = "Cindar's Hope"; // PLACEHOLDER ate confirmacao humana
        public const string BundleVersion = "0.1.0";

        private const string OutputFolder = "Builds/Windows";
        private const string ExecutableName = "CindarsHope.exe";

        [MenuItem("CindarsHope/Build/Build Standalone Windows")]
        public static void BuildFromMenu()
        {
            var report = RunBuild(out var summaryMessage);
            if (report == null || report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError("[StandaloneBuildPipeline] " + summaryMessage);
            }
            else
            {
                Debug.Log("[StandaloneBuildPipeline] " + summaryMessage);
            }
        }

        /// <summary>
        /// Entry point para -executeMethod batchmode. Exit 0 em sucesso, Exit 1 em falha.
        /// </summary>
        public static void BuildBatch()
        {
            var report = RunBuild(out var summaryMessage);
            var succeeded = report != null && report.summary.result == BuildResult.Succeeded;

            if (!succeeded)
            {
                Debug.LogError("[StandaloneBuildPipeline] BUILD FAILED. " + summaryMessage);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[StandaloneBuildPipeline] BUILD SUCCEEDED. " + summaryMessage);
            EditorApplication.Exit(0);
        }

        private static BuildReport RunBuild(out string summaryMessage)
        {
            // 1) Registro de cenas (delegado — fonte unica da lista canonica).
            var registration = BuildSceneRegistrar.Register();
            if (!registration.Success)
            {
                summaryMessage = "Registro de cenas falhou: " + registration.Message;
                return null;
            }

            var scenePaths = registration.RegisteredPaths.ToArray();
            if (scenePaths.Length == 0)
            {
                summaryMessage = "Nenhuma cena registrada — nada para buildar.";
                return null;
            }

            // 2) Identidade via PlayerSettings API.
            ApplyPlayerIdentity();

            // 3) Garante a pasta de output.
            Directory.CreateDirectory(OutputFolder);
            var outputPath = Path.Combine(OutputFolder, ExecutableName);

            var options = new BuildPlayerOptions
            {
                scenes = scenePaths,
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None,
            };

            Debug.Log(
                "[StandaloneBuildPipeline] Iniciando build StandaloneWindows64 -> " + outputPath +
                "\n  cenas (" + scenePaths.Length + "): \n  " + string.Join("\n  ", scenePaths) +
                "\n  identidade: product='" + ProductName + "' company='" + CompanyName +
                "' version='" + BundleVersion + "'");

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            var sizeMb = summary.totalSize / (1024.0 * 1024.0);
            summaryMessage =
                "result=" + summary.result +
                "; errors=" + summary.totalErrors +
                "; warnings=" + summary.totalWarnings +
                "; sizeMB=" + sizeMb.ToString("F2") +
                "; output=" + outputPath +
                "; exists=" + File.Exists(outputPath);

            return report;
        }

        private static void ApplyPlayerIdentity()
        {
            if (PlayerSettings.productName != ProductName)
            {
                PlayerSettings.productName = ProductName;
            }

            if (PlayerSettings.companyName != CompanyName)
            {
                PlayerSettings.companyName = CompanyName;
            }

            if (PlayerSettings.bundleVersion != BundleVersion)
            {
                PlayerSettings.bundleVersion = BundleVersion;
            }

            // Resolucao alvo (EMENDA 2026-06-12-D): 1920x1080 fullscreen + windowed opcional.
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.resizableWindow = true;

            Debug.Log(
                "[StandaloneBuildPipeline] Identidade aplicada via PlayerSettings API: product='" +
                PlayerSettings.productName + "' company='" + PlayerSettings.companyName +
                "' version='" + PlayerSettings.bundleVersion + "' res=1920x1080 fullscreen=FullScreenWindow.");
        }
    }
}
