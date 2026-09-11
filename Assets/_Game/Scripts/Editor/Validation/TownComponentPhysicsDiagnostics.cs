using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using CindarsHope.Editor.SceneCreation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CindarsHope.Editor.Validation
{
    /// <summary>Read-only saved Town diagnosis. Exports fresh JSON; never applies, repairs or saves a scene.</summary>
    public static class TownComponentPhysicsDiagnostics
    {
        private const string Tag = "[TownComponentDiagnostics] ";
        private const string OutputRoot = "art/town-keyart-rework/components/evidence";
        [Serializable]
        public sealed class InputHash { public string path, sha256; }
        [Serializable]
        public sealed class Diagnostic
        {
            public string utc, scenePath, sceneSha256Before, sceneSha256After, compiledAssemblyPath, compiledAssemblySha256;
            public string operation = "READ_ONLY_BUILD_PLAN_AND_AUDIT";
            public string materialization = "NOT RUN";
            public string unityMovementObservation = "NOT RUN";
            public string playerPresentationStatus = "EXPERIMENTAL_INPUT_ONLY; animation/visual acceptance NOT RUN";
            public string c5Global = "PENDING";
            public int validationErrors, validationWarnings;
            public List<InputHash> sourceInputs = new List<InputHash>();
            public List<string> errors = new List<string>();
            public TownKeyartInteriorPresentation.Plan proposedInterior;
            public TownKeyartComponentPhysics.Report materializedPhysics;
        }

        [MenuItem("CindarsHope/Validation/Export Town Component Diagnostics")]
        public static void ExportSavedTown()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException(Tag + "Saved-scene diagnostics require Edit Mode.");
            string scenePath = TownKeyartComponentPreflight.TownScenePath;
            if (!File.Exists(scenePath)) throw new FileNotFoundException("Canonical Town scene does not exist.",scenePath);
            var scene = SceneManager.GetSceneByPath(scenePath);
            var previousActiveScene = SceneManager.GetActiveScene();
            bool openedHere = !scene.isLoaded;
            if (!openedHere && scene.isDirty)
                throw new InvalidOperationException(Tag + "Loaded Town is dirty. Diagnostics will not save or discard the owner's state.");
            var result = new Diagnostic
            {
                utc = DateTime.UtcNow.ToString("O"), scenePath = scenePath, sceneSha256Before = FileHash(scenePath),
                compiledAssemblyPath = typeof(TownKeyartComponentPhysics).Assembly.Location
            };
            result.compiledAssemblySha256 = FileHash(result.compiledAssemblyPath);
            if (result.compiledAssemblySha256 == "MISSING") result.errors.Add("Compiled helper assembly could not be fingerprinted.");
            foreach (string name in new[] { "TownKeyartComponentSupportCatalog","TownKeyartComponentPreflight",
                "TownKeyartComponentPhysics","TownKeyartInteriorPresentation" })
            {
                string path = "Assets/_Game/Scripts/Editor/SceneCreation/City/" + name + ".cs";
                result.sourceInputs.Add(new InputHash { path = path,sha256 = FileHash(path) });
            }
            string diagnosticPath = "Assets/_Game/Scripts/Editor/Validation/TownComponentPhysicsDiagnostics.cs";
            result.sourceInputs.Add(new InputHash { path = diagnosticPath,sha256 = FileHash(diagnosticPath) });
            foreach (string path in new[] { "ProjectSettings/Physics2DSettings.asset", "ProjectSettings/TagManager.asset",
                "Assets/_Game/Scripts/Player/PlayerController.cs", "Assets/_Game/Scripts/Player/PlayerWalkAnimator.cs" })
                result.sourceInputs.Add(new InputHash { path = path,sha256 = FileHash(path) });
            foreach (var input in result.sourceInputs)
                if (input.sha256 == "MISSING") result.errors.Add("Input provenance missing: " + input.path);
            try
            {
                try
                {
                    if (openedHere) scene = EditorSceneManager.OpenScene(scenePath,OpenSceneMode.Additive);
                    if (scene.isDirty) result.errors.Add("Town became dirty while loading; no planning or audit was run against ambiguous saved state.");
                    else
                    {
                        try { result.proposedInterior = TownKeyartInteriorPresentation.BuildPlan(scene); }
                        catch (Exception exception) { result.errors.Add("BuildPlan: " + exception); }
                        try { result.materializedPhysics = TownKeyartComponentPhysics.Audit(scene); }
                        catch (Exception exception) { result.errors.Add("Audit: " + exception); }
                    }
                }
                catch (Exception exception) { result.errors.Add("OpenTown: " + exception); }
                if (scene.IsValid() && scene.isLoaded && scene.isDirty) result.errors.Add("Town is dirty after the read-only diagnosis; no scene was saved.");
                result.sceneSha256After = FileHash(scenePath);
                if (result.sceneSha256After != result.sceneSha256Before) result.errors.Add("Saved Town bytes changed during diagnosis; inputs are not stable.");
                if (result.materializedPhysics != null) result.c5Global = result.materializedPhysics.c5Global;
                int conflicts = result.proposedInterior == null ? 0 : result.proposedInterior.conflicts.Count;
                int pending = result.materializedPhysics == null ? 0 : result.materializedPhysics.pending;
                result.validationErrors = result.errors.Count + conflicts + (result.materializedPhysics == null ? 0 : result.materializedPhysics.measuredFail);
                result.validationWarnings = pending + (result.proposedInterior == null ? 0 : result.proposedInterior.pending.Count);
                if (conflicts > 0) result.c5Global = "FAIL";
                if (result.errors.Count > 0) result.c5Global = "FAIL_DIAGNOSTIC";
                string output = Path.Combine(OutputRoot,"diagnostic-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + "-" + Guid.NewGuid().ToString("N").Substring(0,8));
                if (Directory.Exists(output)) throw new IOException("Diagnostic output must be new: " + output);
                Directory.CreateDirectory(output);
                string reportPath = Path.Combine(output,"town-component-diagnostics.json");
                File.WriteAllText(reportPath,JsonUtility.ToJson(result,true));
                Debug.Log(Tag + "Exported " + Path.GetFullPath(reportPath) + "; Errors=" + result.validationErrors +
                    "; Warnings=" + result.validationWarnings + "; DiagnosticErrors=" + result.errors.Count +
                    "; PlannedConflicts=" + conflicts + "; PendingPhysicsEntries=" + pending +
                    "; C5=" + result.c5Global + "; Apply/SaveScene/movement NOT RUN.");
                if (result.validationWarnings > 0) Debug.LogWarning(Tag + "Unresolved families/presentation inputs remain PENDING; inspect the exported entries.");
                if (result.validationErrors > 0) Debug.LogError(Tag + "DIAGNOSTIC/COMPONENT GATES FAILED; inspect the exported errors, conflicts and measured failures.");
            }
            finally
            {
                // Only the additive copy opened here is closed; pre-existing loaded scenes remain untouched.
                if (openedHere && scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene,true);
                if (previousActiveScene.IsValid() && previousActiveScene.isLoaded)
                    SceneManager.SetActiveScene(previousActiveScene);
            }
        }

        private static string FileHash(string path)
        {
            if (!File.Exists(path)) return "MISSING";
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }
    }
}
