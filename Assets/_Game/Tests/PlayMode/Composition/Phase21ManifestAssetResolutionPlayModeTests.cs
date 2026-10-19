using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CindarsHope.Combat;
using CindarsHope.Combat.Telemetry;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Inventory.Data;
using CindarsHope.Skills;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CindarsHope.Tests.PlayMode.Composition
{
    /// <summary>
    /// Smoke de resolução dos assets usados pela matriz. Não executa uma build nem declara
    /// aceitação: confirma apenas que os IDs do manifesto fecham nos bancos autorados.
    /// </summary>
    public sealed class Phase21ManifestAssetResolutionPlayModeTests
    {
        [UnityTest]
        public IEnumerator ManifestIdsResolveAgainstFarmBootstrapAndRuntimeRegistries()
        {
            // O runner vazio inicializa o bootstrap antes da Farm e emite este diagnóstico;
            // numa suíte contínua, o GameBootstrap já persiste entre cenas e ele não reaparece.
            if (GameBootstrap.Instance == null)
                LogAssert.Expect(LogType.Error,
                    new Regex("^\\[DeathSystemBootstrap\\] GameBootstrap/PlayerManager/CorpseRecoveryManager nao ficaram prontos"));
            yield return SceneManager.LoadSceneAsync("FarmScene", LoadSceneMode.Single);
            yield return null;

            Assert.That(GameBootstrap.Instance, Is.Not.Null, "FarmScene não materializou GameBootstrap.");
            var combat = Resources.Load<CombatRuntimeDatabasesRegistrySO>("CombatRuntimeDatabasesRegistry");
            var skills = Resources.Load<SkillRuntimeCatalogRegistrySO>("Skills/SkillRuntimeCatalogRegistry");
            Assert.That(combat, Is.Not.Null, "Registro de bancos de combate não encontrado.");
            Assert.That(skills, Is.Not.Null, "Registro de catálogo de skills não encontrado.");

            var items = GameBootstrap.Instance.ItemDatabase as ItemDatabaseSO;
            var enemies = combat.EnemyDatabase;
            Assert.That(items, Is.Not.Null, "FarmScene/GameBootstrap não expôs ItemDatabaseSO.");
            Assert.That(enemies, Is.Not.Null, "EnemyDatabaseSO não está ligado ao registro de runtime.");
            Assert.That(skills.NodeDatabase, Is.Not.Null, "SkillNodeDatabaseSO não está ligado ao registro.");
            Assert.That(skills.ActionDatabase, Is.Not.Null, "SkillActionDatabaseSO não está ligado ao registro.");

            foreach (var stageAction in Phase21BuildManifest.Level20Actions)
            {
                var action = skills.ActionDatabase.GetRequired(stageAction.ActionId);
                var node = skills.NodeDatabase.All.FirstOrDefault(n => n != null &&
                    n.UnlockedSkillActionId == stageAction.ActionId);
                Assert.That(node, Is.Not.Null, $"L20/{stageAction.Id}: ação sem nó autorado.");
                Assert.That(node.AuthoredMaxRank, Is.GreaterThanOrEqualTo(Phase21BuildManifest.Rank3),
                    $"L20/{stageAction.Id}: R3 não é autorado.");
                Assert.That(action.SkillActionId, Is.EqualTo(stageAction.ActionId));
                AssertPrerequisiteClosure(skills.NodeDatabase, node, stageAction.Id);
            }

            foreach (var build in Phase21BuildManifest.Builds)
            {
                var capstone = ResolveNode(skills.NodeDatabase, build.CapstoneNodeId, build.BuildId);
                Assert.That(capstone.IsCapstone, Is.True, $"{build.BuildId}: capstone não está marcado como capstone.");
                Assert.That(capstone.AuthoredMaxRank, Is.GreaterThanOrEqualTo(Phase21BuildManifest.Rank3),
                    $"{build.BuildId}: capstone não suporta R3 autorado.");
                if (!string.IsNullOrEmpty(build.OffensiveActionId))
                    Assert.That(capstone.MinimumPlayerLevel, Is.LessThanOrEqualTo(Phase21BuildManifest.Level50),
                        $"{build.BuildId}: capstone de combate não admite estágio L50.");
                if (!string.IsNullOrEmpty(build.VariantId))
                    Assert.That(capstone.CapstoneVariants, Does.Contain(build.VariantId),
                        $"{build.BuildId}: variante não está autorada no capstone.");
                AssertPrerequisiteClosure(skills.NodeDatabase, capstone, build.BuildId);

                if (!string.IsNullOrEmpty(build.OffensiveActionId))
                {
                    var action = skills.ActionDatabase.GetRequired(build.OffensiveActionId);
                    var actionNode = skills.NodeDatabase.All.FirstOrDefault(n => n != null &&
                        n.UnlockedSkillActionId == build.OffensiveActionId);
                    Assert.That(actionNode, Is.Not.Null, $"{build.BuildId}: ação não tem nó autorado.");
                    AssertPrerequisiteClosure(skills.NodeDatabase, actionNode, build.BuildId);
                    Assert.That(action.SkillActionId, Is.EqualTo(build.OffensiveActionId));
                }

                AssertFixture(items, build.Level20Equipment, build.BuildId);
                AssertFixture(items, build.Level50Equipment, build.BuildId);
                Assert.That(build.ManifestFields,
                    string.IsNullOrEmpty(build.OffensiveActionId)
                        ? Is.EqualTo(Phase21ManifestFields.Missing)
                        : Is.EqualTo(Phase21ManifestFields.Specified),
                    $"{build.BuildId}: presença dos campos do manifesto não reflete os IDs autorados.");

                foreach (var rank in Phase21BuildManifest.Ranks)
                    Assert.That(rank, Is.InRange(1, capstone.AuthoredMaxRank),
                        $"{build.BuildId}: rank R{rank} fora do limite do capstone.");
            }

            foreach (var scenario in Phase21BuildManifest.Scenarios)
            foreach (var target in scenario.Targets)
            {
                if (target.Availability == Phase21TargetAvailability.Unavailable)
                {
                    Assert.That(target.EnemyId, Is.Null, $"{scenario.ScenarioId}: alvo indisponível tem ID.");
                    continue;
                }

                if (target.Availability == Phase21TargetAvailability.CatalogOnly)
                    Assert.That(target.UnavailableReason, Is.Not.Empty,
                        $"{scenario.ScenarioId}: alvo sem gate precisa motivo explícito.");

                var enemy = enemies.GetRequired(target.EnemyId);
                AssertEnemyCompatibility(enemy, target, scenario.Level, scenario.ScenarioId);
            }
        }

        private static SkillNodeDataSO ResolveNode(SkillNodeDatabaseSO database, string id, string buildId)
        {
            Assert.That(database.TryGetById(id, out var node), Is.True, $"{buildId}: capstone '{id}' ausente.");
            return node;
        }

        private static void AssertPrerequisiteClosure(SkillNodeDatabaseSO database, SkillNodeDataSO node, string buildId)
        {
            var visited = new HashSet<string>();
            var pending = new Stack<string>(node.PrerequisiteNodeIds ?? new List<string>());
            while (pending.Count > 0)
            {
                var id = pending.Pop();
                if (string.IsNullOrEmpty(id) || !visited.Add(id)) continue;
                Assert.That(database.TryGetById(id, out var prerequisite), Is.True,
                    $"{buildId}: pré-requisito '{id}' ausente para '{node.SkillNodeId}'.");
                foreach (var parent in prerequisite.PrerequisiteNodeIds ?? new List<string>())
                    pending.Push(parent);
            }
        }

        private static void AssertFixture(ItemDatabaseSO database, Phase21EquipmentFixture fixture, string buildId)
        {
            if (fixture == null) return;
            Assert.That(fixture.Selection, Is.EqualTo(Phase21EquipmentSelection.Fixture),
                $"{buildId}: equipamento precisa permanecer marcado como fixture.");
            Assert.That(database.TryGetById(fixture.ItemId, out var item), Is.True,
                $"{buildId}: equipamento fixture '{fixture.ItemId}' ausente.");
            Assert.That(item.IsEquippable, Is.True, $"{buildId}: fixture '{fixture.ItemId}' não é equipável.");
        }

        private static void AssertEnemyCompatibility(EnemyDataSO enemy, Phase21TargetDefinition target,
            int level, string scenarioId)
        {
            Assert.That(enemy.enemyId, Is.EqualTo(target.EnemyId));
            Assert.That(enemy.CaveBand, Is.EqualTo(level == Phase21BuildManifest.Level20 ? 2 : 4),
                $"{scenarioId}: banda incompatível para '{target.EnemyId}'.");
            Assert.That(enemy.enemyLevel, Is.LessThanOrEqualTo(level),
                $"{scenarioId}: nível autorado incompatível para '{target.EnemyId}'.");

            var actualClass = enemy.IsBoss ? TelemetryCreatureClass.Boss :
                enemy.IsMiniBoss ? TelemetryCreatureClass.Miniboss :
                enemy.IsElite ? TelemetryCreatureClass.Elite : TelemetryCreatureClass.Common;
            Assert.That(actualClass, Is.EqualTo(target.CreatureClass),
                $"{scenarioId}: classe incompatível para '{target.EnemyId}'.");
        }
    }
}
