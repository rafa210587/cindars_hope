using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CindarsHope.Cave;
using CindarsHope.Cave.Runtime;
using CindarsHope.Combat;
using CindarsHope.Combat.Telemetry;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using NUnit.Framework;
using CindarsHope.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    public sealed class Phase21CombatAcceptancePlayModeTests
    {
        private const string RuntimeClassSource = "EnemyDataSO+SpawnPlan";

        [System.Serializable]
        private sealed class ProbeTargetRow
        {
            public string scenarioId;
            public int level;
            public string seed;
            public string enemyId;
            public string creatureClass;
            public string runtimeClass;
            public string classSource;
            public string telemetryClass;
            public string legacyBestiaryClass;
            public string manifestAvailability;
            public string outcome;
            public int planMatchCount;
            public string planInstanceIds;
            public int activeMatchCount;
            public string activeInstanceIds;
            public string planEliteAffixes;
            public bool targetPresent;
            public string note;
        }

        [System.Serializable]
        private sealed class ProbeNaturalEnemyDiagnosticRow
        {
            public string enemyId;
            public string enemyInstanceId;
            public string runtimeClass;
            public string classSource;
            public string legacyBestiaryClass;
            public string eliteAffix;
            public bool planIsElite;
            public string outcome;
            public string note;
        }

        [System.Serializable]
        private sealed class ProbeScenarioOutput
        {
            public string scenarioId;
            public int level;
            public string seed;
            public string caveScene;
            public string status;
            public string naturalEnemyIds;
            public List<ProbeTargetRow> targets = new();
            public List<ProbeNaturalEnemyDiagnosticRow> naturalEnemyDiagnostics = new();
        }

        [System.Serializable]
        private sealed class ProbeReport
        {
            public string probeName = "Phase21CombatAcceptanceTargetAvailabilityProbe";
            public string capturedAtUtc = DateTime.UtcNow.ToString("o");
            public int scenarioCount;
            public List<ProbeScenarioOutput> scenarios = new();
            public string jsonFilePath;
            public string csvFilePath;
            public string note = "Disponibilidade de alvos; sem validação AC02/TTK/combate.";
        }

        [UnityTest]
        public IEnumerator AC21_TargetAvailabilityProbe_L20L50L54_UsesNaturalSpawnPlanAndActiveInstances()
        {
            var report = new ProbeReport();
            var materializedScenarios = 0;
            var legacyClassLookup = new BestiaryTelemetryClassLookup();
            var combatDatabases = Resources.Load<CombatRuntimeDatabasesRegistrySO>("CombatRuntimeDatabasesRegistry");

            if (GameBootstrap.Instance == null)
                LogAssert.Expect(LogType.Error,
                    new Regex("^\\[DeathSystemBootstrap\\] GameBootstrap/PlayerManager/CorpseRecoveryManager nao ficaram prontos"));

            foreach (var scenario in Phase21BuildManifest.Scenarios)
            {
                var scenarioOutput = new ProbeScenarioOutput
                {
                    scenarioId = scenario.ScenarioId,
                    level = scenario.Level,
                    seed = scenario.Seed,
                    caveScene = "CaveScene",
                    status = "PASS"
                };

                yield return StartToFarmScene();

                var entrance = FindInActiveScene<CaveEntranceInteractable>();
                if (entrance == null)
                {
                    AddNotRunRow(scenarioOutput, "CaveEntranceInteractable ausente em FarmScene", string.Empty);
                    report.scenarios.Add(scenarioOutput);
                    continue;
                }

                var player = PlayerController.ActiveInstance;
                if (player == null)
                {
                    AddNotRunRow(scenarioOutput, "PlayerController.ActiveInstance nulo antes da entrada", string.Empty);
                    report.scenarios.Add(scenarioOutput);
                    continue;
                }

                entrance.Interact(player.gameObject);
                yield return WaitForScene("CaveScene");

                if (SceneManager.GetActiveScene().name != "CaveScene")
                {
                    AddNotRunRow(scenarioOutput, "Transição para CaveScene não ocorreu", string.Empty);
                    report.scenarios.Add(scenarioOutput);
                    continue;
                }

                var levelController = FindInActiveScene<CaveLevelRuntimeController>();
                var runManager = CaveRunManager.Instance;
                if (levelController == null || runManager == null)
                {
                    AddNotRunRow(scenarioOutput,
                        "CaveLevelRuntimeController ou CaveRunManager indisponível", string.Empty);
                    report.scenarios.Add(scenarioOutput);
                    yield return LoadSceneAsync("FarmScene");
                    continue;
                }

                runManager.RestoreRuntimeState(new CaveRuntimeState
                {
                    HasActiveRun = true,
                    CurrentCaveLevel = scenario.Level,
                    DeepestLayerReached = scenario.Level,
                    CaveWorldSeed = "world_phase21_probe",
                    CaveRunSeed = scenario.Seed,
                    CaveRunId = $"probe_{scenario.ScenarioId}"
                });

                levelController.GenerateCurrentLevel();
                yield return WaitForMaterializedSpawn(levelController);

                var materializer = levelController.Materializer;
                var plan = materializer?.LastEnemySpawnPlan;
                var hasMaterialization = levelController.CurrentGeneratedLevel != null
                    && materializer != null
                    && materializer.LastMaterializationResult != null
                    && materializer.LastMaterializationResult.CreatedFloorTiles > 0
                    && plan != null
                    && plan.Entries != null
                    && plan.Entries.Count > 0
                    && plan.CaveLevel == scenario.Level
                    && string.Equals(plan.CaveRunSeed, scenario.Seed, StringComparison.Ordinal);

                if (!hasMaterialization)
                {
                    AddNotRunRow(scenarioOutput,
                        "CaveRuntimeMaterializer não materializou spawn plan com floor/nível válidos", string.Empty);
                    report.scenarios.Add(scenarioOutput);
                    yield return LoadSceneAsync("FarmScene");
                    continue;
                }

                var planEntries = plan.Entries;
                materializedScenarios++;
                scenarioOutput.naturalEnemyIds = string.Join(",",
                    planEntries.Select(entry => entry.EnemyId).Distinct().OrderBy(id => id));
                scenarioOutput.naturalEnemyDiagnostics = BuildNaturalEnemyDiagnostics(
                    planEntries,
                    combatDatabases,
                    legacyClassLookup);
                var activeHealths = EnemyHealth.ActiveInstances
                    .Where(health => health != null && health.gameObject != null && health.gameObject.scene.isLoaded)
                    .ToList();

                foreach (var target in scenario.Targets)
                {
                    var row = new ProbeTargetRow
                    {
                        scenarioId = scenario.ScenarioId,
                        level = scenario.Level,
                        seed = scenario.Seed,
                        enemyId = target.EnemyId ?? string.Empty,
                        creatureClass = target.CreatureClass.ToString(),
                        manifestAvailability = target.Availability.ToString(),
                        classSource = RuntimeClassSource,
                        telemetryClass = legacyClassLookup.GetClass(target.EnemyId).ToString(),
                        legacyBestiaryClass = legacyClassLookup.GetClass(target.EnemyId).ToString()
                    };

                    if (target.Availability == Phase21TargetAvailability.CatalogOnly)
                    {
                        row.outcome = "NOT RUN";
                        row.planMatchCount = 0;
                        row.activeMatchCount = 0;
                        row.note = target.UnavailableReason;
                        scenarioOutput.status = "NOT RUN";
                        scenarioOutput.targets.Add(row);
                        continue;
                    }

                    if (target.Availability == Phase21TargetAvailability.Unavailable)
                    {
                        row.outcome = "NOT RUN";
                        row.planMatchCount = 0;
                        row.activeMatchCount = 0;
                        row.note = target.UnavailableReason;
                        scenarioOutput.status = "NOT RUN";
                        scenarioOutput.targets.Add(row);
                        continue;
                    }

                    var planMatches = planEntries
                        .Where(entry => string.Equals(entry.EnemyId, target.EnemyId, StringComparison.Ordinal))
                        .ToList();

                    var planInstanceIds = planMatches
                        .Select(entry => entry.EnemyInstanceId)
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Distinct()
                        .ToList();

                    row.planMatchCount = planMatches.Count;
                    row.planInstanceIds = string.Join(",", planInstanceIds);

                    var activeMatches = activeHealths
                        .Where(health => string.Equals(health.EnemyId, target.EnemyId, StringComparison.Ordinal))
                        .ToList();

                    var activeInstanceIds = activeMatches
                        .Select(health => health.EnemyInstanceId)
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Distinct()
                        .ToList();

                    var matchedInstanceIds = activeInstanceIds
                        .Where(id => planInstanceIds.Contains(id))
                        .ToList();

                    row.activeMatchCount = matchedInstanceIds.Count;
                    row.activeInstanceIds = string.Join(",", matchedInstanceIds);
                    row.planEliteAffixes = string.Join(",",
                        planMatches
                            .Select(entry => entry.EliteAffix)
                            .Where(affix => affix != Enemy.EliteAffix.None)
                            .Distinct()
                            .Select(affix => affix.ToString()));

                    if (row.planMatchCount == 0)
                    {
                        row.outcome = "NOT RUN";
                        row.note = "Manifesto target não aparece no LastEnemySpawnPlan atual.";
                        scenarioOutput.status = "NOT RUN";
                    }
                    else if (row.activeMatchCount == 0)
                    {
                        row.outcome = "NOT RUN";
                        row.note = "Target aparece no plano, mas sem EnemyHealth ativo associado.";
                        scenarioOutput.status = "NOT RUN";
                    }
                    else
                    {
                        if (combatDatabases == null || combatDatabases.EnemyDatabase == null)
                        {
                            row.outcome = "NOT RUN";
                            row.note = "CombatRuntimeDatabasesRegistry/EnemyDatabase não disponível no runtime da cena.";
                            scenarioOutput.status = "NOT RUN";
                        }
                        else if (!combatDatabases.EnemyDatabase.TryGetById(target.EnemyId, out var enemyData) || enemyData == null)
                        {
                            row.outcome = "NOT RUN";
                            row.note = "Target natural presente, mas EnemyDataSO ausente no registry de runtime.";
                            scenarioOutput.status = "NOT RUN";
                        }
                        else
                        {
                            var runtimeClasses = planMatches
                                .Select(entry => ResolveRuntimeClass(enemyData, entry))
                                .Distinct()
                                .ToList();
                            // The same EnemyId may have normal and named-elite instances in one seed.
                            // Select only instances whose effective combat class matches the manifest.
                            var classMatches = planMatches
                                .Where(entry => ResolveRuntimeClass(enemyData, entry) == target.CreatureClass)
                                .ToList();
                            var classInstanceIds = classMatches
                                .Select(entry => entry.EnemyInstanceId)
                                .Where(id => !string.IsNullOrWhiteSpace(id))
                                .Distinct()
                                .ToList();
                            var classActiveMatches = activeMatches
                                .Where(health => classInstanceIds.Contains(health.EnemyInstanceId))
                                .ToList();

                            row.planMatchCount = classMatches.Count;
                            row.planInstanceIds = string.Join(",", classInstanceIds);
                            row.activeMatchCount = classActiveMatches.Count;
                            row.activeInstanceIds = string.Join(",", classActiveMatches.Select(health => health.EnemyInstanceId));
                            row.planEliteAffixes = string.Join(",", classMatches
                                .Select(entry => entry.EliteAffix)
                                .Where(affix => affix != Enemy.EliteAffix.None)
                                .Distinct()
                                .Select(affix => affix.ToString()));
                            row.runtimeClass = classMatches.Count > 0
                                ? target.CreatureClass.ToString()
                                : string.Join(",", runtimeClasses.Select(c => c.ToString()));

                            if (classInstanceIds.Count == 0)
                            {
                                row.outcome = "NOT RUN";
                                row.note = $"Nenhuma instância {target.CreatureClass} para {target.EnemyId}; classes no plano: {row.runtimeClass}.";
                                scenarioOutput.status = "NOT RUN";
                            }
                            else if (classActiveMatches.Count == 0 ||
                                     classActiveMatches.Select(health => health.EnemyInstanceId).Distinct().Count() != classActiveMatches.Count)
                            {
                                row.outcome = "NOT RUN";
                                row.note = "Instância da classe pretendida ausente ou duplicada entre EnemyHealth ativos.";
                                scenarioOutput.status = "NOT RUN";
                            }
                            else
                            {
                                row.targetPresent = true;
                                row.outcome = "CLASS_READY";
                                row.note = runtimeClasses.Count > 1
                                    ? "Classe por EnemyInstanceId confirmada; mesmo EnemyId tem instâncias de outra classe. Combate não medido."
                                    : "Classe por EnemyInstanceId confirmada; combate não medido.";
                            }
                        }
                    }

                    scenarioOutput.targets.Add(row);
                }

                if (scenarioOutput.targets.Count == 0)
                {
                    scenarioOutput.status = "NOT RUN";
                    AddNotRunRow(scenarioOutput, "Manifesto sem alvos definidos", string.Empty);
                }

                report.scenarios.Add(scenarioOutput);
                yield return LoadSceneAsync("FarmScene");
            }

            WriteProbeOutputs(report);
            Assert.That(report.scenarios.Count, Is.EqualTo(Phase21BuildManifest.Scenarios.Count));
            Assert.That(materializedScenarios, Is.EqualTo(Phase21BuildManifest.Scenarios.Count),
                "Nem todos os cenários materializaram Cave real; veja o JSON/CSV.");
            yield return null;
        }

        private static void AddNotRunRow(ProbeScenarioOutput scenarioOutput, string note, string enemyId)
        {
            scenarioOutput.status = "NOT RUN";
            scenarioOutput.targets.Add(new ProbeTargetRow
            {
                scenarioId = scenarioOutput.scenarioId,
                level = scenarioOutput.level,
                seed = scenarioOutput.seed,
                enemyId = enemyId,
                classSource = RuntimeClassSource,
                creatureClass = string.Empty,
                runtimeClass = string.Empty,
                telemetryClass = TelemetryCreatureClass.Unknown.ToString(),
                legacyBestiaryClass = TelemetryCreatureClass.Unknown.ToString(),
                manifestAvailability = "Unknown",
                outcome = "NOT RUN",
                planMatchCount = 0,
                activeMatchCount = 0,
                planEliteAffixes = string.Empty,
                planInstanceIds = string.Empty,
                activeInstanceIds = string.Empty,
                targetPresent = false,
                note = note
            });
        }

        private static List<ProbeNaturalEnemyDiagnosticRow> BuildNaturalEnemyDiagnostics(
            IReadOnlyCollection<CaveEnemySpawnPlanEntry> planEntries,
            CombatRuntimeDatabasesRegistrySO combatDatabases,
            BestiaryTelemetryClassLookup legacyClassLookup)
        {
            var rows = new List<ProbeNaturalEnemyDiagnosticRow>();
            if (planEntries == null || planEntries.Count == 0)
            {
                return rows;
            }

            foreach (var entry in planEntries)
            {
                var runtimeClass = TelemetryCreatureClass.Unknown;
                var legacyClass = TelemetryCreatureClass.Unknown;
                var note = "Classificação disponível via EnemyDataSO+SpawnPlan.";

                if (combatDatabases == null || combatDatabases.EnemyDatabase == null)
                {
                    note = "CombatRuntimeDatabasesRegistry/EnemyDatabase indisponível no runtime da cena.";
                }
                else if (combatDatabases.EnemyDatabase.TryGetById(entry.EnemyId, out var enemyData) && enemyData != null)
                {
                    runtimeClass = ResolveRuntimeClass(enemyData, entry);
                }
                else
                {
                    note = "EnemyDataSO ausente no registry de runtime para esta entrada.";
                }

                if (!string.IsNullOrWhiteSpace(entry.EnemyId))
                {
                    legacyClass = legacyClassLookup.GetClass(entry.EnemyId);
                }

                rows.Add(new ProbeNaturalEnemyDiagnosticRow
                {
                    enemyId = entry.EnemyId,
                    enemyInstanceId = entry.EnemyInstanceId,
                    runtimeClass = runtimeClass.ToString(),
                    classSource = RuntimeClassSource,
                    legacyBestiaryClass = legacyClass.ToString(),
                    eliteAffix = entry.EliteAffix == Enemy.EliteAffix.None ? string.Empty : entry.EliteAffix.ToString(),
                    planIsElite = entry.IsElite,
                    outcome = "PLAN_DIAGNOSTIC",
                    note = note
                });
            }

            return rows;
        }

        private static TelemetryCreatureClass ResolveRuntimeClass(EnemyDataSO enemyData, CaveEnemySpawnPlanEntry entry)
        {
            if (enemyData == null)
            {
                return TelemetryCreatureClass.Unknown;
            }

            if (enemyData.IsBoss || enemyData.PrimaryRole == EnemyRole.Boss)
            {
                return TelemetryCreatureClass.Boss;
            }

            if (enemyData.IsMiniBoss || enemyData.PrimaryRole == EnemyRole.MiniBoss)
            {
                return TelemetryCreatureClass.Miniboss;
            }

            if (enemyData.IsElite
                || enemyData.PrimaryRole == EnemyRole.Elite
                || (entry != null && entry.EliteAffix != Enemy.EliteAffix.None))
            {
                return TelemetryCreatureClass.Elite;
            }

            return TelemetryCreatureClass.Common;
        }

        private static IEnumerator StartToFarmScene()
        {
            if (SceneManager.GetActiveScene().name != "FarmScene")
            {
                yield return LoadSceneAsync("FarmScene");
            }

            var bootstrapTimeout = 240;
            for (var frame = 0; frame < bootstrapTimeout && GameBootstrap.Instance == null; frame++)
            {
                yield return null;
            }

            for (var frame = 0; frame < 240 && PlayerController.ActiveInstance == null; frame++)
            {
                yield return null;
            }
        }

        private static IEnumerator LoadSceneAsync(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                yield break;
            }

            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (op != null && !op.isDone)
            {
                yield return null;
            }

            for (var frame = 0; frame < 240 && SceneManager.GetActiveScene().name != sceneName; frame++)
            {
                yield return null;
            }
        }

        private static IEnumerator WaitForScene(string sceneName)
        {
            for (var frame = 0; frame < 360 && SceneManager.GetActiveScene().name != sceneName; frame++)
            {
                yield return null;
            }
        }

        private static IEnumerator WaitForMaterializedSpawn(CaveLevelRuntimeController levelController)
        {
            for (var frame = 0; frame < 360; frame++)
            {
                var materializer = levelController != null ? levelController.Materializer : null;
                var level = levelController != null ? levelController.CurrentGeneratedLevel : null;
                var plan = materializer?.LastEnemySpawnPlan;

                if (levelController != null
                    && level != null
                    && materializer != null
                    && materializer.LastMaterializationResult != null
                    && materializer.LastMaterializationResult.CreatedFloorTiles > 0
                    && plan != null
                    && plan.Entries != null)
                {
                    yield break;
                }

                yield return null;
            }
        }

        private static T FindInActiveScene<T>() where T : Component
        {
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var component = root.GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static string QuoteCsv(string value)
        {
            var safe = value ?? string.Empty;
            if (safe.Contains('"') || safe.Contains(',') || safe.Contains('\n') || safe.Contains('\r'))
            {
                safe = safe.Replace("\"", "\"\"");
                return $"\"{safe}\"";
            }

            return safe;
        }

        private static void WriteProbeOutputs(ProbeReport report)
        {
            report.scenarioCount = report.scenarios.Count;

            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var outputRoot = Path.Combine(projectRoot, "Logs", "phase21_combat_probe");
            Directory.CreateDirectory(outputRoot);

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var jsonPath = Path.Combine(outputRoot, $"phase21_combat_target_probe_{timestamp}.json");
            var csvPath = Path.Combine(outputRoot, $"phase21_combat_target_probe_{timestamp}.csv");

            report.jsonFilePath = jsonPath;
            report.csvFilePath = csvPath;

            File.WriteAllText(jsonPath, JsonUtility.ToJson(report, true));
            File.WriteAllText(csvPath, BuildCsv(report));

            Debug.Log($"Phase21 Target Probe output JSON: {jsonPath}");
            Debug.Log($"Phase21 Target Probe output CSV: {csvPath}");
        }

        private static string BuildCsv(ProbeReport report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("scenarioId,level,seed,naturalEnemyIds,targetEnemyId,creatureClass,runtimeClass,classSource,telemetryClass,legacyBestiaryClass,manifestAvailability,outcome,targetPresent,planMatchCount,planInstanceIds,activeMatchCount,activeInstanceIds,planEliteAffixes,note,recordType,diagnosticEnemyId,diagnosticInstanceId,diagnosticRuntimeClass,diagnosticEliteAffix,diagnosticClassSource,diagnosticLegacyBestiaryClass,diagnosticNote,diagnosticPlanIsElite");

            foreach (var scenario in report.scenarios)
            {
                if (scenario.targets == null || scenario.targets.Count == 0)
                {
                    sb.AppendLine(string.Join(",",
                        QuoteCsv(scenario.scenarioId),
                        scenario.level.ToString(),
                        QuoteCsv(scenario.seed),
                        QuoteCsv(scenario.naturalEnemyIds),
                        "",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "Unknown",
                        "NOT RUN",
                        "False",
                        "0",
                        "",
                        "0",
                        "",
                        "",
                        QuoteCsv("Sem alvos no manifesto para este cenário."),
                        "TARGET",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "",
                        ""));
                    continue;
                }

                foreach (var row in scenario.targets)
                {
                    sb.AppendLine(string.Join(",",
                        QuoteCsv(row.scenarioId),
                        row.level.ToString(),
                        QuoteCsv(row.seed),
                        QuoteCsv(scenario.naturalEnemyIds),
                        QuoteCsv(row.enemyId),
                        QuoteCsv(row.creatureClass),
                        QuoteCsv(row.runtimeClass),
                        QuoteCsv(row.classSource),
                        QuoteCsv(row.telemetryClass),
                        QuoteCsv(row.legacyBestiaryClass),
                        QuoteCsv(row.manifestAvailability),
                        QuoteCsv(row.outcome),
                        row.targetPresent.ToString(),
                        row.planMatchCount.ToString(),
                        QuoteCsv(row.planInstanceIds),
                        row.activeMatchCount.ToString(),
                        QuoteCsv(row.activeInstanceIds),
                        QuoteCsv(row.planEliteAffixes),
                        QuoteCsv(row.note),
                        "TARGET",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "",
                        ""));
                }

                if (scenario.naturalEnemyDiagnostics == null || scenario.naturalEnemyDiagnostics.Count == 0)
                {
                    continue;
                }

                foreach (var diag in scenario.naturalEnemyDiagnostics)
                {
                    sb.AppendLine(string.Join(",",
                        QuoteCsv(scenario.scenarioId),
                        scenario.level.ToString(),
                        QuoteCsv(scenario.seed),
                        QuoteCsv(scenario.naturalEnemyIds),
                        "",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "PlanOnly",
                        QuoteCsv(diag.outcome),
                        "False",
                        "0",
                        "",
                        "0",
                        "",
                        "",
                        QuoteCsv(diag.note),
                        "NATURAL_PLAN",
                        QuoteCsv(diag.enemyId),
                        QuoteCsv(diag.enemyInstanceId),
                        QuoteCsv(diag.runtimeClass),
                        QuoteCsv(diag.eliteAffix),
                        QuoteCsv(diag.classSource),
                        QuoteCsv(diag.legacyBestiaryClass),
                        QuoteCsv(diag.note),
                        diag.planIsElite.ToString()));
                }
            }

            return sb.ToString();
        }
    }
}
