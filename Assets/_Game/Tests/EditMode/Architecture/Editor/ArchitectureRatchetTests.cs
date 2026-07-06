using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace CindarsHope.Tests.EditMode.Architecture
{
    public class ArchitectureRatchetTests
    {
        private const string RulesRelativePath = "tools/architecture/architecture-ratchet-rules.tsv";
        private const string BaselineRelativePath = "tools/architecture/architecture-ratchet-baseline.tsv";
        private const string SerializedGuidBaselineRelativePath =
            "tools/architecture/serialized-guid-baseline.tsv";

        private sealed class Rule
        {
            public Rule(string id, string pattern)
            {
                Id = id;
                Pattern = new Regex(pattern, RegexOptions.CultureInvariant);
            }

            public string Id { get; }
            public Regex Pattern { get; }
        }

        [Test]
        public void RuntimeSource_DoesNotIncreaseTrackedArchitecturalDebt()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            Assert.That(projectRoot, Is.Not.Null.And.Not.Empty);

            IReadOnlyList<Rule> rules = ReadRules(Path.Combine(projectRoot, RulesRelativePath));
            IReadOnlyDictionary<string, int> baseline = ReadBaseline(
                Path.Combine(projectRoot, BaselineRelativePath),
                rules);
            string runtimeRoot = Path.Combine(projectRoot, "Assets", "_Game", "Scripts");
            string[] files = Directory.GetFiles(runtimeRoot, "*.cs", SearchOption.AllDirectories)
                .Where(path => !Normalize(path).Contains("/Editor/"))
                .ToArray();

            var violations = new List<string>();
            foreach (Rule rule in rules)
            {
                foreach (string file in files)
                {
                    string relativePath = GetRelativePath(projectRoot, file);
                    int currentCount = rule.Pattern.Matches(File.ReadAllText(file)).Count;
                    baseline.TryGetValue(BuildKey(rule.Id, relativePath), out int allowedCount);

                    if (currentCount > allowedCount)
                    {
                        violations.Add(
                            $"{rule.Id}: {relativePath} has {currentCount} occurrence(s); " +
                            $"baseline allows {allowedCount}.");
                    }
                }
            }

            Assert.That(
                violations,
                Is.Empty,
                "Architectural debt increased. Remove the new occurrence; do not raise the baseline " +
                "without an explicit architecture decision.\n" + string.Join("\n", violations));
        }

        [Test]
        public void SensitiveSerializedFiles_PreservePathsAndGuids()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            Assert.That(projectRoot, Is.Not.Null.And.Not.Empty);
            string baselinePath = Path.Combine(projectRoot, SerializedGuidBaselineRelativePath);
            Assert.That(File.Exists(baselinePath), Is.True, $"Serialized GUID baseline not found: {baselinePath}");

            var violations = new List<string>();
            foreach (string line in ReadDataLines(baselinePath))
            {
                string[] parts = line.Split('\t');
                Assert.That(parts.Length, Is.EqualTo(3), $"Invalid serialized GUID baseline line: {line}");
                string assetPath = Path.Combine(projectRoot, parts[1].Replace('/', Path.DirectorySeparatorChar));
                string metaPath = assetPath + ".meta";

                if (!File.Exists(assetPath) || !File.Exists(metaPath))
                {
                    violations.Add($"{parts[0]} missing asset or meta: {parts[1]}");
                    continue;
                }

                string guidLine = File.ReadLines(metaPath)
                    .FirstOrDefault(metaLine => metaLine.StartsWith("guid: ", StringComparison.Ordinal));
                string actualGuid = guidLine?.Substring("guid: ".Length).Trim();
                if (!string.Equals(actualGuid, parts[2], StringComparison.Ordinal))
                {
                    violations.Add(
                        $"{parts[0]} GUID changed for {parts[1]}: expected {parts[2]}, actual {actualGuid ?? "<none>"}.");
                }
            }

            Assert.That(
                violations,
                Is.Empty,
                "Serialized paths/GUIDs changed. Move Unity assets with their .meta and provide an explicit " +
                "migration decision.\n" + string.Join("\n", violations));
        }

        [Test]
        public void CSharpSource_DoesNotHardcodeUnityPredefinedAssemblyName()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            Assert.That(projectRoot, Is.Not.Null.And.Not.Empty);
            string predefinedAssemblyName = "Assembly" + "-CSharp";
            string[] sourceRoots =
            {
                Path.Combine(projectRoot, "Assets", "_Game", "Scripts"),
                Path.Combine(projectRoot, "Assets", "_Game", "Tests")
            };

            var violations = new List<string>();
            foreach (string sourceRoot in sourceRoots)
            {
                foreach (string file in Directory.GetFiles(sourceRoot, "*.cs", SearchOption.AllDirectories))
                {
                    if (File.ReadAllText(file).Contains(predefinedAssemblyName))
                    {
                        violations.Add(GetRelativePath(projectRoot, file));
                    }
                }
            }

            Assert.That(
                violations,
                Is.Empty,
                "C# source must resolve types/projects by direct type reference or controlled discovery, " +
                "not by Unity predefined assembly name.\n" + string.Join("\n", violations));
        }

        [Test]
        public void FoundationAssembly_ContainsOnlyTheCuratedPureContracts()
        {
            Type identifiedDataType = typeof(CindarsHope.Core.Data.IIdentifiedData);
            Type registryType = typeof(CindarsHope.Core.Data.IDataRegistry<>);
            string foundationAssemblyName = "CindarsHope.Foundation";

            Assert.That(identifiedDataType.Assembly, Is.SameAs(registryType.Assembly));
            Assert.That(identifiedDataType.Assembly.GetName().Name, Is.EqualTo(foundationAssemblyName));
            Assert.That(
                identifiedDataType.Assembly.GetReferencedAssemblies()
                    .Select(reference => reference.Name)
                    .Where(name => name != null)
                    .Any(name => name.StartsWith("UnityEngine", StringComparison.Ordinal)),
                Is.False,
                "Foundation must remain independent from UnityEngine assemblies.");

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            Assert.That(projectRoot, Is.Not.Null.And.Not.Empty);
            string foundationRoot = Path.Combine(
                projectRoot,
                "Assets",
                "_Game",
                "Scripts",
                "Foundation");
            string[] sourceFiles = Directory.GetFiles(foundationRoot, "*.cs", SearchOption.AllDirectories);

            string[] expectedFoundationFiles =
            {
                "IIdentifiedData.cs",
                "IDataRegistry.cs",
                "IGameClock.cs",
                "IRandomSource.cs",
                "IInventoryWalletTransactionPorts.cs"
            };
            Assert.That(
                sourceFiles.Select(Path.GetFileName),
                Is.EquivalentTo(expectedFoundationFiles),
                "Foundation scope must grow only by explicit architecture decision.");
            Assert.That(
                sourceFiles.Select(File.ReadAllText).Any(source => source.Contains("UnityEngine")),
                Is.False,
                "Foundation source must not depend on UnityEngine.");
        }

        [Test]
        public void GameplayAssembly_ContainsOnlyCuratedPureDecisionTypes()
        {
            Type shortcutType = typeof(CindarsHope.UI.Routing.GameplayShortcutDecision);
            Type interactionType = typeof(CindarsHope.NPC.NpcShopInteractionSession);
            Type narrativeIdsType = typeof(CindarsHope.Gameplay.Narrative.NarrativeIds);

            Assert.That(shortcutType.Assembly, Is.SameAs(interactionType.Assembly));
            Assert.That(shortcutType.Assembly, Is.SameAs(narrativeIdsType.Assembly));
            Assert.That(shortcutType.Assembly.GetName().Name, Is.EqualTo("CindarsHope.Gameplay"));
            Assert.That(
                shortcutType.Assembly.GetReferencedAssemblies()
                    .Select(reference => reference.Name)
                    .Where(name => name != null)
                    .Any(name => name.StartsWith("UnityEngine", StringComparison.Ordinal)),
                Is.False,
                "Gameplay decisions must remain independent from UnityEngine assemblies.");

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            Assert.That(projectRoot, Is.Not.Null.And.Not.Empty);
            string gameplayRoot = Path.Combine(projectRoot, "Assets", "_Game", "Scripts", "Gameplay");
            string[] sourceFiles = Directory.GetFiles(gameplayRoot, "*.cs", SearchOption.AllDirectories);
            string[] expectedFiles =
            {
                "GameplayShortcutDecision.cs",
                "GameplayInputBlocker.cs",
                "NarrativeIds.cs",
                "NpcShopDialogueChoicePolicy.cs",
                "NpcShopInteractionSession.cs",
                "NpcShopTransactionReadinessPolicy.cs",
                "QuestGiverInteractionMode.cs",
                "SkillActionEffectCatalog.cs",
                "ThalindraQuestDialoguePolicy.cs"
            };

            Assert.That(sourceFiles.Select(Path.GetFileName), Is.EquivalentTo(expectedFiles));
            Assert.That(
                sourceFiles.Select(File.ReadAllText).Any(source => source.Contains("UnityEngine")),
                Is.False,
                "Gameplay source must not depend on UnityEngine.");
        }

        private static IReadOnlyList<Rule> ReadRules(string path)
        {
            Assert.That(File.Exists(path), Is.True, $"Architecture rule file not found: {path}");

            var rules = new List<Rule>();
            foreach (string line in ReadDataLines(path))
            {
                string[] parts = line.Split(new[] { '\t' }, 2);
                Assert.That(parts.Length, Is.EqualTo(2), $"Invalid architecture rule line: {line}");
                rules.Add(new Rule(parts[0], parts[1]));
            }

            Assert.That(rules.Count, Is.GreaterThan(0), "Architecture rule set cannot be empty.");
            return rules;
        }

        private static IReadOnlyDictionary<string, int> ReadBaseline(
            string path,
            IReadOnlyList<Rule> rules)
        {
            Assert.That(File.Exists(path), Is.True, $"Architecture baseline not found: {path}");

            var knownRuleIds = new HashSet<string>(rules.Select(rule => rule.Id), StringComparer.Ordinal);
            var baseline = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (string line in ReadDataLines(path))
            {
                string[] parts = line.Split('\t');
                Assert.That(parts.Length, Is.EqualTo(3), $"Invalid architecture baseline line: {line}");
                Assert.That(knownRuleIds, Does.Contain(parts[0]), $"Unknown rule in baseline: {parts[0]}");
                Assert.That(int.TryParse(parts[2], out int count), Is.True, $"Invalid count: {line}");
                baseline.Add(BuildKey(parts[0], parts[1]), count);
            }

            return baseline;
        }

        private static IEnumerable<string> ReadDataLines(string path)
        {
            return File.ReadLines(path)
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"));
        }

        private static string BuildKey(string ruleId, string relativePath)
        {
            return ruleId + "\n" + Normalize(relativePath);
        }

        private static string Normalize(string path)
        {
            return path.Replace('\\', '/');
        }

        private static string GetRelativePath(string root, string path)
        {
            string rootPrefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                Path.DirectorySeparatorChar;
            Assert.That(path.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase), Is.True);
            return Normalize(path.Substring(rootPrefix.Length));
        }
    }
}
