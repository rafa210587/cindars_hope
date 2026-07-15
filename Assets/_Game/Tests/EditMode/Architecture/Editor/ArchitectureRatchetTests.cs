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
                "IInventoryWalletTransactionPorts.cs",
                // arch: quebra do ciclo Economy|Save (spec_arch_economy_save_cycle_reduction_v22) —
                // DTOs puros (sem UnityEngine) movidos de CindarsHope.Save para o schema de save em
                // Foundation, decisão explícita de arquitetura.
                "EconomySaveDtos.cs",
                // arch: quebra do ciclo Quests|Save (spec_arch_quests_save_cycle_reduction_v24) —
                // enum puro (sem UnityEngine) movido de CindarsHope.Quests para Foundation, decisão
                // explícita de arquitetura.
                "QuestSource.cs",
                // arch: quebra do ciclo Save|UI (spec_arch_save_ui_cycle_reduction_v26) — tipos puros
                // (sem UnityEngine) movidos de CindarsHope.UI.Hotbar para Foundation, decisão
                // explícita de arquitetura.
                "HotbarState.cs",
                "HotbarSaveData.cs",
                // arch: quebra do ciclo mutuo Equipment|Save (spec_arch_equipment_save_cycle_reduction_v29)
                // — enum EquipmentSlot e os DTOs de save de equipment (puros, sem UnityEngine) movidos
                // de CindarsHope.Equipment/CindarsHope.Save para Foundation, decisão explícita de
                // arquitetura.
                "EquipmentSlot.cs",
                "EquipmentSaveDtos.cs",
                // arch: infraestrutura para quebra do ciclo Core|Inventory
                // (spec_arch_core_inventory_cycle_reduction_v36) — registry generico puro (sem
                // UnityEngine) que permite a InventoryManager se anunciar sem static Instance/Active
                // (proibido pela regra de ratchet GlobalInventoryAccess) e sem Core referenciar
                // CindarsHope.Inventory diretamente.
                "DomainManagerRegistry.cs",
                // arch: quebra do ciclo Core|Player (spec_arch_core_player_cycle_reduction_v37) —
                // enums puros (sem UnityEngine) movidos de CindarsHope.Player/CindarsHope.Player.Progression
                // para Foundation, decisão explícita de arquitetura.
                "HazardType.cs",
                "PlayerAttributeType.cs",
                // arch: quebra do ciclo Core|UI (spec_arch_core_ui_cycle_reduction_v38) — port puro
                // (sem UnityEngine) que permite a Core.GameTimeManager consultar HasActiveModal sem
                // referenciar CindarsHope.UI.Modal, decisao explicita de arquitetura.
                "IModalStateProvider.cs",
                // arch: quebra do ciclo real Core|Craft após snapshot fully-qualified (2026-07-10) —
                // port puro para GameBootstrap inicializar/desligar crafting sem referenciar
                // CindarsHope.Craft.CraftingManager.
                "ICraftingRuntimeManager.cs",
                // arch: quebra do ciclo Inventory|Magic após snapshot fully-qualified (2026-07-10) —
                // enum puro usado por ItemDataSO e pelo runtime de magia, movido sem alterar valores
                // inteiros serializados.
                "SpellSourceType.cs",
                // arch: quebra do ciclo real Core|Economy (2026-07-12) — port puro para Save/Core
                // consumirem estoque de lojas sem referenciar CindarsHope.Economy.ShopManager.
                "IShopStockRuntime.cs",
                // arch: quebra da direcao Core->Equipment (2026-07-12) — port puro usado por
                // GameBootstrap/CombatRuntimeInstaller/CorpseRecoveryManager sem referenciar
                // EquipmentManager.
                "IEquipmentRuntime.cs",
                // arch: quebra da direcao World->NPC (2026-07-12) — portas de cena consultam
                // disponibilidade/marker de NPC via ports puros, sem referenciar NpcScheduleService
                // ou NpcDweller concretos.
                "INpcScheduleAvailabilityRuntime.cs",
                "INpcDoorTraveler.cs",
                // arch: quebra da direcao Save->UI (2026-07-12) — SaveManager persiste hints de
                // onboarding via port puro em vez de instanciar provider do modulo UI.
                "IOnboardingHintsRuntime.cs",
                // arch: quebra da direcao Economy->Equipment (2026-07-12) — Economy aplica bonus
                // de ouro via port puro, sem referenciar AccessoryEffectRouter concreto.
                "IGoldGainModifierRuntime.cs",
                // arch: quebra do par Core|Skills (2026-07-14) — GameBootstrap resolve o skill tree
                // via port puro ISkillTreeRuntime em vez do tipo concreto SkillTreeManager.
                "ISkillTreeRuntime.cs",
                // arch: quebra do par mutuo Player|World (2026-07-14) — SceneNames/SceneId (consts
                // puras, sem UnityEngine) movidos de CindarsHope.World.Scenes para Foundation, para
                // que AnyaFountainRespawnFlow (Player) resolva o id da cena/anchor da Fonte sem
                // nomear CindarsHope.World. SceneTransitionRequest/Router permanecem em World.Scenes;
                // Player os alcanca via ISceneTransitionRouter (porta em CindarsHope.SceneManagement).
                "SceneNames.cs",
                "SceneId.cs",
                // arch: quebra do par Equipment|Player (2026-07-14) — Equipment aplica o reparo efetivo
                // via porta neutra em vez de nomear PlayerVitalsApplier/DerivedFollowupFormulas.
                "RepairEfficiencyProvider.cs",
                // arch: quebra da direcao Quests->NPC (2026-07-14) — QuestGiverInteractable resolve o
                // id do NPC dono via porta pura (string apenas) em vez de nomear CindarsHope.NPC.
                "INpcIdentity.cs",
                // arch: quebra dos pares mutuos Combat|Skills e Player|Skills (2026-07-15) —
                // SkillPassiveModifier (tipo puro) e os enums SkillModifierType/SkillTreeId movidos
                // de CindarsHope.Skills para Foundation; Combat/Player consomem sem nomear
                // CindarsHope.Skills. SkillTreeManager.Instance foi trocado por
                // DomainManagerRegistry.Get<ISkillTreeRuntime>() nos callers de Combat/Player.
                "SkillPassiveModifier.cs",
                "SkillModifierType.cs",
                "SkillTreeId.cs",
                // arch: quebra do par mutuo Core|UI (2026-07-15) — enum ModalType e port IModalRuntime
                // (puros, sem UnityEngine) movidos/criados a partir de CindarsHope.UI.Modal para que
                // GameBootstrap (Core) exponha push/pop/clear de modal sem nomear ModalManager concreto.
                "ModalType.cs",
                "IModalRuntime.cs",
                // arch: quebra do par mutuo Core|Save (2026-07-15) — port puro (sem UnityEngine) que
                // permite a GameBootstrap (Core) expor save/load/hotbar sem nomear
                // CindarsHope.Save.SaveManager. Rebind*/Initialize/Shutdown ficam de fora da porta
                // (assinaturas cross-modulo); GameBootstrap resolve esses via
                // IGameBootstrapRuntimeService (Core.Bootstrap).
                "ISaveRuntime.cs"
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
