using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CindarsHope.EditorTools.Build
{
    /// <summary>
    /// F61 — Registra as cenas do jogo em <see cref="EditorBuildSettings.scenes"/> via API
    /// (NUNCA editando o YAML do .asset na mao — ADR-0008). Idempotente: re-rodar produz
    /// exatamente a mesma lista. Loga antes/depois (count + paths). Falha (Exit 1 em batchmode)
    /// se qualquer cena obrigatoria nao existir no disco.
    ///
    /// Lista canonica unica (entry primeiro):
    ///   [TitleScene SE existir] -> FarmScene (entry provisorio) -> TownScene -> CaveScene.
    /// O projeto carrega cenas por NOME (nenhum LoadScene por build index), entao a ordem nao
    /// afeta loaders existentes; o entry index 0 e relevante apenas para o boot do standalone.
    /// </summary>
    public static class BuildSceneRegistrar
    {
        private const string SceneFolder = "Assets/_Game/Scenes";

        // Cena de titulo opcional (F56/E56). F56 entregou o titulo como overlay programatico
        // (TitleScreenController), nao como .unity; portanto este path normalmente NAO existe e
        // FarmScene e o entry provisorio (debito documentado ate uma cena de titulo ser criada).
        private const string OptionalTitleScenePath = SceneFolder + "/TitleScene.unity";

        // Cenas obrigatorias do jogo, na ordem de registro (apos o titulo, se houver).
        private static readonly string[] RequiredScenePaths =
        {
            SceneFolder + "/FarmScene.unity",
            SceneFolder + "/TownScene.unity",
            SceneFolder + "/CaveScene.unity",
        };

        public static void RegisterFromMenu()
        {
            var result = Register();
            if (!result.Success)
            {
                Debug.LogError("[BuildSceneRegistrar] " + result.Message);
            }
            else
            {
                Debug.Log("[BuildSceneRegistrar] " + result.Message);
            }
        }

        /// <summary>
        /// Entry point para batchmode (-executeMethod). Exit 0 em sucesso, Exit 1 em falha.
        /// </summary>
        public static void RegisterBatch()
        {
            var result = Register();
            if (!result.Success)
            {
                Debug.LogError("[BuildSceneRegistrar] " + result.Message);
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("[BuildSceneRegistrar] " + result.Message);
            EditorApplication.Exit(0);
        }

        /// <summary>
        /// Calcula a lista canonica, valida existencia no disco e aplica via API. Idempotente.
        /// Nao chama EditorApplication.Exit (uso programatico do pipeline de build).
        /// </summary>
        public static RegistrationResult Register()
        {
            var desiredPaths = ResolveCanonicalScenePaths(out var missingRequired);

            if (missingRequired.Count > 0)
            {
                return RegistrationResult.Failure(
                    "Cenas obrigatorias ausentes no disco: " + string.Join(", ", missingRequired) +
                    ". Registro abortado (nenhuma mudanca aplicada).");
            }

            var beforePaths = EditorBuildSettings.scenes
                .Select(s => s.path)
                .ToArray();

            Debug.Log(
                "[BuildSceneRegistrar] ANTES: count=" + beforePaths.Length +
                (beforePaths.Length == 0 ? " (vazio)" : "\n  " + string.Join("\n  ", beforePaths)));

            var desiredScenes = desiredPaths
                .Select(path => new EditorBuildSettingsScene(path, true))
                .ToArray();

            var alreadyIdentical = ScenesEqual(EditorBuildSettings.scenes, desiredScenes);
            if (!alreadyIdentical)
            {
                EditorBuildSettings.scenes = desiredScenes;
            }

            var afterPaths = EditorBuildSettings.scenes
                .Select(s => s.path)
                .ToArray();

            Debug.Log(
                "[BuildSceneRegistrar] DEPOIS: count=" + afterPaths.Length +
                "\n  " + string.Join("\n  ", afterPaths));

            var entry = afterPaths.Length > 0 ? afterPaths[0] : "(nenhuma)";
            var message =
                "Cenas registradas via API: " + afterPaths.Length +
                "; entry=" + entry +
                (alreadyIdentical ? "; idempotente (nenhuma mudanca)." : "; lista atualizada.");

            return RegistrationResult.Ok(message, afterPaths);
        }

        /// <summary>
        /// Lista canonica de paths na ordem de build: titulo opcional (se existir) seguido das
        /// cenas obrigatorias. <paramref name="missingRequired"/> recebe as obrigatorias ausentes.
        /// </summary>
        public static IReadOnlyList<string> ResolveCanonicalScenePaths(out List<string> missingRequired)
        {
            missingRequired = new List<string>();
            var ordered = new List<string>();

            if (SceneExistsOnDisk(OptionalTitleScenePath))
            {
                ordered.Add(OptionalTitleScenePath);
            }

            foreach (var path in RequiredScenePaths)
            {
                if (!SceneExistsOnDisk(path))
                {
                    missingRequired.Add(path);
                    continue;
                }

                ordered.Add(path);
            }

            return ordered;
        }

        private static bool SceneExistsOnDisk(string assetRelativePath)
        {
            // assetRelativePath e relativo a raiz do projeto ("Assets/..."). File.Exists usa o
            // working dir do processo Unity (raiz do projeto), entao o path relativo resolve certo.
            return File.Exists(assetRelativePath);
        }

        private static bool ScenesEqual(EditorBuildSettingsScene[] a, EditorBuildSettingsScene[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            for (var i = 0; i < a.Length; i++)
            {
                if (!string.Equals(a[i].path, b[i].path, StringComparison.Ordinal))
                {
                    return false;
                }

                if (a[i].enabled != b[i].enabled)
                {
                    return false;
                }
            }

            return true;
        }

        public readonly struct RegistrationResult
        {
            public bool Success { get; }
            public string Message { get; }
            public IReadOnlyList<string> RegisteredPaths { get; }

            private RegistrationResult(bool success, string message, IReadOnlyList<string> paths)
            {
                Success = success;
                Message = message;
                RegisteredPaths = paths ?? Array.Empty<string>();
            }

            public static RegistrationResult Ok(string message, IReadOnlyList<string> paths)
                => new RegistrationResult(true, message, paths);

            public static RegistrationResult Failure(string message)
                => new RegistrationResult(false, message, Array.Empty<string>());
        }
    }
}
